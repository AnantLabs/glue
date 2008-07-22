using System;
using System.Collections;

namespace Glue.Lib.Mime
{
    /// <summary>
    /// Manages a collection of MimePart objects. A MimePartCollection 
    /// is used by the MimePart class to hold child MimePart objects. It 
    /// can't be used as a stand-alone object.
    /// </summary>
    public class MimePartCollection : IEnumerable
	{
		MimePart owner;
        internal ArrayList list;

        internal MimePartCollection(MimePart owner)
        {
            this.list = new ArrayList();
            this.owner = owner;
        }
        
        public void Clear()
        {
            for (int i = Count - 1; i >= 0; i--)
                owner.Remove(this[i]);
        }
        
        public int IndexOf(MimePart part)
        {
            return list.IndexOf(part);
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(MimePart part)
        {
            owner.Insert(part, null);
        }

        public void Insert(MimePart part, MimePart before)
        {
            owner.Insert(part, before);
        }

        public void Remove(MimePart part)
        {
            owner.Remove(part);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public MimePart this[int i]
        {
            get { return (MimePart)list[i]; }
        }
	}
}
