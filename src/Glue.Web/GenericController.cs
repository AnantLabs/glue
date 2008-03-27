//----------------------------------------------------------------------------
// Copyright (C) 2004-2005 Electric Dream Factory. All rights reserved.
// http://www.edf.nl
//
// You must not remove this notice, or any other, from this software.
//----------------------------------------------------------------------------
using System;
using System.Text;
using Edf.Lib;
using Edf.Lib.Data;
using Edf.Lib.Data.Mapping;

namespace Edf.Glue
{
	/// <summary>
	/// ForbiddenController.
	/// </summary>
    public class GenericController : Controller
    {
        public GenericController(IRequest request, IResponse response) : base(request, response)
        {
        }

        protected Type FindGenericType()
        {
            string name = Request.Params["entity"];
            Type type = Edf.Lib.Configuration.FindType(name);
            return type;
        }
        
        public override void Default()
        {
            List();
        }
        
        public void List()
        {
            Type type = FindGenericType();
            Entity entity = Edf.Lib.Data.Mapping.Entity.Obtain(type);
            StringBuilder code = new StringBuilder();
            code.Append("<html><body>");
            foreach (EntityMember member in entity.AllMembers)
            {

            }
            code.Append("</body></html>");
        }
	}
}
