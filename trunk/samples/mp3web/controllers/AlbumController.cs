using System;
using Glue.Web;

namespace mp3web.Controllers
{
    public class AlbumController : BaseController
    {
        public AlbumController(IRequest request, IResponse response)
            : base(request, response)
        {
        }

        public void List()
        {
            Render("/views/album/list.html");
        }
    }
}
