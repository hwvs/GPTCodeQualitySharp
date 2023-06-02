using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Dataset
{
    internal class HashProvider
    {
        // Return a sha256 of a string
        private static string SHA256FromString(string input)
        {
            // Create a SHA256
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // ComputeHash - returns byte[]
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte[] to string
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString().ToUpper();
            }
        }

        /// <summary>
        /// Returns a truncated SHA256 hash of a string, to the first 16 characters (64 bits / 8 bytes).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SHA256TruncFromString(string input)
        {
            return SHA256FromString(input).Substring(0, 16);
        }
    }
}
