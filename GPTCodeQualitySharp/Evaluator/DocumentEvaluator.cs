using GPTCodeQualitySharp.Dataset;
using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Document.Provider;
using GPTCodeQualitySharp.Evaluator.API.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator
{
    public class DocumentEvaluator
    {
        private readonly ICodeChunkEvaluatorAsync _evaluator;
        private readonly IValueStore _valueStore;


        public DocumentEvaluator(ICodeChunkEvaluatorAsync evaluator, IValueStore valueStore)
        {
            _evaluator = evaluator;
            _valueStore = valueStore;
        }

        public async IAsyncEnumerable<EvaluatedCodeChunk> EvaluateDocument(string documentContent, DocumentInfo documentInfo, CodeChunkReaderSettings? codeChunkReaderSettings = null)
        {
            var codeChunkReader = new CodeChunkReader(code: documentContent, documentInfo: documentInfo, settings: codeChunkReaderSettings);

            foreach (CodeChunkInfo codeChunkInfo in codeChunkReader)
            {
                EvaluatorResult result;
                string? cacheResult;

                // Cache hit
                if (_valueStore.TryGetValue(ValueStoreTable.ApiResult, codeChunkInfo.CodeChunk, out cacheResult) && cacheResult != null) { // TODO: Make this robust
                    result = new EvaluatorResult( true, cacheResult);
                }
                else
                {
                    // Cache miss
                    result = await _evaluator.EvaluateAsync(codeChunkInfo.CodeChunk);
                }

                yield return new EvaluatedCodeChunk(codeChunkInfo, result);
            }
        }

    }
}
