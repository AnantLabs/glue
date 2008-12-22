using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace Glue.Lib.Mime
{
	/// <summary>
	/// MimeHeader encapsulates a single MIME header as specified in 
	/// RFC 1521  and 822.
	/// 
	/// A typical MIME header looks thusly:
	/// 
	///     Content-Type: text/plain
	///     
	/// or:
	/// 
    ///     Content-Type: multipart/alternative; 
    ///         boundary="bound_1";
    ///         notherparam=wok
    ///         
    /// Please note a header can span multiple lines (continuation is 
    /// marked by 1 or more spaces at beginning of a line). 
    /// 
    /// Please also note that a MIME header has a Name, a Value and 
    /// optional Params: the second header is named 'Content-Type', 
    /// has a Value of 'multipart/alternative' and contains two Params: 
    /// 'boundary' with value 'bound_1' and 'notherparam' with value 
    /// 'wok'.
	/// </summary>
    public class MimeHeader : ICloneable
    {
        protected string _name;
        protected string _value;
        protected NameValueCollection _parms = new NameValueCollection(0);

        public static MimeHeader Parse(TextReader reader)
        {
            string text = reader.ReadLine();
            if (text == null || text.Length == 0)
                return null;
            char c = (char)reader.Peek();
            while (c == ' ' || c == '\t')
            {
                text += "\r\n";
                text += reader.ReadLine();
                c = (char)reader.Peek();
            }
            return new MimeHeader(text);
        }

        public static MimeHeader Parse(string text)
        {
            return new MimeHeader(text);
        }

        protected MimeHeader(string text)
        {
            InternalParse(text);
        }

        public MimeHeader(MimeHeader source)
        {
            this._name = source._name;
            this._value = source._value;
            this._parms = new NameValueCollection(source._parms);
        }

        public MimeHeader(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(Name);
            s.Append(": ");
            s.Append(Value);
            foreach (string n in Params.AllKeys)
            {
                s.Append(";\r\n\t");
                s.Append(n);
                s.Append("=\"");
                s.Append(Params[n]);
                s.Append("\"");
            }
            return s.ToString();
        }

        public string Value
        {
            get { return _value; }
            set { this._value = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public NameValueCollection Params
        {
            get { return _parms; }
        }

        protected void InternalParse(string text)
        {
            // Parse name
            int i = text.IndexOf(':');
            if (i < 0)
            {
                _name = "";
                _value = text.Trim();
                return;
            }

            _name = text.Substring(0, i).Trim();
            text = text.Substring(i + 1).Trim();

            // Split rest into data and optional params
            i = IndexInQuoted(text, ';');
            if (i < 0)
            {
                _value = text;
            }
            else
            {
                _value = text.Substring(0, i).Trim();
                text = text.Substring(i + 1).Trim();

                // Split optional params
                foreach (string s in SplitQuoted(text, ';'))
                {
                    i = s.IndexOf('=');
                    if (i >= 0)
                    {
                        string n = s.Substring(0, i).Trim();
                        string v = s.Substring(i + 1).Trim().Trim('"');
                        _parms[n] = v;
                    }
                    else
                    {
                        _parms[s.Trim()] = "";
                    }
                }
            }
        }
        
        private static int IndexInQuoted(string s, char c)
        {
            int i = 0;
            int n = s.Length;
            while (i < n)
            {
                if (s[i] == '"')
                    for (i++; i < n && s[i++] != '"'; )
                    {
                    }
                else if (s[i] == c)
                    return i;
                else
                    i++;
            }
            return -1;
        }

        private static string[] SplitQuoted(string s, char separator)
        {
            ArrayList list = new ArrayList();
            int i = 0;
            int n = s.Length;
            int l = 0;
            while (i < n)
            {
                if (s[i] == '"')
                {
                    for (i++; i < n && s[i++] != '"'; )
                    {
                    }
                }
                else if (s[i] == separator)
                {
                    if (i > l)
                        list.Add(s.Substring(l, i - l));
                    i++;
                    l = i;
                }
                else
                {
                    i++;
                }
            }
            if (i > l)
                list.Add(s.Substring(l, i - l));
            return (string[])list.ToArray(typeof(string));
        }

        object ICloneable.Clone()
        {
            return new MimeHeader(this);
        }

        public MimeHeader Clone()
        {
            return new MimeHeader(this);
        }
    }
}
