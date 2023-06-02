using GPTCodeQualitySharp.Dataset;
using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Evaluator.API.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator.API
{
    internal class OpenAIResultEvaluator : EvaluatorResultJSON
    {
        private readonly string _prompt;
        private readonly string _response;
        private readonly CodeChunk _codeChunk;

        public OpenAIResultEvaluator(CodeChunk codeChunk, string prompt, string response)
        {
            _prompt = prompt;
            _response = response;
            _codeChunk = codeChunk;
        }

        public override string ToHashableString()
        {
            return _prompt + "~~~\n\n\n" + _response;
        }

        internal override string GetResult()
        {
            return _response;
        }

    }
}
