using GPTCodeQualitySharp.Document.Partial;
using GPTCodeQualitySharp.Document.Provider;
using System.Text;

namespace GPTCodeQualitySharpTests
{
    [TestClass]
    public class CodeChunkReaderTests
    {
        const int TEST_DOCUMENT_LENGTH = 5000;

        const string TEST_DOCUMENT_START_WORD = "start of document";
        const string TEST_DOCUMENT_MIDDLE_WORD = "middle of document";
        const string TEST_DOCUMENT_END_WORD = "end of document";

        // Make sure code chunks are sane
        [TestMethod]
        public async void SanityCheck()
        {
            string exampleDocument = GetExampleDocument();
            Assert.IsTrue(exampleDocument.Length == TEST_DOCUMENT_LENGTH, "GetExampleDocument() returned document of incorrect length from SanityCheck()"); // Sanity check - if it fails then you did something wrong
        
            // Make sure the document contains the start, middle, and end words
            Assert.IsTrue(exampleDocument.Contains(TEST_DOCUMENT_START_WORD), "GetExampleDocument() returned document that does not contain TEST_DOCUMENT_START_WORD from SanityCheck()");
            Assert.IsTrue(exampleDocument.Contains(TEST_DOCUMENT_MIDDLE_WORD), "GetExampleDocument() returned document that does not contain TEST_DOCUMENT_MIDDLE_WORD from SanityCheck()");
            Assert.IsTrue(exampleDocument.Contains(TEST_DOCUMENT_END_WORD), "GetExampleDocument() returned document that does not contain TEST_DOCUMENT_END_WORD from SanityCheck()");

            // Now to check the code chunks
            DocumentInfo documentInfo = new DocumentInfo("test");
            CodeChunkReaderSettings settings = new CodeChunkReaderSettings();
            settings.HardCodeChunkLengthLimit = 1000;
            settings.SoftCodeChunkLengthLimit = 500;
            CodeChunkReader codeChunkReader = new CodeChunkReader(exampleDocument, documentInfo, settings);

            int codeChunkCount = 0;
            List<CodeChunkInfo> allCodeChunks = codeChunkReader.ToList();
            foreach (CodeChunkInfo codeChunkInfo in allCodeChunks)
            {
                codeChunkCount++;
                CodeChunk codeChunk = codeChunkInfo.CodeChunk;
                Assert.IsTrue(codeChunk.Code.Length <= settings.HardCodeChunkLengthLimit, "CodeChunkReader returned a CodeChunk that is longer than the hard limit from SanityCheck()");
                Assert.IsTrue(codeChunk.Code.Length <= settings.SoftCodeChunkLengthLimit, "CodeChunkReader returned a CodeChunk that is longer than the soft limit from SanityCheck()");
            }

            // Make sure there are at least 3 code chunks
            Assert.IsTrue(codeChunkCount >= 3, "CodeChunkReader returned less than 3 code chunks from SanityCheck()");

            // Make sure a chunk starts with the start word
            bool startsWithStartWord = allCodeChunks.Where(codeChunkInfo => codeChunkInfo.CodeChunk.Code.StartsWith(TEST_DOCUMENT_START_WORD)).Count() > 0;
            Assert.IsTrue(startsWithStartWord, "CodeChunkReader did not return a CodeChunk starting with the TEST_DOCUMENT_START_WORD");


            // Make sure a chunk ends with the end word
            bool endsWithEndWord = allCodeChunks.Where(codeChunkInfo => codeChunkInfo.CodeChunk.Code.TrimEnd().EndsWith(TEST_DOCUMENT_END_WORD)).Count() > 0; // TrimEnd() is needed because the last chunk will have a bunch of whitespace at the end
            Assert.IsTrue(endsWithEndWord, "CodeChunkReader did not return a CodeChunk ending with the TEST_DOCUMENT_END_WORD");


            // Make sure a chunk in the middle contains the middle word
            bool containsMiddleWord = allCodeChunks.Where(codeChunkInfo => codeChunkInfo.CodeChunk.Code.Contains(TEST_DOCUMENT_MIDDLE_WORD)).Count() > 0;
            Assert.IsTrue(containsMiddleWord, "CodeChunkReader did not return a CodeChunk containing the TEST_DOCUMENT_MIDDLE_WORD");




        
        }

