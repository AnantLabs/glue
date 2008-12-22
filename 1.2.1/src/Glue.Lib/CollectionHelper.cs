using System;
using System.Collections;
using System.Collections.Specialized;

namespace Glue.Lib
{
	/// <summary>
	/// Summary description for CollectionHelper.
	/// </summary>
	public class CollectionHelper
	{

        /// <summary>
        /// Separator for child-collections. Needed for (among other things) jQuery-compatibility.
        /// </summary>
        public static char Separator = '.';

        private CollectionHelper() { }

        public static Array ToArray(IEnumerable enumerable, Type type)
        {
            ArrayList list = ToArrayList(enumerable);
            return list.ToArray(type);
        }

        public static Array ToArray(ICollection collection, Type type)
        {
            Array array = Array.CreateInstance(type, collection.Count);
            collection.CopyTo(array, 0);
            return array;
        }

        public static ArrayList ToArrayList(IEnumerable enumerable)
        {
            ArrayList list = new ArrayList();
            foreach (object item in enumerable)
                list.Add(item);
            return list;
        }
        
        /// <summary>
        /// Utility function to convert a flat NameValueCollection to a hierarchical 
        /// IDictionary (case-insensitive).
        /// </summary>
        public static IDictionary ToBag(NameValueCollection list)
        {
            HybridDictionary bag = new HybridDictionary(list.Count, true);
            foreach (string key in list.AllKeys)
            {
                if (key == null)
                    continue;
                
                string[] values = list.GetValues(key);
                IDictionary current = bag;
                int i = 0; 
                int j = key.IndexOf(Separator);
                while (j >= 0)
                {
                    string k = key.Substring(i, j - i);
                    IDictionary sub = current[k] as IDictionary;
                    if (sub == null)
                        current[k] = sub = new HybridDictionary(true);
                    i = j + 1;
                    j = key.IndexOf(Separator, i);
                    current = sub;
                }
                if (values.Length == 1)
                    current[key.Substring(i)] = values[0];
                else
                    current[key.Substring(i)] = values;
            }
            return bag;
        }

        /// <summary>
        /// Utility function to convert a param list (consisting of 
        /// alternating key/value pairs) to a case-insensitive hierarchical 
        /// dictionary.
        /// 
        /// CollectionHelper.ToBag("name", "John", "age", 25, "address.street", "Elm St") => { "name" => "John", "age", 25, "addres" => { "street" => "Elm St" } }
        /// </summary>
        public static IDictionary ToBag(params object[] namevalues)
        {
            HybridDictionary bag = new HybridDictionary(namevalues.Length / 2, true);
            int m = 0;
            while (m < namevalues.Length - 1)
            {
                string key = namevalues[m++].ToString();
                IDictionary current = bag;
                int i = 0;
                int j = key.IndexOf(Separator);
                while (j >= 0)
                {
                    string k = key.Substring(i, j - i);
                    IDictionary sub = current[k] as IDictionary;
                    if (sub == null)
                        current[k] = sub = new HybridDictionary(true);
                    i = j + 1;
                    j = key.IndexOf(Separator, i);
                    current = sub;
                }
                current[key.Substring(i)] = namevalues[m++];
            }
            return bag;
        }

        /// <summary>
        /// Utility function to convert a param list (consisting of 
        /// alternating key/value pairs) to a case-insensitive hierarchical 
        /// dictionary.
        /// 
        /// CollectionHelper.ToOrderedBag("name", "John", "age", 25) => { "age" => 25, "name" => "John" }
        /// </summary>
        public static IDictionary ToOrderedBag(params object[] namevalues)
        {
            OrderedDictionary bag = new OrderedDictionary(namevalues.Length / 2);
            int m = 0;
            while (m < namevalues.Length - 1)
            {
                string key = namevalues[m++].ToString();
                IDictionary current = bag;
                int i = 0;
                int j = key.IndexOf(Separator);
                while (j >= 0)
                {
                    string k = key.Substring(i, j - i);
                    IDictionary sub = current[k] as IDictionary;
                    if (sub == null)
                        current[k] = sub = new OrderedDictionary();
                    i = j + 1;
                    j = key.IndexOf(Separator, i);
                    current = sub;
                }
                current[key.Substring(i)] = namevalues[m++];
            }
            return bag;
        }

