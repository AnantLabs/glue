using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

using Glue.Data;
using Glue.Data.Mapping;
using Glue.Lib;
using Npgsql;

namespace Glue.Data.PostgreSQL
{
    public class PostgreSQLDataProvider : BaseDataProvider
    {
        public PostgreSQLDataProvider(string connectionstring)
            : base(connectionstring)
        {
        }

        public PostgreSQLDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server = Configuration.GetAttr(node, "server", "localhost");
                string port = Configuration.GetAttr(node, "port", "5432");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                _connectionString = "server=" + server + ";port=" + port + ";database=" + database + ";user id=" + username + ";password=" + password;
            }
        }

        protected PostgreSQLDataProvider(PostgreSQLDataProvider provider)
            : base(provider)
        {
        }

        protected override object Copy() 
        {
            return new PostgreSQLDataProvider(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('?', '"', '"', ";", "lastval()");
        }

        protected override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorCompiler.GenerateAccessor(type, "Npgsql", "Npgsql", "?");
            return (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
        }

        public override IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            NpgsqlCommand command = new NpgsqlCommand();
            if (commandText != null)
                command.CommandText = commandText;
            AddParameters(command, paramNameValueList);
            return command;
        }

        public override IDbConnection CreateConnection() 
        {
            return new NpgsqlConnection(_connectionString);
        }

        public override ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }
    }
}
