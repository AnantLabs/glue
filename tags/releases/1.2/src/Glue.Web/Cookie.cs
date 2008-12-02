using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Glue.Web
{
	/// <summary>
	/// Summary description for Cookie.
	/// </summary>
	public class Cookie
	{
        public readonly string Name;
        public string Domain;
        public string Path;
        public DateTime Expires;
        public bool Secure;
        
        MultiValue _values = new MultiValue();

        public Cookie(string name)
        {
            Name = name;
        }

        public Cookie(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Value
        {
            get { return _values.ToString(); }
            set 
            { 
                _values.Clear();
                if (value != null && value.Length > 0)
                {
                    foreach (string s in value.Split('&'))
                    {
                        int i = s.IndexOf('=');
                        if (i < 0)
                            _values.Add(null, s);
                        else
                            _values.Add(s.Substring(0, i), s.Substring(i + 1));
                    }
                }
            }
        }

        public string this[string key]
        {
            get { return _values[key]; }
            set { _values[key] = value; }
        }

        public  NameValueCollection Values
        {
            get { return _values; }
        }

        internal string GetHeaderString()
        {
            StringBuilder s = new StringBuilder();
            
            s.Append(Name);
            s.Append("=");
            s.Append(Value);

            if (Domain != null) 
            {
                s.Append("; domain=");
                s.Append(Domain);
            }
	       
            if (Expires != DateTime.MinValue) 
            {
                s.Append("; expires=");
                s.Append(Expires.ToUniversalTime().ToString("r"));
            }

            s.Append("; path=");
            if (Path != null && Path.Length > 0)
                s.Append(Path);
            else
                s.Append("/");

            if (Secure) 
                s.Append("; secure");

            return s.ToString();
        }

        class MultiValue : NameValueCollection
        {
            public override string ToString()
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                bool first = true;
                foreach (string key in Keys)
                {
                    foreach (string val in GetValues(key))
                    {
                        if (!first)
                            s.Append('&');
                        if (key != null)
                        {
                            s.Append(key);
                            s.Append('=');
                        }
                        s.Append(val);
                        first = false;
                    }
                }
                return s.ToString();
            }
            
            public override void Set(string name, string value)
            {
                if (name == null)
                    Clear();
                base.Set(name, value);
            }
        }
	}
}
