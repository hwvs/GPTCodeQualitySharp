using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Dataset
{
    public interface IHashableData
    {
        public string ToHashableString();
    }

    public class HashableDataset : IHashableData
    {
        private List<string> _strings = new List<string>();
        public HashableDataset(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                _strings.Add(value.Replace("\n","~~~~N~~~~"));
            }

        }

        public string ToHashableString()
        {
            return string.Join("\n\n", _strings);
        }



    }
}
