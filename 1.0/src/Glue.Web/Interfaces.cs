using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Globalization;
using System.Collections.Specialized;
using System.Security.Principal;

namespace Glue.Web
{
	/// <summary>
	/// Summary description for Request.
	/// </summary>
    public interface IRequest
    {
        Uri Url { get; }
        string Root { get; }
        string Path { get; set; }
        string PathInfo { get; }
        string Method { get; }
        string ContentType { get; }
        Encoding ContentEncoding { get; }
        int ContentLength { get; }
        NameValueCollection QueryString { get; }
        NameValueCollection Form { get; }
        NameValueCollection Params { get; }
        CookieCollection Cookies { get; }
        PostedFileCollection Files { get; }
        IPrincipal User { get; set; }
        ISession Session { get; }
    }

    /// <summary>
    /// Summary description for Request.
    /// TODO: Add support for headers
    /// TODO: Clean up buffering semantics
    /// </summary>
    public interface IResponse
    {
        void End();
        void Clear();
        void Flush();
        void Write(char c);
        void Write(string s);
        void Write(string format, params object[] arg);
		void BinaryWrite(byte[] buffer);
        void BinaryWrite(byte[] buffer, int offset, int length);
        void TransmitFile(string filename);
        string ContentType { get; set; }
        Encoding ContentEncoding { get; set; }
        void AddHeader(string name, string value);
        string RedirectLocation { get; set; }
        TextWriter Output { get; }
        int StatusCode { get; set; }
        string StatusDescription { get; set; }
        void SetCookie(Cookie cookie);
    }

    /// <summary>
    /// ISession interface.
    /// </summary>
    public interface ISession : ICollection
    {
        object this[string key]
        {
            get;
            set;
        }
        void Clear();
    }

    /// <summary>
    /// IModule interface.
    /// </summary>
    public interface IModule
    {
        bool Before(IRequest request, IResponse response);
        bool Process(IRequest request, IResponse response, Type controller);
        bool After(IRequest request, IResponse response);
        bool Error(IRequest request, IResponse response, Exception exception);
        bool Finally(IRequest request, IResponse response);
    }
}
