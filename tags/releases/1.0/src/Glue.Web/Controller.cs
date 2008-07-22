using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.ComponentModel;
using System.Reflection;
using Glue.Lib;
using Glue.Lib.Text;
using Glue.Lib.Compilation;

namespace Glue.Web
{
	/// <summary>
    /// You should override this class to create a controller for a Glue web application.
    /// Glue web applications parse the request URL to find a controller. It then invokes its 'Initialize',
    /// 'Execute', and 'Close' methods. You can optionally override Initialize() and Close().
    /// The Execute method then looks up which "action" to perform.
    /// Actions are public methods on your controller that do something - typically rendering a view by calling
    /// the Render() method.
    /// If you need to add a public method to your controller, but do not want it to be available as an action, use
    /// the [Forbidden] attribute.
    /// See also: App.Process(), Controller.Execute()
	/// </summary>
	public class Controller
	{
        public IRequest Request;
        public IResponse Response;
        public App App;
        public CombinedException Errors = new CombinedException();
        private IDictionary _parameters;

        private Controller()
        {
        }

        public Controller(IRequest request, IResponse response) 
        {
            this.App = App.Current;
            this.Request = request;
            this.Response = response;
        }

        /// <summary>
        /// Session state
        /// </summary>
        public ISession Session
        {
            get { return Request.Session; }
        }

        /// <summary>
        /// Hierarchical dictionary of Params.
        /// </summary>
        public IDictionary Parameters
        {
            get 
            {
                if (_parameters == null)
                {
                    _parameters = CollectionHelper.ToBag(Request.Params);
                    for (int i = 0; i < Request.Files.Count; i++)
                        AddToBag(_parameters, Request.Files[i].Name, Request.Files[i]);
                    // TODO: Add cookies
                }
                return _parameters;
            }
        }

        private void AddToBag(IDictionary bag, string name, object item)
        {
            string[] splits = name.Split('.');
            int i = 0;
            while (i < splits.Length - 1)
            {
                IDictionary sub = bag[splits[i]] as IDictionary;
                if (sub == null)
                    bag[splits[i]] = sub = new HybridDictionary();
                bag = sub;
                i++;
            }
            bag[splits[i]] = item;
        }

        /// <summary>
        /// Shortcut for Request.Params
        /// </summary>
        public NameValueCollection Params 
        { 
            get { return Request.Params; } 
        }
        
        /// <summary>
        /// Shortcut for Request.QueryString
        /// </summary>
        public NameValueCollection QueryString 
        { 
            get { return Request.QueryString; } 
        }
        
        /// <summary>
        /// Shortcut for Request.Form
        /// </summary>
        public NameValueCollection Form 
        { 
            get { return Request.Form; } 
        }

        /// <summary>
        /// Indicates this is a POST request
        /// </summary>
        public bool IsPostBack
        {
            get { return string.Compare(Request.Method, "POST") == 0; }
        }

