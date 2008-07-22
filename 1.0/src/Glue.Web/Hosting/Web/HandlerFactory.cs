using System;
using System.Xml;
using System.Web;
using System.Web.UI;
using System.Web.SessionState;
using Glue.Lib;

namespace Glue.Web.Hosting.Web
{
    /// <summary>
    /// Summary description for HandlerFactory.
    /// </summary>
    public class HandlerFactory : IHttpHandlerFactory
    {
        public class HandlerWrapper : IHttpHandler, IRequiresSessionState
        {
            public void ProcessRequest(HttpContext context)
            {
                try
                {
                    App.Current.Process(
                        new Glue.Web.Hosting.Web.Request(context),
                        new Glue.Web.Hosting.Web.Response(context)
                        );
                }
                catch
                {
                    if (context.Response.StatusCode == 200)
                        throw;
                }
            }

            public bool IsReusable
            {
                get { return true; }
            }
        }

        static HandlerFactory()
        {
            foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath), "*.dll"))
            {
                System.Reflection.Assembly.LoadFrom(file);
            }
        }
        
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            bool findDefault = false;
            try
            {
                findDefault = System.IO.Path.HasExtension(pathTranslated) && System.IO.File.Exists(pathTranslated);
            }
            catch
            {
            }
            if (findDefault)
            {
                // The path has an extension AND a file exists at that location,
                // so Find default MS supplied handler type (for static files probably).
                Type type = FindDefaultHandlerType(requestType, System.IO.Path.GetExtension(pathTranslated));
                if (type == null)
                    throw new HttpException("Handler type not found.");

                object o = Activator.CreateInstance(type, true);
                IHttpHandlerFactory f = o as IHttpHandlerFactory;
                if (f != null)
                    return f.GetHandler(context, requestType, url, pathTranslated);
                
                IHttpHandler h = o as IHttpHandler;
                if (h != null)
                    return h;
                
                throw new HttpException("Handler not found.");
            }
            else
            {
                return new HandlerWrapper();
            }
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }

        private static System.Reflection.Assembly webAssembly = typeof(System.Web.HttpContext).Assembly;
        private static System.Reflection.Assembly webServicesAssembly = typeof(System.Web.Services.WebService).Assembly;

        private Type FindDefaultHandlerType(string verb, string ext)
        {
            switch (ext.ToLower())
            {
                case ".vjsproj":
                case ".java":
                case ".jsl":
                case ".asax":
                case ".ascx":
                case ".config":
                case ".cs":
                case ".csproj":
                case ".vb":
                case ".vbproj":
                case ".webinfo":
                case ".asp":
                case ".licx":
                case ".resx":
                case ".resources":
                case ".axd":
                    return webAssembly.GetType("System.Web.HttpForbiddenHandler");
                case ".aspx":
                    return webAssembly.GetType("System.Web.UI.PageHandlerFactory");
                case ".ashx":
                    return webAssembly.GetType("System.Web.UI.SimpleHandlerFactory");
                case ".asmx":
                    return webServicesAssembly.GetType("System.Web.Services.Protocols.WebServiceHandlerFactory");
                case ".rem":
                    return webServicesAssembly.GetType("System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory");
                case ".soap":
                    return webServicesAssembly.GetType("System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory");
                default:
                    if (string.Compare(verb, "GET", true) == 0 ||string.Compare(verb, "HEAD", true) == 0)
                        return webAssembly.GetType("System.Web.StaticFileHandler");
                    else
                        return webAssembly.GetType("System.Web.HttpMethodNotAllowedHandler");
            }
        }
    }
}