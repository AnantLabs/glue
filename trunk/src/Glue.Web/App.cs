using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Reflection;
using Glue.Lib;

namespace Glue.Web 
{
    /// <summary>
    /// Application object for the Glue.Web engine. Reads configuration from
    /// the "application" key in Glue.Lib.Configuration.
    /// </summary>
    public class App 
    {
        protected string _baseDirectory;
        string _productDirectory;
        string _binDirectory;
        string _tempDirectory;
        Type   _defaultControllerType = typeof(UnknownController);
        Type   _baseViewType = typeof(View);
        Type   _helperType = typeof(Helper);
        string _errorPage = "";
        bool _offline = false;
        string _offlinePage = "";

        /// <summary>
        /// Current application instance, will be constructed with parameters
        /// passed from configuration file.
        /// </summary>
        public static App Current 
        {
            get { return Configuration.Get("application", typeof(App)) as App; }
        }

        /// <summary>
        /// Request for the current thread.
        /// </summary>
        [ThreadStatic]
        public static IRequest Request = null;

        /// <summary>
        /// Response for the current thread.
        /// </summary>
        [ThreadStatic]
        public static IResponse Response = null;

        /// <summary>
        /// Routing of URL's to controllers and actions.
        /// </summary>
        public readonly Routing Routing = new Routing();

        /// <summary>
        /// Turn on or off detailed errors
        /// </summary>
        public readonly bool Debug = false;

        /// <summary>
        /// Turn on or off tracing
        /// </summary>
        public readonly bool Trace = false;

        /// <summary>
        /// Modules
        /// </summary>
        public readonly IModule[] Modules; 
/*      {
            new Modules.Common(), 
            new Modules.Logging(),
if DEBUG
            new Modules.Debug(),
endif
            null
        };
*/

        /// <summary>
        /// Application base directory.
        /// </summary>
        public virtual string BaseDirectory 
        {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        /// <summary>
        /// Directory for binaries, normally [BaseDirectory]/bin
        /// </summary>
        public virtual string BinDirectory 
        {
            get { return _binDirectory; }
        }

        /// <summary>
        /// Directory for temporary files.
        /// </summary>
        public virtual string TempDirectory 
        {
            get { return _tempDirectory; }
        }

        /// <summary>
        /// Base class for views. Views which do not explicitly specify a different base class
        /// will be derived from this one.
        /// </summary>
        public Type BaseViewType 
        {
            get { return _baseViewType; }
        }

        /// <summary>
        /// Default controller, the root of the site ("/") maps to this one.
        /// </summary>
        public Type DefaultControllerType 
        {
            get { return _defaultControllerType; }
        }

        /// <summary>
        /// Type of Helper class.
        /// </summary>
        public Type HelperType 
        {
            get { return _helperType; }
        }

        /// <summary>
        /// Returns reference to system cache.
        /// </summary>
        public System.Web.Caching.Cache Cache 
        { 
            get { return System.Web.HttpRuntime.Cache; } 
        }

        /// <summary>
        /// Path to user-friendly error page.
        /// </summary>
        public string ErrorPage 
        { 
            get { return _errorPage; } 
        }

        /// <summary>
        /// Offline mode.
        /// </summary>
        public bool Offline 
        { 
            get { return _offline; } 
        }

        /// <summary>
        /// Path to offline page.
        /// </summary>
        public string OfflinePage 
        { 
            get { return _offlinePage; } 
        }

        public string DateFormat = "yyyy-MM-dd";
        public string TimeFormat = "HH:mm";
        public string DateTimeFormat = "yyyy-MM-dd HH:mm";

        public Version Version
        {
            get { return this.GetType().Assembly.GetName().Version; }
        }

        /// <summary>
        /// Private default constructor to prevent standalone instantiation.
        /// </summary>
        private App() : this(null) 
        {
        }

        /// <summary>
        /// Construct an App object instance from configuration data.
        /// </summary>
        protected App(XmlNode node) 
        {
            Type type;
            string ns = Path.GetFileNameWithoutExtension(GetType().FullName);

            Debug = Configuration.GetAttrBool(node, "debug", Debug);
            Trace = Configuration.GetAttrBool(node, "trace", Trace);
            
            type = Configuration.FindType(ns + ".DefaultController");
            if (type == null)
                type = Configuration.FindType(ns + ".Controllers.DefaultController");
            if (type == null)
                type = typeof(Glue.Web.UnknownController);
            _defaultControllerType = type;
            
            type = Configuration.FindType(ns + ".Helper");
            if (type == null)
                type = typeof(Glue.Web.Helper);
            _helperType = type;
            
            _baseDirectory         = Configuration.GetAttrPath(node, "baseDirectory", AppDomain.CurrentDomain.BaseDirectory, "");
            if (_baseDirectory.EndsWith("\\"))
                _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.Length - 1);
            if (_baseDirectory.EndsWith("\\bin"))
                _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.Length - 4);
            if (_baseDirectory.EndsWith("\\public"))
                _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.Length - 7);