        public static IDictionary Intersect(IDictionary a, string[] keys)
        {
            IDictionary bag = new HybridDictionary(true);
            foreach (string key in keys)
            {
                IDictionary dest = bag;
                IDictionary source = a;
                int i = 0;
                int j = key.IndexOf(Separator);
                while (j >= 0)
                {
                    string k = key.Substring(i, j - i);
                    source = source[k] as IDictionary;
                    if (source == null)
                        goto next;
                    IDictionary sub = dest[k] as IDictionary;
                    if (sub == null)
                        dest[k] = sub = new HybridDictionary(true);
                    i = j + 1;
                    j = key.IndexOf(Separator, i);
                    dest = sub;
                }
                dest[key.Substring(i)] = source[key.Substring(i)];
            next:
                ;
            }
            return bag;
        }

        /// <summary>
        /// Copy a dictionary
        /// </summary>
        public static IDictionary Copy(IDictionary a)
        {
            if (a == null)
                return null;
            IDictionary c = new HybridDictionary(a.Count);
            foreach (DictionaryEntry e in a)
                c.Add(e.Key, e.Value);
            return c;
        }

        /// <summary>
        /// Add two dictionaries
        /// </summary>
        public static IDictionary Add(IDictionary a, IDictionary b)
        {
            if (b == null)
                return Copy(a);
            else if (a == null)
                return Copy(b);
            IDictionary c = Copy(a);
            foreach (DictionaryEntry e in b)
                c[e.Key] = e.Value;
            return c;
        }

        /// <summary>
        /// Add two dictionaries
        /// </summary>
        public static IDictionary Subtract(IDictionary a, IDictionary b)
        {
            if (b == null)
                return Copy(a);
            if (a == null)
                return null;
            IDictionary c = new HybridDictionary(a.Count);
            foreach (DictionaryEntry e in a)
                if (!b.Contains(e.Key))
                    c.Add(e.Key, e.Value);
            return c;
        }

        /// <summary>
        /// Utility to parse a string representation
        /// of a hash / array / value.
        /// Mirrors the ToString functions.
        /// </summary>
        public static IDictionary Parse(string s)
        {
            int i = 0;
            while (char.IsWhiteSpace(s,i))
                i++;
            return (IDictionary)Parse(s, ref i);
        }

        private static object Parse(string s, ref int i)
        {
            if (s == null || s.Length == 0)
                return null;
            if (s[i] == '{')
            {
                i++;
                IDictionary bag = new HybridDictionary(true);
                while (s[i] != '}')
                {
                    while (char.IsWhiteSpace(s,i))
                        i++;
                    int j = i;
                    while (s[j] != ':' && !char.IsWhiteSpace(s,j))
                        j++;
                    int k = j + 1;
                    while (char.IsWhiteSpace(s,k))
                        k++;
                    if (s[k] == '{')
                    {
                        bag[s.Substring(i, j-i)] = Parse(s, ref k);
                        i = k;
                    }
                    else if (s[k] == '[')
                    {
                        bag[s.Substring(i, j-i)] = Parse(s, ref k);
                        i = k;
                    }
                    else
                    {
                        int l = k;
                        while (s[l] != ',' && s[l] != '}')
                            l++;
                        bag[s.Substring(i, j-i)] = s.Substring(k, l-k);
                        i = l;
                    }
                    if (s[i] == ',')
                        i++;
                }
                return bag;
            }
            if (s[i] == '[')
            {
                ArrayList list = new ArrayList();
                i++;
                while (s[i] != ']')
                {
                    int j = i;
                    while (s[j] != ',' && s[j] != ']')
                        j++;
                    list.Add(s.Substring(i, j-i));
                    i = j;
                    if (s[i] == ',')
                        i++;
                }
                return list;
            }
            return s;
        }

        /// <summary>
        /// Utility function for dumping contents of a IDictionary bag
        /// </summary>
        public static string ToString(IDictionary bag)
        {
            if (bag == null)
                return "";
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("{\n");
            bool first = true;
            foreach (DictionaryEntry e in bag)
            {
                if (first)
                    first = false;
                else
                    s.Append(",\n");
                s.Append(e.Key);
                s.Append(':');
                if (e.Value is IDictionary)
                    s.Append(CollectionHelper.ToString(e.Value as IDictionary));
                else if (e.Value is IList)
                    s.Append(CollectionHelper.ToString(e.Value as IList));
                else
                    s.Append(e.Value);
            }
            s.Append("\n}");
            return s.ToString();
        }

        /// <summary>
        /// Utility function for dumping contents of a IList
        /// </summary>
        public static string ToString(IList list)
        {
            if (list == null)
                return "";
            string s = "[";
            bool first = true;
            foreach (object item in list)
            {
                if (first)
                    first = false;
                else
                    s += ",";
                if (item is IDictionary)
                    s += ToString(item as IDictionary);
                else if (item is IList)
                    s += ToString(item as IList);
                else
                    s += item;
            }
            s += "]";
            return s;
        }
	}
}
