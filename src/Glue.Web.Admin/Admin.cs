using System;
using System.Reflection;
using System.Xml;

using Glue.Data;
using Glue.Lib;

namespace Glue.Web
{
    public class Admin
    {
        // Configuration values set in constructor:
        protected string RootDirectory;
        protected string AppAssemblyFile;
        protected string AppType;
        protected string WebConfigFile;
        protected string UpdateUrl;

        protected string DataproviderElementName = "dataprovider"; // default value, can be overridden

        XmlDocument _webconfigXml;
        protected XmlDocument WebConfigXml
        {
            get
            {
                if (_webconfigXml == null)
                {
                    _webconfigXml = new XmlDocument();
                    _webconfigXml.Load(WebConfigFile);
                }
                return _webconfigXml;
            }
        }

        IDataProvider _dataprovider;
        protected IDataProvider DataProvider
        {
            get
            {
                if (_dataprovider == null) _dataprovider = (IDataProvider)Configuration.Get(DataproviderElementName);
                return _dataprovider;
            }
        }

        Assembly _appAssembly;
        protected Assembly AppAssembly
        {
            get
            {
                if (_appAssembly == null) _appAssembly = Assembly.LoadFile(AppAssemblyFile);
                return _appAssembly;
            }
        }

        protected Admin(string rootDirectory, string webConfigFile, string appAssemblyFile, string appType, string updateUrl)
        {
            RootDirectory = rootDirectory;
            WebConfigFile = webConfigFile;
            AppAssemblyFile = appAssemblyFile;
            AppType = appType;
            UpdateUrl = updateUrl;

            if (WebConfigFile != null) Configuration.Register(WebConfigFile, false); // load web.config into Glue configuration
        }

        /// <summary>
        /// Read database schema version from database. override?
        /// </summary>
        protected int CurrentSchemaVersion 
        { 
            get { return DataProvider.ExecuteScalarInt32("SELECT public.version()"); }
        }

        protected int CurrentConfigVersion
        {
            get 
            {
                XmlNode node = WebConfigXml.SelectSingleNode("/configuration/appSettings/add[@key='configversion']");
                if (node != null) return int.Parse(node.Attributes["value"].Value);
                return 0;
            }
        }

        protected bool IsOnline
        {
            get { return (null != WebConfigXml.SelectSingleNode("/configuration/system.web/httpHandlers/add[contains(@type,'OfflineHandler')]")); }
        }

        protected int AppVersion
        { 
            get { return (int)Assembly.LoadFile(AppAssemblyFile).GetType(AppType).GetField("AppVersion").GetValue(null); }
        }

        protected int AppSchemaVersion
        {
            get { return (int)Assembly.LoadFile(AppAssemblyFile).GetType(AppType).GetField("AppSchemaVersion").GetValue(null); }
        }

        protected int AppConfigVersion
        {
            get { return (int)Assembly.LoadFile(AppAssemblyFile).GetType(AppType).GetField("AppConfigVersion").GetValue(null); }
        }

        protected void Status()
        {
            Console.WriteLine("Dll version: {0}", AppVersion);
            Console.WriteLine("Config version: {0}. Expected: {1}", CurrentConfigVersion, AppConfigVersion);
            Console.WriteLine("Schema version: {0}. Expected: {1}", CurrentSchemaVersion, AppSchemaVersion);
            Console.WriteLine("Status: {0}", IsOnline? "Online" : "Offline");
        }
    }
}
