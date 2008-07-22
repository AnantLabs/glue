using System;
using System.Collections;

namespace Glue.Web.Hosting.Server
{
	/// <summary>
	/// Summary description for Session.
	/// </summary>
	public class Session : ISession
	{
        static Hashtable _sessionData = new Hashtable();

        Hashtable _data;
		
        public Session(string sessionId)
		{
            _data = (Hashtable)_sessionData[sessionId];
            if (_data == null)
                _sessionData[sessionId] = _data = new Hashtable();
		}
        
        public object this[string key]
        {
            get { return _data[key]; }
            set { _data[key] = value; }
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool IsSynchronized
        {
            get { return _data.IsSynchronized; }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public void CopyTo(Array array, int index)
        {
            _data.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get { return _data.SyncRoot; }
        }

        public IEnumerator GetEnumerator()
        {
            return _data.GetEnumerator();
        }
	}
}
