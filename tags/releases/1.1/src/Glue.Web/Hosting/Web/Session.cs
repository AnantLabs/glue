using System;
using System.Collections;

namespace Glue.Web.Hosting.Web
{
	/// <summary>
	/// Summary description for Session.
	/// </summary>
    public class Session : ISession
    {
        System.Web.HttpContext _context;

        public Session(System.Web.HttpContext context)
        {
            _context = context;
        }
        
        public object this[string key]
        {
            get { return _context.Session[key]; }
            set { _context.Session[key] = value; }
        }

        public void Clear()
        {
            _context.Session.Clear();
        }

        public bool IsSynchronized
        {
            get { return _context.Session.IsSynchronized; }
        }

        public int Count
        {
            get { return _context.Session.Count; }
        }

        public void CopyTo(Array array, int index)
        {
            _context.Session.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get { return _context.Session.SyncRoot; }
        }

        public IEnumerator GetEnumerator()
        {
            return _context.Session.GetEnumerator();
        }
    }
}
