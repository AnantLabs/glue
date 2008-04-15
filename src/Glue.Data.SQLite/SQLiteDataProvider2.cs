using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Providers.SQLite
{
    public class SQLiteDataProvider2 : BaseDataProvider
    {
        public SQLiteDataProvider2(string connectionString)
            : base(connectionString)
        {
        }

        public SQLiteDataProvider2(string server, string database, string username, string password)
            : base("Data Source=" + database + "; Pooling=True; Version=3; UTF8Encoding=True;")
        {
        }

        protected SQLiteDataProvider2(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                _connectionString = "Data Source=" + database + "; Pooling=True; Version=3; UTF8Encoding=True;";
            }
        }

        protected SQLiteDataProvider2(SQLiteDataProvider2 provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new SQLiteDataProvider2(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('@', '[', ']');
        }

        public override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public override IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            SQLiteCommand command = new SQLiteCommand(commandText);
            AddParameters(command, paramNameValueList);
            return command;
        }
    }
}