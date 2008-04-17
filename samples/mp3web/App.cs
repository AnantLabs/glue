using System;
using System.Xml;

using Glue.Data;
using Glue.Lib;
using Glue.Web;

namespace mp3web
{
    public class App : Glue.Web.App
    {
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
        }
    }
}