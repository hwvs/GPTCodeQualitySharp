using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Document.Partial
{
    public struct DocumentInfo
    {
        public string RelativeFilePath { get; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(RelativeFilePath);
            }
        }

        public DocumentInfo(string relativeFilePath)
        {
            RelativeFilePath = relativeFilePath;
        }

    }
}
