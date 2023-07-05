using GPTCodeQualitySharp.Dataset;
using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Evaluator.API.Impl;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator.API
{
    public class OpenAIChatEvaluator : ICodeChunkEvaluatorAsync
    {
        // Prompt template property
        string PromptTemplate { get; }
        public string ApiKey { get; set; }
        private readonly decimal _temperature;
        private readonly IValueStore _valueStore;

        public OpenAIChatEvaluator(string promptTemplate, IValueStore valueStore, string ApiKey, decimal temperature = 0)
        {
            PromptTemplate = promptTemplate;
            _temperature = temperature;
            _valueStore = valueStore;

            if (!PromptTemplate.Contains("{CODE}"))
            {
                throw new ArgumentException("Prompt template must contain {CODE} placeholder");
            }

            if (ApiKey != null && ApiKey.Length > 1)
            {
                this.ApiKey = ApiKey;
            }
            else
            {

                throw new ArgumentException("No API key provided - please provide an API key or set the OPENAI_API_KEY environment variable");

            }
        }

        private string PreparePrompt(CodeChunk codeChunk)
        {
            return PromptTemplate.Replace("{CODE}", codeChunk.Code);
        }

        // Used by GetBestModelCachedAsync
        private volatile static Model? cachedModel = null;

        /// <summary>
        /// Retrieves the best available model from the OpenAI API, caching the result for future use.
        /// </summary>
        /// <param name="api">The OpenAI API instance to use for querying models.</param>
        /// <returns>The best available model, or null if no suitable model is found.</returns>
        private static async Task<Model?> GetBestModelCachedAsync(OpenAIAPI api)
        {
            // Return the cached model if it's already available
            if (cachedModel != null)
            {
                return cachedModel;
            }

            // Retrieve the list of available models from the API
            var models = await api.Models.GetModelsAsync();

            // Filter the models to only include GPT-3.5 models that are not large (32k or 16k)
            var chatModelsNonLarge = models
                .Where(m => m.ModelID.StartsWith("gpt-3.5-"))
                .Where(m => !m.ModelID.Contains("32k") && !m.ModelID.Contains("16k"));

            // If there are any suitable models, cache and return the first one
            if (chatModelsNonLarge.Any())
            {
                cachedModel = chatModelsNonLarge.First();
                return cachedModel;
            }

            // Return null if no suitable model is found
            return null;
        }

        public async Task<EvaluatorResult> EvaluateAsync(CodeChunk codeChunk)
        {
            // TODO: Make this robust
            string prompt = PreparePrompt(codeChunk);

            // Split prompt by {ROLE} and anything before it is a system message
            // {ROLE} this is user text {ROLE} this is assistant text {ROLE} this is user text etc.


            string[] promptSplit = prompt.Split("{ROLE}");
            var cacheKey = new HashableDataset(promptSplit);

            // Cache hit
            // TODO: If temperature == 0 only
            if (_valueStore.TryGetValue(ValueStoreTable.ApiResult, cacheKey, out string cacheResult))
            {
                return new EvaluatorResult(true, cacheResult);
            }

            if (promptSplit.Length == 0)
            {
                throw new ArgumentException("Prompt template must contain {ROLE} placeholder");
            }


            if (promptSplit.Length == 0)
            {
                throw new ArgumentException("No chat messages found in prompt");
            }

            // Create the chat
            /*var chat = api.Chat.CreateConversation();

            /// give instruction as System
            chat.AppendSystemMessage("You are a teacher who helps children understand if things are animals or not.  If the user tells you an animal, you say \"yes\".  If the user tells you something that is not an animal, you say \"no\".  You only ever respond with \"yes\" or \"no\".  You do not say anything else.");

            // give a few examples as user and assistant
            chat.AppendUserInput("Is this an animal? Cat");
            chat.AppendExampleChatbotOutput("Yes");
            chat.AppendUserInput("Is this an animal? House");
            chat.AppendExampleChatbotOutput("No");*/


            OpenAIAPI api = new OpenAIAPI(new APIAuthentication(ApiKey));

            ChatRequest chatArgs = new ChatRequest();
            var chatModel = await GetBestModelCachedAsync(api);
            if (chatModel != null) {
                chatArgs.Model = chatModel.ModelID;
            }
            var chat = api.Chat.CreateConversation(chatArgs);



            // Add the chat messages to the chat

            chat.AppendSystemMessage(promptSplit[0]);

            if (promptSplit.Length >= 1)
            {
                // Otherwise we have a system message and a list of user and assistant messages
                for (int i = 1; i < promptSplit.Length; i++)
                {
                    if (i % 2 == 1)
                    {
                        // User message
                        chat.AppendUserInput(promptSplit[i]);
                    }
                    else
                    {
                        // Assistant message
                        chat.AppendExampleChatbotOutput(promptSplit[i]);
                    }
                }
            }


            chat.RequestParameters.Temperature = (double)_temperature;

            // Get the response
            int retries = 0;
            var retryDelay = TimeSpan.FromSeconds(15); // TODO: Make this configurable
            string response = null;
            while (++retries < 3) // TODO: Refactor this
            {
                try
                {
                    response = await chat.GetResponseFromChatbotAsync()!;
                    break;
                }
                catch (HttpRequestException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("TooManyRequests"))
                    {
                        Console.WriteLine($"Too many requests, retrying after {retryDelay.TotalSeconds} seconds");
                        await Task.Delay(retryDelay);
                    }
                    else
                    {
                        throw ex; // Re-throw
                    }
                }
            }

            if(response != null)
            {
                return EvaluatorResultFromJSONResponse(
                    valueStore: in _valueStore, 
                    cacheKey: cacheKey, 
                    codeChunk: codeChunk, 
                    prompt: prompt, 
                    response: response
                    );
            }
            else
            {
                throw new Exception("OpenAI API returned null response");
            }


        }

        internal static EvaluatorResult EvaluatorResultFromJSONResponse(in IValueStore valueStore, HashableDataset? cacheKey, CodeChunk codeChunk, string prompt, string response) //TODO: Refactor this, public because of the Mock class
        {

            // Cache the response
            if (valueStore != null && cacheKey != null)
            {
                valueStore!.StoreValue(ValueStoreTable.OpenAIResponses, cacheKey!, response);
            }

            var evaluator = new JSONResultEvaluator(codeChunk, prompt, response);

            EvaluatorResult result;
            evaluator.TryGetJSONString(out result);

            return result;
        }
    }
}
