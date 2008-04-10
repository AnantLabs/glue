using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.IO;
using System.Reflection;
using System.Data.SQLite;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;
            
namespace Glue.Data.Providers.SQLite
{
    /// <summary>
    /// Provider
    /// </summary>
    public class SQLiteMappingProvider : SQLiteDataProvider, IMappingProvider
    {
        MappingOptions options;

        public SQLiteMappingProvider(string connectionString): base(connectionString)
        {
        }

        public SQLiteMappingProvider(
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
        protected SQLiteMappingProvider(System.Xml.XmlNode node) : base(node)
        {
        }

        protected SQLiteMappingProvider(SQLiteConnection connection) : base(connection)
        {
        }

        /*
        IMappingProvider MyProvider;
        
         * foreach (Item i in MyProvider.List...) {
         *  ...
         * }
         * using (IMappingProvider provider = MyProvider.Open()) {
         *      foreach (Item i in provider.List...) {
         *      }
         *      provider.Insert(new Item(1,"x"));
         *      provider.Delete<Item>(1);
         * }
        */

        public IMappingProvider Open()
        {
            return new SQLiteMappingProvider(CreateConnection());
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }

        /// <summary>
        /// Create new UnitOfWork-instance with a specified IsolationLevel
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level</param>
        /// <returns>New UnitOfWork-instance</returns>
        public UnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtain cached Entity information for given type.
        /// </summary>
        Entity Obtain(Type type)
        {
            Entity info = Entity.Obtain(type);
            if (info.Accessor == null)
            {
                Type accessorType = AccessorHelper.GenerateAccessor(type, "System.Data.SQLite", "SQLite", "@");
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
                    s.Append(" FROM ");
                    s.Append(info.Table.Name);
                    s.Append("");
                    Log.Debug("Caching: " + s);
                    info.Cache = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveSortedList();
                    
                    if (info.KeyMembers.Count != 1)
                        throw new InvalidOperationException("A cached entity should have precisely one key column: " + info.Type.ToString() + " - " + info.Table.Name);

                    EntityMember id = info.KeyMembers[0];
                    using (SQLiteDataReader reader = ExecuteReader(s.ToString()))
                        while (reader.Read())
                        {
                            object instance = info.Accessor.CreateFromReaderFixed(reader, 0);
                            object key = id.Field.GetValue(instance);
                            info.Cache[key] = instance;
                        }
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
                s.Append(" FROM ");
                s.Append(info.Table.Name);
                s.Append(" WHERE ");
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
                Log.Debug("Find SQL: " + info.FindCommandText);
            }
            SQLiteCommand cmd = CreateCommand(info.FindCommandText);
            i = 0;
            foreach (EntityMember m in info.KeyMembers)
            {
                cmd.Parameters.Add(new SQLiteParameter("@" + m.Column.Name, keys[i]));
                i++;
            }
            using (SQLiteDataReader reader = ExecuteReader(cmd))
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
            Array list = List(type, filter, null, Limit.New(0, 1));
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        public object FindByFilter(string table, Type type, Filter filter)
        {
            Array list = List(table, type, filter, null, Limit.New(0, 1));
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

        public object FindByFilter(Type type, Filter filter, Order order)
        {
            throw new NotImplementedException();
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

        public Array List(Type type, Filter filter, Order order, Limit limit)
        {
            return List(null, type, filter, order, limit);
        }

        public List<T> List<T>(Filter filter, Order order, Limit limit)
        {
            return new List<T>((IEnumerable<T>)List(typeof(T), filter, order, limit));
        }
 
        public Array List(string table, Type type, Filter filter, Order order, Limit limit)
        {
            Entity info = Obtain(type);
            if (table == null)
                table = info.Table.Name;
            int i;
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
            s.Append(" FROM ");
            s.Append(table);
            s.Append("");
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
            if (limit != null && !limit.IsUnlimited)
            {
                s.Append(" LIMIT " + limit.Index + "," + limit.Count);
            }
            Log.Debug("List SQL: " + s);
            SQLiteCommand cmd = CreateCommand(s.ToString());
            using (SQLiteDataReader reader = ExecuteReader(cmd))
            {
                return info.Accessor.ListFromReaderFixed(reader).ToArray(type);
            }
        }

        public Array List(Type type, IDbCommand command)
        {
            Entity info = Obtain(type);
            using (SQLiteDataReader reader = ExecuteReader(command as SQLiteCommand))
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
            Entity leftInfo = Obtain(left.GetType());
            Entity rightInfo = Obtain(right);
            string between = leftInfo.Table.Name + rightInfo.Table.Name;
            filter = Filter.And(
                leftInfo.KeyMembers[0].Column.Name + "=@" + leftInfo.KeyMembers[0].Column.Name,
                filter
                );
            if (order == null)
                order = Order.Empty;
            string sql = string.Format(@"
                SELECT {0}.* 
                FROM {0} INNER JOIN {1} ON {0}.{2}={1}.{2}
                {3}
                {4}", 
                rightInfo.Table.Name, 
                between, 
                rightInfo.KeyMembers[0].Column.Name,
                filter.ToSql(),
                order.ToSql()
                );
            SQLiteCommand command = CreateCommand(
                sql, 
                "@" + leftInfo.KeyMembers[0].Column.Name,
                leftInfo.KeyMembers[0].GetValue(left)
                );
            // HACK
            return ListHack(right, command, limit); 
        }
        
        public Array ListManyToMany(Type left, object right)
        {
            return ListManyToMany(left, right, null, null, null);
        }

        public Array ListManyToMany(Type left, object right, Filter filter, Order order, Limit limit)
        {
            Entity leftInfo = Obtain(left);
            Entity rightInfo = Obtain(right.GetType());
            string between = leftInfo.Table.Name + rightInfo.Table.Name;
            filter = Filter.And(
                rightInfo.KeyMembers[0].Column.Name + "=@" + rightInfo.KeyMembers[0].Column.Name,
                filter
                );
            if (order == null)
                order = Order.Empty;
            string sql = string.Format(@"
                SELECT {0}.* 
                FROM {0} INNER JOIN {1} ON {0}.{2}={1}.{2} 
                {3} 
                {4}",
                leftInfo.Table.Name, 
                between, 
                leftInfo.KeyMembers[0].Column.Name,
                filter.ToSql(),
                order.ToSql()
                );
            SQLiteCommand command = CreateCommand(
                sql, 
                "@" + rightInfo.KeyMembers[0].Column.Name,
                rightInfo.KeyMembers[0].GetValue(right)
                );
            // HACK
            return ListHack(left, command, limit); 
        }
        
        private Array ListHack(Type type, IDbCommand command, Limit limit)
        {
            Entity info = Obtain(type);
            using (SQLiteDataReader reader = ExecuteReader(command as SQLiteCommand))
            {
                return info.Accessor.ListFromReaderDynamic(reader, limit).ToArray(type);
            }
        }

        /// <summary>
        /// If TableA and TableB are linked in a many-to-many relation, the linking table should be called "TableA_TableB",
        /// with an underscore. If the key column from a table 'TableX' does not start with 'TableX', the key column in the
        /// linking table will be prepended with 'TableX'.
        /// Example: TableA with key 'Id' and 'TableB' with key 'Id' will be linked in table 'TableA_TableB' with two key
        /// columns 'TableA_Id' and 'TableB_Id'.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void AddManyToMany(object left, object right)
        {
            Entity leftInfo = Obtain(left.GetType());
            Entity rightInfo = Obtain(right.GetType());
            string between = leftInfo.Table.Name + "_" + rightInfo.Table.Name;
            string leftkey = leftInfo.KeyMembers[0].Column.Name;
            if (!leftkey.StartsWith(leftInfo.Table.Name, StringComparison.OrdinalIgnoreCase)) leftkey = leftInfo.Table.Name + '_' + leftkey;
            string rightkey = rightInfo.KeyMembers[0].Column.Name;
            if (!rightkey.StartsWith(rightInfo.Table.Name, StringComparison.OrdinalIgnoreCase)) rightkey = rightInfo.Table.Name + '_' + rightkey;
            string sql = string.Format(@"
                REPLACE INTO [{0}]({1}, {2}) VALUES(@{1}, @{2})",
                between, 
                leftkey,
                rightkey
                );
            SQLiteCommand command = CreateCommand(
                sql, 
                "@" + leftkey,
                leftInfo.KeyMembers[0].GetValue(left),
                "@" + rightkey,
                rightInfo.KeyMembers[0].GetValue(right)
                );
            ExecuteNonQuery(command);
        }
        
        /*
         * void AddManyToMany(object left, object right, Entity leftInfo, Entity rightInfo, string between, string leftkey, string rightkey)
        {
        }
         */
        
        public void DelManyToMany(object left, object right)
        {
            Entity leftInfo = Obtain(left.GetType());
            Entity rightInfo = Obtain(right.GetType());
            string between = leftInfo.Table.Name + rightInfo.Table.Name;
            string sql = string.Format(@"
                DELETE FROM [{0}] WHERE {1}=@{1} AND {2}=@{2}",
                between, 
                leftInfo.KeyMembers[0].Column.Name,
                rightInfo.KeyMembers[0].Column.Name
                );
            SQLiteCommand command = CreateCommand(
                sql, 
                "@" + leftInfo.KeyMembers[0].Column.Name,
                leftInfo.KeyMembers[0].GetValue(left),
                "@" + rightInfo.KeyMembers[0].Column.Name,
                rightInfo.KeyMembers[0].GetValue(right)
                );
            ExecuteNonQuery(command);
        }
        
        public void Insert(object obj)
        {
            Entity info = Obtain(obj.GetType());
            int i;
            if (info.InsertCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("INSERT ");
                s.Append("INTO ");
                s.Append(info.Table.Name);
                s.Append(" (");
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
                    s.Append("; SELECT last_insert_rowid();");
                }
                info.InsertCommandText = s.ToString();
                Log.Debug("Insert SQL: " + info.InsertCommandText);
            }
            SQLiteCommand cmd = CreateCommand(info.InsertCommandText);
            
            info.Accessor.AddParametersToCommandFixed(obj, cmd);
            
            object autokey = ExecuteScalar(cmd);
            if (info.AutoKeyMember != null)
            {
                info.AutoKeyMember.SetValue(obj, Convert.ToInt32(autokey));
            }
        }

        public void Insert(UnitOfWork unitOfWork, object obj)
        {
            throw new NotImplementedException();
        }

        public void Update(object obj)
        {
            Entity info = Obtain(obj.GetType());
            int i;
            if (info.UpdateCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("UPDATE ");
                s.Append("");
                s.Append(info.Table.Name);
                s.Append(" SET ");
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
            SQLiteCommand cmd = CreateCommand(info.UpdateCommandText);
            info.Accessor.AddParametersToCommandFixed(obj, cmd);
            ExecuteNonQuery(cmd);
        }

        public void Update(UnitOfWork unitOfWork, object obj)
        {
            throw new NotImplementedException();
        }

        public void Save(object obj)
        {
            throw new NotImplementedException();
        }
        
        public void Delete(object obj)
        {
            throw new NotImplementedException();
        }
        
        public void Delete(Type type, params object[] keys)
        {
            Entity info = Obtain(type);
            int i;
            if (info.DeleteCommandText == null)
            {
                StringBuilder s = new StringBuilder();
                s.Append("DELETE ");
                s.Append(" FROM ");
                s.Append(info.Table.Name);
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
                info.DeleteCommandText = s.ToString();
                Log.Debug("Delete SQL: " + info.DeleteCommandText);
            }
            SQLiteCommand cmd = CreateCommand(info.DeleteCommandText);
            i = 0;
            foreach (EntityMember m in info.KeyMembers)
            {
                cmd.Parameters.Add(new SQLiteParameter("@" + m.Column.Name, keys[i]));
                i++;
            }
            ExecuteNonQuery(cmd);
        }

        public void Delete(UnitOfWork unitOfWork, object obj)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll(Type type, Filter filter)
        {
            Entity info = Obtain(type);
            StringBuilder s = new StringBuilder();
            s.Append("DELETE ");
            s.Append(" FROM ");
            s.Append(info.Table.Name);
            s.Append("");
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
            string s = "SELECT COUNT(*) FROM " + info.Table.Name + "";
            if (filter != null && !filter.IsEmpty)
                s += " WHERE " + filter;
            long result = (long)ExecuteScalar(s);
            return (int)result;
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
                s.Append(" FROM ");
                s.Append(info.Table);
                s.Append("");
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
                using (SQLiteDataReader reader = ExecuteReader(s.ToString()))
                    while (reader.Read())
                        result.Add(reader[key], info.Accessor.CreateFromReaderFixed(reader, 0));
                return result;
            }
            else
            {
                StringBuilder s = new StringBuilder();
                s.Append("SELECT ").Append(key).Append(",").Append(value);
                s.Append(" FROM ").Append(info.Table).Append("");
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
                using (SQLiteDataReader reader = ExecuteReader(s.ToString()))
                    while (reader.Read())
                        result.Add(reader[0], reader[1]);
            }
            return result;
        }
    }
}
