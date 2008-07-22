using System;
using System.IO;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// HttpConnection represents a HTTP over TCP conversation. Raw byte parsing
	/// and socket specifics are handled by this class, which creates (transport-
	/// agnostic) HttpRequest and HttpResponse classes and passes those to an 
	/// actual handler.
	/// </summary>
	public class HttpConnection : TcpConnection
	{
        // Header byte buffer
        const     int       maxHeaderBytes = 32768;
        protected byte[]    headerBytes;
        protected int       startHeadersOffset;
        protected int       endHeadersOffset;
        protected ArrayList headerByteStrings;
        protected string    allRawHeaders;

        // Content byte buffer
        protected int       contentLength;
        protected byte[]    preloadedContent;
        protected int       preloadedContentLength;
        
        // Headers
        protected string[]      knownRequestHeaders;
        protected string[][]    unknownRequestHeaders;

        protected bool keepAlive;

        // Parsed request variables
        protected string    verb;
        protected string    prot;
        protected string    url;
        protected string    path;
        protected string    pathInfo;
        protected string    filePath;
        protected string    queryString;
        protected byte[]    queryStringBytes;

        // Cached response
        private bool          headersSent;
        private int           responseStatus;
        private string        responseDescription;
        private int           responseContentLength;
        protected string[]    knownResponseHeaders;
        private bool          specialCaseStaticFileHeaders;

        /// <summary>
        /// 
        /// </summary>
        public HttpConnection(HttpServer server, Socket socket) : base(server, socket)
        {
        }

        public override void Process()
        {
            // Wait for at least some input
            if (WaitForBytes() == 0) 
            {
                Close();
                return;
            }
            do 
            {
                ReadHeaders();
                if (headerBytes == null)
                    break;

                ParseRequestLine();
                // Check for bad path
                // if (IsBadPath()) 
                ParseHeaders();
                ParsePostedContent();

                keepAlive = 
                    string.Compare(GetHttpVersion(), "1.1") >= 0 &&
                    GetKnownRequestHeader(HttpProtocol.HeaderConnection) != "Close" || 
                    GetKnownRequestHeader(HttpProtocol.HeaderConnection) == "Keep-Alive";
                keepAlive = 
                    GetKnownRequestHeader(HttpProtocol.HeaderConnection) == "Keep-Alive";

                PrepareResponse();
                HttpRequest request = Server.CreateRequest(this);
                HttpResponse response = Server.CreateResponse(this);
                ProcessRequest(request, response);
                response.Flush();
                if (!keepAlive)
                    break;

            } while (Connected);

            Close();
        }

        protected virtual void ProcessRequest(HttpRequest request, HttpResponse response)
        {
            Server.ProcessRequest(request, response);
        }

        // Request methods

        protected void ReadHeaders()
        {
            headerBytes = null;
            do 
            {
                if (!TryReadHeaders())
                {
                    break; // something bad happened
                }
            }
            while (endHeadersOffset < 0); // found \r\n\r\n
        }

        private bool TryReadHeaders() 
        {
            // read the first packet (up to 32K)
            byte[] buffer = base.ReadBytes(maxHeaderBytes);

            if (buffer == null || buffer.Length == 0)
                return false;

            if (headerBytes != null) 
            {
                // previous partial read
                int len = buffer.Length + headerBytes.Length;
                if (len > maxHeaderBytes)
                    return false;

                byte[] bytes = new byte[len];
                Buffer.BlockCopy(headerBytes, 0, bytes, 0, headerBytes.Length);
                Buffer.BlockCopy(buffer, 0, bytes, headerBytes.Length, buffer.Length);
                headerBytes = bytes;
            }
            else 
            {
                headerBytes = buffer;
            }

            // start parsing
            startHeadersOffset = -1;
            endHeadersOffset = -1;
            headerByteStrings = new ArrayList();

            // find the end of headers
            LineParser parser = new LineParser(headerBytes);

            for (;;) 
            {
                ByteString line = parser.ReadLine();

                if (line == null)
                    break;

                if (startHeadersOffset < 0) 
                {
                    startHeadersOffset = parser.CurrentOffset;
                }

                if (line.IsEmpty) 
                {
                    endHeadersOffset = parser.CurrentOffset;
                    break;
                }

                headerByteStrings.Add(line);
            }

            return true;
        }

        protected void ParseRequestLine()
        {
            ByteString requestLine = (ByteString)headerByteStrings[0];
            ByteString[] elems = requestLine.Split(' ');

            if (elems == null || elems.Length < 2 || elems.Length > 3) 
            {
                return;
            }

            // Get verb, url and protocol
            this.verb = elems[0].GetString();
            
            ByteString urlBytes = elems[1];
            this.url = urlBytes.GetString();

            if (elems.Length == 3)
                this.prot = elems[2].GetString();
            else
                this.prot = "HTTP/1.0";

            // query string
            int iqs = urlBytes.IndexOf('?');
            if (iqs > 0)
                queryStringBytes = urlBytes.Substring(iqs+1).GetBytes();
            else
                queryStringBytes = new byte[0];

            iqs = this.url.IndexOf('?');
            if (iqs > 0) 
            {
                this.path = this.url.Substring(0, iqs);
                this.queryString = this.url.Substring(iqs+1);
            }
            else 
            {
                this.path = this.url;
            }

            // url-decode path
            if (path.IndexOf('%') >= 0) 
                path = System.Web.HttpUtility.UrlDecode(path);

            // path info
            int lastDot = path.LastIndexOf('.');
            int lastSlh = path.LastIndexOf('/');

            if (lastDot >= 0 && lastSlh >= 0 && lastDot < lastSlh) 
            {
                int ipi = path.IndexOf('/', lastDot);
                filePath = path.Substring(0, ipi);
                pathInfo = path.Substring(ipi);
            }
            else 
            {
                filePath = path;
                pathInfo = string.Empty;
            }
        }
        
        protected void ParseHeaders()
        {
            knownRequestHeaders = new string[HttpProtocol.RequestHeaderMaximum];
            
            // construct unknown headers as array list of name1,value1,...
            ArrayList headers = new ArrayList();

            for (int i = 1; i < headerByteStrings.Count; i++) 
            {
                string s = ((ByteString)headerByteStrings[i]).GetString();

                int c = s.IndexOf(':');
                if (c >= 0) 
                {
                    String name = s.Substring(0, c).Trim();
                    String value = s.Substring(c+1).Trim();

                    // remember
                    int knownIndex = HttpProtocol.GetKnownRequestHeaderIndex(name);
                    if (knownIndex >= 0) 
                    {
                        knownRequestHeaders[knownIndex] = value;
                    }
                    else 
                    {
                        headers.Add(name);
                        headers.Add(value);
                    }
                }
            }

            // copy to array unknown headers
            int n = headers.Count / 2;
            unknownRequestHeaders = new string[n][];

            for (int i = 0, j = 0; i < n; i++) 
            {
                unknownRequestHeaders[i] = new string[2];
                unknownRequestHeaders[i][0] = (string)headers[j++];
                unknownRequestHeaders[i][1] = (string)headers[j++];
            }

            // remember all raw headers as one string
            if (headerByteStrings.Count > 1)
                allRawHeaders = Encoding.UTF8.GetString(headerBytes, startHeadersOffset, endHeadersOffset - startHeadersOffset);
            else
                allRawHeaders = string.Empty;
        }
        
        protected void ParsePostedContent() 
        {
            contentLength = 0;
            preloadedContentLength = 0;

            string contentLengthValue = knownRequestHeaders[HttpProtocol.HeaderContentLength];
            if (contentLengthValue != null) 
            {
                try 
                {
                    contentLength = Int32.Parse(contentLengthValue);
                }
                catch 
                {
                }
            }

            if (headerBytes.Length > endHeadersOffset) 
            {
                preloadedContentLength = headerBytes.Length - endHeadersOffset;

                if (preloadedContentLength > contentLength && contentLength > 0)
                    preloadedContentLength = contentLength; // don't read more than the content-length

                preloadedContent = new byte[preloadedContentLength];
                Buffer.BlockCopy(headerBytes, endHeadersOffset, preloadedContent, 0, preloadedContentLength);
            }
        }

        private static char[] badPathChars = new char[] { '%', '>', '<', '$', ':' };
        
        private bool IsBadPath() 
        {
            if (path == null)
                return true;

            if (path.IndexOfAny(badPathChars) >= 0)
                return true;

            if (path.IndexOf("..") >= 0)
                return true;

            return false;
        }

        private void PrepareResponse() 
        {
            headersSent = false;
            responseStatus = 200;
            knownResponseHeaders = new string[HttpProtocol.ResponseHeaderMaximum];
            responseContentLength = 0;
        }

        public HttpServer Server
        {
            get { return (HttpServer)server; }
        }

        public string GetUriPath() 
        {
            return path;
        }

        public string GetQueryString() 
        {
            return queryString;
        }

        public byte[] GetQueryStringBytes() 
        {
            return queryStringBytes;
        }

        public string GetRawUrl() 
        {
            return url;
        }

        public string GetHttpVerbName() 
        {
            return verb;
        }

        public string GetHttpVersion() 
        {
            return prot;
        }

        public string GetFilePath() 
        {
            return filePath;
        }

        public string GetPathInfo() 
        {
            return pathInfo;
        }

        public byte[] GetPreloadedEntityBody() 
        {
            return preloadedContent;
        }

        public bool IsEntireEntityBodyIsPreloaded() 
        {
            return (contentLength == preloadedContentLength);
        }

        public int ReadEntityBody(byte[] buffer, int size)  
        {
            int bytesRead = 0;
            byte[] bytes = ReadBytes(size);

            if (bytes != null && bytes.Length > 0) 
            {
                bytesRead = bytes.Length;
                Buffer.BlockCopy(bytes, 0, buffer, 0, bytesRead);
            }

            return bytesRead;
        }

        public string GetKnownRequestHeader(int index)  
        {
            return knownRequestHeaders[index];
        }
    
        public string GetUnknownRequestHeader(String name) 
        {
            int n = unknownRequestHeaders.Length;

            for (int i = 0; i < n; i++) 
            {
                if (string.Compare(name, unknownRequestHeaders[i][0], true, CultureInfo.InvariantCulture) == 0)
                    return unknownRequestHeaders[i][1];
            }

            return null;
        }

        public string[][] GetUnknownRequestHeaders() 
        {
            return unknownRequestHeaders;
        } 

        public string GetServerVariable(string name) 
        {
            string s = String.Empty;

            switch (name) 
            {
                case "ALL_RAW":
                    s = allRawHeaders;
                    break;
                case "SERVER_PROTOCOL":
                    s = prot;
                    break;
                    // more needed?
            }

            return s;
        }

        public bool HasEntityBody()
        {
            string cl = this.GetKnownRequestHeader(HttpProtocol.HeaderContentLength);
            if (cl != null && cl != "0")
                return true;

            if (GetKnownRequestHeader(HttpProtocol.HeaderTransferEncoding) != null)
                return true;

            if (GetPreloadedEntityBody() != null)
                return true;

            if (IsEntireEntityBodyIsPreloaded())
                return false;

            return false;
        }

        public bool HeadersSent() 
        {
            return headersSent;
        }

        public void SetStatus(int statusCode, String statusDescription) 
        {
            responseStatus = statusCode;
            responseDescription = statusDescription;
        }

        public void SetKnownResponseHeader(int index, string value) 
        {
            if (headersSent)
                return;

            switch (index) 
            {
                case HttpProtocol.HeaderServer:
                case HttpProtocol.HeaderDate:
                case HttpProtocol.HeaderConnection:
                    // ignore these
                    return;

                    // special case headers for static file responses
                case HttpProtocol.HeaderAcceptRanges:
                    if (value == "bytes") 
                    {
                        specialCaseStaticFileHeaders = true;
                        return;
                    }
                    break;
                case HttpProtocol.HeaderExpires:
                case HttpProtocol.HeaderLastModified:
                    if (specialCaseStaticFileHeaders)
                        return;
                    break;
                case HttpProtocol.HeaderSetCookie:
                    if (knownResponseHeaders[index] != null)
                        value = knownResponseHeaders[index] + "\r\nSet-Cookie: " + value;
                    break;
            }
            knownResponseHeaders[index] = value;
        }

        public void SetUnknownResponseHeader(string name, string value) 
        {
            if (headersSent)
                return;
            throw new NotImplementedException();
            /*
            responseHeadersBuilder.Append(name);
            responseHeadersBuilder.Append(": ");
            responseHeadersBuilder.Append(value);
            responseHeadersBuilder.Append("\r\n");
            */
        }

        public void SetCalculatedContentLength(int contentLength) 
        {
            if (headersSent) 
                return;
            SetKnownResponseHeader(HttpProtocol.HeaderContentLength, contentLength.ToString());
        }

        public void SendHeaders()
        {
            if (!headersSent)
            {
                StringBuilder headers = new StringBuilder();
                headers.Append("HTTP/1.1 " + responseStatus + " " + HttpProtocol.GetStatusDescription(responseStatus) + "\r\n");
                headers.Append("Server: Glue.Lib/1.1\r\n");
                headers.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");
                if (!keepAlive)
                    headers.Append("Connection: Close\r\n");
                if (responseContentLength > 0)
                    headers.Append("Content-Length: " + responseContentLength + "\r\n");
                for (int i = 0; i < HttpProtocol.ResponseHeaderMaximum; i++)
                    if (knownResponseHeaders[i] != null)
                        headers.Append(HttpProtocol.GetKnownResponseHeaderName(i) + ": " + knownResponseHeaders[i] + "\r\n");
                headers.Append("\r\n");
                SendBytes(Encoding.ASCII.GetBytes(headers.ToString()));
                headersSent = true;
            }
        }

        public void SendBody(byte[] bytes)
        {
            SendBody(bytes, 0, bytes.Length);
        }

        public void SendBody(byte[] bytes, int offset, int length)
        {
            SendHeaders();
            SendBytes(bytes, offset, length);
        }

        public void SendEntireResponseFromString(int statusCode, string body, bool keepAlive) 
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            SetStatus(statusCode, null);
            SetCalculatedContentLength(bytes.Length);
            SetKnownResponseHeader(HttpProtocol.HeaderContentType, "text/html; charset=utf-8");
            this.keepAlive = keepAlive;
            SendHeaders();
            SendBody(bytes);
        }

        public void SendErrorAndClose(int statusCode) 
        {
            SendErrorAndClose(statusCode, null);
        }

        public void SendErrorAndClose(int statusCode, string message) 
        {
            string body = string.Format(@"<html>
<head><title>{0} - {1}</title></head>
<body>
<h3>{0} - {1}</h3>
<p>{2}</p>
</body>
</html>",
                statusCode,
                HttpProtocol.GetStatusDescription(statusCode),
                message
                );
            if (message != null && message.Length > 0)
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            SendEntireResponseFromString(statusCode, body, false);
        }
    }
}
