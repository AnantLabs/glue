using System;
using System.Collections;

namespace Glue.Lib
{
    /// <summary>
    /// OrderedDictionary contains key/value pairs. In contrast to standard IDictionary behaviour
    /// all pairs are in kept in the order in which they were added.
    /// N.B. Just as with an IDictionary, the enumerator (used with foreach) returns
    /// DictionaryEntry items.
    /// </summary>
    public class OrderedDictionary : IDictionary
    {
        Hashtable _hash;
        ArrayList _list;
        IComparer _comparer;
        bool _readonly;

        public OrderedDictionary() : this(0, null, null)
        {
        }

        public OrderedDictionary(int capacity) : this(capacity, null, null)
        {
        }

        public OrderedDictionary(IComparer comparer) : this(0, null, comparer)
        {
        }

        public OrderedDictionary(int capacity, IHashCodeProvider hashProvider, IComparer comparer)
        {
            _hash = new Hashtable(capacity, hashProvider, comparer);
            _list = new ArrayList(capacity);
            _readonly = false;
            _comparer = comparer;
        }

        public void Add(object key, object value)
        {
            _hash.Add(key, value);
            _list.Add(new DictionaryEntry(key, value));
        }

        public void Clear()
        {
            _hash.Clear();
            _list.Clear();
        }

        public bool Contains(object key)
        {
            return _hash.Contains(key);
        }

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new InnerEnumerator(_list.GetEnumerator(), 2);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        int IndexOfKey(object key)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                DictionaryEntry e = (DictionaryEntry)_list[i];
                if (_comparer != null && _comparer.Compare(e.Key, key) == 0)
                    return i;
                else if (e.Key.Equals(key))
                    return i;
            }
            return -1;
        }

        public void Insert(int index, object key, object value)
        {
            _hash.Add(key, value);
            _list.Insert(index, new DictionaryEntry(key, value));
        }

        public void Remove(object key)
        {
            if (_hash.Contains(key)) 
            {
                _hash.Remove(key);
                int i = IndexOfKey(key);
                _list.RemoveAt(i);
            }
        }

        public void RemoveAt(int index)
        {
            DictionaryEntry e = (DictionaryEntry)_list[index];
            _list.RemoveAt(index);
            _hash.Remove(e.Key);
        }

        public object this[int index]
        {
            get { return ((DictionaryEntry)_list[index]).Value; }
            set 
            { 
                DictionaryEntry e = (DictionaryEntry)_list[index];
                e.Value = value;
                _list[index] = e;
                _hash[e.Key] = value;
            }
        }

        public object this[object key]
        {
            get { return _hash[key]; }
            set 
            {
                if (_hash.Contains(key))
                    _list[IndexOfKey(key)] = new DictionaryEntry(key, value);
                else
                    _list.Add(new DictionaryEntry(key, value));
                _hash[key] = value;
            }
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public ICollection Keys
        {
            get { return new InnerCollection(_list, 0); }
        }

        public ICollection Values
        {
            get { return new InnerCollection(_list, 1); }
        }

        public bool IsReadOnly
        {
            get { return _readonly; }
        }

        public bool IsFixedSize
        {
            get { return _readonly; }
        }

        public bool IsSynchronized
        {
            get { return _list.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _list.SyncRoot; }
        }
        
        class InnerCollection : ICollection
        {
            ArrayList _list;
            // 0=Key[], 1=Value[], 2=DictionaryEntry[]
            int _type;

            public InnerCollection(ArrayList list, int type)
            {
                _list = list;
                _type = type;
            }

            public void CopyTo(Array array, int index)
            {
                if (_type == 0)
                    for (int i = 0; i < _list.Count; i++)
                        array.SetValue(((DictionaryEntry)_list[i]).Key, i);
                else if (_type == 1)
                    for (int i = 0; i < _list.Count; i++)
                        array.SetValue(((DictionaryEntry)_list[i]).Value, i);
            }

            public bool IsSynchronized
            {
                get { return _list.IsSynchronized; }
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public object SyncRoot
            {
                get { return _list.SyncRoot; }
            }

            public IEnumerator GetEnumerator()
            {
                return new InnerEnumerator(_list.GetEnumerator(), _type);
            }
        }

        class InnerEnumerator : IDictionaryEnumerator
        {
            IEnumerator _iter;
            // 0=Key[], 1=Value[], 2=DictionaryEntry[]
            int _type; 

            public InnerEnumerator(IEnumerator iter, int type)
            {
                _iter = iter;
                _type = type;
            }

            public void Reset()
            {
                _iter.Reset();
            }

            public bool MoveNext()
            {
                return _iter.MoveNext();
            }

            public object Current
            {
                get 
                { 
                    if (_type == 0)
                        return ((DictionaryEntry)_iter.Current).Key;
                    else if (_type == 1)
                        return ((DictionaryEntry)_iter.Current).Value;
                    else
                        return _iter.Current;
                }
            }
			
            public DictionaryEntry Entry
            {
                get { return (DictionaryEntry)_iter.Current; }
            }
			
            public object Key
            {
                get { return Entry.Key; }
            }
			
            public object Value
            {
                get { return Entry.Value; }
            }
        }

    }
}
