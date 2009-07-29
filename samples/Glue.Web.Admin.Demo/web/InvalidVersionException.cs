using System;

namespace web
{
    public class InvalidVersionException : Exception
    {
        public InvalidVersionException(string type, int version, int expected)
            : base(String.Format("Invalid version for {0}. Version={1}, expected={2}", type, version, expected))
        {
        }
    }
}