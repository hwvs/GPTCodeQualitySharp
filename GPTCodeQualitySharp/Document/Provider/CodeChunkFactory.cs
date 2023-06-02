using GPTCodeQualitySharp.Document.Partial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Document.Provider
{
    internal class CodeChunkFactory
    {
        // We take in a string of code, and DocumentInfo to create a CodeChunkInfo
        public static CodeChunkInfo CreateCodeChunkInfo(string code, DocumentInfo documentInfo, 
            int startLineNumber, int endLineNumber)
        {
            CodeChunk codeChunk = new CodeChunk(code);
            return new CodeChunkInfo(codeChunk, documentInfo, startLineNumber, endLineNumber);
        }


    }
}
