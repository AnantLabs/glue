using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;
using System.Web;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// Summary description for HttpProtocol.
	/// </summary>
    public class HttpProtocol
    {
        public const int HeaderAccept = 20;
        public const int HeaderAcceptCharset = 21;
        public const int HeaderAcceptEncoding = 22;
        public const int HeaderAcceptLanguage = 23;
        public const int HeaderAcceptRanges = 20;
        public const int HeaderAge = 21;
        public const int HeaderAllow = 10;
        public const int HeaderAuthorization = 24;
        public const int HeaderCacheControl = 0;
        public const int HeaderConnection = 1;
        public const int HeaderContentEncoding = 13;
        public const int HeaderContentLanguage = 14;
        public const int HeaderContentLength = 11;
        public const int HeaderContentLocation = 15;
        public const int HeaderContentMd5 = 16;
        public const int HeaderContentRange = 17;
        public const int HeaderContentType = 12;
        public const int HeaderCookie = 25;
        public const int HeaderDate = 2;
        public const int HeaderEtag = 22;
        public const int HeaderExpect = 26;
        public const int HeaderExpires = 18;
        public const int HeaderFrom = 27;
        public const int HeaderHost = 28;
        public const int HeaderIfMatch = 29;
        public const int HeaderIfModifiedSince = 30;
        public const int HeaderIfNoneMatch = 31;
        public const int HeaderIfRange = 32;
        public const int HeaderIfUnmodifiedSince = 33;
        public const int HeaderKeepAlive = 3;
        public const int HeaderLastModified = 19;
        public const int HeaderLocation = 23;
        public const int HeaderMaxForwards = 34;
        public const int HeaderPragma = 4;
        public const int HeaderProxyAuthenticate = 24;
        public const int HeaderProxyAuthorization = 35;
        public const int HeaderRange = 37;
        public const int HeaderReferer = 36;
        public const int HeaderRetryAfter = 25;
        public const int HeaderServer = 26;
        public const int HeaderSetCookie = 27;
        public const int HeaderTe = 38;
        public const int HeaderTrailer = 5;
        public const int HeaderTransferEncoding = 6;
        public const int HeaderUpgrade = 7;
        public const int HeaderUserAgent = 39;
        public const int HeaderVary = 28;
        public const int HeaderVia = 8;
        public const int HeaderWarning = 9;
        public const int HeaderWwwAuthenticate = 29;
        public const int ReasonCachePolicy = 2;
        public const int ReasonCacheSecurity = 3;
        public const int ReasonClientDisconnect = 4;
        public const int ReasonDefault = 0;
        public const int ReasonFileHandleCacheMiss = 1;
        public const int ReasonResponseCacheMiss = 0;
        public const int RequestHeaderMaximum = 40;
        public const int ResponseHeaderMaximum = 30;

        public static string GetStatusDescription(int code)
        {
            if (code >= 100 && code < 600)
            {
                if ((code % 100) < statusDescriptions[code / 100].Length)
                    return statusDescriptions[code / 100][code % 100];
            }
            return "";
        }
        
        public static string GetKnownRequestHeaderName(int index)
        {
            return requestHeaderNames[index];
        }

        public static int GetKnownRequestHeaderIndex(string header)
        {
            object o = requestHeaderLookup[header];
            return o != null ? (int)o : -1;
        }

        public static string GetKnownResponseHeaderName(int index)
        {
            return responseHeaderNames[index];
        }

        public static int GetKnownResponseHeaderIndex(string header)
        {
            object o = responseHeaderLookup[header];
            return o != null ? (int)o : -1;
        }

        public static string GetAttributeFromHeader(string headerValue, string attrName)
        {
            if (headerValue == null)
                return null;
            int hlen = headerValue.Length;
            int alen = attrName.Length;
            int pos = 1;
            while (pos < hlen) 
            {
                pos = CultureInfo.InvariantCulture.CompareInfo.IndexOf(headerValue, attrName, pos, CompareOptions.IgnoreCase);
                if (pos < 0 || (pos + alen) >= hlen)
                    return null;
                if ((headerValue[pos-1] == ';' || char.IsWhiteSpace(headerValue[pos-1])) &&
                    (headerValue[pos+alen] == '=' || char.IsWhiteSpace(headerValue[pos+alen])))
                    break;
                pos += alen;
            }
            if (pos >= hlen)
                return null;
            pos += alen;
            while (pos < hlen && char.IsWhiteSpace(headerValue[pos]))
                pos++;
            if (pos >= hlen || headerValue[pos] != '=')
                return null;
            pos++;
            while (pos < hlen && char.IsWhiteSpace(headerValue[pos]))
                pos++;
            if (pos >= hlen)
                return null;
            if (headerValue[pos] == '"')
            {
                pos++;
                if (pos >= hlen)
                    return null;
                int end = headerValue.IndexOf('"', pos);
                if (end < 0 || end == pos)
                    return null;
                return headerValue.Substring(pos, end - pos).Trim();
            } 
            else
            {
                int end = pos;
                while (end < hlen && !char.IsWhiteSpace(headerValue[end]))
                    end++;
                if (end == pos)
                    return null;
                return headerValue.Substring(pos, end - pos).Trim();
            }
        }

        public static void FillValuesFromEncodedBytes(NameValueCollection values, byte[] bytes, Encoding encoding)
        {
            int len = (bytes != null) ? bytes.Length : 0;
            for (int i = 0; i < len; i++)
            {
                string name;
                string val;
                int pos = i;
                int sep = -1;
                while (i < len)
                {
                    if (bytes[i] == 0x3d) // =
                    {
                        if (sep < 0)
                            sep = i;
                    }
                    else if (bytes[i] == 0x26) // &
                    {
                        break;
                    }
                    i++;
                }
                if (sep >= 0)
                {
                    name = HttpUtility.UrlDecode(bytes, pos, sep - pos, encoding);
                    val = HttpUtility.UrlDecode(bytes, sep + 1, (i - sep) - 1, encoding);
                }
                else
                {
                    name = null;
                    val = HttpUtility.UrlDecode(bytes, pos, i - pos, encoding);
                }
                values.Add(name, val);
                if ((i == (len - 1)) && (bytes[i] == 0x26))
                {
                    values.Add(null, "");
                }
            }
        }

        public static void FillValuesFromString(NameValueCollection values, string s)
        {
            FillValuesFromString(values, s, false, null);
        }

        public static void FillValuesFromString(NameValueCollection values, string s, bool urlencoded, Encoding encoding)
        {
            int len = (s != null) ? s.Length : 0;
            for (int i = 0; i < len; i++)
            {
                int pos = i;
                int sep = -1;
                while (i < len)
                {
                    if (s[i] == '=')
                    {
                        if (sep < 0)
                            sep = i;
                    }
                    else if (s[i] == '&')
                    {
                        break;
                    }
                    i++;
                }
                string name = null;
                string val = null;
                if (sep >= 0)
                {
                    name = s.Substring(pos, sep - pos);
                    val = s.Substring(sep + 1, (i - sep) - 1);
                }
                else
                {
                    val = s.Substring(pos, i - pos);
                }
                if (urlencoded)
                {
                    values.Add(HttpUtility.UrlDecode(name, encoding), HttpUtility.UrlDecode(val, encoding));
                }
                else
                {
                    values.Add(name, val);
                }
                if ((i == (len - 1)) && (s[i] == '&'))
                {
                    values.Add(null, "");
                }
            }
        }

        public static bool StringStartsWithIgnoreCase(string test, string search)
        {
            return (string.Compare(test, 0, search, 0, search.Length, true, CultureInfo.InvariantCulture) == 0);
        }

        static string[]     requestHeaderNames;
        static Hashtable    requestHeaderLookup;
        static string[]     responseHeaderNames;
        static Hashtable    responseHeaderLookup;
        static string[][]   statusDescriptions;

        static HttpProtocol()
        {
            statusDescriptions = new string[6][];

            string[] r;

            statusDescriptions[1] = r = new string[3];
            r[0] = "Continue";
            r[1] = "Switching Protocols";
            r[2] = "Processing";
            
            statusDescriptions[2] = r = new string[8];
            r[0] = "OK";
            r[1] = "Created";
            r[2] = "Accepted";
            r[3] = "Non-Authoritative Information";
            r[4] = "No Content";
            r[5] = "Reset Content";
            r[6] = "Partial Content";
            r[7] = "Multi-Status";
 
            statusDescriptions[3] = r = new string[8];
            r[0] = "Multiple Choices";
            r[1] = "Moved Permanently";
            r[2] = "Found";
            r[3] = "See Other";
            r[4] = "Not Modified";
            r[5] = "Use Proxy";
            r[6] = "";
            r[7] = "Temporary Redirect";
            
            statusDescriptions[4] = r = new string[25];
            r[0] = "Bad Request";
            r[1] = "Unauthorized";
            r[2] = "Payment Required";
            r[3] = "Forbidden";
            r[4] = "Not Found";
            r[5] = "Method Not Allowed";
            r[6] = "Not Acceptable";
            r[7] = "Proxy Authentication Required";
            r[8] = "Request Timeout";
            r[9] = "Conflict";
            r[10] = "Gone";
            r[11] = "Length Required";
            r[12] = "Precondition Failed";
            r[13] = "Request Entity Too Large";
            r[14] = "Request-Uri Too Long";
            r[15] = "Unsupported Media Type";
            r[16] = "Requested Range Not Satisfiable";
            r[17] = "Expectation Failed";
            r[18] = "";
            r[19] = "";
            r[20] = "";
            r[21] = "";
            r[22] = "Unprocessable Entity";
            r[23] = "Locked";
            r[24] = "Failed Dependency";

            statusDescriptions[5] = r = new string[8];
            r[0] = "Internal Server Error";
            r[1] = "Not Implemented";
            r[2] = "Bad Gateway";
            r[3] = "Service Unavailable";
            r[4] = "Gateway Timeout";
            r[5] = "Http Version Not Supported";
            r[6] = "";
            r[7] = "Insufficient Storage";
 
            requestHeaderNames = new string[RequestHeaderMaximum];
            requestHeaderLookup = new Hashtable();
            responseHeaderNames = new string[ResponseHeaderMaximum];
            responseHeaderLookup = new Hashtable();
            
            DefineHeader(true,  true,   0, "Cache-Control");
            DefineHeader(true,  true,   1, "Connection");
            DefineHeader(true,  true,   2, "Date");
            DefineHeader(true,  true,   3, "Keep-Alive");
            DefineHeader(true,  true,   4, "Pragma");
            DefineHeader(true,  true,   5, "Trailer");
            DefineHeader(true,  true,   6, "Transfer-Encoding");
            DefineHeader(true,  true,   7, "Upgrade");
            DefineHeader(true,  true,   8, "Via");
            DefineHeader(true,  true,   9, "Warning");
            DefineHeader(true,  true,  10, "Allow");
            DefineHeader(true,  true,  11, "Content-Length");
            DefineHeader(true,  true,  12, "Content-Type");
            DefineHeader(true,  true,  13, "Content-Encoding");
            DefineHeader(true,  true,  14, "Content-Language");
            DefineHeader(true,  true,  15, "Content-Location");
            DefineHeader(true,  true,  16, "Content-MD5");
            DefineHeader(true,  true,  17, "Content-Range");
            DefineHeader(true,  true,  18, "Expires");
            DefineHeader(true,  true,  19, "Last-Modified");
            DefineHeader(true,  false, 20, "Accept");
            DefineHeader(true,  false, 21, "Accept-Charset");
            DefineHeader(true,  false, 22, "Accept-Encoding");
            DefineHeader(true,  false, 23, "Accept-Language");
            DefineHeader(true,  false, 24, "Authorization");
            DefineHeader(true,  false, 25, "Cookie");
            DefineHeader(true,  false, 26, "Expect");
            DefineHeader(true,  false, 27, "From");
            DefineHeader(true,  false, 28, "Host");
            DefineHeader(true,  false, 29, "If-Match");
            DefineHeader(true,  false, 30, "If-Modified-Since");
            DefineHeader(true,  false, 31, "If-None-Match");
            DefineHeader(true,  false, 32, "If-Range");
            DefineHeader(true,  false, 33, "If-Unmodified-Since");
            DefineHeader(true,  false, 34, "Max-Forwards");
            DefineHeader(true,  false, 35, "Proxy-Authorization");
            DefineHeader(true,  false, 36, "Referer");
            DefineHeader(true,  false, 37, "Range");
            DefineHeader(true,  false, 38, "TE");
            DefineHeader(true,  false, 39, "User-Agent");
            DefineHeader(false, true,  20, "Accept-Ranges");
            DefineHeader(false, true,  21, "Age");
            DefineHeader(false, true,  22, "ETag");
            DefineHeader(false, true,  23, "Location");
            DefineHeader(false, true,  24, "Proxy-Authenticate");
            DefineHeader(false, true,  25, "Retry-After");
            DefineHeader(false, true,  26, "Server");
            DefineHeader(false, true,  27, "Set-Cookie");
            DefineHeader(false, true,  28, "Vary");
            DefineHeader(false, true,  29, "WWW-Authenticate");
        }

        private static void DefineHeader(bool isRequest, bool isResponse, int index, string name)
        {
            if (isRequest)
            {
                requestHeaderNames[index] = name;
                requestHeaderLookup.Add(name, index);
            }
            if (isResponse)
            {
                responseHeaderNames[index] = name;
                responseHeaderLookup.Add(name, index);
            }
        }
    }
}
