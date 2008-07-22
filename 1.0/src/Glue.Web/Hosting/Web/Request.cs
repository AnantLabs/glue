using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using Glue.Lib;
using Glue.Web;

namespace Glue.Web.Hosting.Web
{
	/// <summary>
	/// Summary description for Request.
	/// </summary>
	public class Request : IRequest
	{
        System.Web.HttpContext context;
        string path;
        CookieCollection cookies;
        PostedFileCollection files;
        NameValueCollection parameters;
        Session session;
        IPrincipal user;

		public Request(System.Web.HttpContext context)
		{
            this.context = context;
        }

        #region IRequest Members

        public Uri Url
        {
            get { return context.Request.Url; }
        }

        public string Root
        {
            get { return context.Request.ApplicationPath == "/" ? "" : context.Request.ApplicationPath; }
        }

        public string Path
        {
            get { return path == null ? context.Request.Path.Substring(Root.Length) : path; }
            set { path = value; }
        }

        public string PathInfo
        {
            get { return context.Request.PathInfo; }
        }

        public string Method
        {
            get { return context.Request.RequestType; }
        }

        public NameValueCollection QueryString
        {
            get { return context.Request.QueryString; }
        }

        public Encoding ContentEncoding
        {
            get { return context.Request.ContentEncoding; }
        }

        public int ContentLength
        {
            get { return context.Request.ContentLength; }
        }

        public string ContentType
        {
            get { return context.Request.ContentType; }
        }

        public NameValueCollection Form
        {
            get { return context.Request.Form; }
        }

        public NameValueCollection Params
        {
            get 
            { 
                if (parameters == null)
                    parameters = new NameValueCollection(context.Request.Params);
                return parameters;
            }
        }

        public CookieCollection Cookies
        {
            get 
            { 
                if (cookies == null)
                {
                    cookies = new CookieCollection();
                    for (int i = 0; i < context.Request.Cookies.Count; i++)
                    {
                        System.Web.HttpCookie src = context.Request.Cookies[i];
                        Cookie cookie = new Cookie(src.Name);
                        cookie.Path = src.Path;
                        cookie.Domain = src.Domain;
                        cookie.Expires = src.Expires;
                        cookie.Secure = src.Secure;
                        cookie.Value = src.Value;
                        cookies.Add(cookie);
                    }
                }
                return cookies; 
            }
        }

        public PostedFileCollection Files
        {
            get 
            { 
                if (files == null)
                {
                    files = new PostedFileCollection();
                    for (int i = 0; i < context.Request.Files.Count; i++)
                    {
                        string s = context.Request.Files.GetKey(i);
                        System.Web.HttpPostedFile f = context.Request.Files.Get(i);
                        if (f.FileName != null && f.FileName != "")
                            files.Add(new Glue.Web.Hosting.Web.PostedFile(s, f));
                    }
                }
                return files; 
            }
        }

        public ISession Session
        {
            get 
            { 
                if (session == null)
                    session = new Session(context);
                return session; 
            }
        }

        public IPrincipal User
        {
            get 
            {
                if (user == null)
                    user = System.Threading.Thread.CurrentPrincipal;
                return user;
            }
            set
            {
                user = value;
            }
        }

        #endregion
    }
}
