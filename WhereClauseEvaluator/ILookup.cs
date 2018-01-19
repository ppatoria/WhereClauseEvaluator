using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser
{
    public interface ILookup
    {
        string GetValue(string columnName);
    }
}
