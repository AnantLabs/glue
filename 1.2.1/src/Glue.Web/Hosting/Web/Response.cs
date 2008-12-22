using System;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using Glue.Web;

namespace Glue.Web.Hosting.Web
{
	/// <summary>
	/// Summary description for Response.
	/// </summary>
	public class Response : IResponse
	{
        System.Web.HttpContext context;
        
        public Response(System.Web.HttpContext context)
		{
            this.context = context;
        }

        public System.Web.HttpCachePolicy Cache
        {
            get { return this.context.Response.Cache; }
        }

        #region IResponse Members

        public void End()
        {
            context.Response.End();
        }

        public void Clear()
        {
            context.Response.Clear();
        }

        public void Flush()
        {
            context.Response.Flush();
        }

        public void Write(char c)
        {
            context.Response.Write(c);
        }

        public void Write(string s)
        {
            context.Response.Write(s);
        }

        public void Write(string format, params object[] arg)
        {
            context.Response.Write(string.Format(format, arg));
        }

		public void BinaryWrite(byte[] buffer)
		{
			context.Response.BinaryWrite(buffer);
		}

        public void BinaryWrite(byte[] buffer, int offset, int length)
        {
            context.Response.OutputStream.Write(buffer, offset, length);
        }

        public void TransmitFile(string filename)
        {
            //context.Response.TransmitFile(filename);
            context.Response.WriteFile(filename);
        }

        public void SetCookie(Cookie cookie)
        {
            HttpCookie c = new HttpCookie(cookie.Name, cookie.Value);
            if (cookie.Expires != DateTime.MinValue)
                c.Expires = cookie.Expires;
            c.Domain = cookie.Domain;
            c.Path = cookie.Path;
            c.Secure = cookie.Secure;
            context.Response.Cookies.Add(c);
        }

        public string ContentType
        {
            get  { return context.Response.ContentType; }
            set { context.Response.ContentType = value; }
        }

        public Encoding ContentEncoding
        {
            get  { return context.Response.ContentEncoding; }
            set { context.Response.ContentEncoding = value; }
        }

        public void AddHeader(string name, string value)
        {
            context.Response.AppendHeader(name, value);            
        }
        
        public string RedirectLocation
        {
            get  { return context.Response.RedirectLocation; }
            set { context.Response.RedirectLocation = value; }
        }

        public System.IO.TextWriter Output
        {
            get { return context.Response.Output; }
        }

        public int StatusCode
        {
            get { return context.Response.StatusCode; }
            set { context.Response.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return context.Response.StatusDescription; }
            set { context.Response.StatusDescription = value; }
        }

         #endregion
    }
}
