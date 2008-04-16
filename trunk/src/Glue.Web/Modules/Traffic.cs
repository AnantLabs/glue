using System;
using System.IO;
using Glue.Lib;
using Glue.Data;
using Glue.Web;

namespace Glue.Web.Modules
{
	/// <summary>
	/// Create a Log table in your database, like this
	/// .
    /// CREATE TABLE [Traffic] (
    ///     [Id] [int] IDENTITY (1, 1) NOT NULL ,
    ///     [DT] [datetime] NOT NULL CONSTRAINT [DF_Traffic_DT] DEFAULT (getdate()),
    ///     [IP] [varchar] (16) NOT NULL ,
    ///     [Status] [int] NOT NULL ,
    ///     [Method] [varchar] (16) NOT NULL ,
    ///     [Path] [varchar] (100) NOT NULL ,
    ///     [Query] [varchar] (100) NOT NULL ,
    ///     [Referrer] [varchar] (100) NOT NULL ,
    ///     [UserAgent] [varchar] (150) NOT NULL ,
    ///     CONSTRAINT [PK_Traffic] PRIMARY KEY CLUSTERED ([Id])
    ///     )
	/// </summary>
	public class Traffic : IModule
	{
        static IDataProvider provider = (IDataProvider)Configuration.Get("dataprovider");

        /// <summary>
        /// Before
        /// </summary>
        public bool Before(IRequest request, IResponse response)
        {
            return false;
        }

        /// <summary>
        /// Process
        /// </summary>
        public bool Process(IRequest request, IResponse response, Type controller)
        {
            return false;
        }

        /// <summary>
        /// Error
        /// </summary>
        public bool Error(IRequest request, IResponse response, Exception exception)
        {
            return false;
        }

        /// <summary>
        /// After
        /// </summary>
        public bool After(IRequest request, IResponse response)
        {
            return false;
        }

        /// <summary>
        /// Finally
        /// </summary>
        public bool Finally(IRequest request, IResponse response)
        {
            provider.ExecuteNonQuery(@"
INSERT INTO [Traffic] (IP,Status,Method,Path,Query,Referrer,UserAgent) 
VALUES (@IP,@Status,@Method,@Path,@Query,@Referrer,@UserAgent)",
                "IP", Trunc(request.Params["REMOTE_ADDR"], 16),
                "Status", response.StatusCode,
                "Method", Trunc(request.Method, 16),
                "Path", Trunc(request.Params["URL"], 100),
                "Query", Trunc(request.Params["QUERY_STRING"], 100),
                "Referrer", Trunc(request.Params["HTTP_REFERER"], 100),
                "UserAgent", Trunc(request.Params["HTTP_USER_AGENT"], 150)
                );
            return false;
        }

        private static string Trunc(string s, int maxlen)
        {
            if (s == null)
                return "";
            if (s.Length <= maxlen)
                return s;
            return s.Substring(0, maxlen);
        }
	}
}
