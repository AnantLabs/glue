using System;

using Glue.Web;
using mp3sql;

namespace mp3web.Controllers
{
    public class BaseController : Glue.Web.Controller
    {
        public new App App;

        public BaseController(IRequest request, IResponse response)
            : base(request, response)
        {
            App = (App)App.Current;
        }

        public void Index()
        {
            Render("index.html");
        }

        public void Info()
        {
            Render("info.html");
        }

        public void Python()
        {
            PyTemplate template = new Glue.Web.PyTemplate(App.Current.MapPath("views/base/python.html"));
            template.Compile();
            template.Variables["title"] = "TheTitle";
            template.Render(Response.Output);
        }
    }
}