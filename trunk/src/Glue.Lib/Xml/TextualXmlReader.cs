using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Edf.Lib.Xml
{
    /// <summary>
    /// Summary description for TextualXmlReader.
    /// </summary>
    public abstract class TextualXmlReader : XmlReader
    {
        /*
        private TextReader reader;
        private XmlNameTable nametable;
        private string baseUri;

        /// <summary>
        /// Construct TextualXmlReader. You must specify a 
        /// TextReader before calling Read().
        /// </summary>
        public TextualXmlReader() 
        {
            nametable = new NameTable();
        }

        /// <summary>
        /// Construct an XmlCsvReader.
        /// </summary>
        /// <param name="input">The input textreader</param>
        public TextualXmlReader(TextReader input) 
        {
            this.nametable = new NameTable();
            this.reader = input;
        }

        /// <summary>
        /// Construct an XmlCsvReader.
        /// </summary>
        /// <param name="input">The input stream</param>
        public TextualXmlReader(Stream input) 
        {
            this.nametable = new NameTable();
            this.reader = new StreamReader(input);
        }

        /// <summary>
        /// Construct an XmlCsvReader.
        /// </summary>
        /// <param name="input">The URL for the file containing the XML data. The BaseURI is set to this value</param>
        /// <param name="nametable">The nametable to use for atomizing element names</param>
        public TextualXmlReader(string url) 
        {
            this.nametable = new NameTable();
            this.reader = new TextReader(url);
            this.baseURI = url;
        }

        /// <summary>
        /// Construct TextualXmlReader. You must specify a 
        /// TextReader before calling Read().
        /// </summary>
        /// <param name="nametable">The nametable to use for atomizing element names</param>
        public TextualXmlReader(XmlNameTable nametable) 
        {
            this.nametable = nametable;
        }

        /// <summary>
        /// Construct an XmlCsvReader.
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="nametable">The nametable to use for atomizing element names</param>
        public TextualXmlReader(TextReader input, XmlNameTable nametable) 
        {
            this.nametable = nametable;
            this.reader = input;
        }

        /// <summary>
        /// Construct an XmlCsvReader.
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="nametable">The nametable to use for atomizing element names</param>
        public TextualXmlReader(Stream input, XmlNameTable nametable) 
        {
            this.nametable = nametable;
            this.reader = new StreamReader(input);
        }

        /// <summary>
        /// Construct an XmlCsvReader.
        /// </summary>
        /// <param name="input">The input text</param>
        /// <param name="nametable">The nametable to use for atomizing element names</param>
        public TextualXmlReader(string url, XmlNameTable nametable) 
        {
            this.nametable = nametable;
            this.reader = new TextReader(url);
            this.baseURI = url;
        }

        public override XmlNodeType NodeType 
        { 
            get 
            {
                switch (_state) 
                {
                    case State.Initial: 
                    case State.Eof:
                        return XmlNodeType.None;
                    case State.Root:
                    case State.Row:
                    case State.Field:
                        return XmlNodeType.Element;
                    case State.Attr:
                        return XmlNodeType.Attribute;
                    case State.AttrValue:
                    case State.FieldValue:
                        return XmlNodeType.Text;
                    default:
                        return XmlNodeType.EndElement;
                }       
            }
        }

        public override string Name 
        {
            get { return this.LocalName; }
        }

        public override string LocalName 
        { 
            get  
            {
                switch (_state) 
                {
                    case State.Attr:
                    case State.Field:
                    case State.EndField:
                        if (_names == null || _attr >= _names.Length) 
                        {
                            return this._nt.Add("a"+_attr);
                        } 
                        return XmlConvert.EncodeLocalName(_names[_attr]);
                    case State.Root:
                    case State.EndRoot:
                        return _root;
                    case State.Row:
                    case State.EndRow:
                        return _rowname;
                }
                return string.Empty;
            }
        }

        public override string BaseURI
        {
            get { return baseUri == null ? "" : baseUri; }
        }

        public override string NamespaceURI 
        { 
            get { return String.Empty; }
        }

        public override string Prefix 
        { 
            get { return String.Empty; }
        }

        public override bool HasValue 
        { 
            get 
            {
                if (_state == State.Attr || _state == State.AttrValue || _state == State.FieldValue) 
                {
                    return Value != String.Empty;
                }
                return false;
            }
        }

        public override string Value 
        { 
            get 
            {
                if (_state == State.Attr || _state == State.AttrValue || _state == State.FieldValue) 
                {
                    return _csvReader[_attr];
                }
                return null;
            }
        }

        public override int Depth 
        { 
            get 
            {
                switch (_state) 
                {
                    case State.Row:
                    case State.EndRow:
                        return 1;
                    case State.Attr:
                    case State.Field:
                    case State.EndField:
                        return 2;
                    case State.AttrValue:
                    case State.FieldValue:
                        return 3;
                }       
                return 0;
            }
        }

        public override bool IsEmptyElement 
        { 
            get 
            {
                if (_state == State.Row && _asAttrs) 
                    return true;

                if (_state == State.Field && _csvReader[_attr] == String.Empty) 
                    return true;

                return false;
            }
        }
        public override bool IsDefault 
        { 
            get { return false; }
        }
        public override char QuoteChar 
        { 
            get { return _csvReader.QuoteChar; }
        }

        public override XmlSpace XmlSpace 
        { 
            get  { return XmlSpace.Default; }
        }

        public override string XmlLang 
        { 
            get { return String.Empty; }
        }

        public override int AttributeCount 
        { 
            get 
            {
                if (! _asAttrs) return 0;

                if (_state == State.Row || _state == State.Attr || _state == State.AttrValue) 
                {
                    return _csvReader.FieldCount;
                }
                return 0;
            }
        }

        public override string GetAttribute(string name) 
        {
            if (! _asAttrs) return null;

            if (_state == State.Row || _state == State.Attr || _state == State.AttrValue) 
            {
                int i = GetOrdinal(name);
                if (i >= 0) 
                    return GetAttribute(i);
            }
            return null;
        }

        int GetOrdinal(string name) 
        {
            if (_names != null) 
            {
                string n = _nt.Add(name);
                for (int i = 0; i < _names.Length; i++) 
                {
                    if ((object)_names[i] == (object)n)
                        return i;
                }
                throw new Exception("Attribute '"+name+"' not found.");
            }
            // names are assigned a0, a1, a2, ...
            return Int32.Parse(name.Substring(1));
        }

        public override string GetAttribute(string name, string namespaceURI) 
        {
            if (namespaceURI != string.Empty && namespaceURI != null) return null;
            return GetAttribute(name);
        }

        public override string GetAttribute(int i) 
        {
            if (! _asAttrs) return null;
            if (_state == State.Row || _state == State.Attr || _state == State.AttrValue) 
            {
                return _csvReader[i];
            }
            return null;
        }

        public override string this [ int i ] 
        { 
            get { return GetAttribute(i); }
        }

        public override string this [ string name ] 
        { 
            get { return GetAttribute(name); }
        }

        public override string this [ string name,string namespaceURI ] 
        { 
            get { return GetAttribute(name, namespaceURI); }
        }

        public override bool MoveToAttribute(string name) 
        {
            if (! _asAttrs) return false;
            if (_state == State.Row || _state == State.Attr || _state == State.AttrValue) 
            {
                int i = GetOrdinal(name);
                if (i < 0) return false;
                MoveToAttribute(i);
            }
            return false;
        }

        public override bool MoveToAttribute(string name, string ns) 
        {
            if (ns != string.Empty && ns != null) return false;
            return MoveToAttribute(name);
        }

        public override void MoveToAttribute(int i) 
        {
            if (_asAttrs) 
            {
                if (_state == State.Row || _state == State.Attr || _state == State.AttrValue) 
                {
                    _state = State.Attr;
                    _attr = i;
                }     
            }
        }

        public override bool MoveToFirstAttribute() 
        {
            if (! _asAttrs) return false;
            if (AttributeCount > 0) 
            {
                _attr = 0;
                _state = State.Attr;
                return true;
            }
            return false;
        }

        public override bool MoveToNextAttribute() 
        {
            if (! _asAttrs) return false;
            if (_attr < AttributeCount-1) 
            {
                _attr = (_state == State.Attr || _state == State.AttrValue) ? _attr+1 : 0;
                _state = State.Attr;
                return true;
            }
            return false;
        }

        public override bool MoveToElement() 
        {
            if (! _asAttrs) return true;

            if (_state == State.Root || _state == State.EndRoot || _state == State.Row) 
            {
                return true;
            }
            else if (_state == State.Attr || _state == State.AttrValue) 
            {
                _state = State.Row;
                return true;
            }                               
            return false;
        }

        public override bool Read() 
        {
            switch (_state) 
            {
                case State.Initial:
                    if (_csvReader == null) 
                    {
                        if (_href == null) 
                        {
                            throw new Exception("You must provide an input location via the Href property, or provide an input stream via the TextReader property.");
                        }
                        _csvReader = new CsvReader(_href, _proxy, 4096);
                        _csvReader.Delimiter = this.Delimiter;
                    }
                    if (_firstRowHasColumnNames) 
                    {
                        ReadColumnNames();
                    }
                    _state = State.Root;
                    return true;
                case State.Eof:
                    return false;
                case State.Root:
                case State.EndRow:          
                    if (_csvReader.Read()) 
                    {
                        _state = State.Row;
                        return true;
                    }
                    _state = State.EndRoot;
                    return true;        
                case State.EndRoot:
                    _state = State.Eof;
                    return false;
                case State.Row:
                    if (_asAttrs) 
                    {
                        _attr = 0;
                        goto case State.EndRow;
                    } 
                    else 
                    {
                        _state = State.Field;
                        _attr = 0;
                        return true;
                    }
                case State.Field:
                    if (!IsEmptyElement) 
                    {
                        _state = State.FieldValue;
                    } 
                    else 
                    {
                        goto case State.EndField;
                    }
                    return true;
                case State.FieldValue:
                    _state = State.EndField;
                    return true;
                case State.EndField:
                    if (_attr < _csvReader.FieldCount-1) 
                    {
                        _attr++;
                        _state = State.Field;
                        return true;
                    }
                    _state = State.EndRow;
                    return true;
                case State.Attr:
                case State.AttrValue:
                    _state = State.Root;
                    _attr = 0;
                    goto case State.Root;
            }
            return false;
        }

        public override bool EOF 
        { 
            get 
            {
                return _state == State.Eof;
            }
        }

        public override void Close() 
        {
            _csvReader.Close();     
        }

        public override ReadState ReadState 
        { 
            get 
            {
                if (_state == State.Initial) return ReadState.Initial;
                else if (_state == State.Eof) return ReadState.EndOfFile;
                return ReadState.Interactive;
            }
        }

        public override string ReadString() 
        {
            if (_state == State.AttrValue || _state == State.Attr) 
            {
                return _csvReader[_attr];
            }
            return String.Empty;
        }

        public override string ReadInnerXml() 
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xw.Formatting = Formatting.Indented;
            while (!this.EOF && this.NodeType != XmlNodeType.EndElement) 
            {
                xw.WriteNode(this, true);
            }
            xw.Close();
            return sw.ToString();
        }

        public override string ReadOuterXml() 
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xw.Formatting = Formatting.Indented;
            xw.WriteNode(this, true);
            xw.Close();
            return sw.ToString();
        }

        public override XmlNameTable NameTable 
        { 
            get { return _nt; }
        }

        public override string LookupNamespace(string prefix) 
        {     
            return null;
        }

        public override void ResolveEntity() 
        {
            throw new NotImplementedException();
        }

        public override bool ReadAttributeValue() 
        {
            if (_state == State.Attr) 
            {
                _state = State.AttrValue;
                return true;
            }
            else if (_state == State.AttrValue) 
            {
                return false;
            }
            throw new Exception("Not on an attribute.");
        } 
        */
    }
}
