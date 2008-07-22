using System;
using System.Collections.Generic;
using System.Text;
using Glue.Lib;
using Glue.Data;

namespace Glue.Web
{
    public class EntityController : Controller
    {
        Route Route = new Route(@"
                ^/entity/ 
                (?<entitytype>[^/]+)   
                    (   (/(?<action>[^/]+))     
                                (/(?<key>[^/]+))?   )?",
            null
            );
        string Namespace = "Glue.Web.Minimal.Model";
        public Type EntityType;
        public IDataProvider DataProvider;
        public object Instance;

        public EntityController(IRequest request, IResponse response)
            : base(request, response)
        {
        }

        protected override void Unknown()
        {
            if (!Route.IsMatch(Request))
                base.Unknown();
            
            string typename = Namespace + "." + Request.Params["entitytype"];
            
            DataProvider = (IDataProvider)Configuration.Get("dataprovider");
            EntityType = Configuration.FindType(typename);

            string action = NullConvert.Coalesce(Request.Params["action"], "list");
            if (action == "edit")
                Instance = DataProvider.Find(EntityType, Request.Params["key"]);

            Render("/views/entity/" + action + ".html");
        }
    }
}
