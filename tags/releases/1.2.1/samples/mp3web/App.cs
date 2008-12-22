using System;
using System.IO;
using System.Xml;

using Glue.Data;
using Glue.Data.Providers.SQLite;
using Glue.Lib;
using Glue.Web;

namespace mp3web
{
    public class App : Glue.Web.App
    {
        public string DbPath; // Path to database file
        public IDataProvider Provider;

        public static new App Current
        {
            get { return (App)Glue.Web.App.Current; }
        }

        protected App(XmlNode node)
            : base(node)
        {
            Routing.Add("^/?$", Helper.Bag("controller", "base", "action", "index"));
            Provider = (IDataProvider)Configuration.Get("dataprovider");

            mp3sql.DataContext dc = new mp3sql.DataContext(Provider);
            dc.CreateTables();

            //DbPath = Path.GetFullPath(
            //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mp3sql/sqlite.db"));
            //DbPath = "c:/mp3.db";
            //string connectionstring = "Data Source=" + DbPath;
            //if (!File.Exists(DbPath))
            //    connectionstring += ";New=True";
            //Provider = new SQLiteDataProvider(connectionstring);
        }
    }
}