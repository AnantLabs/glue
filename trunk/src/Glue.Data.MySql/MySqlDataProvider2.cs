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
    public class MySqlDataProvider2 : BaseDataProvider
    {
        public MySqlDataProvider2(string connectionString)
            : base(connectionString)
        {
        }

        public MySqlDataProvider2(string server, string database, string username, string password)
            : base("server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password)
        {
        }

        public MySqlDataProvider2(XmlNode node)
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

        protected MySqlDataProvider2(MySqlDataProvider2 provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new MySqlDataProvider2(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('?', '`', '`');
        }

        protected override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorHelper.GenerateAccessor(type, "MySql.Data.MySqlClient", "MySql", "?");
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
                    s.Append("; SELECT @@IDENTITY;");
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