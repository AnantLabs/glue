using System;
using System.IO;
using System.Collections;

namespace Glue.Lib.Mime
{
	/// <summary>
	/// Manages a collection of MimeHeader objects. A MimeHeaderCollection 
	/// is used by the MimePart class for accessing headers. It can also 
	/// be used a a stand-alone object.
	/// </summary>
    public class MimeHeaderCollection : IEnumerable
    {
        //Hashtable lookup = new Hashtable(
        //    System.Collections.CaseInsensitiveHashCodeProvider.DefaultInvariant,
        //    System.Collections.CaseInsensitiveComparer.DefaultInvariant
        //    );
        Hashtable lookup = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        ArrayList list = new ArrayList();

        public static MimeHeaderCollection Parse(TextReader reader)
        {
            return new MimeHeaderCollection(reader);
        }
        
        public MimeHeaderCollection()
        {
        }
        
        private MimeHeaderCollection(TextReader reader)
        {
            MimeHeader header = MimeHeader.Parse(reader);
            while (header != null)
            {
                Add(header);
                header = MimeHeader.Parse(reader);
            }
        }

        public string GetValue(string name)
        {
            MimeHeader header = this[name];
            if (header == null)
                return null;
            else
                return header.Value;
        }

        public MimeHeader SetValue(string name, string value)
        {
            MimeHeader header = this[name];
            if (header == null)
                Add(header = new MimeHeader(name, value));
            else
                header.Value = value;
            return header;
        }

        public string GetParam(string name, string param)
        {
            MimeHeader header = this[name];
            if (header == null)
                return null;
            else
                return header.Params[param];
        }

        public void SetParam(string name, string param, string value)
        {
            MimeHeader header = this[name];
            if (header == null)
                throw new ArgumentException("Header '" + name + "' must exist before setting '" + param + "'");
            header.Params[param] = value;
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(MimeHeader header)
        {
            Insert(Count, header);
        }

        public void Clear()
        {
            list.Clear();
            lookup.Clear();
        }

        public void Insert(int index, MimeHeader header)
        {
            list.Insert(index, header);
            // The collection can contain headers with 
            // duplicate names *and* lookup should point
            // out the first of the duplicates: so check.
            if (!lookup.Contains(header.Name))
                lookup.Add(header.Name, header);
        }

        public MimeHeader Remove(string key)
        {
            MimeHeader header = this[key];
            if (header != null)
                Remove(header);
            return header;
        }

        public void Remove(MimeHeader header)
        {
            list.Remove(header);
            lookup.Remove(header.Name);
            // The collection can contain headers with 
            // duplicate names, so realign the lookup
            // table.
            foreach (MimeHeader other in list)
                if (string.Compare(other.Name, header.Name, true) == 0)
                {
                    lookup[other.Name] = other;
                    break;
                }
        }

        public int Count
        {
            get { return list.Count; }
        }
        
        public MimeHeader this[int index]
        {
            get { return (MimeHeader)list[index]; }
        }

        public MimeHeader this[string key]
        {
            get { return (MimeHeader)lookup[key]; }
        }
    }
}
