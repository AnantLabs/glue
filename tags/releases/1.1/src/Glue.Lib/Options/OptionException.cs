using System;

namespace Glue.Lib.Options
{
    public class OptionException : ArgumentException
    {
        public OptionException(string message) : base(message) {}
        public OptionException(string message, string param) : base(message, param) {}
        public OptionException(string message, Exception inner) : base(message, inner) {}
    }

    public class OptionStopException : OptionException
    {
        public int Code;
        public OptionStopException(string message, int code) : base(message) { this.Code = code; }
    }
}
