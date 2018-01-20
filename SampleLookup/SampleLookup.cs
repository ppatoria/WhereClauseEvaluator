using Lookup;
using System.Collections.Generic;

namespace SampleLookup
{

    public class SampleLookup  : ILookup
    {
        private readonly IDictionary<string, string> _dictionary = new Dictionary<string, string>();
        public SampleLookup()
        {
            _dictionary.Add("c","0");
            _dictionary.Add("c1","1");
            _dictionary.Add("c2","2");
            _dictionary.Add("c3","3");
            _dictionary.Add("c4","4");
            _dictionary.Add("c5","5");
            _dictionary.Add("c6","6");
            _dictionary.Add("c7","7");
            _dictionary.Add("c8","8");
            _dictionary.Add("c9","9");
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
        public ILookup GetLookup()
        {
            return new SampleLookup();
        }
    }
}
