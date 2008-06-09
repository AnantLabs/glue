using System;
using System.Collections.Generic;
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

            //PyTemplate.CompileTest();
        }

        public void Info()
        {
            Render("info.html");
        }

        public void Python()
        {
            Dictionary<string, object> variables = new Dictionary<string,object>();
            variables["title"] = "MyTitle";

            PyTemplate.Render(App.Current.MapPath("views/base/python.html"), variables, Response.Output);
        }
    }
}