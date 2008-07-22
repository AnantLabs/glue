using System;
using System.IO;
using System.Text;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// Summary description for HttpResponse.
	/// </summary>
	public class HttpResponse
	{
        // Owning connection
        protected HttpConnection conn;
        protected TextWriter writer;
        protected string contentType;
        protected Encoding encoding;
        protected bool ended;
        protected int statusCode;
        protected string statusDescription;
        protected string redirectLocation;
        
        public HttpResponse(HttpConnection conn)
		{
            this.conn = conn;
            this.writer = new StringWriter();
            this.contentType = "text/html";
		}

        public void End()
        {
            Flush();
        }

        public void Clear()
        {
            writer = new StringWriter();
        }

        public void Flush()
        {
            if (!ended)
            {
                conn.SetStatus(StatusCode, StatusDescription);
                conn.SetKnownResponseHeader(HttpProtocol.HeaderContentType, ContentType + "; charset=" + ContentEncoding.BodyName);
                if (RedirectLocation != null)
                    conn.SetKnownResponseHeader(HttpProtocol.HeaderLocation, RedirectLocation);
                //conn.SetKnownResponseHeader(HttpProtocol.HeaderContentEncoding, ContentEncoding.BodyName);
                byte[] bytes = ContentEncoding.GetBytes(writer.ToString());
                conn.SetCalculatedContentLength(bytes.Length);
                conn.SendHeaders();
                conn.SendBody(bytes);
                ended = true;
            }
        }

        public void Write(char c)
        {
            writer.Write(c);
        }
    
        public void Write(string s)
        {
            writer.Write(s);
        }

        public void Write(string format, params object[] arg)
        {
            writer.Write(format, arg);
        }

        public void TransmitFile(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long len = stream.Length;
                byte[] bytes = new byte[len];
                int n = stream.Read(bytes, 0, (int)len);
                conn.SetStatus(StatusCode, StatusDescription);
                conn.SetKnownResponseHeader(HttpProtocol.HeaderContentType, ContentType);
                conn.SetCalculatedContentLength(n);
                conn.SetKnownResponseHeader(HttpProtocol.HeaderAcceptRanges, "bytes");
                conn.SendHeaders();
                conn.SendBody(bytes, 0, n);
                ended = true;
            }
        }

        public string ContentType
        {
            get 
            {
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        public Encoding ContentEncoding
        {
            get 
            {
                if (encoding == null)
                {
                    this.encoding = Encoding.UTF8; // TODO: config? Default?;
                }
                return encoding;
            }
            set
            {
                if (encoding == null || !encoding.Equals(value))
                {
                    this.encoding = value;
                }
            }
        }

        public string RedirectLocation
        {
            get { return redirectLocation; }
            set { redirectLocation = value; }
        }

        public TextWriter Output
        {
            get { return writer; }
            set { writer.Flush(); writer = value; }
        }

        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public string StatusDescription
        {
            get { return statusDescription != null ? statusDescription : HttpProtocol.GetStatusDescription(StatusCode); }
            set { statusDescription = value; }
        }
    }
}
