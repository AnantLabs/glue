//
// Glue.Lib.Pop3.SmtpException.cs
//
//
using System.IO;

namespace Glue.Lib.Net.Pop3 
{

    // Exception thrown when a POP3 error occurs
    public class Pop3Exception : IOException 
    {
        public Pop3Exception(string message) : base(message) { }
    }

}
