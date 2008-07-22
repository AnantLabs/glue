using System;
using System.Collections;

namespace Glue.Lib
{
    /// <summary>
    /// Stores multiple contained exceptions related to mapping 
    /// values to members of objects. See Mapper.
    /// </summary>
    public class CombinedException : Exception, IEnumerable
    {
        ArrayList exceptions = new ArrayList();
        ArrayList members = new ArrayList();

        public CombinedException() : this(null) { }

        public CombinedException(IEnumerable exceptions) : base("Encountered a number of errors.")
        {
            if (exceptions != null)
                foreach (Exception e in exceptions)
                    Add(e);
        }

        public void Add(Exception exception)
        {
            Add(null, exception);
        }

		public void Add(string message)
		{
			Add(null, new Exception(message));
		}
		
		public void Add(string member, string message)
        {
            Add(member, new Exception(message));
        }

        public void Add(string member, string message, Exception inner)
        {
            Add(member, new Exception(message, inner));
        }

        public void Add(string member, Exception exception)
        {
            if (exception is CombinedException)
            {
                CombinedException other = exception as CombinedException;
                for(int n=0; n < other.Count; n++)
					this.Add(other.Members[n], other[n]);
            }
            else
            {
                exceptions.Add(exception);
				members.Add(member);                
            }
        }

        public void Clear()
        {
            exceptions.Clear();
            members.Clear();
            
        }

        public Exception this[int index]
        {
            get { return (Exception)exceptions[index]; }
        }
        
        public Exception this[string member]
        {
            get { int i = IndexOf(member); return i < 0 ? null : (Exception)exceptions[i]; }
        }
        
        public string[] Members
		{
			get { return (string[])members.ToArray(typeof(string)); } 
		}

        public int Count
        {
            get { return exceptions.Count; }
        }

        public int IndexOf(string member)
        {
            for (int i = 0; i < members.Count; i++)
                if (string.Compare(member, (string)members[i], true) == 0)
                    return i;
            return -1;
        }

        public IEnumerator GetEnumerator()
        {
            return exceptions.GetEnumerator();
        }

        public override string ToString()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append(this.GetType().Name + ":\r\n");
			for(int n=0; n < exceptions.Count; n++)
				if (members[n] == null || (string)members[n] == "")
                    s.Append(this[n].Message).Append("\r\n");
				else
					s.Append("  ").Append(members[n]).Append(": ").Append(this[n].Message).Append("\r\n");

            return s.ToString();
        }

        /// <summary>
        /// Returns a UL list of errors
        /// </summary>
        /// <returns></returns>
        public string ToHtml()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("<ul>\r\n");
            for(int n=0; n < exceptions.Count; n++)
                if (members[n] == null || (string)members[n] == "")
                    s.Append("<li>").Append(this[n].Message).Append("</li>\r\n");
                else
                    s.Append("<li>").Append(members[n]).Append(": ").Append(this[n].Message).Append("</li>\r\n");
            s.Append("</ul>\r\n");
            return s.ToString();
        }
    }
}
