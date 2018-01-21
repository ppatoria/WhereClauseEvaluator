using Lookup;
using System.Linq;
using System.Collections.Generic;

namespace SampleLookup
{
    public class LookupFactory : ILookup
    {
        public IDictionary<string, string> GetDictionaryFrom(string record)
            => record
            .Trim()
            .Split(' ')
            .Select(s => s.Split('='))
            .ToDictionary(s => s[0], s => s[1]);
    }
}
