using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Glue.Lib;
#if (!ASPNET)
using Glue.Lib.Servers;
#else
using System.Web;
#endif

namespace Glue.Web
{
	/// <summary>
	/// Summary description for View.
	/// </summary>
    public class View
    {
        public readonly Controller Controller;
        
        #region Shortcuts

        protected IDictionary B(params object[] keyvals)
        {
            return Helper.B(keyvals);
        }

        protected string T(string s)
        {
            return Helper.T(s);
        }

        protected string H(string s)
        {
            return Helper.H(s);
        }
        
        protected string U(string s)
        {
            return Helper.U(s);
        }

        public string Root
        {
            get { return Controller.Request.Root; }
        }

        #endregion

        public IRequest Request 
        { 
            get { return Controller.Request; } 
        }

        public IResponse Response 
        { 
            get { return Controller.Response; } 
        }
        
        public NameValueCollection Params 
        { 
            get { return Controller.Request.Params; } 
        }
        
        public NameValueCollection QueryString 
        { 
            get { return Controller.Request.QueryString; } 
        }

        public NameValueCollection Form 
        { 
            get { return Controller.Request.Form; } 
        }
        
        public CombinedException Errors
        {
            get { return Controller.Errors; }
        }

        protected View(Controller controller)
        {
            this.Controller = controller;
        }

        protected virtual void Render(string virtualPath)
        {
            this.Controller.Render(virtualPath);
        }

        public virtual void Render(TextWriter writer)
        {
        }
    }
}
