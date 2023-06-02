using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GPTCodeQualitySharp.Dataset;

namespace GPTCodeQualitySharp.Document.Partial
{
    public struct CodeChunk : IHashableData
    {
        public string Code { get; }


        // Hash for the database
        public string ToHashableString()
        {
            // Remove all whitespace
            string codeWithoutWhitespace = Regex.Replace(Code, @"\s+", "");
            return codeWithoutWhitespace;
        }

        public CodeChunk(string code)
        {
            Code = code;
        }

    }
}
