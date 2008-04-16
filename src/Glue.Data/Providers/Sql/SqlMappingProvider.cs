using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Reflection;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.Sql
{
    /// <summary>
    /// Provider
    /// </summary>
    public class SqlMappingProvider : SqlDataProvider, IMappingProvider
    {
        MappingOptions _options;

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlMappingProvider(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlMappingProvider(
            string server,
            string database,
            string user,
            string pass,
            MappingOptions options) : base(server, database, user, pass)
        {
            _options = options;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        public SqlMappingProvider(System.Xml.XmlNode node) : base(node)
        {
        }

        /// <summary>
        /// Copy constructor for opening sessions and transactions.
        /// </summary>
        protected SqlMappingProvider(SqlMappingProvider provider) : base(provider)
        {
            _options = provider._options;
        }

        /// <summary>
        /// Copy method
        /// </summary>
        protected override SqlDataProvider Copy()
        {
            return new SqlMappingProvider(this);
        }

        public new SqlMappingProvider Open()
        {
            return (SqlMappingProvider)base.Open();
        }

        public new SqlMappingProvider Open(IsolationLevel level)
        {
            return (SqlMappingProvider)base.Open(level);
        }

        IMappingProvider IMappingProvider.Open()
        {
            return Open();
        }

        IMappingProvider IMappingProvider.Open(IsolationLevel level)
        {
            return Open(level);
        }

        Accessor GetAccessor(Type type)
        {
            Entity info = Obtain(type);
            return info.Accessor;
        }
        
        /// <summary>
        /// Obtain cached Entity information for given type.
        /// </summary>
        Entity Obtain(Type type)
        {
            Entity info = Entity.Obtain(type);
            if (info.Accessor == null)
            {
                Type accessorType = AccessorHelper.GenerateAccessor(type, "System.Data.SqlClient", "Sql", "@");
                info.Accessor = (Accessor)Activator.CreateInstance(accessorType, new object[] { this, type });
            }
            return info;
        }

        public object Find(Type type, params object[] keys)
        {
            Entity info = Obtain(type);

            if (info.Table.Cached)
            {
                if (info.Cache == null)
                    info.Cache = Map(type, null, null);
                return info.Cache[keys[0]];
            }
            
            if (info.FindCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("SELECT ");
                s.ColumnList(EntityMemberList.Flatten(info.AllMembers));
                s.Append(" FROM ");
                s.Identifier(info.Table.Name);
                s.Append(" WHERE ");
                s.ColumnAndParameterList(info.KeyMembers, "=", " AND ");
                
                info.FindCommandText = s.ToString();
                Log.Debug("Find SQL: " + info.FindCommandText);
            }

            SqlCommand command = CreateCommand(info.FindCommandText);
            int i = 0;
            foreach (EntityMember m in info.KeyMembers)
                AddParameter(command, m.Column.Name, keys[i++]);

            using (IDataReader reader = ExecuteReader(command))
                if (reader.Read())
                    return info.Accessor.CreateFromReaderFixed(reader, 0);
                else
                    return null;
        }

        public T Find<T>(params object[] keys)
        {
            return (T)Find(typeof(T), keys);
        }

        public object FindByFilter(Type type, Filter filter)
        {
            return FindByFilter(type, filter, null);
        }
        
        public object FindByFilter(Type type, Filter filter, Order order)
        {
            Array list = List(type, filter, order, Limit.One);
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        public object FindByFilter(string table, Type type, Filter filter)
        {
            return FindByFilter(table, type, filter, null);
        }

        public object FindByFilter(string table, Type type, Filter filter, Order order)
        {
            Array list = List(table, type, filter, null, Limit.One);
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        public object FindByFilter(Type type, IDbCommand command)
        {
            Array list = List(type, command);
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        public T FindByFilter<T>(Filter filter)
        {
            return (T)FindByFilter(typeof(T), filter);
        }

        public T FindByFilter<T>(string table, Filter filter)
        {
            return (T)FindByFilter(table, typeof(T), filter);
        }

        public T FindByFilter<T>(IDbCommand command)
        {
            return (T)FindByFilter(typeof(T), command);
        }

        public T FindByFilter<T>(Filter filter, Order order)
        {
            return (T)FindByFilter(typeof(T), filter, order);
        }

        private string GetSqlTypeSpecHack(Type type)
        {
            if (type == typeof(String))
                return "nvarchar(100)";
            else if (type == typeof(Boolean))
                return "bit";
            else if (type == typeof(Guid))
                return "uniqueidentifier";
            else if (type == typeof(DateTime))
                return "datetime";
            else
                return "int";
        }

        private string GetSqlEqualityExpr(EntityMember m)
        {
            if (m.Key != null)
                return m.Column.Name + "=@start_" + m.Column.Name;
            else
                return "(" + m.Column.Name + " IS NULL) AND (@start_" + m.Column.Name + " IS NULL) OR (" + m.Column.Name + "=@start_" + m.Column.Name + ")";
        }

        private string GetSqlLessThanExpr(EntityMember m)
        {
            if (m.Key != null)
                return m.Column.Name + "<@start_" + m.Column.Name;
            else
                return "(" + m.Column.Name + " IS NULL) AND NOT(@start_" + m.Column.Name + " IS NULL) OR (" + m.Column.Name + "<@start_" + m.Column.Name + ")";
        }

        private string GetSqlGreaterThanExpr(EntityMember m)
        {
            if (m.Key != null)
                return m.Column.Name + ">@start_" + m.Column.Name;
            else
                return "NOT(" + m.Column.Name + " IS NULL) AND (@start_" + m.Column.Name + " IS NULL) OR (" + m.Column.Name + ">@start_" + m.Column.Name + ")";
        }

        public Array List(Type type, Filter filter, Order order, Limit limit)
        {
            return List(null, type, filter, order, limit);
        }
 
        public Array List(string table, Type type, Filter filter, Order order, Limit limit)
        {
            if (order == null) order = Order.Empty;
            if (limit == null) limit = Limit.Unlimited;

            Entity info = Obtain(type);
            if (table == null)
                table = info.Table.Name;
            
            QueryBuilder s = CreateQueryBuilder();

            if (limit.Index > 0)
            {
                // For paged queries use the following construct: 
                // Note: primary keys will be appended to the sort order.
                //
                //    DECLARE @Start_AanvraagDatum DATETIME
                //    DECLARE @Start_AanvraagCode VARCHAR(100)
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

                // Make sure the order by clause contains all key members
                foreach (EntityMember m in info.KeyMembers)
                    if (!order.Contains(m.Column.Name))
                        order = order.Append(m.Column.Name);

                // Declare variables for all ordering members
                EntityMember[] orderMembers = new EntityMember[order.Count];
                int i = 0;
                for (i = 0; i < order.Count; i++)
                {
                    orderMembers[i] = info.AllMembers.FindByColumnName(order[i]);
                    if (orderMembers[i] == null)
                    {
                        orderMembers[i] = new EntityMember();
                        orderMembers[i].Column = new EntityColumn();
                        if (order[i][0] == '+' || order[i][0] == '-')
                            orderMembers[i].Column.Name = order[i].Substring(1);
                        else
                            orderMembers[i].Column.Name = order[i];
                        orderMembers[i].Column.Type = typeof(string);
                    }
                }

                foreach (EntityMember m in orderMembers)
                    s.AppendLine("DECLARE @start_").Append(m.Column.Name).Append(" ").Append(GetSqlTypeSpecHack(m.Column.Type));

                s.AppendLine("SET ROWCOUNT " + limit.Index);
                s.Append("SELECT ");
                i = 0;
                foreach (EntityMember m in orderMembers)
                {
                    if (i > 0)
                        s.Append(",");
                    s.Append("@start_").Append(m.Column.Name).Append("=").Identifier(m.Column.Name);
                    i++;
                }
                s.Append(" FROM ");
                s.Identifier(table);
                s.Append(" WITH (NOLOCK) ");
                s.Filter(filter);
                s.AppendLine();
                s.Order(order);
                s.AppendLine();
                
                // Now adapt the filter for use in the subsequent select.
                Filter outside = null;
                for (i = orderMembers.Length-1; i >= 0; i--)
                {
                    if (outside != null)
                        if (order.GetDirection(orderMembers[i].Column.Name) > 0)
                            outside = Filter.And(GetSqlEqualityExpr(orderMembers[i]), outside);
                        else
                            outside = Filter.And(GetSqlEqualityExpr(orderMembers[i]), outside);
                    
                    if (order.GetDirection(orderMembers[i].Column.Name) > 0)
                        outside = Filter.Or(GetSqlGreaterThanExpr(orderMembers[i]), outside);
                    else
                        outside = Filter.Or(GetSqlLessThanExpr(orderMembers[i]), outside);
                }
                
                filter = Filter.And(filter, outside);
            }
            
            if (limit.Count >= 0)
                s.AppendLine("SET ROWCOUNT " + limit.Count);

            s.Append("SELECT ");
            s.ColumnList(EntityMemberList.Flatten(info.AllMembers));
            s.Append(" FROM ");
            s.Identifier(table);
            s.Append(" WITH (NOLOCK) ");
            s.Filter(filter);
            s.Order(order);
            s.AppendLine();
            
            if (limit.Count >= 0)
                s.AppendLine("SET ROWCOUNT 0");

            Log.Debug("List SQL: " + s);
            using (IDataReader reader = ExecuteReader(s.ToString()))
            {
                return GetAccessor(type).ListFromReaderFixed(reader).ToArray(type);
            }
        }

        public Array List(Type type, IDbCommand command)
        {
            using (IDataReader reader = ExecuteReader((SqlCommand)command))
            {
                return GetAccessor(type).ListFromReaderDynamic(reader, Limit.Unlimited).ToArray(type);
            }
        }

        public IList<T> List<T>(Filter filter, Order order, Limit limit)
        {
            return new List<T>((IEnumerable<T>)List(typeof(T), filter, order, limit));
        }

        public IList<T> List<T>(string table, Filter filter, Order order, Limit limit)
        {
            return new List<T>((IEnumerable<T>)List(table, typeof(T), filter, order, limit));
        }

        public IList<T> List<T>(IDbCommand command)
        {
            return new List<T>((IEnumerable<T>)List(typeof(T), command));
        }

        public Array ListManyToMany(object left, Type right, string jointable)
        {
            return ListManyToMany(left, right, jointable, null, null, null);
        }

        public Array ListManyToMany(object left, Type right, string jointable, Filter filter, Order order, Limit limit)
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
            SqlCommand command = CreateSelectCommand(
                join.ToString(), columns.ToString(), filter, order, limit,
                "@" + info.JoinLeftKey, info.LeftKeyInfo.GetValue(left)
                );

            // Get right-hand side objects
            using (IDataReader reader = ExecuteReader(command))
            {
                return GetAccessor(info.RightType).ListFromReaderFixed(reader).ToArray(info.RightType);
            }
        }

        /// <summary>
        /// Add a :m relation between object left and object right. Explicitly specify the
        /// joining table.
        /// </summary>
        public void AddManyToMany(object left, object right, string jointable)
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
            
            SqlCommand command = CreateCommand(s.ToString());
            AddParameter(command, info.JoinLeftKey, info.LeftKeyInfo.GetValue(left));
            AddParameter(command, info.JoinRightKey, info.RightKeyInfo.GetValue(right));
            ExecuteNonQuery(command);
        }
        
        /// <summary>
        /// Remove a n:m relation between object left and object right. Explicitly specify the
        /// joining table.
        /// </summary>
        public void DelManyToMany(object left, object right, string jointable)
        {
            ManyToManyInfo info = new ManyToManyInfo(left.GetType(), right.GetType(), jointable);
            
            QueryBuilder s = CreateQueryBuilder();
            s.Append(" DELETE FROM ").Identifier(info.JoinTable);
            s.Append(" WHERE ").Identifier(info.JoinLeftKey).Append("=").Parameter(info.JoinLeftKey);
            s.Append(" AND ").Identifier(info.JoinRightKey).Append("=").Parameter(info.JoinRightKey);
            
            SqlCommand command = CreateCommand(s.ToString());
            AddParameter(command, info.JoinLeftKey, info.LeftKeyInfo.GetValue(left));
            AddParameter(command, info.JoinRightKey, info.RightKeyInfo.GetValue(right));
            ExecuteNonQuery(command);
        }

        public void Insert(object obj)
        {
            Entity info = Obtain(obj.GetType());

            if (info.InsertCommandText == null)
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
                    s.AppendLine("SELECT @@IDENTITY");
                }
                info.InsertCommandText = s.ToString();
                Log.Debug("Insert SQL: " + info.InsertCommandText);
            }

            SqlCommand cmd = CreateCommand(info.InsertCommandText);
            info.Accessor.AddParametersToCommandFixed(obj, cmd);
            
            object autokey = ExecuteScalar(cmd);
            if (info.AutoKeyMember != null)
            {
                info.AutoKeyMember.SetValue(obj, Convert.ToInt32(autokey));
            }

            info.Cache = null; // invalidate cache
        }

        public void Update(object obj)
        {
            Entity info = Obtain(obj.GetType());
            if (info.UpdateCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("UPDATE ");
                s.Identifier(info.Table.Name);
                s.AppendLine(" SET ");
                // set all non-key, non-auto members
                EntityMemberList cols = EntityMemberList.Flatten(EntityMemberList.Subtract(info.AllMembers, info.KeyMembers, info.AutoMembers));
                s.ColumnAndParameterList(cols, "=", ",");
                s.AppendLine();
                s.AppendLine(" WHERE ");
                s.ColumnAndParameterList(info.KeyMembers, "=", " AND ");
                info.UpdateCommandText = s.ToString();
                Log.Debug("Update SQL: " + info.UpdateCommandText);
            }

            SqlCommand cmd = CreateCommand(info.UpdateCommandText);
            info.Accessor.AddParametersToCommandFixed(obj, cmd);
            ExecuteNonQuery(cmd);

            info.Cache = null; // invalidate cache
        }

        public void Save(object obj)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Delete object.
        /// </summary>
        public void Delete(object obj)
        {
            Entity info = Obtain(obj.GetType());
            EntityMemberList keys = info.KeyMembers;

            object[] values = new object[keys.Count];
            for (int i = 0; i < keys.Count; i++)
            {
                values[i] = keys[i].GetValue(obj);
            }
            Delete(obj.GetType(), values);
        }

        /// <summary>
        /// Delete object by keys
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <param name="keys">Keys</param>
        public void Delete(Type type, params object[] keys)
        {
            Entity info = Obtain(type);

            if (info.DeleteCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("DELETE FROM ");
                s.Append(info.Table.Name);
                s.AppendLine(" WHERE ");
                s.ColumnAndParameterList(info.KeyMembers, "=", " AND ");
                
                info.DeleteCommandText = s.ToString();
                Log.Debug("Delete SQL: " + info.DeleteCommandText);
            }

            SqlCommand cmd = CreateCommand(info.DeleteCommandText);
            int i = 0;
            foreach (EntityMember m in info.KeyMembers)
                // cmd.Parameters.Add(new SqlParameter("@" + m.Column.Name, NullConvert.From(keys[i++])));
                AddParameter(cmd, m.Column.Name, keys[i++]);
            ExecuteNonQuery(cmd);
            info.Cache = null; // invalidate cache if present
        }

        public void DeleteAll(Type type, Filter filter)
        {
            Entity info = Obtain(type);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("DELETE FROM ");
            s.Identifier(info.Table.Name);
            s.Filter(filter);
            
            ExecuteNonQuery(s.ToString());
            info.Cache = null; // invalidate cache if present
        }

        public int Count(Type type, Filter filter)
        {
            Entity info = Obtain(type);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT COUNT(*) FROM ");
            s.Identifier(info.Table.Name);
            s.Filter(filter);
            
            return Convert.ToInt32(ExecuteScalar(s.ToString()));
        }

        public void Delete<T>(params object[] keys)
        {
            Delete(typeof(T), keys);
        }

        public void DeleteAll<T>(Filter filter)
        {
            DeleteAll(typeof(T), filter);
        }

        public int Count<T>(Filter filter)
        {
            return Count(typeof(T), filter);
        }

        public IDictionary Map(Type type, Filter filter, Order order)
        {
            OrderedDictionary result = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            Entity info = Obtain(type);
            if (info.KeyMembers.Count != 1)
                throw new InvalidOperationException("Entity should have precisely one key column: " + info.Type.ToString() + " - " + info.Table.Name);
            EntityMember key = info.KeyMembers[0];

            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT ");
            s.ColumnList(EntityMemberList.Flatten(info.AllMembers));
            s.Append(" FROM ");
            s.Identifier(info.Table.Name);
            s.Filter(filter);
            s.Order(order);

            Log.Debug("Map SQL: " + s);
            using (IDataReader reader = ExecuteReader(s.ToString()))
                while (reader.Read())
                {
                    object instance = info.Accessor.CreateFromReaderFixed(reader, 0);
                    object keyvalue = key.Field.GetValue(instance);
                    result.Add(keyvalue, instance);
                }
            return result;
        }

        public IDictionary Map(string table, string key, string value, Filter filter, Order order)
        {
            OrderedDictionary result = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT ");
            s.Identifier(key);
            s.Append(",");
            s.Identifier(value);
            s.Append(" FROM ");
            s.Identifier(table);
            s.Filter(filter);
            s.Order(order);

            Log.Debug("Map SQL: " + s);
            using (IDataReader reader = ExecuteReader(s.ToString()))
                while (reader.Read())
                    result.Add(reader[0], reader[1]);
            return result;
        }
    }
}
