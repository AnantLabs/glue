using System;
using System.IO;
using System.Text;
using Glue.Web;

namespace Glue.Web.Hosting.Memory
{
	/// <summary>
	/// Summary description for Response.
	/// </summary>
	public class Response : IResponse
	{
        private TextWriter writer;

		public Response(TextWriter writer)
		{
            this.writer = writer;
        }

        #region IResponse Members

        public void End()
        {
        }

        public void Clear()
        {
        }

        public void Flush()
        {
            writer.Flush();
        }

        public void Write(char c)
        {
            writer.Write(c);
        }

        public void Write(string s)
        {
            writer.Write(s);
        }

        public void Write(string format, params object[] arg)
        {
            writer.Write(format, arg);
        }

		public void BinaryWrite(byte[] buffer)
		{
			throw new NotImplementedException();
		}
        
        public void BinaryWrite(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

		public void TransmitFile(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long len = stream.Length;
                byte[] bytes = new byte[len];
                int n = stream.Read(bytes, 0, (int)len);
                writer.Write(Encoding.UTF8.GetChars(bytes, 0, n));
            }
        }

        public Cookie NewCookie(string name)
        {
            throw new NotImplementedException();
        }

        public void DeleteCookie(string name)
        {
            throw new NotImplementedException();
        }

        public void SetCookie(Cookie cookie)
        {
            throw new NotImplementedException();
        }

        public string ContentType
        {
            get { return null; }
            set { }
        }

        public Encoding ContentEncoding
        {
            get { return writer.Encoding; }
            set { }
        }

        public void AddHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public string RedirectLocation
        {
            get { return null; }
            set { }
        }

        public TextWriter Output
        {
            get { return writer; }
        }

        public int StatusCode
        {
            get { return 0; }
            set { }
        }

        public string StatusDescription
        {
            get { return null; }
            set { } 
        }

        #endregion
    }
}