        private static string GetExampleDocument()
        {
            // Note: 10 <= Length <= 100000
            Assert.IsTrue(TEST_DOCUMENT_LENGTH <= 100000, "TEST_DOCUMENT_LENGTH must be <= 100000");
            Assert.IsTrue(TEST_DOCUMENT_LENGTH >= 10, "TEST_DOCUMENT_LENGTH must be >= 10");

            // Create a sample document of EXACTLY TEST_DOCUMENT_LENGTH length
            // [
            // TEST_DOCUMENT_START_WORD at the start
            // random words in between
            // TEST_DOCUMENT_MIDDLE_WORD in the middle
            // random words in between
            // TEST_DOCUMENT_END_WORD at the end
            // ]
            // ^ Entire string is TEST_DOCUMENT_LENGTH long (including newlines)

            int lengthOfStartWord = TEST_DOCUMENT_START_WORD.Length;
            int lengthOfMiddleWord = TEST_DOCUMENT_MIDDLE_WORD.Length;
            int lengthOfEndWord = TEST_DOCUMENT_END_WORD.Length;


            // Account for newlines between each line
            //int lengthBetweenStartAndMiddle = (TEST_DOCUMENT_LENGTH - lengthOfStartWord - lengthOfMiddleWord) / 2 - 2;
            // Calculate this one to make sure it's exactly TEST_DOCUMENT_LENGTH long based on lengthBetweenStartAndMiddle
            //int lengthBetweenMiddleAndEnd = TEST_DOCUMENT_LENGTH - lengthBetweenStartAndMiddle - lengthOfStartWord - lengthOfMiddleWord - 3;
            // ERROR: THIS DOES NOT WORK, 5022 instead of 5000 at the end
            // FIX that does it properly
            int lengthBetweenStartAndMiddle = (TEST_DOCUMENT_LENGTH - lengthOfStartWord - lengthOfMiddleWord - lengthOfEndWord - 3) / 2 - 2;
            int lengthBetweenMiddleAndEnd = TEST_DOCUMENT_LENGTH - lengthBetweenStartAndMiddle - lengthOfStartWord - lengthOfMiddleWord - lengthOfEndWord - 3 - 7;


            StringBuilder sb = new StringBuilder();
            sb.AppendLine(TEST_DOCUMENT_START_WORD);
            // Add random words in between
            sb.AppendLine(string.Join("\n", GetRandomWordLine(lengthBetweenStartAndMiddle)));
            sb.AppendLine(TEST_DOCUMENT_MIDDLE_WORD);
            // Add random words in between
            sb.AppendLine(string.Join("\n", GetRandomWordLine(lengthBetweenMiddleAndEnd)));
            sb.AppendLine(TEST_DOCUMENT_END_WORD);
            return sb.ToString();

        }


        /// <summary>
        /// Returns a series of random lines that when joined together with "\n" will be EXACTLY totalLength long
        /// </summary>
        /// <param name="totalLength"></param>
        /// <returns></returns>
        private static string GetRandomWordLine(int totalLength, string randomWord = "aaa")
        {
            int lengthOfWord = randomWord.Length;
            // account for newline for each line,
            // substring to make the last line fit
            int numberOfLines = (totalLength + lengthOfWord) / (lengthOfWord + 1);
            List<string> lines = new List<string>();
            for (int i = 0; i < numberOfLines - 2; i++)
            {
                lines.Add(randomWord);
            }
            // Add a line that is the remainder of the length
            int numCharToAdd = totalLength - (numberOfLines - 2) * (lengthOfWord + 1) - 3;
            char toAdd = 'A';
            string lastLine = randomWord + new string(toAdd, numCharToAdd);
            lines.Add(lastLine);

            string result = string.Join("\n", lines);
            Assert.AreEqual(result.Length, totalLength);
            return result;
        }
    }
}