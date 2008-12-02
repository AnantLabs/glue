using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using Glue.Lib.Servers;

namespace Glue.Web.Hosting.Server
{
	/// <summary>
	/// Summary description for Request.
	/// </summary>
    public class Request : HttpRequest, IRequest
    {
        PostedFileCollection _files;
        CookieCollection _cookies;
        IPrincipal _user;
        Session _session;

        public Request(HttpConnection connection) : base(connection) 
        { 
        }

        public PostedFileCollection Files
        {
            get
            {
                if (_files == null)
                {
                    _files = new PostedFileCollection();
                }
                return _files; 
            }
        }

        public new CookieCollection Cookies
        {
            get
            {
                if (_cookies == null)
                {
                    _cookies = new CookieCollection();
                    string header = conn.GetKnownRequestHeader(HttpProtocol.HeaderCookie);
                    if (header != null && header.Length > 0)
                    {
                        foreach (string s in header.Split(';'))
                        {
                            int i = s.IndexOf('=');
                            if (i > 0)
                                _cookies.Add(new Cookie(s.Substring(0, i).Trim(), s.Substring(i + 1)));
                        }
                    }
                }
                return _cookies; 
            }
        }

        public ISession Session
        {
            get 
            { 
                if (_session == null)
                {
                    Cookie cookie = Cookies["SESSIONID"];
                    if (cookie == null)
                    {
                        // Create new session cookie
                        cookie = new Cookie("SESSIONID", new Random().Next().ToString());
                        conn.SetKnownResponseHeader(HttpProtocol.HeaderSetCookie, cookie.GetHeaderString());
                    }
                    _session = new Session(cookie.Value);
                }
                return _session; 
            }
        }

        public IPrincipal User
        {
            get 
            {
                if (_user == null)
                    _user = System.Threading.Thread.CurrentPrincipal;
                return _user;
            }
            set
            {
                _user = value;
            }
        }

        public string Root
        {
            get { return ""; }
        }

        public string Path
        {
            get { return this.Url.LocalPath; }
            set { UriBuilder b = new UriBuilder(this.url); b.Path = value; this.url = b.Uri; }
        }

        public string Method
        {
            get { return base.conn.GetHttpVerbName(); }
        }
    }
}
