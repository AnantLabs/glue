using System;
using System.Collections;
using System.Collections.Specialized;

namespace Glue.Web
{
	/// <summary>
	/// PostedFileCollection holds a number of Cookie objects.
	/// See IRequest.Files
	/// </summary>
	public class CookieCollection : NameObjectCollectionBase
	{
        internal CookieCollection()
		{
        }
        
        internal void Add(Cookie cookie)
        {
            BaseAdd(cookie.Name, cookie);
        }

        public Cookie this[int i]
        {
            get { return (Cookie)BaseGet(i); }
        }
        
        public Cookie this[string name]
        {
            get { return (Cookie)BaseGet(name); }
        }
    }
}
