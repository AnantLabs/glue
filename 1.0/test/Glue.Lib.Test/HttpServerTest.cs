using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.Collections.Specialized;
using System.Xml;
using System.Text;
using Glue.Lib.Servers;
using NUnit.Framework;

namespace Glue.Lib.Test
{
    /// <summary>
    /// Summary description for HttpServerTest
    /// </summary>
    [TestFixture]
    public class HttpServerTest 
    {
        HttpServer server = null;

        public HttpServerTest()
        {
        }

        [SetUp]
        public void Setup()
        {
            server = new MyHttpServer();
        }

        [TearDown]
        public void Done()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }
        }

        [Test]
        public void Run()
        {
            server.Start();
            server.Stop();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class MyHttpServer : HttpServer
    {
        /// <summary>
        /// MyHttpServer
        /// </summary>
        public MyHttpServer() : base(new IPEndPoint(IPAddress.Any, 8888))
        {
        }

        public override void ProcessRequest(HttpRequest request, HttpResponse response)
        {
            return;
        }
    }

    class MyHttpConnection : HttpConnection
    {
        const string rootDirectory = "c:\\temp\\";

        protected StringBuilder responseBuffer = new StringBuilder();

        public MyHttpConnection(HttpServer server, Socket socket) : base(server, socket)
        {
        }
        
        public override void Process()
        {
            // wait for at least some input
            if (WaitForBytes() == 0) 
            {
                return;
            }
            
            do
            {
                headerBytes = null;
                startHeadersOffset = -1;
                endHeadersOffset = -1;
                responseBuffer = new StringBuilder();

                ReadHeaders();

                ParseRequestLine();
                ParseHeaders();
                
                keepAlive = knownRequestHeaders[HttpProtocol.HeaderConnection] == "Keep-Alive";

                Log.Info("Verb={0}", verb);

                switch (verb)
                {
                    case "GET":
                        Get(true);
                        break;
                    case "HEAD":
                        Get(false);
                        break;
                    case "OPTIONS":
                        Options();
                        break;
                    case "POST":
                        Post();
                        break;
                    case "PROPFIND":
                        PropFind();
                        break;
                    default:
                        keepAlive = false;
                        break;
                }
            
            } while (keepAlive);

            Close();
        }

        public void Get()
        {
            Get(true);
        }
        
        public void Get(bool content)
        {
            string s = 
                "Hello hello\r\n" +
                "Prot=" + prot + "\r\n" +
                "Url=" + url + "\r\n" +
                "Path=" + path + "\r\n" +
                "FilePath=" + filePath + "\r\n" +
                "PathInfo=" + pathInfo + "\r\n" +
                "QueryString=" + queryString + "\r\n";

            AddResponseHeader("Content-Type", "text/plain; charset=\"utf-8\"");
            SendResponse(200, s.Length, false);
            if (content)
                SendBytes(Encoding.UTF8.GetBytes(s));
        }

        public void Head()
        {
            Get(false);
        }

        public void Options()
        {
            AddResponseHeader("Allow", "GET, HEAD, POST, PUT, PROPFIND, PROPPATCH, OPTIONS");
            AddResponseHeader("Content-Type", "text/plain");
            AddResponseHeader("DAV", "1, 2");
            AddResponseHeader("MS-Author-Via", "DAV");
            SendResponse(200, 0, false);
        }

        public void Post()
        {
            SendResponse(404, 0, false);
        }

        public void PropFind()
        {
            ParsePostedContent();

            if (preloadedContent != null)
                Log.Info(Encoding.UTF8.GetString(preloadedContent));

            AddResponseHeader("Content-Type", "text/xml");
            
            //byte[] data = GetPropFindResponse(0, "c:\\temp");
            // Log(Encoding.UTF8.GetString(data));
            SendResponse(207, -1, false);
            SendPropFindResponse(0, "c:\\temp");

            //SendBytes(data);
            /*
            SendComplete(207, false, @"<?xml version='1.0' encoding='utf-8' ?>\r
<multistatus xmlns='DAV:' xmlns:b='urn:schemas-microsoft-com:datatypes'>\r
    <response>\r
        <href>/</href>\r
        <propstat>\r
            <status>HTTP/1.1 200 OK</status>\r
            <prop>\r
                <getcontentlength>1320</getcontentlength>\r
                <creationdate>2004-02-27T04:45:37.228210Z</creationdate>\r
                <displayname>Root collection</displayname>\r
                <getetag>FAE04EC0301F11D3BF4B00C04F79EFBC</getetag>\r
                <getlastmodified>Fri, 27 Feb 2004 04:45:37 GMT</getlastmodified>\r
                <resourcetype><collection/></resourcetype>\r
                <iscollection>1</iscollection>\r
                <isroot>1</isroot>\r
                <getcontenttype>text/html</getcontenttype>\r
            </prop>\r
        </propstat>\r
    </response>\r
    <response>\r
        <href>/front.html</href>\r
        <propstat>\r
            <status>HTTP/1.1 200 OK</status>\r
            <prop>\r
                <getcontentlength>1320</getcontentlength>\r
                <getcontenttype>text/html</getcontenttype>\r
                <creationdate>2004-02-27T04:45:37.228210Z</creationdate>\r
                <displayname>Example HTML resource</displayname>\r
                <getetag>FAE04EC1301F11D3BF4B00C04F79EFBC</getetag>\r
                <getlastmodified>Fri, 27 Feb 2004 04:45:37 GMT</getlastmodified>\r
                <resourcetype/>\r
                <iscollection>0</iscollection>\r
            </prop>\r
        </propstat>\r
    </response>\r
    <response>\r
        <href>/container/</href>\r
        <propstat>\r
            <status>HTTP/1.1 200 OK</status>\r
            <prop>\r
                <getcontentlength>1320</getcontentlength>\r
                <getcontenttype>text/html</getcontenttype>\r
                <creationdate>2004-02-27T04:45:37.228210Z</creationdate>\r
                <displayname>Example HTML resource</displayname>\r
                <getetag>FAE04EC1301F11D3BF4B00C04F79EFBC</getetag>\r
                <getlastmodified>Thu, 26 Feb 2004 04:45:37 GMT</getlastmodified>\r
                <resourcetype><collection/></resourcetype>\r
                <iscollection>1</iscollection>\r
                <getcontenttype>text/html</getcontenttype>\r
            </prop>\r
        </propstat>\r
    </response>\r
</multistatus>\r
"
                );
            */
        }

        protected void SendPropFindResponse(int depth, string path)
        {
            NetworkStream stm = new NetworkStream(this.socket, false);
            XmlTextWriter writer = new XmlTextWriter(stm, Encoding.UTF8);
            
            writer.WriteStartDocument();
            writer.WriteStartElement("multistatus", "DAV:");
            writer.WriteAttributeString("xmlns:b","urn:schemas-microsoft-com:datatypes");

            DirectoryInfo dir = new DirectoryInfo(path);
            
            writer.WriteStartElement("response");
            writer.WriteStartElement("href");
            writer.WriteString("/");
            writer.WriteEndElement();
            writer.WriteStartElement("propstat");
            writer.WriteStartElement("status");
            writer.WriteString("HTTP/1.1 200 OK");
            writer.WriteEndElement();
            WriteProperties(writer, dir);
            writer.WriteEndElement();
            writer.WriteEndElement();

            foreach (DirectoryInfo sub in dir.GetDirectories())
            {
                writer.WriteStartElement("response");
                writer.WriteStartElement("href");
                writer.WriteString("/");
                writer.WriteEndElement();
                writer.WriteStartElement("propstat");
                writer.WriteStartElement("status");
                writer.WriteString("HTTP/1.1 200 OK");
                writer.WriteEndElement();
                WriteProperties(writer, sub);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                writer.WriteStartElement("response");
                writer.WriteStartElement("href");
                writer.WriteString("/");
                writer.WriteEndElement();
                writer.WriteStartElement("propstat");
                writer.WriteStartElement("status");
                writer.WriteString("HTTP/1.1 200 OK");
                writer.WriteEndElement();
                WriteProperties(writer, file);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            
            stm.Close();

            //return stm.ToArray();
        }

        protected void WriteProperties(XmlWriter writer, DirectoryInfo info)
        {
            writer.WriteStartElement("prop");
            
            writer.WriteStartElement("getcontentlength");   
            writer.WriteString("1234");         
            writer.WriteEndElement();
            
            writer.WriteStartElement("getcontenttype");     
            writer.WriteString("text/html");
            writer.WriteEndElement();
            
            writer.WriteStartElement("creationdate");
            writer.WriteString(info.CreationTimeUtc.ToString("yyyy-MM-ddThh:mm:sss.000Z"));//2003-11-28T18:08:57.665Z
            writer.WriteEndElement();
            
            writer.WriteStartElement("displayname");        
            writer.WriteString(info.Name);   
            writer.WriteEndElement();
            
            //writer.WriteStartElement("getetag"); 
            //writer.WriteEndElement();
            
            writer.WriteStartElement("getlastmodified"); 
            writer.WriteString(info.LastWriteTimeUtc.ToString("r"));//rfc1123
            writer.WriteEndElement();
            
            writer.WriteStartElement("resourcetype"); 
            writer.WriteStartElement("collection"); 
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("iscollection"); 
            writer.WriteString("1");   
            writer.WriteEndElement();
            
            writer.WriteEndElement();
        }

        protected void WriteProperties(XmlWriter writer, FileInfo info)
        {
            writer.WriteStartElement("prop");
            
            writer.WriteStartElement("getcontentlength");
            writer.WriteString(info.Length.ToString());
            writer.WriteEndElement();
            
            writer.WriteStartElement("getcontenttype");
            writer.WriteString("text/html");
            writer.WriteEndElement();
            
            writer.WriteStartElement("creationdate");
            writer.WriteString(info.CreationTimeUtc.ToString("yyyy-MM-ddThh:mm:sss.000Z"));//2003-11-28T18:08:57.665Z
            writer.WriteEndElement();
            
            writer.WriteStartElement("displayname");        
            writer.WriteString(info.Name);   
            writer.WriteEndElement();
            
            //writer.WriteStartElement("getetag"); 
            //writer.WriteEndElement();
            
            writer.WriteStartElement("getlastmodified"); 
            writer.WriteString(info.LastWriteTimeUtc.ToString("r"));//rfc1123
            writer.WriteEndElement();
            
            writer.WriteStartElement("resourcetype"); 
            writer.WriteEndElement();

            writer.WriteStartElement("iscollection"); 
            writer.WriteString("0");   
            writer.WriteEndElement();
            
            writer.WriteEndElement();
        }

        protected void AddResponseHeader(string name, string value)
        {
            responseBuffer.Append(name + ": " + value + "\r\n");
        }

        protected void SendResponse(int statusCode, int contentLength, bool keepAlive)
        {
            StringBuilder text = new StringBuilder();
            text.Append("HTTP/1.1 " + statusCode + " " + HttpProtocol.GetStatusDescription(statusCode) + "\r\n");
            text.Append("Server: MyTest/1.1\r\n");
            text.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");
            if (contentLength >= 0)
                text.Append("Content-Length: " + contentLength + "\r\n");
            text.Append(responseBuffer.ToString());
            if (!keepAlive)
                text.Append("Connection: Close\r\n");
            else
                text.Append("Connection: Keep-Alive\r\n");
            this.keepAlive = keepAlive;
            text.Append("\r\n");
            SendBytes(Encoding.UTF8.GetBytes(text.ToString()));
        }

        protected void SendComplete(int statusCode, bool keepAlive, string body)
        {
            byte[] data = Encoding.UTF8.GetBytes(body);
            SendResponse(statusCode, data.Length, keepAlive);
            SendBytes(data);
        }

    }


}
