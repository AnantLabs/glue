using System;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// Summary description for SmtpException.
	/// </summary>
	public class SmtpException : Exception
	{
        public int Code;

        public SmtpException(int code) : base("Error")
        {
            this.Code = code;
        }

        public SmtpException(int code, string message) : base(message)
        {
            this.Code = code;
        }
	}
}

