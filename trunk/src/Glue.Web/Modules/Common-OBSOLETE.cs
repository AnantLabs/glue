using System;
using System.IO;
using Edf.Lib;
using Edf.Glue;

namespace Edf.Glue.Modules
{
	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class CommonOBSOLETE : IModule
	{
        /// <summary>
        /// Before
        /// </summary>
        public bool Before(IRequest request, IResponse response)
        {
            Log.Info(
                "Begin Request:" + 
                " url=" + request.Url + 
                " ip=" + request.Params["REMOTE_ADDR"] +
                " identity=" + (request.User == null ? "(null)" : "" + request.User.Identity) + 
                " name=" + (request.User == null || request.User.Identity == null ? "(null)" : request.User.Identity.Name)
                );

            // Check offline
            if (App.Current.Offline) 
            {
                if (App.Current.OfflinePage != null && App.Current.OfflinePage.Length > 0)
                    response.TransmitFile(App.Current.OfflinePage);
                else
                    throw new GlueException("Application is off-line.");
                return true;
            }

            // Return existing files immediately
            // no need to look for actual files, because these are already be handled by the WebHandlerFactory
            /*
            string ext = Path.GetExtension(request.Path).ToLower();
            if (ext != "" && ext != ".aspx") 
            {
                string path = App.Current.MapPath(request.Path);
                if (File.Exists(path)) 
                {
                    response.ContentType = App.Current.GetContentType(path);
                    response.TransmitFile(path);
                    return true;
                }
            }
                
            // Filter out specific stuff
            if (request.Path == "/favicon.ico" || request.Path == "/robots.txt") 
            {
                response.StatusCode = 404;
                return true;
            }
            */

            return false;
        }

        /// <summary>
        /// Process
        /// </summary>
        public bool Process(IRequest request, IResponse response, Type controller)
        {
            return false;
        }

        /// <summary>
        /// Error
        /// </summary>
        public bool Error(IRequest request, IResponse response, Exception exception)
        {
            Exception e = exception;
            while (e != null) 
            {
                if (!(e is System.Reflection.TargetInvocationException))
                    Log.Error(e);
                exception = e;
                e = e.InnerException;
            }
            if (exception is GlueNotFoundException ||
                exception is System.IO.FileNotFoundException)
            {
                TryRenderErrorPage(response, 404);
            }
            else
            {
                TryRenderErrorPage(response, 500);
            }
            return false;
        }

        void TryRenderErrorPage(IResponse response, int code)
        {
            response.Clear();
            response.StatusCode = code;
            try 
            {
                string path = "/views/" + code + ".html";
                if (!File.Exists(App.Current.MapPath(path)))
                {
                    response.Output.WriteLine(response.StatusDescription + " (" + response.StatusCode + ")");
                    return;
                }
                View view = ViewCompiler.GetCompiledInstance(null, path);
                if (view != null)
                    view.Render(response.Output);
            }
            catch (Exception e)
            {
                Log.Error("Exception rendering " + code + " error page.");
                Log.Error(e);
            }
        }

        /// <summary>
        /// After
        /// </summary>
        public bool After(IRequest request, IResponse response)
        {
            return false;
        }

        /// <summary>
        /// Finally
        /// </summary>
        public bool Finally(IRequest request, IResponse response)
        {
            Log.Debug("End Request: url=" + request.Url);
            return false;
        }
	}
}
