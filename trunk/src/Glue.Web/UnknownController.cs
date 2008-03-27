using System;

namespace Glue.Web
{
    /// <summary>
    /// UnknownController.
    /// </summary>
    public class UnknownController : Controller
    {
        public UnknownController(IRequest request, IResponse response) : base(request, response)
        {
        }
        
        protected internal override void Execute()
        {
            throw new GlueNotFoundException("No controller found for: " + Request.Url);
        }
    }
}
