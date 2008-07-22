using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace Glue.Lib.Mime
{
	/// <summary>
	/// MimePart encapsulates a MIME message part (see RFC 1521 for details). 
	/// MIME messages contain a hierarchical tree of MIME parts, each carrying
	/// some information payload. As typical mail message looks like this:
	/// 
	///     Content-Type: multipart/alternative; boundary="bound_1"
	///     Sample-Header-1: mime-header-value
    ///     Sample-Header-2: mime-header-value
    /// 
    ///     This is a multipart mime.
    ///     --bound_1
    ///         Content-Type: text/plain
    ///         Sample-Header: mime-header-value
    ///         
    ///         This is a message in plaintext.
    ///     --bound_1
    ///         Content-Type: text/html
    ///         Sample-Header: mime-header-value
    ///         
    ///         <html><body>This is a HTML!</body></html>
    ///     --bound_1--
    ///         
    /// Using MimePart.Parse will return you a tree of three MimePart objects:
    ///   
    ///     MimePart (mult/alt)
    ///       MimePart (text/plain)
    ///       MimePart (text/html)
    ///       
    /// You can inspect header values through the MimePart.Headers collection.
    /// Access and shuffle child MimeParts with the MimePart.MimeParts list.
	/// </summary>
    public class MimePart
    {
        protected MimePart parent;
        protected MimePartCollection mimeParts;
        protected MimeHeaderCollection headers;
        protected string boundary;
        protected byte[] content;

        // Creation functions

        /// <summary>
        /// Parse and initialize a MIME part from given TextReader.
        /// </summary>
        public static MimePart Parse(string raw) 
        {
            return Load(new StringReader(raw));
        }
        
        /// <summary>
        /// Parse and initialize a MIME part from given TextReader.
        /// </summary>
        public static MimePart Load(TextReader reader) 
        {
            string last;
            return new MimePart(null, reader, out last);
        }
        
        /// <summary>
        /// Load a MIME message from a file. Counterpart of
        /// MimePart.Save.
        /// </summary>
        public static MimePart Load(string path) 
        {
            using (StreamReader reader = new StreamReader(path, MimeUtility.ISO))
            {
                return Load(reader);
            }
        }

        /// <summary>
        /// Creates a MIME part from file. Determines ContentType,
        /// ContentEncoding and TransferEncoding from the extension 
        /// of the file.
        /// </summary>
        public static MimePart CreateFromFile(string path)
        {
            return CreateFromFile(path, null, null, null);
        }
        
        /// <summary>
        /// Creates a MIME part from file. Determines ContentType,
        /// ContentEncoding and TransferEncoding from the extension 
        /// of the file.
        /// </summary>
        public static MimePart CreateFromFile(string path, Encoding encoding)
        {
            return CreateFromFile(path, null, encoding, null);
        }
        
        /// <summary>
        /// Creates a MIME part from a file. If ContentType, ContentEncoding
        /// TransferEncoding are not specified (i.e. null), this function
        /// will guess the right values based on the filename extension.
        /// </summary>
        public static MimePart CreateFromFile(string path, string contentType, Encoding contentEncoding, TransferEncoding transferEncoding)
        {
            MimePart part = new MimePart();
            if (contentType == null)
                contentType = MimeMapping.GetMimeMapping(path);
            part.ContentType = contentType;
            if (contentEncoding != null)
                part.ContentEncoding = contentEncoding;
            if (transferEncoding == null)
                if (contentType.StartsWith("text"))
                    transferEncoding = TransferEncoding.QuotedPrintable;
                else
                    transferEncoding = TransferEncoding.Base64;
            using (Stream input = File.OpenRead(path))
            {
                MemoryStream encoded = new MemoryStream();
                transferEncoding.Encode(input, encoded);
                part.Content = encoded.ToArray();
                part.TransferEncoding = transferEncoding;
            }
            return part;
        }
        
        // Constructors

        /// <summary>
        /// Creates an empty MimePart.
        /// </summary>
        public MimePart()
        {
            this.mimeParts = new MimePartCollection(this);
            this.headers = new MimeHeaderCollection();
        }

        /// <summary>
        /// Creates an MimePart with given Content-Type
        /// </summary>
        public MimePart(string contentType)
        {
            mimeParts = new MimePartCollection(this);
            headers = new MimeHeaderCollection();
            ContentType = contentType;
        }

        /// <summary>
        /// Private constructor used during parsing of MimePart.
        /// </summary>
        private MimePart(MimePart parent, TextReader reader, out string last)
        {
            this.mimeParts = new MimePartCollection(this);
            this.parent = parent;
            
            // Parse headers
            this.headers = MimeHeaderCollection.Parse(reader);
            
            // Check if current part is subdivided into child 
            // parts, if so boundary will be non-null.
            string bnd = Boundary; 
            if (bnd == null)
            {
                // This is a simple part, no child parts, so just read 
                // in data until we hit 
                this.content = MimeUtility.ReadBytesUntil(reader, parent != null ? parent.Boundary : null, out last);
            }
            else
            {
                // This MIME part is a multiform MIME itself, so parse its
                // content first, and then its children.
                this.content = MimeUtility.ReadBytesUntil(reader, bnd, out last);

                while (last == "--" + bnd)
                {
                    MimePart child = new MimePart(this, reader, out last);
                    mimeParts.list.Add(child);
                }
                if (last != null && last != "--" + bnd + "--")
                    throw new ArgumentException("MIME error, found '" + last + "', expected boundary '" + bnd + "'");
                MimeUtility.ReadUntil(reader, parent != null ? parent.Boundary : null, out last);
            }
        }

        public string GetTextContent()
        {
            byte[] encoded = TransferEncoding.Decode(Content);
            return ContentEncoding.GetString(encoded);
        }

        public void SetTextContent(string text)
        {
            byte[] encoded = ContentEncoding.GetBytes(text);
            Content = TransferEncoding.Encode(encoded);
        }

        /// <summary>
        /// Writes MIME message to stream.
        /// </summary>
        public virtual void Write(Stream output)
        {
            // Be sure to have a boundary before writing
            // multipart messages
            if (MimeParts.Count > 0)
                if (Boundary == null)
                    Boundary = MimeUtility.GenerateBoundary();

            // Write headers
            foreach (MimeHeader header in headers)
            {
                MimeUtility.WriteLine(output, header.ToString());
            }
            MimeUtility.WriteLine(output, "");

            // Start writing content
            if (MimeParts.Count > 0)
            {
                if (Parent == null)
                    MimeUtility.WriteLine(output, "This is a multi-part message in MIME format.");
                foreach (MimePart child in MimeParts)
                {
                    MimeUtility.WriteLine(output, "");
                    MimeUtility.WriteLine(output, "--" + Boundary);
                    child.Write(output);
                }
                MimeUtility.WriteLine(output, "");
                MimeUtility.WriteLine(output, "--" + Boundary + "--");
            }
            else
            {
                output.Write(content, 0, content.Length);
            }
        }

        /// <summary>
        /// Internal Insert method, called from MimePartCollection.Insert.
        /// Makes sure parent is ok.
        /// </summary>
        internal void Insert(MimePart child, MimePart before)
        {
            if (child.Parent != null)
                throw new ArgumentException("MIME part already belongs to a parent.");
            int i = mimeParts.list.IndexOf(before);
            if (i < 0)
                mimeParts.list.Add(child);
            else
                mimeParts.list.Insert(i, child);
            child.parent = this;
        }

        /// <summary>
        /// Internal Remove method, called from MimePartCollection.Remove.
        /// Makes sure parent is ok.
        /// </summary>
        internal void Remove(MimePart child)
        {
            mimeParts.list.Remove(child);
            child.parent = null;
        }

        /// <summary>
        /// The raw, transfer-encoded 7 bit ASCII message body.
        /// Caller must ensure this data is properly encoded
        /// and formatted.
        /// </summary>
        public byte[] Content
        {
            get 
            { 
                return content;
            }
            set
            {
                content = value;
            }
        }

        /// <summary>
        /// Gets or sets Content-Type header value. Returns 
        /// null if not set.
        /// </summary>
        public string ContentType 
        {
            get { return Headers.GetValue("Content-Type"); }
            set { Headers.SetValue("Content-Type", value); }
        } 

        /// <summary>
        /// Gets or sets Content-Type "charset" param value. Returns 
        /// null if not set.
        /// </summary>
        public string CharSet
        {
            get { return Headers.GetParam("Content-Type", "charset"); }
            set { Headers.SetParam("Content-Type", "charset", value); }
        }
	
        /// <summary>
        /// Gets or sets Content-Type "boundary" param value. Returns 
        /// null if not set.
        /// </summary>
        public string Boundary
        {
            get { return Headers.GetParam("Content-Type", "boundary"); }
            set { Headers.SetParam("Content-Type", "boundary", value); }
        }

        /// <summary>
        /// Gets or sets Content-Type "name" param value, Returns 
        /// null if not set.
        /// </summary>
        public string Name
        {
            get { return Headers.GetParam("Content-Type", "name"); }
            set { Headers.SetParam("Content-Type", "name", value); }
        }

        /// <summary>
        /// Gets or sets the System.Text.Encoding object based on CharSet 
        /// value. Will throw exception if CharSet is null or unknown.
        /// </summary>
        public Encoding ContentEncoding
        {
            get { return CharSet == null ? Encoding.ASCII : Encoding.GetEncoding(CharSet); }
            set { CharSet = value.BodyName; }
        }

        /// <summary>
        /// Gets or sets Content-Transfer-Encoding header value. Returns 
        /// null if not set.
        /// </summary>
        public string ContentTransferEncoding
        {
            get { return Headers.GetValue("Content-Transfer-Encoding"); }
            set { Headers.SetValue("Content-Transfer-Encoding", value); }
        }

        /// <summary>
        /// Gets or sets TransferEncoding object based on ContentTransferEncoding
        /// value. Will throw exception if ContentTransferEncoding is null or 
        /// unknown.
        /// </summary>
        public TransferEncoding TransferEncoding
        {
            get { return ContentTransferEncoding == null ? TransferEncoding.Bit7 : TransferEncoding.Get(ContentTransferEncoding); } 
            set { ContentTransferEncoding = value.Name; }
        }

        /// <summary>
        /// Gets or sets Content-ID header value. Returns 
        /// null if not set.
        /// </summary>
        public string ContentID
        {
            get { return Headers.GetValue("Content-ID"); }
            set { Headers.SetValue("Content-ID", value); }
        }

        /// <summary>
        /// Gets or sets Content-Disposition header value. Returns 
        /// null if not set.
        /// </summary>
        public string ContentDisposition 
        {
            get { return Headers.GetValue("Content-Disposition"); }
            set { Headers.SetValue("Content-Disposition", value); }
        } 

        /// <summary>
        /// Gets or sets Content-Disposition "filenae" param value.
        /// Returns null if not set.
        /// </summary>
        public string FileName
        {
            get { return Headers.GetParam("Content-Disposition", "filename"); }
            set { Headers.SetParam("Content-Disposition", "filename", value); }
        }

        /// <summary>
        /// Gets or sets Content-Base header value. Returns 
        /// null if not set.
        /// </summary>
        public string ContentBase 
        {
            get { return Headers.GetValue("Content-Base"); }
            set { Headers.SetValue("Content-Base", value); }
        }
	
        /// <summary>
        /// Gets or sets Content-Location header value. Returns 
        /// null if not set.
        /// </summary>
        public string ContentLocation 
        {
            get { return Headers.GetValue("Content-Location"); }
            set { Headers.SetValue("Content-Location", value); }
        }	
	
        /// <summary>
        /// Get all headers for this MIME part.
        /// </summary>
        public MimeHeaderCollection Headers 
        {
            get { return headers; } 
        }
	
        /*
        public bool IsAttachment
        {
            get { return false; }
        }

        public bool IsEmbedded
        {
            get { return false; }
        }

        public bool IsInline
        {
            get { return false; }
        }

        public bool IsMessage
        {
            get { return false; }
        }
        */

        /// <summary>
        /// Get all child MIME parts (in case of a multipart MIME)
        /// </summary>
        public MimePartCollection MimeParts
        {
            get { return mimeParts; }
        }

        /// <summary>
        /// Get parent MIME part.
        /// </summary>
        public MimePart Parent
        {
            get { return parent; }
        }
    }
}
