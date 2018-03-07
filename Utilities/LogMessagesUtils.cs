/**********************************************************************
 * Project:                 PassportPDF.Tools.Framework
 * Authors:					- Evan Carr�re.
 *                          - Lo�c Carr�re.
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
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using PassportPDF.Model;
using PassportPDF.Tools.Framework.Models;

namespace PassportPDF.Tools.Framework.Utilities
{
    public static class LogMessagesUtils
    {
        public static string TimeStampLogMessage(string logMessage)
        {
            return $"{DateTime.Now:r}" + " - " + logMessage;
        }


        public static string GetFileOperationsStartText(string fileName, int retryCount)
        {
            return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_file_processing_start", FrameworkGlobals.ApplicationLanguage), fileName, retryCount: retryCount);
        }


        public static string GetFileUploadingStartText(string fileName, int retryCount)
        {
            return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_file_uploading_start", FrameworkGlobals.ApplicationLanguage), fileName, retryCount: retryCount);
        }


        public static string GetFileDownloadingStartText(string fileName, int retryCount)
        {
            return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_file_downloading_start", FrameworkGlobals.ApplicationLanguage), fileName, retryCount: retryCount);
        }


        public static string GetGenericFileOperationsCompletionText(FileOperationsResult fileOperationsResult)
        {
            return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_generic_file_operations_completion", FrameworkGlobals.ApplicationLanguage), fileOperationsResult.InputFileName);
        }


        public static string GetFileReductionOperationsCompletionText(FileOperationsResult fileOperationsResult)
        {
            if (!fileOperationsResult.ConvertedToPDF)
            {
                return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_pdf_reduction_completion", FrameworkGlobals.ApplicationLanguage), fileOperationsResult.InputFileName, ratio: StatsComputationUtilities.ComputeReductionRatio(fileOperationsResult.InputFileSize, fileOperationsResult.OutputFileSize));
            }
            else
            {
                return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_conversion_to_pdf_completion", FrameworkGlobals.ApplicationLanguage), fileOperationsResult.InputFileName);
            }
        }


        public static string GetGenericWorkCompletionMessage(int processedFileCount, int succesfullyProcessedFileCount, int unsuccesfullyProcessedFileCount, string elapsedTime)
        {
            string resultMessage;

            if (succesfullyProcessedFileCount == 0)
            {
                if (processedFileCount == 1)
                {
                    resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_no_succesful_operation_result_singular", FrameworkGlobals.ApplicationLanguage);
                }
                else
                {
                    resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_no_succesful_operation_result_plurial", FrameworkGlobals.ApplicationLanguage);
                }
            }
            else if (succesfullyProcessedFileCount == 1)
            {
                resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_generic_operations_result_singular", FrameworkGlobals.ApplicationLanguage);
            }
            else
            {
                resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_generic_operations_result_plurial", FrameworkGlobals.ApplicationLanguage);
            }
            return ReplaceMessageSequencesAndReferences(resultMessage, succesfullyProcessedFileCount: succesfullyProcessedFileCount, fileToProcessCount: processedFileCount, elapsedTime: elapsedTime);
        }


        public static string GetReductionWorkCompletionText(int processedFileCount, int succesfullyProcessedFileCount, int unsuccesfullyProcessedFileCount, double inputSize, double outputSize, string elapsedTime)
        {
            string resultMessage;

            if (succesfullyProcessedFileCount == 0)
            {
                if (processedFileCount == 1)
                {
                    resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_no_succesful_operation_result_singular", FrameworkGlobals.ApplicationLanguage);
                }
                else
                {
                    resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_no_succesful_operation_result_plurial", FrameworkGlobals.ApplicationLanguage);
                }
            }
            else if (succesfullyProcessedFileCount == 1)
            {
                resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_reduction_operations_result_singular", FrameworkGlobals.ApplicationLanguage);
            }
            else
            {
                resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_reduction_operations_result_plurial", FrameworkGlobals.ApplicationLanguage);
            }
            return ReplaceMessageSequencesAndReferences(resultMessage, succesfullyProcessedFileCount: succesfullyProcessedFileCount, fileToProcessCount: processedFileCount, elapsedTime: elapsedTime, ratio: StatsComputationUtilities.ComputeSavedSpaceRatio(inputSize, outputSize));
        }


        public static string GetDetailedReductionWorkCompletionText(int processedFileCount, int succesfullyProcessedFileCount, int unsuccesfullyProcessedFileCount, double inputSize, double outputSize, string elapsedTime)
        {
            string resultMessage;

            if (succesfullyProcessedFileCount == 0)
            {
                if (processedFileCount == 1)
                {
                    resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_no_succesful_operation_result_singular", FrameworkGlobals.ApplicationLanguage);
                }
                else
                {
                    resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_no_succesful_operation_result_plurial", FrameworkGlobals.ApplicationLanguage);
                }
            }
            else if (succesfullyProcessedFileCount == 1)
            {
                resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_reduction_operations_result_detailed_singular", FrameworkGlobals.ApplicationLanguage);
            }
            else
            {
                resultMessage = FrameworkGlobals.MessagesLocalizer.GetString("message_reduction_operations_result_detailed_plurial", FrameworkGlobals.ApplicationLanguage);
            }

            return ReplaceMessageSequencesAndReferences(resultMessage, inputSize: ParsingUtils.ConvertSize(inputSize, "MB"), outputSize: ParsingUtils.ConvertSize(outputSize, "MB"), succesfullyProcessedFileCount: succesfullyProcessedFileCount, fileToProcessCount: processedFileCount, elapsedTime: elapsedTime, ratio: StatsComputationUtilities.ComputeReductionPercentage(inputSize, outputSize));
        }


        public static string GetWorkerIdleStateText()
        {
            return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString("message_worker_idle_state", FrameworkGlobals.ApplicationLanguage));
        }


        public static string GetWarningStatustext(ReduceWarningInfo warningInfo, string fileName)
        {
            return ReplaceMessageSequencesAndReferences(FrameworkGlobals.MessagesLocalizer.GetString(LogConstants.WarningLocalesIdDictionary[warningInfo.WarningCode.Value], FrameworkGlobals.ApplicationLanguage), fileName, warningInfo.PageNumber, warningInfo.PageImageNumber, additionalMessage: warningInfo.ExtWarningMessage);
        }


        public static string ReplaceMessageSequencesAndReferences(string message, string fileName = null, int? pageNumber = null, int? pageImageNumber = null, int? pageCount = null, string additionalMessage = null, int? retryCount = null, double? ratio = null, int? httpCode = null, double? inputSize = null, double? outputSize = null, int? succesfullyProcessedFileCount = null, int? fileToProcessCount = null, string elapsedTime = null, long? remainingTokens = null, long? usedTokens = null, string applicationName = null, string appVersionNumber = null)
        {
            StringBuilder finalMessage = new StringBuilder(ReplaceLocalizedStringReferences(message.Replace(LogConstants.DOCUMENT_NAME_SEQUENCE, fileName)));

            if (fileName != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.DOCUMENT_NAME_SEQUENCE, fileName);
            }
            if (pageNumber != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.PAGE_NUMBER_SEQUENCE, pageNumber.Value.ToString());
            }
            if (pageImageNumber != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.PAGE_IMAGE_NUMBER_SEQUENCE, pageImageNumber.Value.ToString());
            }
            if (pageCount != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.PAGE_COUNT_SEQUENCE, pageCount.Value.ToString());
            }
            if (additionalMessage != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.ADDITIONAL_MESSAGE_SEQUENCE, additionalMessage);
            }
            if (retryCount != null)
            {
                finalMessage = retryCount == 0 ? finalMessage.Replace(FrameworkGlobals.MessagesLocalizer.GetString("message_retry", FrameworkGlobals.ApplicationLanguage), "") : finalMessage.Replace(LogConstants.RETRY_COUNT_SEQUENCE, retryCount.Value.ToString());
            }
            if (ratio != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.REDUCTION_RATIO_SEQUENCE, ratio.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (httpCode != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.HTTP_CODE_SEQUENCE, httpCode.Value.ToString());
            }
            if (inputSize != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.INPUT_SIZE_SEQUENCE, inputSize.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (outputSize != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.OUTPUT_SIZE_SEQUENCE, outputSize.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (succesfullyProcessedFileCount != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.SUCCESFULLY_PROCESSED_FILE_COUNT_SEQUENCE, succesfullyProcessedFileCount.Value.ToString());
            }
            if (fileToProcessCount != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.FILE_TO_PROCESS_COUNT, fileToProcessCount.Value.ToString());
            }
            if (elapsedTime != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.ELAPSED_TIME_SEQUENCE, elapsedTime);
            }
            if (remainingTokens != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.REMAINING_TOKENS_SEQUENCE, remainingTokens.ToString());
            }
            if (usedTokens != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.USED_TOKENS_SEQUENCE, usedTokens.ToString());
            }
            if (applicationName != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.APPLICATION_NAME_SEQUENCE, applicationName);
            }
            if (appVersionNumber != null)
            {
                finalMessage = finalMessage.Replace(LogConstants.APP_VERSION_NUMBER_SEQUENCE, appVersionNumber);
            }

            return finalMessage.ToString();
        }


        private static string ReplaceLocalizedStringReferences(string inputMessage)
        {
            string outputMessage = inputMessage;
            int referenceStartIndex;

            while ((referenceStartIndex = inputMessage.IndexOf(LogConstants.LOCALIZED_STRING_REFERENCE_TOKEN)) != -1)
            {
                int referenceLength = inputMessage.Substring(referenceStartIndex + 1, inputMessage.Length - (referenceStartIndex + 1)).IndexOf(LogConstants.LOCALIZED_STRING_REFERENCE_TOKEN) + 2;
                int newInputMessageStartIndex = referenceStartIndex + 1;

                if (referenceLength != -1)
                {
                    string referenceString = inputMessage.Substring(referenceStartIndex, referenceLength);
                    string localizedStringId = inputMessage.Substring(referenceStartIndex, referenceLength).TrimStart(LogConstants.LOCALIZED_STRING_REFERENCE_TOKEN).TrimEnd(LogConstants.LOCALIZED_STRING_REFERENCE_TOKEN);
                    string localizedStringValue;

                    try
                    {
                        localizedStringValue = FrameworkGlobals.MessagesLocalizer.GetString(localizedStringId, FrameworkGlobals.ApplicationLanguage);
                        outputMessage = outputMessage.Replace(referenceString, localizedStringValue);
                        newInputMessageStartIndex += referenceLength;

                    }
                    catch (Exception) { }
                }

                inputMessage = newInputMessageStartIndex < inputMessage.Length ? inputMessage.Substring(newInputMessageStartIndex, inputMessage.Length - newInputMessageStartIndex) : "";
            }

            return outputMessage;
        }


        private static class LogConstants
        {
            // These sequences will be replaced by the provided value associated with them
            public const string PAGE_NUMBER_SEQUENCE = "#page_number";
            public const string PAGE_COUNT_SEQUENCE = "#page_count";
            public const string PAGE_IMAGE_NUMBER_SEQUENCE = "#page_image_number";
            public const string DOCUMENT_NAME_SEQUENCE = "#document_name";
            public const string ADDITIONAL_MESSAGE_SEQUENCE = "#error_message";
            public const string REDUCTION_RATIO_SEQUENCE = "#ratio";
            public const string HTTP_CODE_SEQUENCE = "#http_code";
            public const string SUCCESFULLY_PROCESSED_FILE_COUNT_SEQUENCE = "#succesfully_processed_file_count";
            public const string FILE_TO_PROCESS_COUNT = "#file_to_process_count";
            public const string INPUT_SIZE_SEQUENCE = "#input_size";
            public const string OUTPUT_SIZE_SEQUENCE = "#output_size";
            public const string ELAPSED_TIME_SEQUENCE = "#elapsed_time";
            public const string RETRY_COUNT_SEQUENCE = "#retry_count";
            public const string REMAINING_TOKENS_SEQUENCE = "#remaining_tokens";
            public const string USED_TOKENS_SEQUENCE = "#used_tokens";
            public const string APPLICATION_NAME_SEQUENCE = "#application_name";
            public const string APP_VERSION_NUMBER_SEQUENCE = "#version_number";

            // This character allows to reference a localized string using its id, (ie: @message_exit@) and will be replaced by the value of the referenced string
            public const char LOCALIZED_STRING_REFERENCE_TOKEN = '@';

            public static readonly Dictionary<ReduceWarningInfo.WarningCodeEnum, string> WarningLocalesIdDictionary = new Dictionary<ReduceWarningInfo.WarningCodeEnum, string>
            {
                { ReduceWarningInfo.WarningCodeEnum.ImageExtractionFailure, "message_image_extraction_failure" },
                { ReduceWarningInfo.WarningCodeEnum.ColorDetectionFailure, "message_color_detection_failure" },
                { ReduceWarningInfo.WarningCodeEnum.ImageResizeFailure, "message_image_resize_failure" },
                { ReduceWarningInfo.WarningCodeEnum.ImageCropFailure, "message_image_crop_failure" },
                { ReduceWarningInfo.WarningCodeEnum.ImageResolutionObtentionFailure, "message_image_resolution_obtention_failure" },
                { ReduceWarningInfo.WarningCodeEnum.ImageReplacementFailure, "message_image_replacement_failure" },
                { ReduceWarningInfo.WarningCodeEnum.MRCImageReplacementFailure, "message_mrc_image_replacement_failure" },
                { ReduceWarningInfo.WarningCodeEnum.PageSelectionFailure, "message_page_selection_failure" },
                { ReduceWarningInfo.WarningCodeEnum.FileSizeReductionFailure, "message_file_size_reduction_failure" },
                { ReduceWarningInfo.WarningCodeEnum.ImageObtentionFailure, "message_image_obtention_failure" },
            };
        }
    }
}