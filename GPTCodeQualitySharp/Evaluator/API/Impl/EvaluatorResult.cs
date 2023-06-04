using GPTCodeQualitySharp.Document.Partial;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator.API.Impl
{
    public class EvaluatorResult
    {
        // Success
        // String JSON result

        public bool Success { get; }
        public string JSONResult {  get; }


        public bool TryGetScore(out double score)
        {
            score = 0d;
            if(!Success)
            {
                return false;
            }

            try {
                var obj = JObject.Parse(JSONResult);
                double total_score = 0;
                double max_score = 0;

                foreach (var kvp in obj.Cast<KeyValuePair<string, JToken>>().ToList())
                {

                    if (kvp.Key.StartsWith("s") && kvp.Key.Contains('_'))
                    {
                        total_score += kvp.Value.Value<double>();
                        max_score += 10;
                    }
                }

                score = total_score / (double)max_score * 100;

                return true;

            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return false;

        }


        public EvaluatorResult( bool success, string result)
        {
            Success = success;
            JSONResult = result;
        }
    }
}
