using GPTCodeQualitySharp.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Document.Partial
{
    /// <summary>
    /// A chunk of code that can be analyzed for quality.
    /// - The code as a string
    /// - The file path of the code
    /// - The line number of the code
    /// </summary>
    public struct CodeChunkInfo
    {
        public CodeChunk CodeChunk { get; }
        public DocumentInfo DocumentInfo { get; }
        public int StartLineNumber { get; }
        public int EndLineNumber { get; }

        public CodeChunkInfo(CodeChunk codeChunk, DocumentInfo documentInfo, int startLineNumber, int endLineNumber)
        {
            CodeChunk = codeChunk;
            DocumentInfo = documentInfo;
            StartLineNumber = startLineNumber;
            EndLineNumber = endLineNumber;
        }

    }
}
