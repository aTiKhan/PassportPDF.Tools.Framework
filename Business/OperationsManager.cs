﻿/**********************************************************************
 * Project:                 PassportPDF.Tools.Framework
 * Authors:                 - Evan Carrère.
 *                          - Loïc Carrère.
 *
 * (C) Copyright 2018, ORPALIS.
 ** Licensed under the Apache License, Version 2.0 (the "License");
 ** you may not use this file except in compliance with the License.
 ** You may obtain a copy of the License at
 ** http://www.apache.org/licenses/LICENSE-2.0
 ** Unless required by applicable law or agreed to in writing, software
 ** distributed under the License is distributed on an "AS IS" BASIS,
 ** WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 ** See the License for the specific language governing permissions and
 ** limitations under the License.
 *
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.IO.Compression;
using PassportPDF.Api;
using PassportPDF.Model;
using PassportPDF.Tools.Framework.Models;
using PassportPDF.Tools.Framework.Utilities;
using PassportPDF.Tools.Framework.Configuration;
using PassportPDF.Tools.Framework.Errors;

namespace PassportPDF.Tools.Framework.Business
{
    public sealed class OperationsManager
    {
        private readonly List<FileToProcess> _filesToProcess = new List<FileToProcess>();

        private readonly object _locker = new object();

        private bool _workPaused;

        private bool _cancellationPending;

        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(true);

        public delegate void ErrorDelegate(string errorMessage);
        public delegate void WarningDelegate(string warningMessage);
        public delegate void FileOperationsCompletionDelegate(FileOperationsResult fileOperationsResult);
        public delegate void WorkCompletionDelegate(int workerNumber);
        public delegate void ProgressDelegate(int workerNumber, string fileName, int retries);
        public delegate void PauseDelegate(int workerNumber);
        public delegate void UpdateRemainingTokensDelegate(long remainingTokens);

        public ProgressDelegate UploadOperationStartEventHandler;
        public ProgressDelegate FileOperationStartEventHandler;
        public ProgressDelegate DownloadOperationStartEventHandler;

        public UpdateRemainingTokensDelegate RemainingTokensUpdateEventHandler;

        public FileOperationsCompletionDelegate FileOperationsSuccesfullyCompletedEventHandler;
        public WorkCompletionDelegate WorkCompletionEventHandler;

        public PauseDelegate WorkPauseEventHandler;

        public WarningDelegate WarningEventHandler;
        public ErrorDelegate ErrorEventHandler;

        public void Feed(List<FileToProcess> filesToProcess)
        {
            lock (_locker)
            {
                _filesToProcess.AddRange(filesToProcess);
            }
        }


        public void Start(int workerCount, string destinationFolder, FileProductionRules fileProductionRules, OperationsWorkflow workflow, string apiKey)
        {
            lock (_locker)
            {
                _cancellationPending = false;
            }

            PDFApi apiInstance = new PDFApi(FrameworkGlobals.API_SERVER_URI);
            apiInstance.Configuration.AddDefaultHeader("X-PassportPDF-API-Key", apiKey);
            apiInstance.Configuration.Timeout = FrameworkGlobals.PassportPDFConfiguration.SuggestedClientTimeout;

            destinationFolder = ParsingUtils.EnsureFolderPathEndsWithBackSlash(destinationFolder);

            bool fileSizeReductionIsIntended = OperationsWorkflowUtilities.IsFileSizeReductionIntended(workflow);

            for (int i = 1; i <= workerCount; i++)
            {
                int workerNumber = i;
                // Launch the workers.
                Thread thread = new Thread(() => Process(apiInstance, workerNumber, fileProductionRules, workflow, destinationFolder, fileSizeReductionIsIntended));
                thread.Start();
            }
        }


        public bool PauseWork()
        {
            lock (_locker)
            {
                if (!_cancellationPending && _filesToProcess.Count > 0)
                {
                    _workPaused = true;
                    _waitHandle.Reset();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public void ResumeWork()
        {
            lock (_locker)
            {
                _workPaused = false;
                _waitHandle.Set();
            }
        }


        public void AbortWork()
        {
            lock (_locker)
            {
                _cancellationPending = true;
                _filesToProcess.Clear();
                if (_workPaused)
                {
                    // Resume worker threads so they can exit
                    ResumeWork();
                }
            }
        }


        private void Process(PDFApi apiInstance, int workerNumber, FileProductionRules fileProductionRules, OperationsWorkflow workflow, string destinationFolder, bool fileSizeReductionIsIntended)
        {
            while (PickFile(out FileToProcess fileToProcess))
            {
                if (_cancellationPending)
                {
                    break;
                }

                try
                {
                    long inputFileSize = FileUtils.GetFileSize(fileToProcess.FileAbsolutePath);
                    bool inputIsPDF = Path.GetExtension(fileToProcess.FileAbsolutePath).ToUpper() == ".PDF";

                    if (!CheckInputFileSizeValidity(inputFileSize, fileToProcess.FileAbsolutePath))
                    {
                        continue;
                    }

                    WorkflowProcessingResult workFlowProcessingResult = ProcessWorkflow(apiInstance, workflow, fileToProcess, workerNumber);

                    if (workFlowProcessingResult != null)
                    {
                        string outputFileAbsolutePath = destinationFolder + fileToProcess.FileRelativePath;

                        if (HandleOutputFileProduction(fileToProcess, fileProductionRules, workFlowProcessingResult, fileSizeReductionIsIntended, inputIsPDF, inputFileSize, outputFileAbsolutePath))
                        {
                            FileOperationsSuccesfullyCompletedEventHandler.Invoke(new FileOperationsResult(fileToProcess.FileAbsolutePath, inputFileSize, FileUtils.GetFileSize(outputFileAbsolutePath), !inputIsPDF));
                            HandleActionsWarningMessages(workFlowProcessingResult.WarningMessages, fileToProcess.FileAbsolutePath);
                        }
                        else
                        {
                            continue;
                        }

                        TryCloseDocumentAsync(apiInstance, workFlowProcessingResult.FileID); //todo: I think it should be handled by ProcessWorkflow.
                    }
                }
                catch (Exception exception)
                {
                    ErrorEventHandler.Invoke(ErrorManager.GetMessageFromException(exception, fileToProcess.FileAbsolutePath));
                }

                if (_workPaused && !_cancellationPending)
                {
                    // If pause has been requested, wait for resume signal
                    WorkPauseEventHandler.Invoke(workerNumber);
                    _waitHandle.WaitOne();
                }
            }

            WorkCompletionEventHandler.Invoke(workerNumber);
        }


        private WorkflowProcessingResult ProcessWorkflow(PDFApi apiInstance, OperationsWorkflow workflow, FileToProcess fileToProcess, int workerNumber)
        {
            List<string> warningMessages = new List<string>();
            byte[] producedFileData = null;
            bool contentRemoved = false;
            bool versionChanged = false;
            bool linearized = false;
            string fileID = null;

            foreach (Operation operation in workflow.OperationsToBePerformed)
            {
                Error actionError = null;
                ReduceErrorInfo reduceErrorInfo = null;
                long remainingTokens = 0;

                if (_cancellationPending)
                {
                    return null;
                }

                switch (operation.Type)
                {
                    case Operation.OperationType.Load:
                        PDFReduceParameters.OutputVersionEnum outputVersion = (PDFReduceParameters.OutputVersionEnum)operation.Parameters;
                        PDFLoadDocumentResponse loadDocumentResponse = HandleLoadDocument(apiInstance, outputVersion, fileToProcess, workerNumber);
                        if (loadDocumentResponse == null)
                        {
                            ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_invalid_response_received", FrameworkGlobals.ApplicationLanguage), actionName: "Load"));
                            return null;
                        }
                        remainingTokens = loadDocumentResponse.RemainingTokens.Value;
                        actionError = loadDocumentResponse.Error;
                        fileID = loadDocumentResponse.FileId;
                        break;

                    case Operation.OperationType.Reduce:
                        ReduceActionConfiguration reduceActionConfiguration = (ReduceActionConfiguration)operation.Parameters;
                        PDFReduceResponse reduceResponse = HandleReduceDocument(apiInstance, reduceActionConfiguration, fileToProcess, fileID, workerNumber, warningMessages);
                        if (reduceResponse == null)
                        {
                            ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_invalid_response_received", FrameworkGlobals.ApplicationLanguage), actionName: "Reduce"));
                            return null;
                        }
                        remainingTokens = reduceResponse.RemainingTokens.Value;
                        contentRemoved = (bool)reduceResponse.ContentRemoved;
                        versionChanged = (bool)reduceResponse.VersionChanged;
                        actionError = reduceResponse.Error;
                        reduceErrorInfo = reduceResponse.ErrorInfo;
                        linearized = reduceActionConfiguration.FastWebView;
                        break;

                    case Operation.OperationType.OCR:
                        OCRActionConfiguration ocrActionConfiguration = (OCRActionConfiguration)operation.Parameters;
                        PDFOCRResponse ocrResponse = HandleOCRDocument(apiInstance, ocrActionConfiguration, fileToProcess, fileID, workerNumber);
                        if (ocrResponse == null)
                        {
                            ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_invalid_response_received", FrameworkGlobals.ApplicationLanguage), actionName: "OCR"));
                            return null;
                        }
                        remainingTokens = ocrResponse.RemainingTokens.Value;
                        actionError = ocrResponse.Error;
                        break;

                    case Operation.OperationType.Save:
                        PDFSaveDocumentResponse saveDocumentResponse = HandleSaveDocument(apiInstance, fileToProcess, fileID, workerNumber);
                        if (saveDocumentResponse == null)
                        {
                            ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_invalid_response_received", FrameworkGlobals.ApplicationLanguage), actionName: "Save"));
                            return null;
                        }
                        remainingTokens = saveDocumentResponse.RemainingTokens.Value;
                        actionError = saveDocumentResponse.Error;
                        producedFileData = saveDocumentResponse.Data;
                        break;
                }

                if (actionError != null)
                {
                    string errorMessage = reduceErrorInfo != null && reduceErrorInfo.ErrorCode != ReduceErrorInfo.ErrorCodeEnum.OK ? ErrorManager.GetMessageFromReduceActionError(reduceErrorInfo, fileToProcess.FileAbsolutePath) : ErrorManager.GetMessageFromPassportPDFError(actionError, operation.Type, fileToProcess.FileAbsolutePath);
                    ErrorEventHandler.Invoke(errorMessage);
                    return null;
                }
                else
                {
                    RemainingTokensUpdateEventHandler.Invoke(remainingTokens);
                }
            }

            return producedFileData != null ? new WorkflowProcessingResult(contentRemoved, versionChanged, linearized, fileID, producedFileData, warningMessages) : null;
        }


        private bool PickFile(out FileToProcess file)
        {
            lock (_locker)
            {
                if (_filesToProcess.Count > 0)
                {
                    file = _filesToProcess[0];
                    _filesToProcess.RemoveAt(0);
                    return true;
                }
                else
                {
                    file = default(FileToProcess);
                    return false;
                }
            }
        }


        private bool CheckInputFileSizeValidity(float inputFileSize, string inputFileAbsolutePath)
        {
            if (inputFileSize == 0)
            {
                ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_empty_file", FrameworkGlobals.ApplicationLanguage), fileName: inputFileAbsolutePath));
                return false;
            }
            else if (inputFileSize > FrameworkGlobals.PassportPDFConfiguration.MaxAllowedContentLength)
            {
                ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_input_file_too_large", FrameworkGlobals.ApplicationLanguage), fileName: inputFileAbsolutePath, inputSize: FrameworkGlobals.PassportPDFConfiguration.MaxAllowedContentLength));
                return false;
            }
            else
            {
                return true;
            }
        }


        private PDFLoadDocumentResponse HandleLoadDocument(PDFApi apiInstance, PDFReduceParameters.OutputVersionEnum outputVersion, FileToProcess fileToProcess, int workerNumber)
        {
            FileStream inputFileStream = null;

            try
            {
                // Load document on remote server
                PassportPDFParametersUtilities.GetLoadDocumentMultipartParameters(fileToProcess.FileAbsolutePath, outputVersion, out inputFileStream, out string conformance, out string fileName);

                using (FileStream tmpFile = File.Create(Path.GetTempFileName(), 4096, FileOptions.DeleteOnClose))
                {
                    using (GZipStream dataStream = new GZipStream(tmpFile, CompressionLevel.Optimal, true))
                    {
                        inputFileStream.CopyTo(dataStream);
                        inputFileStream.Dispose();
                        inputFileStream = null;
                    }

                    tmpFile.Seek(0, SeekOrigin.Begin);
                    apiInstance.Configuration.Timeout = FrameworkGlobals.PassportPDFConfiguration.SuggestedClientTimeout;

                    return PassportPDFRequestsUtilities.SendLoadDocumentMultipartRequest(apiInstance, workerNumber, fileToProcess.FileAbsolutePath, fileName, conformance, tmpFile, "Gzip", UploadOperationStartEventHandler);
                }
            }
            catch
            {
                if (inputFileStream != null)
                {
                    inputFileStream.Dispose();
                }
                throw;
            }
        }


        private PDFReduceResponse HandleReduceDocument(PDFApi apiInstance, ReduceActionConfiguration reduceActionConfiguration, FileToProcess fileToProcess, string fileID, int workerNumber, List<string> warnings)
        {
            PDFReduceParameters reduceParameters = PassportPDFParametersUtilities.GetReduceParameters(reduceActionConfiguration, fileID);
            PDFReduceResponse reduceResponse = PassportPDFRequestsUtilities.SendReduceRequest(apiInstance, reduceParameters, workerNumber, fileToProcess.FileAbsolutePath, FileOperationStartEventHandler);

            if (reduceResponse.WarningsInfo != null)
            {
                foreach (ReduceWarningInfo warning in reduceResponse.WarningsInfo)
                {
                    warnings.Add(LogMessagesUtils.GetWarningStatustext(warning, fileToProcess.FileAbsolutePath));
                }
            }

            return reduceResponse;
        }


        private PDFOCRResponse HandleOCRDocument(PDFApi apiInstance, OCRActionConfiguration ocrActionConfiguration, FileToProcess fileToProcess, string fileID, int workerNumber)
        {
            PDFOCRParameters ocrParameters = PassportPDFParametersUtilities.GetOCRParameters(ocrActionConfiguration, fileID);
            PDFOCRResponse ocrResponse = PassportPDFRequestsUtilities.SendOCRRequest(apiInstance, ocrParameters, workerNumber, fileToProcess.FileAbsolutePath, FileOperationStartEventHandler);

            return ocrResponse;
        }


        private PDFSaveDocumentResponse HandleSaveDocument(PDFApi apiInstance, FileToProcess fileToProcess, string fileID, int workerNumber)
        {
            PDFSaveDocumentParameters saveDocumentParameters = PassportPDFParametersUtilities.GetSaveDocumentParameters(fileID);

            return PassportPDFRequestsUtilities.SendSaveDocumentRequest(apiInstance, saveDocumentParameters, workerNumber, fileToProcess.FileAbsolutePath, DownloadOperationStartEventHandler);
        }


        private static async void TryCloseDocumentAsync(PDFApi apiInstance, string fileID)
        {
            if (string.IsNullOrWhiteSpace(fileID))
            {
                throw new ArgumentNullException("FileID");
            }

            PDFCloseDocumentParameters closeDocumentParameters = new PDFCloseDocumentParameters(fileID);

            try
            {
                await apiInstance.ClosePDFAsync(closeDocumentParameters); //we do not want to stop the process by waiting such response.
            }
            catch
            {
                return;
            }
        }


        private bool HandleOutputFileProduction(FileToProcess fileToProcess, FileProductionRules fileProductionRules, WorkflowProcessingResult workflowProcessingResult, bool fileSizeReductionIsIntended, bool inputIsPDF, long inputFileSize, string outputFileAbsolutePath)
        {
            bool outputIsInput = FileUtils.AreSamePath(fileToProcess.FileAbsolutePath, outputFileAbsolutePath);
            bool keepProducedFile = MustProducedFileBeKept(workflowProcessingResult, fileSizeReductionIsIntended, inputIsPDF, inputFileSize);

            // Save reduced document to output folder
            if (keepProducedFile)
            {
                FileUtils.SaveFile(workflowProcessingResult.ProducedFileData, fileToProcess.FileAbsolutePath, outputFileAbsolutePath, fileProductionRules.KeepWriteAndAccessTime);

                if (fileProductionRules.DeleteOriginalFileOnSuccess && !outputIsInput)
                {
                    try
                    {
                        FileUtils.DeleteFileEx(fileToProcess.FileAbsolutePath);
                    }
                    catch (Exception exception)
                    {
                        ErrorEventHandler.Invoke(LogMessagesUtils.ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_original_file_deletion_failure", FrameworkGlobals.ApplicationLanguage), fileName: fileToProcess.FileAbsolutePath, additionalMessage: exception.Message));
                        return false;
                    }
                }
            }
            else
            {
                if (!outputIsInput)
                {
                    FileUtils.EnsureDirectoryExists(Path.GetDirectoryName(outputFileAbsolutePath));
                    File.Copy(fileToProcess.FileAbsolutePath, outputFileAbsolutePath, true);

                    if (fileProductionRules.KeepWriteAndAccessTime)
                    {
                        FileUtils.SetOriginalLastAccessTime(fileToProcess.FileAbsolutePath, outputFileAbsolutePath);
                    }
                }

                if (fileSizeReductionIsIntended)
                {
                    // Inform file size reduction failure
                    workflowProcessingResult.WarningMessages.Add(LogMessagesUtils.GetWarningStatustext(new ReduceWarningInfo() { WarningCode = ReduceWarningInfo.WarningCodeEnum.FileSizeReductionFailure }, fileToProcess.FileAbsolutePath));
                }
            }

            return true;
        }


        private bool MustProducedFileBeKept(WorkflowProcessingResult workflowProcessingResult, bool fileSizeReductionIsIntended, bool inputIsPdf, float inputFileSize)
        {
            if (fileSizeReductionIsIntended)
            {
                return workflowProcessingResult.ProducedFileData.LongLength < inputFileSize || workflowProcessingResult.Linearized || !inputIsPdf || workflowProcessingResult.ContentRemoved || workflowProcessingResult.VersionChanged;
            }
            else
            {
                return true;
            }
        }


        private void HandleActionsWarningMessages(List<string> warningMessages, string inputFileAbsolutePath)
        {
            foreach (string warningMessage in warningMessages)
            {
                WarningEventHandler.Invoke(warningMessage);
            }
        }


        private sealed class WorkflowProcessingResult
        {
            public bool Linearized { get; }
            public bool ContentRemoved { get; }
            public bool VersionChanged { get; }
            public string FileID { get; }
            public byte[] ProducedFileData { get; }
            public List<string> WarningMessages { get; }

            public WorkflowProcessingResult(bool contentRemoved, bool versionChanged, bool linearized, string fileID, byte[] producedFileData, List<string> warningMessages)
            {
                ContentRemoved = contentRemoved;
                VersionChanged = versionChanged;
                Linearized = linearized;
                FileID = fileID;
                ProducedFileData = producedFileData;
                WarningMessages = warningMessages;
            }
        }
    }
}