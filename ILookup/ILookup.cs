using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Lookup
{    
    [InheritedExport(typeof(ILookup))]
    public interface ILookup
    {
        IDictionary<string, string> GetDictionaryFrom(string record);
    }
}
