using System;
using System.Xml;
using System.Globalization;
using Glue.Lib;

namespace Glue.Web.Modules
{
	/// <summary>
	/// Summary description for Localized.
	/// </summary>
	public class Localized : IModule
	{
        public Localized(XmlNode config)
        {
        }

        public bool Before(IRequest request, IResponse response)
        {
            return false;
        }

        public bool Process(IRequest request, IResponse response, Type controller)
        {
            return false;
        }

        public bool After(IRequest request, IResponse response)
        {
            return false;
        }

        public bool Error(IRequest request, IResponse response, Exception exception)
        {
            return false;
        }

        public bool Finally(IRequest request, IResponse response)
        {
            return false;
        }
    }
}
