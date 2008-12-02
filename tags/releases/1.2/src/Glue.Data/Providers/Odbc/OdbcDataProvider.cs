using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.Odbc
{
    /// <summary>
    /// DataProvider for ODBC data sources.
    /// </summary>
    public class OdbcDataProvider : BaseDataProvider
    {
        /// <summary>
        /// Initialize the DataProvider with given connection string.
        /// </summary>
        public OdbcDataProvider(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Initialize the DataProvider with given connection string.
        /// </summary>
        public OdbcDataProvider(string server, string database, string username, string password)
            : base("server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password)
        {
        }

        /// <summary>
        /// Initialize the DataProvider from configuration
        /// </summary>
        public OdbcDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                if (username != null)
                    _connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
                else
                    _connectionString = "server=" + server + ";database=" + database + ";integrated security=true";
            }
        }

        /// <summary>
        /// Protected copy constructor. Needed for Open() methods.
        /// </summary>
        protected OdbcDataProvider(OdbcDataProvider provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new OdbcDataProvider(this);  
        }

        public override ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        public override IDbConnection CreateConnection()
        {
            throw new NotImplementedException();
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('?', '[', ']', "\r\n", "@@IDENTITY");
        }

        public override IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            OdbcCommand command = new OdbcCommand(commandText);
            AddParameters(command, paramNameValueList);
            return command;
        }

        protected internal override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorCompiler.GenerateAccessor(type, "System.Data.OdbcClient", "Odbc", "?");
            return (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
        }
    }
}
