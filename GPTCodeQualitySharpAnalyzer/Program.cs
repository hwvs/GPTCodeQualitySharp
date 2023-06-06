using GPTCodeQualitySharp.Dataset;
using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Document.Provider;
using GPTCodeQualitySharp.Evaluator;
using GPTCodeQualitySharp.Evaluator.API;
using GPTCodeQualitySharp.Evaluator.API.Impl;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GPTCodeQualitySharpAnalyzer
{
    internal class Program
    {
        private static void PrintHelp()
        {
            string helpdoc = @"
Usage: GPTCodeQualitySharpAnalyzer [OPTIONS]

Options:
    --help      Print this help message
    --folder    Folder to analyze [REQUIRED]
    --pattern   Pattern to match files [default: *.cs]
    --db        Path to the database [default: GPTCodeQualitySharpAnalyzer3.db - stored in %APPDATA% (WIN) or /~ (UNIX)]
    --prompt    Path to the prompt file [default: prompt.txt]
    --apikey    OpenAI API key [default: %OPENAI_API_KEY% Environment Variable]
    --result    Path to store a JSON file with the results [default: results_###.json]

Experimental:
    --temperature  Causes non-deterministic results - breaks caching and may use excessive API costs, use at own risk [default: 0] - EXPERIMENTAL!
";
            Console.WriteLine(helpdoc);
        }


        static async Task Main(string[] args)
        {
            string dbPath = "GPTCodeQualitySharpAnalyzer3.db";

            // HELP
            if (GetArgument(args, "--help", "") != "" || args.Length == 0)
            {
                PrintHelp();
                if (args.Length == 0) // Only wait if no args were passed
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                }
                return;
            }

            // Try to get the db path from the command line
            string dbPathFromCommandLine = GetArgument(args, "--db");
            if (dbPathFromCommandLine != "")
            {
                dbPath = dbPathFromCommandLine;
            }
            else
            {
                // Only WINDOWS
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    dbPath = Path.Join(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"), dbPath); // Store in local appdata
                }
                else
                {
                    if (Path.Exists("~"))
                    {
                        dbPath = Path.Join(Environment.ExpandEnvironmentVariables("~"), dbPath); // Store in /home/user
                    }
                    else
                    {
                        // Store in PWD
                    }
                }
            }

            var valueStore = new SQLiteValueStore(dbPath);
            string promptPath = GetArgument(args, "--prompt", "prompt.txt");
            if(!File.Exists(promptPath))
            {
                Console.WriteLine($"Error: Prompt file \"{promptPath}\" does not exist");
                return;
            }

            string openAIAPIKey = GetArgument(args, "--apikey", Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "");

            decimal temperature = 0;
            string temperatureStr = GetArgument(args, "--temperature", "0");
            if(!decimal.TryParse(temperatureStr, out temperature))
            {
                Console.WriteLine($"Error: Invalid temperature \"{temperatureStr}\"");
                return;
            }
            // Show warning if temperature is not 0
            if(temperature != 0)
            {
                Console.WriteLine("Warning: Using a non-zero temperature is experimental and may cause non-deterministic results");
            }

            var evaluatorAPI = new OpenAIChatEvaluator(File.ReadAllText(promptPath), valueStore, openAIAPIKey, temperature);
            DocumentEvaluator documentEvaluator = new DocumentEvaluator(evaluatorAPI, valueStore);



            // Now evaluate an entire folder (recursively) based on the pattern
            string folder = GetArgument(args, "--folder", ""); // REQUIRED
            if (folder == "")
            {
                Console.WriteLine("Error: --folder argument is required");
                return;
            }
            if(!Directory.Exists(folder))
            {
                Console.WriteLine($"Error: Folder \"{folder}\" does not exist");
                return;
            }
            string pattern = GetArgument(args, "--pattern", "*.cs"); // Default to C# files
            string[] files = Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);

            // All document results
            var documentResultPairsUnsorted = new List<Tuple<DocumentInfo, EvaluatedCodeChunk>>();

            // Evaluate each document
            Regex exclusionRegex = new Regex(@"\.AssemblyAttributes\.cs|\.AssemblyInfo\.cs|\.Designer\.cs");
            foreach (string file in files)
            {
                if(exclusionRegex.IsMatch(file))
                {
                    Console.WriteLine($"Skipping {file}...");
                    continue;
                }

                Console.WriteLine($"Evaluating {file}...");
                var documentResultPairs = await EvaluateDocument(documentEvaluator, folder, file);
                documentResultPairsUnsorted.AddRange(documentResultPairs);
            }

            // Check if there are any results
            if(documentResultPairsUnsorted.Count == 0)
            {
                Console.WriteLine("Error: No results");
                return;
            }

            // Save the results
            string timeStampInt = DateTime.Now.ToString("yyyyMMddHHmmss");
            string resultPath = GetArgument(args, "--result", $"results_{timeStampInt}.json");


            // Path
            // Score
            // CodeChunkInfo.StartLineNumber
            // CodeChunkInfo.EndLineNumber
            var jsonObject = new JObject();

            foreach(var resultPair in documentResultPairsUnsorted) {
                DocumentInfo documentInfo = resultPair.Item1;
                EvaluatedCodeChunk evaluatedCodeChunk = resultPair.Item2;

                var result = evaluatedCodeChunk.Result;
                var resultObject = new JObject();
                resultObject["path"] = documentInfo.RelativeFilePath;
                double score = -1;
                resultObject["score"] = result.TryGetScore(out score) ? score : -1;
                resultObject["startline"] = evaluatedCodeChunk.CodeChunkInfo.StartLineNumber;
                resultObject["endline"] = evaluatedCodeChunk.CodeChunkInfo.EndLineNumber;

                resultObject["rawResponse"] = result.JSONResult;

                // Add the result to the JSON object
                jsonObject[documentInfo.RelativeFilePath] = resultObject;
            }

            await File.WriteAllTextAsync(resultPath, jsonObject.ToString());
            
            Console.WriteLine($"Results saved to {resultPath}");
        }

        // Read an argument "--Argument value" from the command line
        private static string GetArgument(string[] args, string argumentName, string defaultValue = "")
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        return args[i + 1];
                    }
                    else
                    {
                        return defaultValue;
                    }
                }
            }
            return defaultValue;
        }

        private static async Task<List<Tuple<DocumentInfo, EvaluatedCodeChunk>>> EvaluateDocument(DocumentEvaluator documentEvaluator, string documentBase, string documentPath)
        {
            var documentResultPairsUnsorted = new List<Tuple<DocumentInfo, EvaluatedCodeChunk>>();
            string relativePath = Path.GetRelativePath(documentBase, documentPath);
            var documentInfo = new DocumentInfo(relativePath);
            string documentContents = await File.ReadAllTextAsync(documentPath);
            await foreach (EvaluatedCodeChunk evaluated in documentEvaluator.EvaluateDocument(documentContents, documentInfo, 
                new CodeChunkReaderSettings()
                    {
                        SoftCodeChunkLengthLimit = 2048,
                        HardCodeChunkLengthLimit = 3000,
                        BacktrackRatio = 0.5,
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

            return documentResultPairsUnsorted;
        }

    }
}