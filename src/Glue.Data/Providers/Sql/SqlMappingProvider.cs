using System;
using System.Collections;
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
        MappingOptions options;

        public SqlMappingProvider(string connectionString): base(connectionString)
        {
        }

        public SqlMappingProvider(
            string server, 
            string database, 
            string user, 
            string pass,
            MappingOptions options
            ) : 
            base(server, database, user, pass)
        {
            this.options = options;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected SqlMappingProvider(System.Xml.XmlNode node) : base(node)
        {
        }

        /// <summary>
        /// Create new UnitOfWork-instance with a specified IsolationLevel
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level</param>
        /// <returns>New UnitOfWork-instance</returns>
        public UnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel)
        {
            return UnitOfWork.Create((IMappingProvider)this, CreateConnection(), isolationLevel);
        }

        Entity Obtain(Type type)
        {
            Entity info = Entity.Obtain(type);
            if (info.Accessor == null)
            {
                Type accessorType = AccessorHelper.GenerateAccessor(type, "System.Data.SqlClient", "Sql", "@");
                info.Accessor = (Accessor)Activator.CreateInstance(accessorType, new object[] {this,type});
            }
            return info;
        }

        bool HasCache(Entity info)
        {
            if (info.Table.Cached)
            {
                if (info.Cache == null)
                {
                    StringBuilder s = new StringBuilder();
                    s.Append("SELECT ");
                    int i = 0;
                    foreach (EntityMember m in EntityMemberList.Flatten(info.AllMembers))
                        if (m.Column != null)
                        {
                            if (i > 0) 
                                s.Append(","); 
                            s.Append(m.Column.Name);
                            i++;
                        }
                    s.Append(" FROM [");
                    s.Append(info.Table.Name);
                    s.Append("]");
                    Log.Debug("Caching: " + s);
                    info.Cache = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveSortedList();
                    using (SqlDataReader reader = ExecuteReader(s.ToString()))
                        while (reader.Read())
                            info.Cache[reader[0]] = info.Accessor.CreateFromReaderFixed(reader, 0);
                }
                return true;
            }
            return false;
        }

        public object Find(Type type, params object[] keys)
        {
            Entity info = Obtain(type);
            
            if (HasCache(info))
                return info.Cache[keys[0]];
            
            int i;
            if (info.FindCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("SELECT ");
                i = 0;
                foreach (EntityMember m in EntityMemberList.Flatten(info.AllMembers))
                    if (m.Column != null)
                    {
                        if (i > 0) 
                            s.Append(","); 
                        s.Append(m.Column.Name);
                        i++;
                    }
                s.Append(" FROM [");
                s.Append(info.Table.Name);
                s.Append("] WHERE ");
                i = 0;
                foreach (EntityMember m in EntityMemberList.Flatten(info.KeyMembers))
                    if (m.Column != null)
                    {
                        if (i > 0) 
                            s.Append(" AND "); 
                        s.Append(m.Column.Name);
                        s.Append("=@");
                        s.Append(m.Column.Name);
                        i++;
                    }
                info.FindCommandText = s.ToString();
            }
            Log.Debug("Find SQL: " + info.FindCommandText);
            SqlCommand cmd = CreateCommand(info.FindCommandText);
            i = 0;
            foreach (EntityMember m in info.KeyMembers)
            {
                cmd.Parameters.AddWithValue("@" + m.Column.Name, NullConvert.From(keys[i]));
                i++;
            }
            using (SqlDataReader reader = ExecuteReader(cmd))
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
            
            StringBuilder s = new StringBuilder();
            int i;

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
                    s.Append("DECLARE @start_" + m.Column.Name + " " + GetSqlTypeSpecHack(m.Column.Type) + "\r\n");
                
                s.Append("SET ROWCOUNT " + limit.Index + "\r\n");
                s.Append("SELECT ");
                i = 0;
                foreach (EntityMember m in orderMembers)
                {
                    if (i > 0)
                        s.Append(",");
                    s.Append("@start_" + m.Column.Name + "=" + m.Column.Name);
                    i++;
                }
                s.Append(" FROM [");
                s.Append(table);
                s.Append("] WITH (NOLOCK) ");
                if (filter != null && !filter.IsEmpty)
                {
                    s.Append(" WHERE ");
                    s.Append(filter);
                }
                s.Append(" ORDER BY ");
                s.Append(order);
                s.Append("\r\n");
                
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
            {
                s.Append("SET ROWCOUNT " + limit.Count + "\r\n");
            }

            s.Append("SELECT ");
            i = 0;
            foreach (EntityMember m in EntityMemberList.Flatten(info.AllMembers))
                if (m.Column != null)
                {
                    if (i > 0) 
                        s.Append(","); 
                    s.Append(m.Column.Name);
                    i++;
                }
            s.Append(" FROM [");
            s.Append(table);
            s.Append("] WITH (NOLOCK) ");
            if (filter != null && !filter.IsEmpty)
            {
                s.Append(" WHERE ");
                s.Append(filter);
            }
            if (order != null && !order.IsEmpty)
            {
                s.Append(" ORDER BY ");
                s.Append(order);
            }
            s.Append("\r\n");
            
            if (limit.Count >= 0)
            {
                s.Append("SET ROWCOUNT 0\r\n");
            }

            Log.Debug("List SQL: " + s);
            SqlCommand cmd = CreateCommand(s.ToString());
            using (SqlDataReader reader = ExecuteReader(cmd))
            {
                return info.Accessor.ListFromReaderFixed(reader).ToArray(type);
            }
        }

        public Array List(Type type, IDbCommand command)
        {
            Entity info = Obtain(type);
            using (SqlDataReader reader = ExecuteReader(command as SqlCommand))
            {
                return info.Accessor.ListFromReaderDynamic(reader, Limit.Unlimited).ToArray(type);
            }
        }

        public Array ListManyToMany(object left, Type right)
        {
            return ListManyToMany(left, right, null, null, null);
        }

        public Array ListManyToMany(object left, Type right, Filter filter, Order order, Limit limit)
        {
            if (order == null) order = Order.Empty;
            if (limit == null) limit = Limit.Unlimited;

            Entity info = Obtain(left.GetType());
            string leftds = info.Table.Name;
            string leftpk = info.KeyMembers[0].Column.Name;
            object leftid = info.KeyMembers[0].GetValue(left);
            
            info = Obtain(right);
            string rightds = info.Table.Name;
            string rightpk = info.KeyMembers[0].Column.Name;

            // Guess the linking table, e.g. 
            //   Contact <--n:m--> Category implies a 'between'-table named ContactCategory.
            string linkds = leftds + rightds;
            string linkleftpk = leftpk.StartsWith(leftds) ? leftpk : leftds + leftpk;
            string linkrightpk = rightpk.StartsWith(rightds) ? rightpk : rightds + rightpk;
            
            // Code below would expand to:
            //   Category INNER JOIN ContactCategory ON Category.CategoryId=ContactCategory.CategoryId
            string join = rightds + " WITH (NOLOCK) INNER JOIN " + 
                          linkds + " WITH (NOLOCK) ON " + 
                          rightds + "." + rightpk + "=" + linkds + "." + linkrightpk;

            // Expand filter to be
            //   ContactCategory.ContactId=@ContactId AND (..additonal..)
            filter = Filter.And(linkds + "." + linkleftpk + "=@" + leftpk, filter);

            // Be sure the order contains the primary key of the item sought, in this example
            //   Category.CategoryId
            if (!order.Contains(rightpk))
                order = order.Append(rightds + "." + rightpk);

            // Create command
            SqlCommand command = CreateSelectCommand(
                join, rightds + ".*", filter, order, limit,
                "@" + leftpk, leftid
                );

            // Get right-hand side objects
            using (SqlDataReader reader = ExecuteReader(command))
            {
                return info.Accessor.ListFromReaderFixed(reader).ToArray(right);
            }
        }
        
        public Array ListManyToMany(Type left, object right)
        {
            return ListManyToMany(left, right, null, null, null);
        }

        public Array ListManyToMany(Type left, object right, Filter filter, Order order, Limit limit)
        {
            if (order == null) order = Order.Empty;
            if (limit == null) limit = Limit.Unlimited;

            Entity info = Obtain(right.GetType());
            string rightds = info.Table.Name;
            string rightpk = info.KeyMembers[0].Column.Name;
            object rightid = info.KeyMembers[0].GetValue(right);
            
            info = Obtain(left);
            string leftds = info.Table.Name;
            string leftpk = info.KeyMembers[0].Column.Name;
            
            // Guess the linking table, e.g. 
            //   Contact <--n:m--> Category implies a 'between'-table named ContactCategory.
            string linkds = leftds + rightds;
            string linkleftpk = leftpk.StartsWith(leftds) ? leftpk : leftds + leftpk;
            string linkrightpk = rightpk.StartsWith(rightds) ? rightpk : rightds + rightpk;

            // Contact m - n Category implies a 'between'-table named ContactCategory.
            // Code below would expand to:
            //   Contact INNER JOIN ContactCategory ON Contact.ContactId=ContactCategory.ContactId
            string join = leftds + " WITH (NOLOCK) INNER JOIN " + 
                          linkds  + " WITH (NOLOCK) ON " + 
                          leftds + "." + leftpk + "=" + linkds + "." + linkleftpk;

            // Expand filter to be
            //   ContactCategory.CategoryId=@CategoryId AND (..additonal..)
            filter = Filter.And(linkds + "." + linkrightpk + "=@" + rightpk, filter);

            // Be sure the order contains the primary key of the item sought, in this example
            //   Contact.ContactId
            if (!order.Contains(leftpk))
                order = order.Append(leftds + "." + leftpk);

            // Create command
            SqlCommand command = CreateSelectCommand(
                join, leftds + ".*", filter, order, limit,
                "@" + rightpk, rightid
                );

            // Get left-hand side objects
            using (SqlDataReader reader = ExecuteReader(command))
            {
                return info.Accessor.ListFromReaderFixed(reader).ToArray(left);
            }
        }
        
        public void AddManyToMany(object left, object right)
        {
            Entity leftInfo = Obtain(left.GetType());
            Entity rightInfo = Obtain(right.GetType());

            string leftkey = leftInfo.KeyMembers[0].Column.Name;
            string rightkey = rightInfo.KeyMembers[0].Column.Name;
            
            if (!leftkey.StartsWith(leftInfo.Table.Name))
                leftkey = leftInfo.Table.Name + leftkey;
            if (!rightkey.StartsWith(rightInfo.Table.Name))
                rightkey = rightInfo.Table.Name + rightkey;

            string between = leftInfo.Table.Name + rightInfo.Table.Name;
            string sql = string.Format(@"
                IF NOT EXISTS(SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2})
                INSERT {0} ({1},{2}) VALUES(@{1},@{2})",
                between, 
                leftkey,
                rightkey
                );
            SqlCommand command = CreateCommand(
                sql, 
                "@" + leftkey,
                leftInfo.KeyMembers[0].GetValue(left),
                "@" + rightkey,
                rightInfo.KeyMembers[0].GetValue(right)
                );
            ExecuteNonQuery(command);
        }
        
        public void DelManyToMany(object left, object right)
        {
            Entity leftInfo = Obtain(left.GetType());
            Entity rightInfo = Obtain(right.GetType());

            string leftkey = leftInfo.KeyMembers[0].Column.Name;
            string rightkey = rightInfo.KeyMembers[0].Column.Name;
            
            if (!leftkey.StartsWith(leftInfo.Table.Name))
                leftkey = leftInfo.Table.Name + leftkey;
            if (!rightkey.StartsWith(rightInfo.Table.Name))
                rightkey = rightInfo.Table.Name + rightkey;

            string between = leftInfo.Table.Name + rightInfo.Table.Name;
            string sql = string.Format(@"
                DELETE FROM {0} WHERE {1}=@{1} AND {2}=@{2}",
                between, 
                leftkey,
                rightkey
                );
            SqlCommand command = CreateCommand(
                sql, 
                "@" + leftkey,
                leftInfo.KeyMembers[0].GetValue(left),
                "@" + rightkey,
                rightInfo.KeyMembers[0].GetValue(right)
                );
            ExecuteNonQuery(command);
        }

        public void Insert(object obj)
        {
            Insert(null, obj);
        }
        
        public void Insert(UnitOfWork unitOfWork, object obj)
        {
            Entity info = Obtain(obj.GetType());
            int i;
            if (info.InsertCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("INSERT ");
                s.Append("INTO [");
                s.Append(info.Table.Name);
                s.Append("] (");
                i = 0;
                foreach (EntityMember m in EntityMemberList.Flatten(EntityMemberList.Subtract(info.AllMembers, info.AutoMembers)))
                    if (m.Column != null)
                    {
                        if (i > 0) 
                            s.Append(","); 
                        s.Append(m.Column.Name);
                        i++;
                    }
                s.Append(") VALUES (");
                // Obtain a flattened list of all columns excluding 
                // automatic ones (autoint, calculated fields)
                i = 0;
                foreach (EntityMember m in EntityMemberList.Flatten(EntityMemberList.Subtract(info.AllMembers, info.AutoMembers)))
                    if (m.Column != null)
                    {
                        if (i > 0) 
                            s.Append(","); 
                        s.Append("@");
                        s.Append(m.Column.Name);
                        i++;
                    }
                s.Append(")");
                if (info.AutoKeyMember != null)
                {
                    s.Append(" SELECT @@IDENTITY");
                }
                info.InsertCommandText = s.ToString();
                Log.Debug("Insert SQL: " + info.InsertCommandText);
            }
            SqlCommand cmd;

            if (unitOfWork != null)
            {
                cmd = CreateCommand((SqlConnection) unitOfWork.Connection, info.InsertCommandText);
                cmd.Transaction = (SqlTransaction) unitOfWork.Transaction;
            }
            else
            {
                cmd = CreateCommand(info.InsertCommandText);
            }
            
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
            Update(null, obj);
        }

        public void Update(UnitOfWork unitOfWork, object obj)
        {
            Entity info = Obtain(obj.GetType());
            int i;
            if (info.UpdateCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("UPDATE ");
                s.Append("[");
                s.Append(info.Table.Name);
                s.Append("] SET ");
                i = 0;
                foreach (EntityMember m in EntityMemberList.Flatten(EntityMemberList.Subtract(info.AllMembers, info.KeyMembers, info.AutoMembers)))
                    if (m.Column != null)
                    {
                        if (i > 0) 
                            s.Append(","); 
                        s.Append(m.Column.Name);
                        s.Append("=@");
                        s.Append(m.Column.Name);
                        i++;
                    }
                s.Append(" WHERE ");
                i = 0;
                foreach (EntityMember m in info.KeyMembers)
                {
                    if (i > 0) 
                        s.Append(" AND "); 
                    s.Append(m.Column.Name);
                    s.Append("=@");
                    s.Append(m.Column.Name);
                    i++;
                }
                info.UpdateCommandText = s.ToString();
                Log.Debug("Update SQL: " + info.UpdateCommandText);
            }
            SqlCommand cmd;
            if (unitOfWork != null)
            {
                cmd = CreateCommand((SqlConnection) unitOfWork.Connection, info.UpdateCommandText);
                cmd.Transaction = (SqlTransaction) unitOfWork.Transaction;
            }
            else
            {
                cmd = CreateCommand(info.UpdateCommandText);
            }

            info.Accessor.AddParametersToCommandFixed(obj, cmd);
            ExecuteNonQuery(cmd);

            info.Cache = null; // invalidate cache
        }
        
        public void Save(object obj)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Delete object by keys
        /// </summary>
        /// <param name="obj">Object instance</param>
        public void Delete(object obj)
        {
            Delete((UnitOfWork) null, obj);
        }

        /// <summary>
        /// Delete object by keys
        /// </summary>
        /// <param name="obj">Object instance</param>
        public void Delete(UnitOfWork unitOfWork, object obj)
        {
            Entity info = Obtain(obj.GetType());
            EntityMemberList keys = info.KeyMembers;

            object[] keyValues = new object[keys.Count];
            for (int n = 0; n < keys.Count; n++)
            {
                keyValues[n] = keys[n].GetValue(obj);
            }
            Delete(unitOfWork, obj.GetType(), keyValues);
        }

        /// <summary>
        /// Delete object by keys
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <param name="keys">Keys</param>
        public void Delete(Type type, params object[] keys)
        {
            Delete(null, type, keys);
        }

        private void Delete(UnitOfWork unitOfWork, Type type, params object[] keys)
        {
            Entity info = Obtain(type);
            int i;
            if (info.DeleteCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("DELETE ");
                s.Append(" FROM [");
                s.Append(info.Table.Name);
                s.Append("] WHERE ");
                i = 0;
                foreach (EntityMember m in info.KeyMembers)
                {
                    if (i > 0) 
                        s.Append(" AND "); 
                    s.Append(m.Column.Name);
                    s.Append("=@");
                    s.Append(m.Column.Name);
                    i++;
                }
                info.DeleteCommandText = s.ToString();
                Log.Debug("Delete SQL: " + info.DeleteCommandText);
            }

            SqlCommand cmd;
            if (unitOfWork != null)
            {
                // use connection and transaction of UnitOfWork
                cmd = CreateCommand((SqlConnection) unitOfWork.Connection, info.DeleteCommandText);
                cmd.Transaction = (SqlTransaction) unitOfWork.Transaction;
            }
            else
            {
                cmd = CreateCommand(info.DeleteCommandText);
            }

            i = 0;
            foreach (EntityMember m in info.KeyMembers)
            {
                Log.Debug("Key: " + m.Column.Name + ", value: " + keys[i]);
                cmd.Parameters.AddWithValue("@" + m.Column.Name, keys[i]);
                i++;
            }
            ExecuteNonQuery(cmd);
            info.Cache = null; // invalidate cache
        }

        public void DeleteAll(Type type, Filter filter)
        {
            Entity info = Obtain(type);
            StringBuilder s = new StringBuilder();
            s.Append("DELETE ");
            s.Append(" FROM [");
            s.Append(info.Table.Name);
            s.Append("]");
            if (filter != null && !filter.IsEmpty)
            {
                s.Append(" WHERE ");
                s.Append(filter);
            }
            ExecuteNonQuery(s.ToString());
        }

        public int Count(Type type, Filter filter)
        {
            Entity info = Obtain(type);
            string s = "SELECT COUNT(*) FROM [" + info.Table.Name + "]";
            if (filter != null && !filter.IsEmpty)
                s += " WHERE " + filter;
            Log.Debug("Count SQL: " + s);
            return (int) ExecuteScalar(s);
        }

        public int Count<T>(Filter filter)
        {
            return Count(typeof(T), filter);
        }

        public IDictionary Map(Type type, string key, string value, Filter filter, Order order)
        {
            OrderedDictionary result = new OrderedDictionary();
            Entity info = Obtain(type);
            if (value == null)
            {
                if (key == null)
                    key = info.KeyMembers[0].Column.Name;
                StringBuilder s = new StringBuilder();
                s.Append("SELECT ");
                int i = 0;
                foreach (EntityMember m in EntityMemberList.Flatten(info.AllMembers))
                    if (m.Column != null)
                    {
                        if (i > 0) 
                            s.Append(","); 
                        s.Append(m.Column.Name);
                        i++;
                    }
                s.Append(" FROM [");
                s.Append(info.Table.Name);
                s.Append("]");
                if (filter != null && !filter.IsEmpty)
                {
                    s.Append(" WHERE ");
                    s.Append(filter);
                }
                if (order != null && !order.IsEmpty)
                {
                    s.Append(" ORDER BY ");
                    s.Append(order);
                }
                Log.Debug("Map SQL: " + s);
                SqlCommand cmd = CreateCommand(s.ToString());
                using (SqlDataReader reader = ExecuteReader(cmd))
                    while (reader.Read())
                        result.Add(reader[key], info.Accessor.CreateFromReaderFixed(reader, 0));
                return result;
            }
            else
            {
                StringBuilder s = new StringBuilder();
                s.Append("SELECT ").Append(key).Append(",").Append(value);
                s.Append(" FROM [").Append(info.Table.Name).Append("]");
                if (filter != null && !filter.IsEmpty)
                {
                    s.Append(" WHERE ");
                    s.Append(filter);
                }
                if (order != null && !order.IsEmpty)
                {
                    s.Append(" ORDER BY ");
                    s.Append(order);
                }
                Log.Debug("Map SQL: " + s);
                using (SqlDataReader reader = ExecuteReader(s.ToString()))
                    while (reader.Read())
                        result.Add(reader[0], reader[1]);
            }
            return result;
        }
    }
}
