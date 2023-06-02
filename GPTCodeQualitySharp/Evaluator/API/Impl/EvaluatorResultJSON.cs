using GPTCodeQualitySharp.Dataset;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Evaluator.API.Impl
{
    public abstract class EvaluatorResultJSON : IHashableData
    {
        public abstract string ToHashableString();

        internal abstract string GetResult();

        public bool TryGetJSONString(out EvaluatorResult result)
        {
            // Match result for regex { .... }
            string pattern = @"{[^}]+}";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Multiline);
            string apiResult = GetResult();
            Debug.WriteLine($"Result: {apiResult}");
            System.Text.RegularExpressions.Match match = regex.Match(apiResult);
            if (match.Success)
            {
                result = new EvaluatorResult(true, match.Value);
                return true;
            }

            // Failure
            result = new EvaluatorResult(false, string.Empty); //TODO: Try a different approach
            return false;
        }
    }
}
