using GPTCodeQualitySharp.Document.Partial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator.API.Impl
{
    public class EvaluatedCodeChunk
    {

        public CodeChunkInfo CodeChunkInfo { get; }
        public EvaluatorResult Result { get; }

        public EvaluatedCodeChunk(CodeChunkInfo codeChunkInfo, EvaluatorResult evaluatorResult)
        {
            CodeChunkInfo = codeChunkInfo;
            Result = evaluatorResult;
        }
    }
}
