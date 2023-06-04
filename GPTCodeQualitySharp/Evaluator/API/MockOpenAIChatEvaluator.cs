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
    public class MockOpenAIChatEvaluator : ICodeChunkEvaluatorAsync
    {
        public const string MOCK_RESULT_CONTAINS = " |mock result| "; // Something that should be in the result
        const string MOCK_RESULT = // multiline literal
            @"some random stuff that needs to be cut off from the AI trying to be helpful and not following directions !*#!*@*)) ((({
    ""summaryOfSnippet"": """+ MOCK_RESULT_CONTAINS + @"Code appears to be a method that takes a prompt and returns a result. It is difficult to determine the context of the code, but it appears to be a part of a larger application. The code is readable and follows language conventions, but there are some minor issues with the code."",
    ""optimizationIdeas"": ""The code could be improved by adding comments to explain the purpose of the code. The method could be refactored to reduce the number of nested if statements. The method could also be refactored to reduce the number of lines of code."",
    ""s1_pub_access"": 10,
    ""s2_one_job"": 8,
    ""s3_many_params"": 10,
    ""s4_exs"": 8,
    ""s5_disposal"": 10,
    ""s6_patterns"": 5,
    ""s7_readable"": 8,
    ""s8_spaghetti"": 10,
    ""s9_cryptic"": 10,
    ""s10_lang"": 10,
    ""s11_dup"": 10,
    ""s12_complex"": 7,
    ""s13_combexp"": 7
} !@#*!* some random stuff that needs to be cut off from the AI trying to be helpful and not following directions";

        string PromptTemplate { get; }

        private readonly decimal _temperature;
        private readonly IValueStore _valueStore;

        public MockOpenAIChatEvaluator(string promptTemplate, IValueStore valueStore, string ApiKey, decimal temperature = 0)
        {
            PromptTemplate = promptTemplate;
            _temperature = temperature;
            _valueStore = valueStore;

            // TODO: Make this robust
            if (!PromptTemplate.Contains("{CODE}"))
            {
                throw new ArgumentException("Prompt template must contain {CODE} placeholder");
            }
        }

        private string PreparePrompt(CodeChunk codeChunk)
        {
            return PromptTemplate.Replace("{CODE}", codeChunk.Code);
        }

        public async Task<EvaluatorResult> EvaluateAsync(CodeChunk codeChunk)
        {
            // TODO: Make this robust
            string prompt = PreparePrompt(codeChunk);

            return OpenAIChatEvaluator.EvaluatorResultFromJSONResponse(null, null, codeChunk: codeChunk, prompt: prompt, response: MOCK_RESULT);
        }

        
    }
}