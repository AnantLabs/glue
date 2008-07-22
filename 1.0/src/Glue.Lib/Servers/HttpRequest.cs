using System;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Glue.Lib.Servers
{
    /// <summary>
    /// Summary description for HttpRequest.
    /// </summary>
    public class HttpRequest
    {
        // Owning connection
        protected HttpConnection conn;
        protected Encoding encoding;
        protected int contentLength;
        protected string contentType;
        protected byte[] queryStringBytes;
        protected string queryStringText;
        protected byte[] rawContent;
        protected Uri url;

        protected NameValueCollection paramsCollection;
        protected NameValueCollection formCollection;
        protected NameValueCollection queryStringCollection;
        protected NameValueCollection cookieCollection;

        public HttpRequest(HttpConnection connection)
        {
            this.conn = connection;
        }

        private void FillFormCollection()
        {
            if (conn != null)
            {
                if (conn.HasEntityBody())
                {
                    string ct = ContentType;
                    if (ct != null)
                    {
                        if (HttpProtocol.StringStartsWithIgnoreCase(ct, "application/x-www-form-urlencoded"))
                        {
                            byte[] bytes = GetEntireRawContent();
                            if (bytes == null)
                                return;
                            try
                            {
                                HttpProtocol.FillValuesFromEncodedBytes(formCollection, bytes, ContentEncoding);
                            }
                            catch (Exception e)
                            {
                                throw new System.Web.HttpException("Invalid_urlencoded_form_data", e);
                            }
                            return;
                        }
                        /*
                        if (HttpProtocol.StringStartsWithIgnoreCase(ct, "multipart/form-data"))
                        {
                            MultipartContentElement[] elementArray1 = this.GetMultipartContent();
                            if (elementArray1 != null)
                            {
                                for (int num1 = 0; num1 < elementArray1.Length; num1++)
                                {
                                    if (elementArray1[num1].IsFormItem)
                                    {
                                        this._form.Add(elementArray1[num1].Name, elementArray1[num1].GetAsString(this.ContentEncoding));
                                    }
                                }
                            }
                        }
                        */
                    }
                }
            }
        }
 
        private void FillQueryStringCollection()
        {
            byte[] bytes = QueryStringBytes;
            if (bytes == null)
                HttpProtocol.FillValuesFromString(queryStringCollection, QueryStringText, true, ContentEncoding);
            else if (bytes.Length != 0)
                HttpProtocol.FillValuesFromEncodedBytes(queryStringCollection, bytes, ContentEncoding);
        }

        private void FillCookieCollection()
        {
            string header = conn.GetKnownRequestHeader(HttpProtocol.HeaderCookie);
            if (header == null || header.Length == 0)
                return;
            foreach (string cookie in header.Split(';'))
            {
                int i = cookie.IndexOf('=');
                if (i < 0)
                    continue;
                cookieCollection.Add(cookie.Substring(0, i), cookie.Substring(i + 1));
            }
        }
 
        private void FillParamsCollection()
        {
            paramsCollection.Add(this.QueryString);
            paramsCollection.Add(this.Form);
            //paramsCollection.Add(this.Cookies);
            //paramsCollection.Add(this.ServerVariables);
        }
 
        private Encoding GetEncodingFromHeaders()
        {
            if (!conn.HasEntityBody())
                return null;
            
            string ct = ContentType;
            if (ct == null)
                return null;

            string ch = HttpProtocol.GetAttributeFromHeader(ct, "charset");
            if (ch == null)
                return null;

            Encoding enc = null;
            try
            {
                enc = Encoding.GetEncoding(ch);
            }
            catch
            {
            }
            return enc;
        }
         

        private byte[] GetEntireRawContent()
        {
            if (conn == null)
                return null;
            if (rawContent != null)
                return rawContent;
            
            /*
            HttpRuntimeConfig config1 = (HttpRuntimeConfig) this._context.GetConfig("system.web/httpRuntime");
            int num1 = (config1 != null) ? config1.MaxRequestLength : 0x400000;
            if (this.ContentLength > num1)
            {
                this.Response.CloseConnectionAfterError();
                throw new HttpException(400, HttpRuntime.FormatResourceString("Max_request_length_exceeded"));
            }
            */

            byte[] buffer1 = conn.GetPreloadedEntityBody();
            if (buffer1 == null)
            {
                buffer1 = new byte[0];
            }
            if (!conn.IsEntireEntityBodyIsPreloaded())
            {
                int num1 = 0x40000;
                int num2 = (this.ContentLength > 0) ? (this.ContentLength - buffer1.Length) : 0x7fffffff;
                ArrayList list1 = new ArrayList();
                int num3 = 0;
                int num4 = 0;
                while (num2 > 0)
                {
                    byte[] buffer2 = new byte[0x10000];
                    int num5 = buffer2.Length;
                    if (num5 > num2)
                    {
                        num5 = num2;
                    }
                    int num6 = conn.ReadEntityBody(buffer2, num5);
                    if (num6 <= 0)
                    {
                        break;
                    }
                    num2 -= num6;
                    list1.Add(num6);
                    list1.Add(buffer2);
                    num3++;
                    num4 += num6;
                    if (num4 > num1)
                    {
                        throw new System.Web.HttpException("Max_request_length_exceeded");
                    }
                }
                if (num4 > 0)
                {
                    int num7 = buffer1.Length;
                    byte[] buffer3 = new byte[num7 + num4];
                    if (num7 > 0)
                    {
                        Array.Copy(buffer1, 0, buffer3, 0, num7);
                    }
                    int num8 = num7;
                    for (int num9 = 0; num9 < num3; num9++)
                    {
                        int num10 = (int) list1[2 * num9];
                        byte[] buffer4 = (byte[]) list1[(2 * num9) + 1];
                        Array.Copy(buffer4, 0, buffer3, num8, num10);
                        num8 += num10;
                    }
                    buffer1 = buffer3;
                }
            }
            return buffer1;
        }

        public HttpConnection Connection
        {
            get { return conn; }
        }

        public Encoding ContentEncoding
        {
            get
            {
                if (encoding == null)
                {
                    encoding = this.GetEncodingFromHeaders();
                    if (encoding == null)
                    {
                        // GlobalizationConfig config1 = (GlobalizationConfig) context.GetConfig("system.web/globalization");
                        //if (config1 != null)
                        //{
                        //    encoding = config1.RequestEncoding;
                        //}
                        //else
                    {
                        encoding = Encoding.UTF8; //TODO: Choose UTF-8 or Default
                        encoding = Encoding.Default; 
                    }
                    }
                }
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }
 
        public int ContentLength
        {
            get
            {
                if ((contentLength == -1) && (conn != null))
                {
                    string s = conn.GetKnownRequestHeader(HttpProtocol.HeaderContentLength);
                    if (s != null)
                    {
                        try
                        {
                            contentLength = int.Parse(s);
                        }
                        catch
                        {
                        }
                    }
                }
                if (contentLength < 0)
                {
                    return 0;
                }
                return contentLength;
            }
        }
 
        public string ContentType
        {
            get
            {
                if (contentType == null)
                {
                    if (conn != null)
                        contentType = conn.GetKnownRequestHeader(HttpProtocol.HeaderContentType);
                    if (contentType == null)
                        contentType = string.Empty;
                }
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        public NameValueCollection Form
        {
            get 
            { 
                if (formCollection == null)
                {
                    formCollection = new NameValueCollection();
                    FillFormCollection();
                }
                return formCollection; 
            }
        }

        public NameValueCollection Params
        {
            get 
            { 
                if (paramsCollection == null)
                {
                    paramsCollection = new NameValueCollection();
                    FillParamsCollection();
                }
                return paramsCollection; 
            }
        }

        public NameValueCollection QueryString
        {
            get 
            {
                if (queryStringCollection == null)
                {
                    queryStringCollection = new NameValueCollection();
                    FillQueryStringCollection();
                }
                return queryStringCollection;
            }
        }

        public NameValueCollection Cookies
        {
            get 
            {
                if (cookieCollection == null)
                {
                    cookieCollection = new NameValueCollection();
                    FillCookieCollection();
                }
                return cookieCollection;
            }
        }

        public string this[string param]
        {
            get { return Params[param]; }
        }

        public Uri Url
        {
            get 
            { 
                if (url == null) 
                    url = new Uri("http://localhost" + conn.GetRawUrl());
                return url; 
            }
        }

        public string RawUrl
        {
            get { return conn.GetRawUrl(); }
        }

        public string PathInfo
        {
            get { return conn.GetPathInfo(); }
        }

        internal byte[] QueryStringBytes
        {
            get
            {
                if ((queryStringBytes == null) && (this.conn != null))
                {
                    queryStringBytes = conn.GetQueryStringBytes();
                }
                return queryStringBytes;
            }
        }
 
        internal string QueryStringText
        {
            get
            {
                if (queryStringText == null)
                {
                    if (this.conn != null)
                    {
                        byte[] bytes = this.QueryStringBytes;
                        if (bytes != null)
                        {
                            if (bytes.Length > 0)
                                queryStringText = ContentEncoding.GetString(bytes);
                            else
                                queryStringText = "";
                        }
                        else
                        {
                            queryStringText = conn.GetQueryString();
                        }
                    }
                    if (queryStringText == null)
                    {
                        queryStringText = "";
                    }
                }
                return queryStringText;
            }
        }
	}
}
