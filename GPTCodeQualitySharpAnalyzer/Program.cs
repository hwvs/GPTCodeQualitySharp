using GPTCodeQualitySharp.Dataset;
using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Document.Provider;
using GPTCodeQualitySharp.Evaluator;
using GPTCodeQualitySharp.Evaluator.API;
using GPTCodeQualitySharp.Evaluator.API.Impl;
using System.Runtime.InteropServices;

namespace GPTCodeQualitySharpAnalyzer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string dbPath = "GPTCodeQualitySharpAnalyzer3.db";
            // Only WINDOWS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                dbPath = Path.Join(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"), dbPath); // Store in local appdata
            }
            else
            {
                if(Path.Exists("~"))
                {
                    dbPath = Path.Join(Environment.ExpandEnvironmentVariables("~"), dbPath); // Store in /home/user
                }
                else
                {
                    // Store in PWD
                }
            }

            var valueStore = new SQLiteValueStore(dbPath);
            var evaluatorAPI = new OpenAIChatEvaluator(File.ReadAllText("prompt.txt"), valueStore, Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "");
            DocumentEvaluator documentEvaluator = new DocumentEvaluator(evaluatorAPI, valueStore);

            Console.WriteLine("Evaluating document.txt");


            // All document results
            var documentResultPairsUnsorted = new List<Tuple<DocumentInfo, EvaluatedCodeChunk>>();


            // Now to evaluate a document
            var documentInfo = new DocumentInfo("document.txt");
            await foreach (EvaluatedCodeChunk evaluated in documentEvaluator.EvaluateDocument(File.ReadAllText("document.txt"), documentInfo, 
                new CodeChunkReaderSettings()
                    {
                        SoftCodeChunkLengthLimit = 2048,
                        HardCodeChunkLengthLimit = 3000
                }
            ))
            {
                EvaluatorResult result = evaluated.Result;

                Console.WriteLine("Result: " + result.JSONResult);
                // Score
                double score;
                if (result.TryGetScore(out score))
                {
                    Console.WriteLine("Score: " + score);
                }
                else
                {
                    Console.WriteLine("No score");
                }

                documentResultPairsUnsorted.Add(new Tuple<DocumentInfo, EvaluatedCodeChunk>(documentInfo, evaluated));
            }

            Console.WriteLine("Done");
        }
    }
}