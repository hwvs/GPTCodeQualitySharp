using GPTCodeQualitySharp.Dataset;
using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Document.Provider;
using GPTCodeQualitySharp.Evaluator;
using GPTCodeQualitySharp.Evaluator.API;
using GPTCodeQualitySharp.Evaluator.API.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharpTests
{
    [TestClass]
    public class EvaluatorTests
    {
        const string db_path = "tests.db"; // Cleanup! 

        private static SQLiteValueStore valueStore;
        private static MockOpenAIChatEvaluator evaluatorAPI;
        private static DocumentEvaluator documentEvaluator;
        private static DocumentInfo documentInfo;
        private static string documentText;

        [ClassInitialize(InheritanceBehavior.None)]
        public static void OnClassInitialize(TestContext context)
        {
            // Setup all values
            valueStore = new SQLiteValueStore(db_path);
            evaluatorAPI = new MockOpenAIChatEvaluator("prompt goes here {CODE} {ROLE} {ROLE}\n\n", valueStore, Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "");

            documentEvaluator = new DocumentEvaluator(evaluatorAPI, valueStore);

            // Setup the document
            documentInfo = new DocumentInfo("document.txt");
            documentText = CodeChunkReaderTests.GetExampleDocument();

            // Null checks
            Assert.IsNotNull(valueStore);
            Assert.IsNotNull(evaluatorAPI);
            Assert.IsNotNull(documentEvaluator);
            Assert.IsNotNull(documentInfo);
            Assert.IsNotNull(documentText);

            Assert.IsTrue(documentText.Length > 0, "Empty document text!");

        }

        [TestMethod]
        public void TestEvaluateDocument()
        {
            bool hadAnyResults = false;
            // Evaluate the document
            //var evaluatedChunks = await GetAllEvaluatedChunks();
            // Call async from this non-async method
            var evaluatedChunks = GetAllEvaluatedChunks().Result;
            foreach(EvaluatedCodeChunk evaluated in evaluatedChunks)
            {
                hadAnyResults = true;
                Assert.IsNotNull(evaluated, "Evaluated code chunk is null!");
                Assert.IsNotNull(evaluated.Result, "Evaluated code chunk result is null!");
                Assert.IsTrue(evaluated.Result.Success, "Evaluation Result.Success is false, failed");

                Assert.IsTrue(evaluated.Result.JSONResult.Length > 0, "Evaluation Result.JSONResult is empty");
                Assert.IsTrue(evaluated.Result.JSONResult.Length > 0, "Evaluation Result.JSONResult is empty");
                Assert.IsTrue(evaluated.Result.TryGetScore(out double score), "Evaluation Result.TryGetScore failed");
                Assert.IsTrue(score > 0, "Evaluation score is less than zero");

                // Specifics in the mock
                Assert.IsTrue(evaluated.Result.JSONResult.Contains(MockOpenAIChatEvaluator.MOCK_RESULT_CONTAINS), "Evaluation Result.JSONResult does not contain mock needle");
            }

            Assert.IsTrue(hadAnyResults, "No results from evaluation");
        }

        private async Task<List<EvaluatedCodeChunk>> GetAllEvaluatedChunks()
        {
            var results = new List<EvaluatedCodeChunk>();
            await foreach (EvaluatedCodeChunk evaluated in documentEvaluator.EvaluateDocument(documentText, documentInfo,
                new CodeChunkReaderSettings()
                {
                    SoftCodeChunkLengthLimit = 2048,
                    HardCodeChunkLengthLimit = 3000
                })) { 

                results.Add(evaluated); 
            }

            return results;
        }

        // cleanup
        [ClassCleanup]
        public static void OnClassCleanup()
        {
            bool deleted = false;
            if (File.Exists(db_path))
            {
                File.Delete(db_path);
                deleted = true;
            }
            Assert.IsFalse(File.Exists(db_path), "DB file was not deleted - manual cleanup may be necessary!");
            // Assert that it was deleted
            Assert.IsTrue(deleted, "DB file was never created");
        }
    }
}
