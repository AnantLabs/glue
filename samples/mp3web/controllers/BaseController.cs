using System;

using Glue.Web;

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
            Response.Write("Nothing to see here.");
        }
    }
}