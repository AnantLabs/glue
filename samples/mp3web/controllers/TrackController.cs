using System;
using Glue.Web;

namespace mp3web.Controllers
{
    public class TrackController : BaseController
    {
        public TrackController(IRequest request, IResponse response)
            : base(request, response)
        {
        }

        public void List()
        {
            Render("/views/track/list.html");
        }
    }
}
