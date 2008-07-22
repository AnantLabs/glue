using System;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Specialized;

namespace Glue.Lib.Servers
{
    /// <summary>
    /// Summary description for HttpServer.
    /// </summary>
    public abstract class HttpServer : TcpServer
    {
        public HttpServer(IPEndPoint localEP) : base(localEP) {}

        /// <summary>
        /// Overridden to return a SMTP specific connection object.
        /// </summary>
        public override TcpConnection CreateConnection(Socket socket)
        {
            return new HttpConnection(this, socket);
        }

        /// <summary>
        /// Override this method to for a specialized HttpRequest class.
        /// </summary>
        public virtual HttpRequest CreateRequest(HttpConnection connection)
        {
            return new HttpRequest(connection);
        }

        /// <summary>
        /// Override this method to for a specialized HttpRequest class.
        /// </summary>
        public virtual HttpResponse CreateResponse(HttpConnection connection)
        {
            return new HttpResponse(connection);
        }

        /// <summary>
        /// Override in derived class to actually process the request.
        /// </summary>
        public abstract void ProcessRequest(HttpRequest request, HttpResponse response);
    }
}
