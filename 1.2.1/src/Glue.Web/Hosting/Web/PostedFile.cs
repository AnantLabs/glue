using System;
using System.IO;
using System.Web;
using Glue.Web;

namespace Glue.Web.Hosting.Web
{
	/// <summary>
	/// PostedFile is a wrapper around System.Web's HttpPostedFile.
	/// </summary>
	public class PostedFile : Glue.Web.PostedFile
	{
        HttpPostedFile _inner;

		internal PostedFile(string name, HttpPostedFile inner) : base(name, inner.FileName, inner.ContentType, null, 0, inner.ContentLength)
		{
            _inner = inner;
		}
        
        public override void SaveAs(string path)
        {
            _inner.SaveAs(path);
        }
        
        public override Stream Content
        {
            get { return _inner.InputStream; }
        }
    }
}
