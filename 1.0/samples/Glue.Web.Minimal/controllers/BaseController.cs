using System;
using Glue.Lib;
using Glue.Web;

namespace Glue.Web.Minimal.Controllers
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
            Render("/views/index.html");
        }
    }
}
