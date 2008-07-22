using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data
{
    class DataException : Exception
    {
        public DataException(string message) : base(message) { }
        public DataException(string message, System.Exception inner) : base(message, inner) { }
    }
}
