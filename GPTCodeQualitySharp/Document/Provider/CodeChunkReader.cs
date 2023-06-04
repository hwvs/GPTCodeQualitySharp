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

        /// <summary>
        /// Backtracking will start the next chunk at BacktrackRatio from the end of the previous chunk, 
        /// which means they will overlap by (BacktrackRatio * 100) percent
        /// </summary>
        public bool AllowBacktracking { get; set; } = true;
        /// <summary>
        /// ! Requires AllowBacktracking = true;
        /// Backtracking will start the next chunk at BacktrackRatio from the end of the previous chunk,
        /// which means they will overlap by (BacktrackRatio * 100) percent
        /// </summary>
        public double BacktrackRatio = 0.25d;
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

            List<int> backtrackedToLines = new List<int>(); // Keep track of lines that we backtracked to so we don't backtrack to them again
            //foreach (string line in lines)
            for(int i = 0; i < lines.Length; i++) // This allows us to backtrack
            {
                Tuple<int, int>? lastChunkReturned = null;

                string line = lines[i];

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
                        lastChunkReturned = new Tuple<int, int>(codeChunkStartLineNumber, codeChunkEndLineNumber); // Flag for backtracking
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
                        lastChunkReturned = new Tuple<int, int>(codeChunkStartLineNumber, codeChunkEndLineNumber); // Flag for backtracking
                        // Reset the StringBuilder
                        codeChunkStringBuilder.Clear();
                        codeChunkStringBuilder.Append(line);
                        codeChunkEndLineNumber++;
                        codeChunkStartLineNumber = codeChunkEndLineNumber;
                    }
                }
                // Increment the current line number
                currentLineNumber++;

                if(lastChunkReturned != null) // Returned a chunk this cycle, backtrack
                {
                    // if AllowBacktracking flag:
                    // Move the start line to BacktrackRatio lines back
                    // Eg: If backtrackRatio is 0.25 (25%) then we move back 25% of the lines
                    //     so if we did 100-200, previously, then we will then do 175-??? for
                    //     the next chunk
                    // ----

                    // If we are allowed to backtrack and we are not at the end of the file
                    if(_settings.AllowBacktracking && (i + 1 < lines.Length))
                    {
                        // Calculate the number of lines to backtrack
                        int backtrackLines = (int)((lastChunkReturned.Item2 - lastChunkReturned.Item1) * _settings.BacktrackRatio);
                        // If we are backtracking at least 1 line
                        if(backtrackLines > 0)
                        {
                            // Calculate the new start line
                            int newStartLine = codeChunkEndLineNumber - backtrackLines;
                            // If we have already backtracked to this line, then we need to move the start line up
                            while(backtrackedToLines.Contains(newStartLine))
                            {
                                newStartLine++;
                            }
                            // Add the new start line to the list of backtracked lines
                            backtrackedToLines.Add(newStartLine);
                            // Update the start line
                            codeChunkStartLineNumber = newStartLine;
                        }
                    }
                }
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