        protected internal virtual bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// Process the request. The Execute methods maps parameters
        /// from the request to the controller's properties by calling
        /// AssignProperties and invokes a user defined method on the controller:
        ///  * calls GetMethodName to extract the name of the method from the URL.
        ///  * calls AssignParameters to map request parameters to parameters on 
        ///    the method.
        /// </summary>
        protected internal virtual void Execute()
        {
            // Assign request values to properties and fields.
            try { Mapper.Assign(this, Parameters); }
            catch {}
            
            // Get action
            string action = (string)Parameters["action"];
            if (action == null)
                action = "default";

            // Get method corresponding to action
            MethodInfo method = GetType().GetMethod(action, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (method == null || 
                method.GetCustomAttributes(typeof(ForbiddenAttribute), true).Length > 0)
                method = GetType().GetMethod("unknown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            // Assign request values to parameters for this method.
            Mapper.Invoke(this, method, Parameters); 
        }

        protected internal virtual bool HandleError(Exception e)
        {
            return false;
        }

        protected internal virtual void Close()
        {
        }

#line hidden
        protected void Assert(bool test)
        {
            if (test == false)
                throw new GlueException("Failed");
        }
#line default

#line hidden
        protected void Assert(object o)
        {
            Assert(o != null);
            if (o is Boolean)
                Assert((Boolean)o);
            else if (o is String)
                Assert(((string)o).Length > 0);
            else if (o is Guid)
                Assert(((Guid)o) != Guid.Empty);
            else if (o is DateTime)
                Assert(((DateTime)o) != DateTime.MinValue);
        }
#line default

        public virtual void Default()
        {
            Unknown();
        }

        protected virtual void Unknown()
        {
            throw new GlueNotFoundException("No action found on " + GetType() + " for: " + Request.Url);
        }

        protected void Redirect(string url)
        {
            Response.Clear();
            Response.StatusCode = 302;
            Response.RedirectLocation = Request.Root + url;
        }

        /// <summary>
        /// Transmit contents of a file with a virtualPath. Used for file downloads.
        /// </summary>
        protected void Transmit(string virtualPath)
        {
            Glue.Lib.Log.Debug("Controller.Transmit: " + App.Current.MapPath(virtualPath) + " (" + App.GetContentType(virtualPath) + ")");
            Response.ContentType = App.GetContentType(virtualPath);
            Response.TransmitFile(App.Current.MapPath(virtualPath));
        }

        [Forbidden]
        public View GetView(string virtualPath)
        {
            return ViewCompiler.GetCompiledInstance(this, virtualPath);
        }

        /// <summary>
        /// Renders a view based on the current action and controller. For 
        /// information on views and templates see ViewCompiler.
        /// </summary>
        [Forbidden]
        public void Render()
        {
            Render(null);
        }

        /// <summary>
        /// Renders a view template. For information on views and templates
        /// see ViewCompiler.
        /// </summary>
        [Forbidden]
        public void Render(string virtualPath)
        {
            if (virtualPath == null || virtualPath.Length == 0 || virtualPath[0] != '/')
            {
                string entity = Params["controller"];
                string action = Params["action"];
                if (virtualPath == null || virtualPath.Length == 0)
                    virtualPath = "/views/" + entity + "/" + action;
                else
                    virtualPath = "/views/" + entity + "/" + virtualPath;
                if (!Path.HasExtension(virtualPath))
                    virtualPath += ".html";
            }
            Render(virtualPath, Response.Output);
        }

        /// <summary>
        /// Renders a view template and writes output to a TextWriter. 
        /// For information on views and templates see ViewCompiler.
        /// </summary>
        [Forbidden]
        public void Render(string virtualPath, TextWriter writer)
        {
            View view = ViewCompiler.GetCompiledInstance(this, virtualPath);
            writer.WriteLine();
            //if (Log.Level >= Level.Debug)
            //    writer.WriteLine("<!-- [" + virtualPath + "> -->");
            Log.Debug("Render: " + virtualPath);
            view.Render(writer);
            writer.WriteLine();
            //if (Log.Level >= Level.Debug)
            //    writer.WriteLine("<!-- <" + virtualPath + "] -->");
            writer.WriteLine();
        }

        /// <summary>
        /// Renders a view template and wirtes output to a string. 
        /// For information on views and templates see ViewCompiler.
        /// </summary>
        [Forbidden]
        public string RenderToString(string virtualPath)
        {
            View view = ViewCompiler.GetCompiledInstance(this, virtualPath);
            StringWriter writer = new StringWriter();
            writer.WriteLine();
            //if (Log.Level >= Level.Debug)
            //    writer.WriteLine("<!-- [" + virtualPath + "> -->");
            Log.Debug("RenderToString: " + virtualPath);
            view.Render(writer);
            writer.WriteLine();
            //if (Log.Level >= Level.Debug)
            //    writer.WriteLine("<!-- <" + virtualPath + "] -->");
            return writer.ToString();
        }

        [Forbidden]
        public StringTemplate Template(string virtualPath)
        {
            string path = App.Current.MapPath(virtualPath);
            Type controllerType = this.GetType();
            string key  = "#Glue.Web#ViewStringTemplate#" + controllerType.FullName + "#" + path + "#";
            Type   type = (Type)App.Cache[key];
            if (type == null)
            {
                type = StringTemplate.CompileFromFile(
                    path, 
                    controllerType,
                    App.HelperType
                    );
                App.Cache.Insert(key, type, new System.Web.Caching.CacheDependency(path));
            }
            return StringTemplate.Instantiate(type, this, null);
        }
    }
}

