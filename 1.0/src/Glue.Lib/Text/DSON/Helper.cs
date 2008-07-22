using System;
using System.Collections;
using System.IO;

namespace Glue.Lib.Text.DSON
{
    public class Item : IEnumerable
    {
        private object _value;

        private IList List { get { return _value as IList; } }
        private IDictionary Hash { get { return _value as IDictionary; } }
        private int Kind
        { 
            get 
            {
                if (_value is IList)
                    return 1;
                if (_value is IDictionary)
                    return 2;
                return 0;
            }
            set 
            {
                if (Kind == value)
                    return;
                if (value == 0)
                    _value = null;
                else if (value == 1)
                    _value = new ArrayList();
                else if (value == 2)
                    _value = new Hashtable();
            }
        }
        
        private static Item Box(object value)
        {
            if (value is Item)
                return (Item)value;
            Item item = new Item();
            if (value is IDictionary)
            {
                IDictionary s = value as IDictionary;
                IDictionary d = new Hashtable(s.Count);
                foreach (object key in d.Keys)
                    d[key.ToString()] = Box(s[key]);
                item._value = d;
            }
            else if (value is IList)
            {
                IList s = value as IList;
                IList d = new ArrayList(s.Count);
                foreach (object val in ((IList)value))
                    d.Add(Box(val));
                item._value = d;
            }
            else
            {
                item._value = value;
            }
            return item;
        }

        private static object UnBox(object value)
        {
            if (value is Item)
                return ((Item)value)._value;
            return value;
        }

        public bool IsValue
        {
            get { return Kind == 0; }
        }
        
        public bool IsList
        {
            get { return Kind == 1; }
        }
        
        public bool IsHash
        {
            get { return Kind == 2; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = UnBox(value); }
        }
        
        public Item Get(int index)
        {
            return (Item)((IList)_value)[index];
        }

        public Item Get(string name)
        {
            return (Item)((IDictionary)_value)[name];
        }

        public int Count
        {
            get { return ((ICollection)_value).Count; }
        }
        
        public IEnumerator GetEnumerator()
        {
            if (_value is IList)
                return ((IList)_value).GetEnumerator();
            else if (_value is IDictionary)
                return ((IDictionary)_value).Values.GetEnumerator();
            else
                throw new InvalidCastException("Item does not have child items.");
        }

        public ICollection Keys
        {
            get { return ((IDictionary)_value).Keys; }
        }

        public Item Add()
        {
            return Add(null);
        }

        public Item Add(object value)
        {
            Kind = 1;
            Item item = Box(value);
            List.Add(item);
            return item;
        }

        public Item Set(string name)
        {
            return Set(name, null);
        }

        public Item Set(string name, object value)
        {
            Kind = 2;
            Item item = (Item)Hash[name];
            if (item == null)
                Hash[name] = item = Box(value);
            return item;
        }
        
        public string Inspect()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            Inspect(s, "");
            return s.ToString();
        }

        private void Inspect(System.Text.StringBuilder s, string indent)
        {
            if (Kind == 2)
            {
                s.Append("\n" + indent + "{\n");
                foreach (string key in Hash.Keys)
                {
                    s.Append(indent + "  " + key + ": ");
                    ((Item)Hash[key]).Inspect(s, indent + "  ");
                }
                s.Append(indent + "}\n");
            }
            else if (Kind == 1)
            {
                s.Append("\n" + indent + "[\n");
                foreach (Item item in List)
                {
                    s.Append(indent + "  ");
                    item.Inspect(s, indent + "  ");
                }
                s.Append(indent + "]\n");
            }
            else if (Kind == 0)
            {
                if (_value is String)
                {
                    string str = (string)_value;
                    string esc = StringHelper.EscapeCStyle(str);
                    int lines = StringHelper.LineCount(str);
                    if (str == esc)
                        s.Append(str + "\n");
                    else if (lines > 2 || (lines == 2 && str.Length > 80))
                        s.Append("\"\"\"\n" + StringHelper.Indent(str, indent + "  ") + "\"\"\"\n");
                    else 
                        s.Append("\"" + esc + "\"");
                }
                else
                {
                    s.Append(_value + "\n");
                }
            }
        }
    }
    
	/// <summary>
	/// Static helper function for JSON notation.
	/// </summary>
	public class Helper
	{
        public static Item Parse(string text)
        {
            Parser parser = new Parser(new Scanner(text));
            parser.Parse();
            if (parser.Errors.Count > 0)
                throw new Exception("Error parsing JSON.: " + parser.Errors);
            return parser.Result as Item;
        }
        
        public static Item Parse(TextReader reader)
        {
            Parser parser = new Parser(new Scanner(reader.ReadToEnd()));
            parser.Parse();
            if (parser.Errors.Count > 0)
                throw new Exception("Error parsing JSON.: " + parser.Errors);
            return parser.Result as Item;
        }

        private Helper() {}
    }
}
