//
// Glue.Lib.Mail.SmtpException.cs
//
//
using System.IO;

namespace Glue.Lib.Mail 
{

    // an exception thrown when an smtp exception occurs
    internal class SmtpException : IOException 
    {
        public SmtpException( string message ) : base( message ) {}
    }

}
