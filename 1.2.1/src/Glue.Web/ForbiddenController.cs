using System;

namespace Glue.Web
{
	/// <summary>
	/// ForbiddenController.
	/// </summary>
	public class ForbiddenController : Controller
	{
        public ForbiddenController(IRequest request, IResponse response) : base(request, response)
		{
		}
        
        protected internal override void Execute()
        {
            throw new GlueForbiddenException("Forbidden.");
        }
	}
}
