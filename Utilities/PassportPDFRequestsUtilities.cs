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
using System.IO;
using System.Threading;
using PassportPDF.Api;
using PassportPDF.Model;
using PassportPDF.Tools.Framework.Business;


namespace PassportPDF.Tools.Framework.Utilities
{
    public static class PassportPDFRequestsUtilities
    {
        public static PassportPDFPassport GetPassportInfo(string passportId)
        {
            PassportManagerApi apiInstance = new PassportManagerApi(passportId)
            {
                BasePath = FrameworkGlobals.PassportPdfApiUri
            };

            Exception e = null;
            int pauseMs = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return apiInstance.PassportManagerGetPassportInfo(passportId);
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pauseMs); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pauseMs += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static int GetMaxClientThreads(string appId)
        {
            PassportPDFApplicationManagerApi passportPDFApplicationManagerApi = new PassportPDFApplicationManagerApi(FrameworkGlobals.PassportPdfApiUri);

            Exception e = null;
            int pauseMs = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return Math.Max(passportPDFApplicationManagerApi.PassportPDFApplicationManagerGetMaxClientThreads(appId).Value, 1);
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pauseMs); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pauseMs += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static string[] GetImageApiSupportedFileExtensions()
        {
            ImageApi apiInstance = new ImageApi(FrameworkGlobals.PassportPdfApiUri);

            Exception e = null;
            int pauseMs = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return apiInstance.ImageGetSupportedFileExtensions().Value.ToArray();
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pauseMs); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pauseMs += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static string[] GetPdfApiSupportedFileExtensions()
        {
            PDFApi apiInstance = new PDFApi(FrameworkGlobals.PassportPdfApiUri);

            Exception e = null;
            int pauseMs = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return apiInstance.GetPDFImportSupportedFileExtensions().Value.ToArray();
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pauseMs); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pauseMs += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static int GetSuggestedClientTimeout()
        {
            ConfigApi apiInstance = new ConfigApi(FrameworkGlobals.PassportPdfApiUri);

            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return apiInstance.ConfigGetSuggestedClientTimeout().Value;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static long GetMaxAllowedContentLength()
        {
            ConfigApi apiInstance = new ConfigApi(FrameworkGlobals.PassportPdfApiUri);

            Exception e = null;
            int pauseMs = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return apiInstance.ConfigGetMaxAllowedContentLength().Value;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pauseMs); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pauseMs += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static StringArrayResponse GetAvailableOCRLanguages()
        {
            ConfigApi apiInstance = new ConfigApi(FrameworkGlobals.PassportPdfApiUri);

            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                try
                {
                    return apiInstance.ConfigGetSupportedOCRLanguages();
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static PdfReduceResponse SendReduceRequest(PDFApi apiInstance, PdfReduceParameters reduceParameters, int workerNumber, string inputFilePath, OperationsManager.ProgressDelegate reduceOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                reduceOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);
                try
                {
                    PdfReduceResponse response = apiInstance.Reduce(reduceParameters);

                    return response;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static PdfOCRResponse SendOCRRequest(PDFApi apiInstance, PdfOCRParameters ocrParameters, int workerNumber, string inputFilePath, string pageRange, int pageCount, OperationsManager.ChunkProgressDelegate chunkProgressEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                chunkProgressEventHandler.Invoke(workerNumber, inputFilePath, pageRange, pageCount, i);
                try
                {
                    PdfOCRResponse response = apiInstance.OCR(ocrParameters);

                    return response;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static PdfLoadDocumentResponse SendLoadPDFMultipartRequest(PDFApi apiInstance, int workerNumber, string inputFilePath, string fileName, PdfConformance conformance, string password, Stream fileStream, ContentEncoding contentEncoding, OperationsManager.ProgressDelegate uploadOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                uploadOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);
                try
                {
                    fileStream.Seek(0, SeekOrigin.Begin);

                    PdfLoadDocumentResponse response = apiInstance.LoadDocumentAsPDFMultipart(fileStream,
                        new PdfLoadDocumentParameters()
                        {
                            ContentEncoding = contentEncoding,
                            Conformance = conformance,
                            Password = password,
                            FileName = fileName
                        });

                    return response;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static ImageLoadResponse SendLoadImageMultipartRequest(ImageApi apiInstance, int workerNumber, string inputFilePath, string fileName, Stream fileStream, ContentEncoding contentEncoding, OperationsManager.ProgressDelegate uploadOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                uploadOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);
                try
                {
                    fileStream.Seek(0, SeekOrigin.Begin);

                    ImageLoadResponse response = apiInstance.ImageLoadMultipart(fileStream,
                        new LoadImageParameters()
                        {
                            ContentEncoding = contentEncoding,
                            FileName = fileName
                        });

                    return response;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static PdfLoadDocumentResponse SendLoadDocumentRequest(PDFApi apiInstance, int workerNumber, string inputFilePath, string fileName, PdfConformance conformance, string password, Stream fileStream, ContentEncoding contentEncoding, OperationsManager.ProgressDelegate uploadOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            if (fileStream.Length > int.MaxValue)
            {
                throw new OutOfMemoryException();
            }

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                uploadOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);
                try
                {
                    fileStream.Seek(0, SeekOrigin.Begin);

                    byte[] data = new byte[fileStream.Length];

                    fileStream.Read(data, 0, (int)fileStream.Length);

                    PdfLoadDocumentFromByteArrayParameters pdfLoadDocumentFromByteArrayParameters = new PdfLoadDocumentFromByteArrayParameters(data)
                    {
                        FileName = fileName,
                        Password = password,
                        Conformance = conformance,
                        ContentEncoding = contentEncoding
                    };
                    PdfLoadDocumentResponse response = apiInstance.LoadDocumentAsPDF(pdfLoadDocumentFromByteArrayParameters);

                    return response;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static PdfGetInfoResponse SendGetInfoRequest(PDFApi apiInstance, PdfGetInfoParameters getInfoParameters, int workerNumber, string inputFilePath, OperationsManager.ProgressDelegate getInfoOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                getInfoOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);
                try
                {
                    return apiInstance.GetInfo(getInfoParameters);
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static void DownloadPDF(PDFApi apiInstance, PdfSaveDocumentParameters saveDocumentParameters, int workerNumber, string inputFilePath, Stream destinationStream, OperationsManager.ProgressDelegate downloadOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                downloadOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);

                try
                {
                    apiInstance.SaveDocumentToFile(saveDocumentParameters, destinationStream);
                    return;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }


        public static void DownloadImageAsPDFMRC(ImageApi apiInstance, ImageSaveAsPDFMRCParameters saveImageParameters, int workerNumber, string inputFilePath, Stream destinationStream, OperationsManager.ProgressDelegate downloadOperationStartEventHandler)
        {
            Exception e = null;
            int pausems = 5000;

            for (int i = 0; i < FrameworkGlobals.MAX_RETRYING_REQUESTS; i++)
            {
                downloadOperationStartEventHandler.Invoke(workerNumber, inputFilePath, i);

                try
                {
                    apiInstance.ImageSaveAsPDFMRCFile(saveImageParameters, destinationStream);
                    return;
                }
                catch (Exception ex)
                {
                    if (i < FrameworkGlobals.MAX_RETRYING_REQUESTS - 1)
                    {
                        Thread.Sleep(pausems); //marking a pause in case of cnx temporarily out and to avoid overhead.
                        pausems += 2000;
                    }
                    else
                    {//last iteration
                        e = ex;
                    }
                }
            }

            throw e;
        }
    }
}