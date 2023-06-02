using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCodeQualitySharp.Dataset
{
    public class ValueStoreTable
    {
        public static ValueStoreTable ApiResult = new ValueStoreTable("ApiResult");
        public static ValueStoreTable OpenAIResponses = new ValueStoreTable("OpenAIResponses");

        public static List<ValueStoreTable> Values = new List<ValueStoreTable>()
        {
            ApiResult
        };

        public string Name { get; }

        public ValueStoreTable(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }


    public interface IValueStore
    {
        // StoreValue (upsert)
        // TryGetValue

        public void StoreValue(ValueStoreTable table, IHashableData key, string value);

        public bool TryGetValue(ValueStoreTable table, IHashableData key, out string value);



    }
}
