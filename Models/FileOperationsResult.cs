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

namespace PassportPDF.Tools.Framework.Models
{
    /// <summary>
    /// Represents the result of one or several succesful operations on a file.
    /// </summary>
    public sealed class FileOperationsResult
    {
        public string InputFileName { get; }
        public float InputFileSize { get; }
        public float OutputFileSize { get; }
        public bool ConvertedToPDF { get; }

        public FileOperationsResult(string inputFileName, float fileInputSize, float fileOutputSize, bool convertedToPDF)
        {
            InputFileName = inputFileName;
            InputFileSize = fileInputSize;
            OutputFileSize = fileOutputSize;
            ConvertedToPDF = convertedToPDF;
        }
    }
}
