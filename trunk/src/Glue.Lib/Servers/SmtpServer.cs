using System;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// Summary description for SmtpServer.
	/// </summary>
    public class SmtpServer : TcpServer
    {
        public SmtpServer(IPEndPoint localEP) : base(localEP) {}

        /// <summary>
        /// Overridden to return a SMTP specific connection object.
        /// </summary>
        public override TcpConnection CreateConnection(Socket socket)
        {
            return new SmtpConnection(this, socket);
        }
    }
}