            // Get product directory
            string s = Path.GetDirectoryName(_baseDirectory);
            if (Path.GetFileName(s) == "web")
                _productDirectory = Path.GetDirectoryName(s);
            else
                _productDirectory = s;

            _binDirectory          = Configuration.GetAttrPath(node, "binDirectory", _baseDirectory, "bin");
            _tempDirectory         = Configuration.GetAttrPath(node, "tempDirectory", _baseDirectory, "temp");
            _defaultControllerType = Configuration.GetAttrType(node, "defaultControllerType", _defaultControllerType);
            _baseViewType          = Configuration.GetAttrType(node, "baseViewType", _baseViewType);
            _helperType            = Configuration.GetAttrType(node, "helperType", _helperType);
            _errorPage             = Configuration.GetAttr(node, "errorPage", _errorPage, true);
            _offline               = Configuration.GetAttrBool(node, "offline", _offline);
            _offlinePage           = Configuration.GetAttr(node, "offlinePage", _errorPage, true);

            // Initialize modules
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            // Add common module
            list.Add(new Modules.Common(Debug, Trace));
            foreach (XmlElement child in Configuration.GetAddRemoveList(node, "modules", null))
            {
                list.Add((IModule)Configuration.GetAttrInstance(child, "type", "Glue.Web.Modules", null));
            }
            Modules = (IModule[])list.ToArray(typeof(IModule));

