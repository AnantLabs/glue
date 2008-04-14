using System;
using System.Xml;

using Glue.Lib;
using Glue.Web;

namespace mp3web
{
    public class App : Glue.Web.App
    {
        public static new App Current
        {
            get { return (App)Glue.Web.App.Current; }
        }

        public string SiteRoot;

        protected App(XmlNode node)
            : base(node)
        {
            SiteRoot = Configuration.GetAttr(node, "siteRoot", "").TrimEnd('/');
            Routing.Clear();
            Routing.Add(@"^/(?<controller>[^/]+)/(?<action>[^/]+)/(?<id>[^/]+)/?", null);
            Routing.Add(@"^/(?<controller>[^/]+)/(?<action>[^/]+)/?", null);
            Routing.Add(@"^/(?<controller>[^/]+)/", Helper.Bag("action", "index"));
            Routing.Add("^/?$", Helper.Bag("controller", "base", "action", "index"));
        }

        public override void ProcessUrl(IRequest request)
        {
            base.ProcessUrl(request);
        }
    }
}