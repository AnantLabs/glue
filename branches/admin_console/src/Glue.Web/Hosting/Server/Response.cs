using System;
using System.Collections;
using System.Collections.Specialized;
using Glue.Lib.Servers;

namespace Glue.Web.Hosting.Server
{
	/// <summary>
	/// Summary description for Response.
	/// </summary>
	public class Response : HttpResponse, IResponse
	{
        public Response(HttpConnection connection) : base(connection) 
        { 
        }

		void IResponse.BinaryWrite(byte[] buffer)
		{
			throw new NotImplementedException();
		}

        public void BinaryWrite(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        void IResponse.TransmitFile(string filename)
        {
            ContentType = Glue.Lib.Mime.MimeMapping.GetMimeMapping(filename);
            base.TransmitFile(filename);
        }

        public void SetCookie(Cookie cookie)
        {
            base.conn.SetKnownResponseHeader(HttpProtocol.HeaderSetCookie, cookie.GetHeaderString());
        }

        public void AddHeader(string name, string value)
        {
            throw new NotImplementedException();
        }
    }
}
