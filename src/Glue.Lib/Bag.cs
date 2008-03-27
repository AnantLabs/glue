using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Edf.Lib
{
    /// <summary>
    /// A Bag is a hierarchical dictionary, containing items and child Bags.
    /// 
    /// bag["hello"] = "world";
    /// bag["foo.bar"] = "foo's bar";
    /// bag["foo.boo"] = "foo's boo";
    /// bag["num"] = 1;
    /// 
    /// bag:
    ///   hello: "world"
    ///   foo:
    ///      bar: "foo's bar"
    ///      boo: "foo's boo"
    ///   num: 1
    ///      
    /// bar.ToString() will yield:
    ///   {hello=world,foo{bar=foo's bar,boo=foo's boo},num=1}
    ///   
    /// </summary>
    public class Bag : IDictionary
    {
        private HybridDictionary _bag;
        private bool _checkTypes;
    
        public Bag()
        {
            _bag = new HybridDictionary(true);
        }

        public void Add(string key, object value)
        {
            Bag bag = this;
            int i = 0;
            int j = key.IndexOf('.', i);
            while (j >= 0)
            {
                string subkey = key.Substring(i, j - i);
                Bag sub = bag._bag[subkey] as Bag;
                if (sub == null)
                    bag._bag[subkey] = sub = new Bag();
                bag = sub;
                i = j + 1;
                j = key.IndexOf('.', i);
            }
            CheckTypeOf(value);
            bag._bag[key.Substring(i)] = value;
        }

        public void Add(IDictionary from)
        {
            foreach (DictionaryEntry item in from)
                Add((string)item.Key, item.Value);
        }
    
        public void Add(NameValueCollection from)
        {
            for (int i = 0; i < from.Count; i++)
            {
                string[] values = from.GetValues(i);
                if (values.Length == 1)
                    Add(from.GetKey(i), values[0]);
                else
                    Add(from.GetKey(i), values);
            }
        }

        void CheckTypeOf(object value)
        {
            Type t = value.GetType();
            if (t == typeof(String) ||
                t == typeof(Boolean) ||
                t == typeof(Int32) ||
                t == typeof(DateTime) ||
                t == typeof(Guid))
                return;
            throw new ArgumentException("Invalid type.");
        }
    
        static MethodInfo _basegetallkeysmethod;
        static MethodInfo _basegetallvaluesmethod;
        static object[] _args0 = {};

        public void Add(NameObjectCollectionBase from)
        {
            if (_basegetallkeysmethod == null)
            {
                _basegetallkeysmethod = typeof(NameObjectCollectionBase).GetMethod("BaseGetAllKeys", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                _basegetallvaluesmethod = typeof(NameObjectCollectionBase).GetMethod("BaseGetAllValues", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
            }
            string[] keys = (string[])_basegetallkeysmethod.Invoke(from, _args0);
            object[] values = (object[])_basegetallvaluesmethod.Invoke(from, _args0);
            for (int i = 0; i < keys.Length; i++)
                Add(keys[i], values[i]);
        }

        public object Get(string key)
        {
            Bag bag = this;
            int i = 0;
            int j = key.IndexOf('.', i);
            while (j >= 0)
            {
                string subkey = key.Substring(i, j - i);
                Bag sub = bag._bag[subkey] as Bag;
                if (sub == null)
                    return null;
                bag = sub;
                i = j + 1;
                j = key.IndexOf('.', i);
            }
            return bag._bag[key.Substring(i)];
        }
    
        public Bag GetBag(string key)
        {
            return Get(key) as Bag;
        }

        public string GetString(string key)
        {
            return Get(key) as string;
        }

        public int GetInt32(string key)
        {
            return Convert.ToInt32(Get(key));
        }

        public int GetInt32(string key, int _default)
        {
            return NullConvert.ToInt32(Get(key), _default);
        }

        public int GetBoolean(string key)
        {
            return Convert.ToBoolean(Get(key));
        }

        public int GetBoolean(string key, bool _default)
        {
            return NullConvert.ToBoolean(Get(key), _default);
        }

        public int GetDateTime(string key)
        {
            return Convert.ToDateTime(Get(key));
        }

        public int GetDateTime(string key, DateTime _default)
        {
            return NullConvert.ToDateTime(Get(key), _default);
        }

        public void Clear()
        {
            _bag.Clear();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return _bag.GetEnumerator();
        }

        public void Remove(string key)
        {
            Bag bag = this;
            int i = 0;
            int j = key.IndexOf('.', i);
            while (j >= 0)
            {
                string subkey = key.Substring(i, j - i);
                Bag sub = bag._bag[subkey] as Bag;
                if (sub == null)
                    return;
                bag = sub;
                i = j + 1;
                j = key.IndexOf('.', i);
            }
            bag._bag.Remove(key.Substring(i));
        }

        public void CopyTo(Array array, int index)
        {
            Values.CopyTo(array, index);
        }

        public override string ToString()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append('{');
            bool first = true;
            foreach (DictionaryEntry e in this)
            {
                if (!first)
                    s.Append(',');
                else
                    first = false;
                s.Append(e.Key);
                if (e.Value is Array)
                {
                    s.Append('[');
                    first = true;
                    foreach (object o in e.Value as IEnumerable)
                    {
                        if (!first)
                            s.Append(',');
                        else
                            first = false;
                        s.Append(o);
                    }
                    s.Append(']');
                }
                else if (e.Value is Bag)
                {
                    s.Append(e.Value);
                }
                else
                {
                    s.Append('=');
                    s.Append(e.Value);
                }
            }
            s.Append('}');
            return s.ToString();
        }

        public object this[string key]
        {
            get { return Get(key); }
            set 
            { 
                if (value == null)
                    Remove(key);
                else
                    Add(key, value);
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection Values
        {
            get { return _bag.Values; }
        }

        public ICollection Keys
        {
            get { return _bag.Keys; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public int Count
        {
            get { return _bag.Count; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        object IDictionary.this[object key]
        {
            get { return this[(string)key]; }
            set { this[(string)key] = value; }
        }

        void IDictionary.Remove(object key)
        {
            Remove((string)key);
        }

        void IDictionary.Add(object key, object value)
        {
            Add((string)key, value);
        }

        bool IDictionary.Contains(object key)
        {
            return Get((string)key) != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
