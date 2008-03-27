using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.IO;
using Edf.Lib;
using Edf.Glue;

namespace Edf.Glue.Modules
{
	/// <summary>
	/// Summary description for Debug.
	/// </summary>
	public class DebugOBSOLETE : LogAppender, IModule
	{
        bool tracing = false;

        public DebugOBSOLETE(System.Xml.XmlNode config)
        {
            tracing = Configuration.GetAttrBool(config, "tracing", tracing);
            if (tracing)
                Edf.Lib.Log.Instance.AddAppender(this);
        }

        public bool Before(IRequest request, IResponse response)
        {
            throw new GlueException("Debug module not longer supported. Remove from web.config.");
            
            // Clear trace contents
            if (tracing)
                trace[Thread.CurrentThread] = null;

            // Precompile views
            if (request.Params[null] == "precompile") 
            {
                PrecompileViews(request, response);
                return true;
            }
            return false;
        }

        public bool Process(IRequest request, IResponse response, Type controller)
        {
            if (request.Params[null] == "help") 
            {
                response.Write(Helper.FormatHtmlHelp(controller));
                return true;
            }
            return false;
        }

        public bool After(IRequest request, IResponse response)
        {
            return false;
        }

        public bool Error(IRequest request, IResponse response, Exception exception)
        {
            response.Clear();
            response.ContentType = "text/html";
            response.Write(FormatError(exception));
            response.StatusCode = 500;
            return true;
        }

        public bool Finally(IRequest request, IResponse response)
        {
            if (!tracing)
                return false;
            response.Write(FormatTrace(request,response));
            trace[Thread.CurrentThread] = null;
            return false;
        }

        private string FormatError(Exception exception)
        {
            return Helper.FormatHtmlError(exception);
        }

        private string FormatTrace(IRequest request, IResponse response)
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
            content += "<h2>Log</h2><pre>" + (string)trace[Thread.CurrentThread] + "</pre>";
            content += "<h2>Request</h2><pre>" + Dump(request) + "</pre>";
            content += "<h3>Request.QueryString</h3><pre>" + Dump(request.QueryString) + "</pre>";
            content += "<h3>Request.Form</h3><pre>" + Dump(request.Form) + "</pre>";
            content += "<h3>Request.Params</h3><pre>" + Dump(request.Params) + "</pre>";
            content += @"
</div>
</html>";
            return content;
        }

        /// <summary>
        /// Map to look up trace content by thread-id.
        /// </summary>
        private IDictionary trace = new HybridDictionary();

        /// <summary>
        /// LogAppender method
        /// </summary>
        public override void Write(string s)
        {
            string content = (string)trace[Thread.CurrentThread];
            content += Helper.HtmlEncode(s) + "\r\n";
            trace[Thread.CurrentThread] = content;
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

        /// <summary>
        /// Helper to precompile views
        /// </summary>
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
