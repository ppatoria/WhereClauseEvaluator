using Lookup;
using System.Linq;
using System.Collections.Generic;

namespace SampleLookup
{

    public class SampleLookup  : ILookup
    {
        private readonly IDictionary<string, string> _dictionary = new Dictionary<string, string>();
             
        public SampleLookup(string record)
        {
            foreach(var r in record.Trim().Split(' '))
            {
                var kv = r.Trim().Split('=');
                _dictionary.Add(kv[0], kv[1]);
            }                         
        }

        public string GetValue(string columnName)
        {
            if (_dictionary.TryGetValue(columnName, out string value))
                return value;
            throw new KeyNotFoundException($@"Key {columnName} not found.");            
        }
    }
    public class LookupFactory : ILookupFactory
    {
        public ILookup GetLookup(string str)
        {
            return new SampleLookup(str);
        }
    }
}
