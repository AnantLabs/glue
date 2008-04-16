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

        public SQLiteDataProvider2(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string database = Configuration.GetAttr(node, "database");
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

        protected override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorHelper.GenerateAccessor(type, "System.Data.SQLite", "SQLite", "@");
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

        public override void Insert(object obj)
        {
            Type type = obj.GetType();
            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);

            if (acc.InsertCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("INSERT INTO ");
                s.Identifier(info.Table.Name);
                s.Append(" (");
                // Obtain a flattened list of all columns excluding 
                // automatic ones (autoint, calculated fields)
                EntityMemberList cols = EntityMemberList.Flatten(EntityMemberList.Subtract(info.AllMembers, info.AutoMembers));
                s.ColumnList(cols);
                s.AppendLine(")");
                s.Append(" VALUES (");
                s.ParameterList(cols);
                s.AppendLine(")");
                if (info.AutoKeyMember != null)
                {
                    s.Append("; SELECT last_insert_rowid();");
                }
                acc.InsertCommandText = s.ToString();
                Log.Debug("Insert SQL: " + acc.InsertCommandText);
            }

            IDbCommand cmd = CreateCommand(acc.InsertCommandText);
            acc.Accessor.AddParametersToCommandFixed(obj, cmd);

            object autokey = ExecuteScalar(cmd);
            if (info.AutoKeyMember != null)
            {
                info.AutoKeyMember.SetValue(obj, Convert.ToInt32(autokey));
            }

            info.Cache = null; // invalidate cache
        }
    }
}