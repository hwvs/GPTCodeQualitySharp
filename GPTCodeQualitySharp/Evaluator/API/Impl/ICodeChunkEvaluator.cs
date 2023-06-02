using GPTCodeQualitySharp.Document.Partial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator.API.Impl
{
    /// <summary>
    /// Takes a CodeChunk and can provide a EvaluatorResult
    /// </summary>
    public interface ICodeChunkEvaluatorAsync
    {
        Task<EvaluatorResult> EvaluateAsync(CodeChunk codeChunk);

    }
}
