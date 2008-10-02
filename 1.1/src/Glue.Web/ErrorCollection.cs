using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using Glue.Lib;

namespace Glue.Web
{
	/// <summary>
	/// Summary description for ErrorCollection.
	/// </summary>
	public class ErrorCollection : StringCollection
	{
		public ErrorCollection()
		{
		}

        public override string ToString()
        {
            return StringHelper.Join(", ", this);
        }

        public string ToHTML()
        {
            return StringHelper.Join("<br/>\r\n", this);
        }
	}
}
