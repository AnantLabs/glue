using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.Sql
{
    public class SqlDataProvider2 : BaseDataProvider
    {
        public SqlDataProvider2(string connectionString)
            : base(connectionString)
        {
        }

        public SqlDataProvider2(string server, string database, string username, string password)
            : base("server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password)
        {
        }

        public SqlDataProvider2(XmlNode node)
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

        protected SqlDataProvider2(SqlDataProvider2 provider)
            : base(provider)
        {
        }

        protected override object Copy()
        {
            return new SqlDataProvider2(this);
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('@', '[', ']');
        }

        protected override internal Accessor CreateAccessor(Type type)
        {
            Type accessorType = AccessorHelper.GenerateAccessor(type, "System.Data.SqlClient", "Sql", "@");
            return (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
        }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
        
        public override IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand(commandText);
            AddParameters(command, paramNameValueList);
            return command;
        }

        public override IDbCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            QueryBuilder s = CreateQueryBuilder();

            // HACK: check if there's a WITH option clause in the data source.
            bool nolock = table.IndexOf(" WITH ") < 0;

            if (limit.Index > 0)
            {
                // For paged queries use the following construct: 
                // Note: primary keys will be appended to the sort order.
                //
                //    DECLARE @Start_AanvraagDatum sql_variant
                //    DECLARE @Start_AanvraagCode sql_variant
                //
                //    -- first select for skipping rows
                //    SET ROWCOUNT $Index
                //    SELECT 
                //        @Start_AanvraagDatum=AanvraagDatum,
                //        @Start_AanvraagCode=AanvraagCode 
                //    FROM 
                //        Aanvraag WITH (NOLOCK) 
                //    WHERE
                //        AanvraagCode < 'A'
                //    ORDER BY
                //        AanvraagDatum DESC, AanvraagCode
                //
                //    -- second select for getting rows
                //    SET ROWCOUNT $Count
                //    SELECT 
                //        AanvraagCode, 
                //        AanvragerCode,
                //        AanvraagDatum,
                //        Omschrijving
                //    FROM 
                //        Aanvraag WITH (NOLOCK) 
                //    WHERE 
                //        AanvraagCode < @Start_AanvraagDatum OR
                //        (AanvraagDatum = @Start_AanvraagDatum AND AanvraagCode > @Start_AanvraagCode)
                //    ORDER BY 
                //        AanvraagDatum DESC, AanvraagCode
                //    SET ROWCOUNT 0

                // This ONLY WORKS if the order by clause contains all key columns

                // Get ordering columns
                string[] ordervars = new string[order.Count];
                string[] ordercols = new string[order.Count];
                for (int i = 0; i < order.Count; i++)
                {
                    ordervars[i] = "@start_" + order[i].Substring(1).Replace('.', '_');
                    ordercols[i] = order[i].Substring(1);
                }

                // Declare variables for all ordering members
                for (int i = 0; i < ordervars.Length; i++)
                    s.Append("DECLARE ").Append(ordervars[i]).AppendLine(" sql_variant");

                // Create the first select, for skipping unwanted rows
                s.AppendLine("SET ROWCOUNT " + limit.Index);
                s.Append("SELECT ");
                for (int i = 0; i < ordervars.Length; i++)
                {
                    if (i > 0)
                        s.Append(",");
                    s.Append(ordervars[i]).Append("=").Append(ordercols[i]);
                }
                s.Append(" FROM ");
                s.Identifier(table);
                if (nolock)
                    s.Append(" WITH (NOLOCK) ");
                s.AppendLine();
                s.Filter(constraint);
                s.Order(order);
                s.AppendLine();

                // Now adapt the constraint for use in the subsequent select.
                Filter outside = null;
                for (int i = ordervars.Length - 1; i >= 0; i--)
                {
                    if (outside != null)
                        outside = Filter.And("(" + ordercols[i] + " IS NULL) AND (" + ordervars[i] + " IS NULL) OR (" + ordercols[i] + "=" + ordervars[i] + ")", outside);

                    if (order.GetDirection(i) > 0)
                        outside = Filter.Or("NOT(" + ordercols[i] + " IS NULL) AND (" + ordervars[i] + " IS NULL) OR (" + ordercols[i] + ">" + ordervars[i] + ")", outside);
                    else
                        outside = Filter.Or("(" + ordercols[i] + " IS NULL) AND NOT(" + ordervars[i] + " IS NULL) OR (" + ordercols[i] + "<" + ordervars[i] + ")", outside);
                }
                constraint = Filter.And(constraint, outside);
            }

            // Create main select
            if (limit.Count >= 0)
                s.AppendLine("SET ROWCOUNT " + limit.Count);
            s.Append("SELECT ");
            s.Append(columns);
            s.Append(" FROM ");
            s.Identifier(table);
            if (nolock)
                s.Append(" WITH (NOLOCK) ");
            s.Filter(constraint);
            s.Order(order);
            s.AppendLine();
            if (limit.Count >= 0)
                s.AppendLine("SET ROWCOUNT 0");
            Log.Debug("List SQL: " + s);

            return CreateCommand(s.ToString(), paramNameValueList);
        }

        public override IDbCommand CreateReplaceCommand(string table, params object[] columnNameValueList)
        {
            throw new NotImplementedException();
            //SqlSchemaCache schema = new SqlSchemaCache(this);
            //Filter filter = null;
            //foreach (string key in schema.PrimaryKeys[table])
            //    filter = Filter.And(filter, key + "=@" + key);
            
            //IDbCommand command = CreateCommand(null, columnNameValueList);

            //QueryBuilder s = CreateQueryBuilder();
            //s.Append("UPDATE ").Identifier(table).Append(" SET ");
            //s.ColumnAndParameterList(command.Parameters, "=", ",");
            //s.Filter(filter);
            //s.AppendLine();
            //s.Append("IF @@ROWCOUNT=0 ");
            //s.Append("INSERT INTO ").Identifier(table);
            //s.Append("(").ColumnList(command.Parameters).Append(") VALUES ");
            //s.Append("(").ParameterList(command.Parameters).Append(")");
            //s.AppendLine();

            //command.CommandText = s.ToString();
            //return command;
        }

        public override Array ListManyToMany(object left, Type right, string jointable, Filter filter, Order order, Limit limit)
        {
            if (order == null) order = Order.Empty;
            if (limit == null) limit = Limit.Unlimited;

            // Get names and info of all stuff involved
            ManyToManyInfo info = new ManyToManyInfo(left.GetType(), right, jointable);

            // For Contact <-- ContactCategory --> Category 
            // code below would expand to:
            //
            //   Category INNER JOIN ContactCategory ON Category.Id=ContactCategory.CategoryId
            QueryBuilder join = CreateQueryBuilder();
            join.Identifier(info.RightTable).Append(" WITH (NOLOCK)");
            join.Append(" INNER JOIN ");
            join.Identifier(info.JoinTable).Append(" WITH (NOLOCK)");
            join.Append(" ON ").Identifier(info.RightTable, info.RightKey).Append("=").Identifier(info.JoinTable, info.JoinRightKey);

            // Expand filter to be
            //   ContactCategory.CategoryId=@CategoryId AND (..additonal if specified..)
            filter = Filter.And(info.JoinTable + "." + info.JoinLeftKey + "=@" + info.JoinLeftKey, filter);

            // Be sure the order contains the primary key of the item sought, in this example
            //   Contact.ContactId
            if (!order.Contains(info.RightKey))
                order = order.Append(info.RightTable + "." + info.RightKey);

            // Create column list
            QueryBuilder columns = CreateQueryBuilder();
            columns.ColumnList(info.RightTable, EntityMemberList.Flatten(info.RightInfo.AllMembers), ",");

            // Create command
            IDbCommand command = CreateSelectCommand(
                join.ToString(), columns.ToString(), filter, order, limit,
                "@" + info.JoinLeftKey, info.LeftKeyInfo.GetValue(left)
                );

            // Get right-hand side objects
            AccessorInfo acc = AccessorInfo.Obtain(this, info.RightType);
            using (IDataReader reader = ExecuteReader(command))
            {
                return acc.Accessor.ListFromReaderFixed(reader).ToArray(info.RightType);
            }
        }

        /// <summary>
        /// Add a :m relation between object left and object right. Explicitly specify the
        /// joining table.
        /// </summary>
        public virtual void AddManyToMany(object left, object right, string jointable)
        {
            ManyToManyInfo info = new ManyToManyInfo(left.GetType(), right.GetType(), jointable);

            QueryBuilder s = CreateQueryBuilder();
            s.Append(" IF NOT EXISTS (");
            s.Append("   SELECT * FROM ").Identifier(info.JoinTable);
            s.Append("   WHERE ").Identifier(info.JoinLeftKey).Append("=").Parameter(info.JoinLeftKey);
            s.Append("   AND ").Identifier(info.JoinRightKey).Append("=").Parameter(info.JoinRightKey);
            s.Append(" ) ");
            s.Append(" INSERT INTO ").Identifier(info.JoinTable);
            s.Append(" (").Identifier(info.JoinLeftKey).Append(",").Identifier(info.JoinRightKey).Append(")");
            s.Append(" VALUES (").Parameter(info.JoinLeftKey).Append(",").Parameter(info.JoinRightKey).Append(")");

            IDbCommand command = CreateCommand(s.ToString());
            AddParameter(command, info.JoinLeftKey, info.LeftKeyInfo.GetValue(left));
            AddParameter(command, info.JoinRightKey, info.RightKeyInfo.GetValue(right));
            ExecuteNonQuery(command);
        }
    }

    class SqlSchemaCache
    {
        public readonly List<string> Tables = new List<string>();
        public readonly List<string> Views = new List<string>();
        public readonly Dictionary<string, List<string>> PrimaryKeys = new Dictionary<string, List<string>>();
        public readonly List<string> AutoInts = new List<string>();

        public SqlSchemaCache(SqlDataProvider2 provider)
        {
            using (IDataReader reader = provider.ExecuteReader("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"))
                while (reader.Read())
                    Tables.Add(reader.GetString(0));
            using (IDataReader reader = provider.ExecuteReader("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS ORDER BY TABLE_NAME"))
                while (reader.Read())
                    Views.Add(reader.GetString(0));
            using (IDataReader reader = provider.ExecuteReader(@"
                select c.TABLE_NAME,c.COLUMN_NAME,COLUMNPROPERTY(object_id(c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
                from INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
	            where CONSTRAINT_TYPE = 'PRIMARY KEY'
	            and	c.TABLE_NAME = pk.TABLE_NAME
	            and	c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME
                order by c.TABLE_NAME,c.ORDINAL_POSITION"
                ))
                while (reader.Read())
                {
                    List<string> keys = PrimaryKeys[reader.GetString(0)];
                    if (keys == null)
                        PrimaryKeys[reader.GetString(0)] = keys = new List<string>();
                    keys.Add(reader.GetString(1));
                    if (reader.GetInt32(2) == 1)
                        AutoInts.Add(reader.GetString(0));
                }
        }
    }
}
