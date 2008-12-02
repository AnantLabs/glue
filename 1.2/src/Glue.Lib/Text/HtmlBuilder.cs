using System;
using System.Collections;
using System.Text;
using Glue.Lib;

namespace Glue.Lib.Text
{
	/// <summary>
	/// Summary description for HtmlBuilder.
	/// </summary>
    public class HtmlBuilder
    {
        StringBuilder data;
        bool skip = false;

        public static implicit operator HtmlBuilder(string str)
        {
            return new HtmlBuilder(str);
        }

        public static implicit operator string(HtmlBuilder html)
        {
            return html.data.ToString();
        }

        public HtmlBuilder()
        {
            data = new StringBuilder();
        }
            
        public HtmlBuilder(string str)
        {
            data = new StringBuilder(str);
        }

        public HtmlBuilder Append(object s)
        {
            if (!skip)
                data.Append(s);
            return this;
        }
        
        public HtmlBuilder Append(string s)
        {
            if (!skip)
                data.Append(s);
            return this;
        }

        public HtmlBuilder AppendLine()
        {
            if (!skip)
                data.AppendLine();
            return this;
        }

        public HtmlBuilder Attr(IDictionary attributes)
        {
            foreach (DictionaryEntry e in attributes)
                Attr("" + e.Key, e.Value);
            return this;
        }

        public HtmlBuilder Attr(string name, object value)
        {
            if (name == null || name.Length == 0 || value == null || value == DBNull.Value)
                return this;
            string v;
            if (value is Boolean)
                v = (bool)value ? "yes" : "no";
            else
                v = value as string;
            if (v == null)
                v = Convert.ToString(value);
            if (name == "disabled")
                if (v == null || v == "no" || v == "" || v == "false" || v == "0")
                    return this;
                else
                    v = "disabled";
            //if (v.Length == 0)
                //return this;
            if (!skip)
                data.Append(' ').Append(name).Append("=\"").Append(v).Append('"');
            return this;
        }

        public HtmlBuilder Attr(string name, IDictionary lookup)
        {
            return Attr(name, lookup[name]);
        }

        public HtmlBuilder Attr(string name, IDictionary lookup, object def)
        {
            return Attr(name, lookup.Contains(name) ? lookup[name] : def);
        }

        public HtmlBuilder AttrIfTrue(string name, IDictionary lookup)
        {
             if (NullConvert.ToBoolean(lookup[name], false))
                return Attr(name, lookup[name]);
            else
                return this;
        }

        public HtmlBuilder If(object test)
        {
            skip = !NullConvert.ToBoolean(test, false);
            return this;
        }

        public HtmlBuilder IfNot(object test)
        {
            skip = !!NullConvert.ToBoolean(test, false);
            return this;
        }

        public HtmlBuilder End()
        {
            skip = false;
            return this;
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }
}
