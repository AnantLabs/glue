using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.SQLite
{
    /// <summary>
    /// DataProvider for SQLite databases.
    /// </summary>
    public class SQLiteDataProvider : BaseDataProvider
    {
        public SQLiteDataProvider(string connectionString)
            : base(connectionString)
        {
        }

        public SQLiteDataProvider(string server, string database, string username, string password)
            : base("Data Source=" + database + "; Pooling=True; Version=3; UTF8Encoding=True;")
        {
        }

        /// <summary>
        /// Construct SQLiteDataprovider with an XmlNode as a parameter. The node has to have either an attribute
        /// "connectionString", containing the complete connection string, or an attribute "database", with a 
        /// path to a database file. 
        /// </summary>
        public SQLiteDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string database = Configuration.GetAttr(node, "database");
                _connectionString = "Data Source=" + database + "; Pooling=True; Version=3; UTF8Encoding=True;";
            }
        }

        protected SQLiteDataProvider(SQLiteDataProvider provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new SQLiteDataProvider(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('@', '[', ']', ";", "last_insert_rowid()");
        }

        protected override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorCompiler.GenerateAccessor(type, "System.Data.SQLite", "SQLite", "@");
            return (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
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

        public override ISchemaProvider GetSchemaProvider()
        {
            return new SQLiteSchemaProvider(this);
        }
    }
}