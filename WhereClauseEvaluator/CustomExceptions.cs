using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser
{
    public class InvalidWhereClauseException : Exception
    {
        public InvalidWhereClauseException():base()
        { }
        public InvalidWhereClauseException(string message) : base(message)
        { }
    }
}
