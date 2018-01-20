using System.ComponentModel.Composition;

namespace Lookup
{    
    public interface ILookup
    {
        string GetValue(string columnName);
    }

    [InheritedExport(typeof(ILookupFactory))]
    public interface ILookupFactory
    {
        ILookup GetLookup(string str);
    }
}
