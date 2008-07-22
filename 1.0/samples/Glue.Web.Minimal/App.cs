using System;
using System.Xml;
using Glue.Lib;

namespace Glue.Web.Minimal
{
    public class App : Glue.Web.App
    {
        public readonly string Title;

        public static new App Current
        {
            get { return (App)Glue.Web.App.Current; }
        }

        protected App(XmlNode node)
            : base(node)
        {
            Routing.Add("^/?$", Helper.Bag("controller", "base", "action", "index"));
            Title = Configuration.GetAttr(node, "title", "(Unknown)");
        }

        public override void ProcessUrl(IRequest request)
        {
            base.ProcessUrl(request);
        }
    }


}
