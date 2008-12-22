using System;
using System.Net;
using System.Net.Sockets;
using Glue.Lib.Servers;

namespace Glue.Web.Hosting.Server
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
    public class Server : HttpServer
    {
        public Server() : base(new IPEndPoint(IPAddress.Any, 8888))
        {
        }

        public override void Start()
        {
            App.Current.CleanTempDirectory();
            base.Start();
        }

        public override HttpRequest CreateRequest(HttpConnection connection)
        {
            return new Request(connection);
        }

        public override HttpResponse CreateResponse(HttpConnection connection)
        {
            return new Response(connection);
        }

        public override void ProcessRequest(HttpRequest request, HttpResponse response)
        {
            try
            {
                App.Current.Process((IRequest)request, (IResponse)response);
            }
            catch 
            {
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Glue.Lib.Configuration.Register("web.config", true);
            Server server = new Server();
            server.Start();
            try
            {
                System.Console.ReadLine();
            }
            finally
            {
                System.Console.WriteLine("Stop");
                server.Stop();
            }
        }
    }
}
