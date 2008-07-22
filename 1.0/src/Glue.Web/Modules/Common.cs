using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.IO;
using Glue.Lib;
using Glue.Web;

namespace Glue.Web.Modules
{
	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class Common : LogAppender, IModule
	{
        bool debug = false;
        bool trace = false;
        // Map to look up trace content by thread-id.
        IDictionary trace_content = new HybridDictionary();

        public Common(bool debug, bool trace)
        {
            this.debug = debug;
            this.trace = trace;
            if (trace)
                Glue.Lib.Log.Instance.AddAppender(this);
        }

        /// <summary>
        /// Before
        /// </summary>
        public bool Before(IRequest request, IResponse response)
        {
            // Log request
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
                    throw new GlueServiceUnavailableException();
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

            // Clear trace contents
            if (trace)
            {
                trace_content[Thread.CurrentThread] = null;
            }
            
            return false;
        }

        /// <summary>
        /// Process
        /// </summary>
        public bool Process(IRequest request, IResponse response, Type controller)
        {
            if (debug)
            { 
                if (request.Params[null] == "precompile") 
                {
                    PrecompileViews(request, response);
                    return true;
                }
                if (request.Params[null] == "help") 
                {
                    response.Write(Helper.FormatHtmlHelp(controller));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Error
        /// </summary>
        public bool Error(IRequest request, IResponse response, Exception exception)
        {
            // Log errors
            Exception e = exception;
            while (e != null) 
            {
                if (!(e is System.Reflection.TargetInvocationException))
                    Log.Error(e);
                exception = e;
                e = e.InnerException;
            }
            
            // Translate to HTTP status code
            int code = 500;
            if (exception is GlueNotFoundException || exception is System.IO.FileNotFoundException || exception is System.IO.DirectoryNotFoundException)
                code = 404;
            else if (exception is GlueUnauthorizedException)
                code = 401;
            else if (exception is GlueForbiddenException)
                code = 403;
            else if (exception is GlueServiceUnavailableException)
                code = 503;
            
            if (!debug)
            {
                response.Clear();
                response.ContentType = "text/html";
                response.StatusCode = code;
                string error = FormatNormalError(exception, code);
                if (error == null)
                    error = "" + response.StatusCode + ": " + response.StatusDescription;
                response.Write(error);
            }
            else
            {
                response.Clear();
                response.ContentType = "text/html";
                response.StatusCode = code;
                response.Write(FormatDebugError(exception));
            }
            return false;
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
            if (trace)
            {
                response.Write(FormatTrace(request,response));
                trace_content[Thread.CurrentThread] = null;
            }
            return false;
        }
        
        /// <summary>
        /// LogAppender method
        /// </summary>
        public override void Write(Level level, string s)
        {
            string content = (string)trace_content[Thread.CurrentThread];
            content += Helper.HtmlEncode(s) + "\r\n";
            trace_content[Thread.CurrentThread] = content;
        }

        // Private methods

        string FormatNormalError(Exception exception, int code)
        {
            string path = App.Current.MapPath("/views/" + code + ".html");
            if (!File.Exists(path))
            {
                if (code == 500)
                    return null;
                if (code == 400)
                    return FormatNormalError(exception, 500);
                return FormatNormalError(exception, 400);
            }
            try 
            {
                TextWriter writer = new StringWriter();
                View view = ViewCompiler.GetCompiledInstance(null, path);
                view.Render(writer);
                return writer.ToString();
            }
            catch (Exception e)
            {
                Log.Error("Exception rendering " + code + " error page.");
                Log.Error(e);
                return null;
            }
        }

        string FormatDebugError(Exception exception)
        {
            return Helper.FormatHtmlError(exception);
        }

        string FormatTrace(IRequest request, IResponse response)
        {
            // Append trace contents
            string content = @"<html>
<style>
#trace         { background: #fff; color: #000; text-align: left; }
#trace p,
#trace td,
#trace th      { font-family: verdana, arial, helvetica; font-size:70%; }
#trace pre     { font-family: lucida console; font-weight: normal; margin-top: 0px; margin-left: 20px; background-color: #f3f0e8; font-size:100%; }
#trace code    { font-weight: 400; font-family: ""courier new""; } 
#trace em      { color: red; font-style: normal; }
#trace h1      { font-family: verdana, helvetica; font-weight: normal; font-size: 165%; }
#trace h2      { font-family: arial, helvetica; font-weight: bold; font-size: 124%; }
</style>
<div id=""trace"">
";
            content += "<h2>Log</h2><pre>" + (string)trace_content[Thread.CurrentThread] + "</pre>";
            content += "<h2>Request</h2><pre>" + Dump(request) + "</pre>";
            content += "<h3>Request.QueryString</h3><pre>" + Dump(request.QueryString) + "</pre>";
            content += "<h3>Request.Form</h3><pre>" + Dump(request.Form) + "</pre>";
            content += "<h3>Request.Params</h3><pre>" + Dump(request.Params) + "</pre>";
            content += @"
</div>
</html>";
            return content;
        }

        string Dump(IRequest req)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("Path:       ").Append(req.Path).Append("\r\n");
            s.Append("Method:     ").Append(req.Method).Append("\r\n");
            s.Append("Controller: ").Append(req.Params["controller"]).Append("\r\n");
            s.Append("Action:     ").Append(req.Params["action"]).Append("\r\n");
            return s.ToString();
        }

        string Dump(NameValueCollection bag)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (string key in bag.Keys)
                s.Append(key).Append(": ").Append(bag[key]).Append("\r\n");
            return s.ToString();
        }

        void PrecompileViews(IRequest request, IResponse response) 
        {
            bool errors = false;
            foreach (string dir in Directory.GetDirectories(App.Current.MapPath("/views"))) 
            {
                string entity = Path.GetFileName(dir);
                Type controllerType = App.Current.FindControllerType(entity);
                if (controllerType == null || controllerType == App.Current.DefaultControllerType)
                    continue;
                foreach (string path in Directory.GetFiles(dir)) 
                {
                    if (Path.GetFileName(path).StartsWith("--"))
                        continue;
                    string virtualPath = "/views/" + entity + "/" + Path.GetFileName(path);
                    try 
                    {
                        Type viewType = ViewCompiler.GetCompiledType(controllerType, virtualPath);
                    }
                    catch (Exception e) 
                    {
                        errors = true;
                        response.Write(Helper.FormatHtmlError(e));
                    }
                }
            }
            if (!errors)
                response.Write(Helper.FormatHtmlError(new Exception("No errors found.")));
        }
    }
}
