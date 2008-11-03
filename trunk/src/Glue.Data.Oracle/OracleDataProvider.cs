using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.Oracle
{
    public class OracleDataProvider : BaseDataProvider
    {
        private static string BuildConnectionString(string server, string database, string username, string password)
        {
            return "Data Source =(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=" + server + ")(PORT=1521))(CONNECT_DATA=(SID=" + database + "))); Unicode=True; User Id=" + username + "; Password=" + password + ";";
        }

        public OracleDataProvider(string connectionString)
            : base(connectionString)
        {
        }

        public OracleDataProvider(string server, string database, string username, string password)
            : base(BuildConnectionString(server, database, username, password))
        {
        }

        public OracleDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                _connectionString = BuildConnectionString(server, database, username, password);
            }
        }

        protected OracleDataProvider(OracleDataProvider provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new OracleDataProvider(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder(':', ' ', ' ', ";", "@@IDENTITY");
        }

        protected override Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorCompiler.GenerateAccessor(type, "System.Data.OracleClient", "Oracle", ":");
            return (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
        }

        public override IDbConnection CreateConnection()
        {
            return new OracleConnection(ConnectionString);
        }

        public override IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            OracleCommand command = new OracleCommand();
            if (commandText != null) 
                command.CommandText = commandText;
            AddParameters(command, paramNameValueList);
            return command;
        }

        public override ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        public override IDbCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            if (order == null) order = Order.Empty;
            if (limit == null) limit = Limit.Unlimited;
            QueryBuilder s = CreateQueryBuilder();
            if (limit.Index > 0 || limit.Count >= 0)
            {
                s.Append("SELECT ").Append(columns).Append(", row_number").Append(" FROM (");
                s.Append("  SELECT ").Append(columns).Append(", ROWNUM row_number").Append(" FROM (");
                s.Append("    SELECT ").Append(columns).Append(" FROM ").Identifier(table).Filter(constraint).Order(order);
                s.Append("  )");
                s.Append(") WHERE row_number >= " + limit.Index + 1 + " AND row_number <= " + limit.Index + limit.Count);
            }
            else
            {
                s.Append("SELECT ").Append(columns).Append(" FROM ").Identifier(table).Filter(constraint).Order(order);
            }
            return CreateCommand(s.ToString(), paramNameValueList);
        }

        /// <summary>
        /// Insert given object.
        /// </summary>
        public override void Insert(object obj)
        {
            Type type = obj.GetType();
            Accessor info = Accessor.Obtain(this, type);

            if (info.InsertCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("INSERT INTO ");
                s.Identifier(info.Entity.Table.Name);
                s.Append(" (");
                // Obtain a flattened list of all columns excluding calculated fields
                EntityMemberList columns = EntityMemberList.Subtract(
                    EntityMemberList.Flatten(info.Entity.AllMembers),
                    EntityMemberList.Flatten(info.Entity.CalculatedMembers)
                    );
                s.Columns(columns);
                s.AppendLine(")");
                s.Append(" VALUES (");
                bool first = true;
                foreach (EntityMember m in columns)
                    if (m.Column != null)
                    {
                        if (!first) s.Append(",");
                        if (m.AutoKey != null)
                            s.Append(info.Entity.Table.Name + "_SEQ.NEXTVAL");
                        else
                            s.Parameter(m.Column.Name);
                        first = false;
                    }
                s.AppendLine(")");
                if (info.Entity.AutoKeyMember != null)
                    s.AppendLine("RETURNING " + info.Entity.AutoKeyMember.Column.Name + " INTO :" + info.Entity.AutoKeyMember.Column.Name);
                info.InsertCommandText = s.ToString();
            }

            IDbCommand command = CreateCommand(info.InsertCommandText);
            info.AddAllParametersToCommand(obj, command);
            OracleParameter identity = null;
            if (info.Entity.AutoKeyMember != null)
            {
                identity = command.Parameters[0] as OracleParameter;
                identity.Direction = ParameterDirection.Output;
            }
            ExecuteNonQuery(command);
            if (info.Entity.AutoKeyMember != null)
                info.Entity.AutoKeyMember.SetValue(obj, Convert.ToInt32(identity.Value));

            InvalidateCache(type);
        }

    }
}