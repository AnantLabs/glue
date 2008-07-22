using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.MySql
{
    public class MySqlDataProvider : BaseDataProvider
    {
        public MySqlDataProvider(string connectionString)
            : base(connectionString)
        {
        }

        public MySqlDataProvider(string server, string database, string username, string password)
            : base("server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password)
        {
        }

        public MySqlDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                _connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
            }
        }

        protected MySqlDataProvider(MySqlDataProvider provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new MySqlDataProvider(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('?', '`', '`', ";", "@@IDENTITY");
        }

        protected override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorCompiler.GenerateAccessor(type, "MySql.Data.MySqlClient", "MySql", "?");
            return (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
        }

        public override IDbConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public override IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            if (commandText != null)
                command.CommandText = commandText;
            AddParameters(command, paramNameValueList);
            return command;
        }

        public override ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }
    }
}