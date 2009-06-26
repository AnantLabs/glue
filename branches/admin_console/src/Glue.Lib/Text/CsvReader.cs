using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace Glue.Lib.Text
{
	/// <summary>
	/// Csv-file reader class. Enables easy reading of Csv (and comparable) formats.
	/// </summary>
	public class CsvReader : IDisposable
	{
        TextReader _reader = null;
        string _line = null;
        int _lineno = 0;
        string[] _values = null;
        string[] _names = null;
        Hashtable _lookup = null;
        bool _header = false;
        char _separator = ',';

        /// <summary>
        /// Constructor. Creates a new CsvReader instance.
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <param name="header">True if the first row in the file is a header row</param>
        /// <remarks>The default seperator "," is used.</remarks>
        public CsvReader(TextReader reader, bool header) : this(reader, header, ',')
        {
        }

        /// <summary>
        /// Constructor. Creates a new CsvReader instance.
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <param name="header">True if the first row in the file is a header row</param>
        /// <param name="separator">Separator charactor</param>
        public CsvReader(TextReader reader, bool header, char separator)
        {
            _reader = reader;
            _separator = separator;
            _header = header;
        }

        /// <summary>
        /// Close the CsvReader and the underlying TextReader.
        /// </summary>
        public void Close()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Return the index of the given column
        /// </summary>
        /// <param name="name">Column name</param>
        /// <returns>Index; -1 if the column name could not be found.</returns>
        public int IndexOf(string name)
        {
            object o = _lookup[name];
            if (o == null)
                return -1;
            else
                return (int)o;
        }

        /// <summary>
        /// Read another line. 
        /// </summary>
        /// <returns>True if another line could be read from the TextReader</returns>
        public bool Read()
        {
            if (_names == null && _header)
                ReadHeader();
            if (!ReadLine())
                return false;
            Split(_line);
            return true;
        }

        public void SetNames(string line)
        {
            Split(line);
            SetNames(_values);
        }

        public void SetNames(string[] names)
        {
            //_lookup = new Hashtable(
            //    new CaseInsensitiveHashCodeProvider(),
            //    new CaseInsensitiveComparer()
            //    );
            _lookup = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            
            _names = new string[names.Length];
            names.CopyTo(_names, 0);
            for (int i = 0; i < _names.Length; i++)
                _lookup.Add(_names[i], i);
        }

        /// <summary>
        /// Return the current line.
        /// </summary>
        public string Line
        {
            get { return _line; }
        }

        /// <summary>
        /// Return the current line number
        /// </summary>
        public int LineNumber
        {
            get { return _lineno; }
        }

        /// <summary>
        /// Separator char. Default is ",".
        /// </summary>
        public char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        /// <summary>
        /// Return the values on the current line
        /// </summary>
        public string[] Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Return the column names
        /// </summary>
        public string[] Names
        {
            get 
            {
                if (_names == null && _header)
                    ReadHeader();
                return _names;
            }
        }

        /// <summary>
        /// Return the value in the given column number
        /// </summary>
        /// <param name="i">Column number</param>
        /// <returns>Value</returns>
        public string this[int i]
        {
            get {  return _values[i]; }
        }

        /// <summary>
        /// Returns the value for given column name.
        /// If no headers are used, this will throw an exception.
        /// If headers are used, but column name not found, will return null (important: no error).
        /// Follows these semantics:
        /// 1. if column does not exist, value is null
        /// 2. if column is empty, value is ''
        /// 3. otherwise, normal string
        /// </summary>
        public string this[string name]
        {
            get 
            { 
                object o = _lookup[name];
                if (o == null)
                    return null;
                return _values[(int)o];
            }
        }

        private bool ReadHeader()
        {
            // get column names from first line
            if (!ReadLine())
                return false;

            SetNames(_line);
            return true;
        }

        private bool ReadLine()
        {
next:
            _line = _reader.ReadLine();
            if (_line == null)
                return false;
            _lineno++;
            int n = _line.Length;
            if (n== 0)
                goto next;
            int i = 0;
            while (i < n)
                if (_line[i] == ' ' || _line[i] == '\t')
                    i++;
                else if (_line[i] == '#')
                    goto next;
                else
                    break;
            return true;
        }

        private void Split(string s)
        {
            ArrayList first;
            if (_values == null)
                first = new ArrayList();
            else
                first = null;

            StringBuilder val = new StringBuilder(1000);
            int col = 0;
            int i = 0;
            int state = 0;
            while (i < s.Length)
            {
                if (state == 0)
                {
                    if (s[i] == _separator)
                    {
                        if (first == null)
                            _values[col++] = val.ToString();
                        else
                            first.Add(val.ToString());
                        val.Length = 0;
                    } 
                    else if (s[i] == '"')
                    {
                        if (s[i+1] == '"')
                        {
                            val.Append('"');
                            i++;
                        } else
                            state = 1;
                    }
                    else
                    {
                        val.Append(s[i]);
                    }
                }
                else if (state == 1)
                {
                    if (s[i] == '"')
                    {
                        if (i < s.Length - 1 && s[i+1] == '"')
                        {
                            val.Append('"');
                            i++;
                        }
                        else
                            state = 0;
                    }
                    else
                    {
                        val.Append(s[i]);
                    }
                }
                i++;
            }
            if (first == null)
            {
                _values[col++] = val.ToString();
                while (col < _values.Length)
                    _values[col++] = "";
            }
            else
            {
                first.Add(val.ToString());
                _values = (string[])first.ToArray(typeof(String));
            }
        }
    }
}
