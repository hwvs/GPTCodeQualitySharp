using GPTCodeQualitySharp.Document.Partial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Document.Provider
{
    public class CodeChunkReaderSettings
    {
        public int SoftCodeChunkLengthLimit { get; set; } = 2048;
        public int HardCodeChunkLengthLimit { get; set; } = 2800;


    }

    public class CodeChunkReader : IEnumerable<CodeChunkInfo>
    {
        private readonly string _code;
        private readonly DocumentInfo _documentInfo;
        private readonly CodeChunkReaderSettings _settings;

        public CodeChunkReader(string code, DocumentInfo documentInfo, CodeChunkReaderSettings? settings = null)
        {
            _code = code;
            _documentInfo = documentInfo;

            if(settings == null)
            {
                _settings = new CodeChunkReaderSettings();
            }
            else
            {
                _settings = settings!;
            }
        }

        // Read the file line-by line and optimize for code block coundaries when close to soft limit
        // If a line won't fit in the soft limit, then we start a new CodeChunk
        // If a line won't fit in the hard limit, then we are forced to split the line
        // Make sure to yield return the last CodeChunk
        // DEF:  public CodeChunkInfo(CodeChunk codeChunk, DocumentInfo documentInfo, int startLineNumber, int endLineNumber)
        public IEnumerator<CodeChunkInfo> GetEnumerator()
        {
            string[] lines = _code.Split('\n');
            int currentLineNumber = 0;
            int codeChunkStartLineNumber = 0;
            int codeChunkEndLineNumber = 0;
            StringBuilder codeChunkStringBuilder = new StringBuilder();
            foreach (string line in lines)
            {
                // If it's the first line, then we just add it to the StringBuilder
                if (currentLineNumber == 0)
                {
                    codeChunkStringBuilder.Append(line);
                    codeChunkEndLineNumber++;
                }
                // If it's not the first line, then we need to check if we can add it to the StringBuilder
                else
                {
                    // If it's less than the soft limit, then we can add it to the StringBuilder
                    if (codeChunkStringBuilder.Length + line.Length < _settings.SoftCodeChunkLengthLimit)
                    {
                        codeChunkStringBuilder.Append(line);
                        codeChunkEndLineNumber++;
                    }
                    // If it's less than the hard limit, then we can add it to the StringBuilder and yield return the CodeChunk
                    else if (codeChunkStringBuilder.Length + line.Length < _settings.HardCodeChunkLengthLimit)
                    {
                        // Yield return the current CodeChunk
                        CodeChunk codeChunk = new CodeChunk(codeChunkStringBuilder.ToString());
                        CodeChunkInfo codeChunkInfo = new CodeChunkInfo(codeChunk, _documentInfo, codeChunkStartLineNumber, codeChunkEndLineNumber);
                        yield return codeChunkInfo;
                        // Reset the StringBuilder
                        codeChunkStringBuilder.Clear();
                        codeChunkStringBuilder.Append(line);
                        codeChunkEndLineNumber++;
                        codeChunkStartLineNumber = codeChunkEndLineNumber;
                    }
                    // If it's greater than the hard limit, then we need to split the line and yield return the CodeChunk
                    else
                    {
                        // Yield return the current CodeChunk
                        CodeChunk codeChunk = new CodeChunk(codeChunkStringBuilder.ToString());
                        CodeChunkInfo codeChunkInfo = new CodeChunkInfo(codeChunk, _documentInfo, codeChunkStartLineNumber, codeChunkEndLineNumber);
                        yield return codeChunkInfo;
                        // Reset the StringBuilder
                        codeChunkStringBuilder.Clear();
                        codeChunkStringBuilder.Append(line);
                        codeChunkEndLineNumber++;
                        codeChunkStartLineNumber = codeChunkEndLineNumber;
                    }
                }
                // Increment the current line number
                currentLineNumber++;
            }
            // Yield return the last CodeChunk
            CodeChunk lastCodeChunk = new CodeChunk(codeChunkStringBuilder.ToString());
            CodeChunkInfo lastCodeChunkInfo = new CodeChunkInfo(lastCodeChunk, _documentInfo, codeChunkStartLineNumber, codeChunkEndLineNumber);
            yield return lastCodeChunkInfo;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            // call the generic version of the method
            return this.GetEnumerator();
        }

    }
}
