using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// Implements many common methods for DataProviders. Currently all Glue DataProviders
    /// use this as a base class to implement the IDataProvider interface.
    /// </summary>
    public abstract class BaseSchemaProvider : ISchemaProvider
    {
        protected string server = null;
        protected string username = null;
        protected string password = null;

        /// <summary>
        /// SqlSchemaProvider
        /// </summary>
        protected BaseSchemaProvider(string server, string username, string password)
        {
            this.server = server;
            this.username = username;
            this.password = password;
        }

        public Glue.Data.Schema.Database  GetDatabase(string name)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Database[]  GetDatabases()
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Table[]  GetTables(Glue.Data.Schema.Database database)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.View[]  GetViews(Glue.Data.Schema.Database database)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Procedure[]  GetProcedures(Glue.Data.Schema.Database database)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Column[]  GetColumns(Glue.Data.Schema.Container container)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Index[]  GetIndexes(Glue.Data.Schema.Container container)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Trigger[]  GetTriggers(Glue.Data.Schema.Container container)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Key[]  GetKeys(Glue.Data.Schema.Table table)
        {
 	        throw new NotImplementedException();
        }

        public string  GetViewText(Glue.Data.Schema.View view)
        {
 	        throw new NotImplementedException();
        }

        public Glue.Data.Schema.Parameter[]  GetParameters(Glue.Data.Schema.Procedure procedure)
        {
 	        throw new NotImplementedException();
        }

        public string  GetProcedureText(Glue.Data.Schema.Procedure procedure)
        {
 	        throw new NotImplementedException();
        }

        public void  Export(Glue.Data.Schema.Container container, IDataExporter writer)
        {
 	        throw new NotImplementedException();
        }

        public void  Import(Glue.Data.Schema.Table table, IDataImporter reader, Glue.Data.Schema.ImportMode mode)
        {
 	        throw new NotImplementedException();
        }

        public void  Script(Glue.Data.Schema.Database database, System.IO.TextWriter writer)
        {
 	        throw new NotImplementedException();
        }

        public string  Scheme
        {
	        get { throw new NotImplementedException(); }
        }
    }
}
