using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using Glue.Data;
using Glue.Lib;

namespace web
{
    public class Global : System.Web.HttpApplication
    {
        // Edit the value for the Application version in default.build
        public static int AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build;
        public static int AppSchemaVersion = 2;
        public static int AppConfigVersion = 1;
        public static IDataProvider _dataprovider;

        public static IDataProvider dataprovider
        {
            get
            {
                return _dataprovider ?? (IDataProvider)Configuration.Get("dataprovider");
            }
        }

        public static int SchemaVersion
        {
            get { return dataprovider.ExecuteScalarInt32("SELECT public.version();"); }
        }

        public static int ConfigVersion
        {
            get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["configversion"] ?? "0"); }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            if (AppSchemaVersion != SchemaVersion) throw new InvalidVersionException("schema", SchemaVersion, AppSchemaVersion);
            if (AppConfigVersion != ConfigVersion) throw new InvalidVersionException("config", ConfigVersion, AppConfigVersion);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}