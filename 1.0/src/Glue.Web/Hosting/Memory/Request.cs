using System;
using System.Text;
using System.Security.Principal;
using System.Collections.Specialized;

namespace Glue.Web.Hosting.Memory
{
	/// <summary>
	/// Summary description for Request.
	/// </summary>
	public class Request : IRequest
	{
        Uri url;
        string queryStringText;
        NameValueCollection formCollection;
        NameValueCollection queryStringCollection;
        NameValueCollection paramsCollection;
        CookieCollection cookieCollection;
        IPrincipal user;

		public Request(string url)
		{
            // HACK: to get the Uri constructor working
            this.url = new Uri("http://localhost" + url);
            this.queryStringText = this.url.Query;
            if (this.queryStringText != null && this.queryStringText.Length > 0 && this.queryStringText[0] == '?')
                this.queryStringText = this.queryStringText.Remove(0, 1);
        }

        private void FillQueryStringCollection()
        {
            Glue.Lib.Servers.HttpProtocol.FillValuesFromString(queryStringCollection, this.queryStringText, true, this.ContentEncoding);
        }
 
        private void FillParamsCollection()
        {
            paramsCollection.Add(this.QueryString);
            paramsCollection.Add(this.Form);
        }
 
        #region IRequest Members

        public Uri Url
        {
            get { return url; }
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

        public string PathInfo
        {
            get { return null; }
        }

        public string Method
        {
            get { return "GET"; }
        }

        public Encoding ContentEncoding
        {
            get { return Encoding.Default; }
        }

        public int ContentLength
        {
            get { return 0; }
        }

        public string ContentType
        {
            get { return null; }
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

        public NameValueCollection Form
        {
            get
            {
                if (formCollection == null)
                {
                    formCollection = new NameValueCollection();
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

        public CookieCollection Cookies
        {
            get
            {
                if (cookieCollection == null)
                {
                    cookieCollection = new CookieCollection();
                }
                return cookieCollection; 
            }
        }

        public PostedFileCollection Files
        {
            get { return null; }
        }

        public ISession Session
        {
            get { return null; }
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