            // Add default routes
            Routing.Add(@"^/(?<controller>[^/]+)/(?<action>[^/]+)/(?<id>[^/]+)/?", null);
            Routing.Add(@"^/(?<controller>[^/]+)/(?<action>[^/]+)/?", null);
            Routing.Add(@"^/(?<controller>[^/]+)/?", Helper.Bag("action", "index"));
        }

        /// <summary>
        /// Process an in-memory request, and return the response in a string.
        /// </summary>
        public string Process(string url) 
        {
            StringWriter writer = new StringWriter();
            Process(
                new Hosting.Memory.Request(url), 
                new Hosting.Memory.Response(writer)
                );
            return writer.ToString();
        }

        /// <summary>
        /// Response to a client request. The Process method is the central
        /// function in the Web application framework. It performs the
        /// following steps:
        /// * Call RewriteUrl to give a derived class the opportunity to 
        ///   change the URL.
        /// * Split up a URL into a 'noun' and a 'verb' (e.g. /document/view/10 => document,view) 
        /// * Try to instantiate a corresponding Controller (e.g. DocumentController)
        /// * Call Controller::Intialize 
        /// * Call Controller::Execute; this will in turn invoke a suitable public 
        ///   method on the controller (e.g. DocumentController::View)
        /// * Call Controller::Finalize 
        /// </summary>
        public virtual void Process(IRequest request, IResponse response) 
        {
            IRequest saveRequest = App.Request;
            IResponse saveResponse = App.Response;
            try 
            {
                App.Request = request;
                App.Response = response;

                // Walk all modules
                foreach (IModule module in Modules)
                    if (module != null && module.Before(request, response))
                        return;

                // Process path, obtain controller, action etc.
                ProcessUrl(request);

                // Map to controller
                Type type = FindControllerType(request.Params["controller"]);
                if (type == null)
                    throw new GlueNotFoundException("Cannot find controller.");

                // Instantiate the controller
                Controller controller = Activator.CreateInstance(type, new object[] {request, response}) as Controller;

                // Walk the modules again
                foreach (IModule module in Modules)
                    if (module != null && module.Process(request, response, type))
                        return;

                // Invoke controller
                if (controller.Initialize()) 
                {
                    try 
                    { 
                        controller.Execute(); 
                    }
                    catch (Exception e)
                    {
                        while (e is System.Reflection.TargetInvocationException && e.InnerException != null)
                            e = e.InnerException;
                        if (!controller.HandleError(e))
                            throw;
                    }
                    finally
                    {
                        controller.Finalize();
                    }
                }

                foreach (IModule module in Modules)
                    if (module != null && module.After(request, response))
                        return;
            }
            catch (Exception e)
            {
                // Let modules handle the exception if
                foreach (IModule module in Modules)
                    if (module != null && module.Error(request, response, e))
                        return;
                throw;
            }
            finally 
            {
                foreach (IModule module in Modules)
                    if (module != null)
                        module.Finally(request, response);
                
                App.Request = saveRequest;
                App.Response = saveResponse;
            }
        }

        /// <summary>
        /// Derived classes can rewrite the Request's URL before Web finds and
        /// invokes a Controller.
        /// </summary>
        public virtual void ProcessUrl(IRequest request) 
        {
            foreach (Route route in Routing)
                if (route.IsMatch(request))
                    return;
        }

        /// <summary>
        /// Search assemblies in current AppDomain for a Controller class with 
        /// specified name. Return DefaultControllerType if no controller type
        /// can be found.
        /// </summary>
        public Type FindControllerType(string name) 
        {
            if (name == null || name.Length == 0)
                return DefaultControllerType;

            name += "Controller";

            Type result = (Type)Cache["#Glue.Web#ControllerType#" + name];
            if (result != null)
                return result;

            result = Configuration.SearchType(name, typeof(Controller), true);
            if (result == null)
                result = DefaultControllerType;

            Cache["#Glue.Web#ControllerType#" + name] = result;
            return result;
        }
        
        /// <summary>
        /// Maps a internal application path to a fysical path.
        /// </summary>
        public virtual string MapPath(string virtualPath) 
        {
            if (virtualPath != null) 
            {
                virtualPath = virtualPath.Replace('/', '\\');
                if (virtualPath.Length > 0 && virtualPath[0] == '\\')
                    virtualPath = virtualPath.Substring(1);
            }
            else
                virtualPath = "";
            return Path.Combine(BaseDirectory, virtualPath);
        }

        /// <summary>
        /// Clean up temporary directory.
        /// </summary>
        public void CleanTempDirectory() 
        {
            try 
            {
                foreach (string file in System.IO.Directory.GetFiles(TempDirectory, "*"))
                    System.IO.File.Delete(file); 

                foreach (string sub in System.IO.Directory.GetDirectories(TempDirectory, "*"))
                    if (System.IO.Path.GetFileName(sub).ToLower() != ".svn")
                        System.IO.Directory.Delete(sub);
            }
            catch 
            {
            }
        }

        public byte[] ReadBinary(string virtualPath) 
        {
            throw new NotImplementedException("ReadBinary not yet implemented.");
        }

        public int ReadBinary(string virtualPath, byte[] data, int offset, int length) 
        {
            throw new NotImplementedException("ReadBinary not yet implemented.");
        }

        public string ReadText(string virtualPath) 
        {
            throw new NotImplementedException("ReadText not yet implemented.");
        }

        public void WriteBinary(string virtualPath, byte[] data, int offset, int length) 
        {
            throw new NotImplementedException("WriteBinary not yet implemented.");
        }
        
        public void WriteText(string virtualPath, string data) 
        {
            WriteText(virtualPath, data, Encoding.UTF8);
        }

        public void WriteText(string virtualPath, string data, Encoding encoding) 
        {
            throw new NotImplementedException("WriteText not yet implemented.");
        }

        public virtual string GetContentType(string virtualPath) 
        {
            return Glue.Lib.Mime.MimeMapping.GetMimeMapping(virtualPath);
        }
    }
}
