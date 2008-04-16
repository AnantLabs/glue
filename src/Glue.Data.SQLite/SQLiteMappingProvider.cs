using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.IO;
using System.Reflection;
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
        public SQLiteMappingProvider(System.Xml.XmlNode node) : base(node)
        {
        }

        public new SQLiteMappingProvider Open()
        {
            return (SQLiteMappingProvider)base.Open();
        }

        public new SQLiteMappingProvider Open(IsolationLevel level)
        {
            return (SQLiteMappingProvider)base.Open(level);
        }

        IMappingProvider IMappingProvider.Open()
        {
            return this.Open();
        }

        IMappingProvider IMappingProvider.Open(IsolationLevel level)
        {
            return this.Open();
        }
        
        /// <summary>
        /// Get Accessor class to reading / writing object instances.
        /// </summary>
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
                Type accessorType = AccessorHelper.GenerateAccessor(type, "System.Data.SQLite", "SQLite", "@");
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

            SQLiteCommand command = CreateCommand(info.FindCommandText);
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

        public Array List(Type type, Filter filter, Order order, Limit limit)
        {
            return List(null, type, filter, order, limit);
        }
 
        public Array List(string table, Type type, Filter filter, Order order, Limit limit)
        {
            Entity info = Obtain(type);
            if (table == null)
                table = info.Table.Name;

            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT ");
            s.ColumnList(EntityMemberList.Flatten(info.AllMembers));
            s.Append(" FROM ");
            s.Identifier(table);
            s.Filter(filter);
            s.Order(order);
            s.Limit(limit);
            s.AppendLine();

            Log.Debug("List SQL: " + s);
            using (IDataReader reader = ExecuteReader(s.ToString()))
            {
                return GetAccessor(type).ListFromReaderFixed(reader).ToArray(type);
            }
        }

        public Array List(Type type, IDbCommand command)
        {
            Entity info = Obtain(type);
            using (IDataReader reader = ExecuteReader((SQLiteCommand)command))
            {
                return info.Accessor.ListFromReaderDynamic(reader, Limit.Unlimited).ToArray(type);
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
            // Get names and info of all stuff involved
            ManyToManyInfo info = new ManyToManyInfo(left.GetType(), right, jointable);
            
            // For Contact <-- ContactCategory --> Category 
            // code below would expand to:
            //
            //   Category INNER JOIN ContactCategory ON Category.Id=ContactCategory.CategoryId
            QueryBuilder join = CreateQueryBuilder();
            join.Append(" SELECT ");
            join.ColumnList(info.RightTable, EntityMemberList.Flatten(info.RightInfo.AllMembers), ",");
            join.Append(" FROM ");
            join.Identifier(info.RightTable);
            join.Append(" INNER JOIN ");
            join.Identifier(info.JoinTable);
            join.Append(" ON ").Identifier(info.RightTable, info.RightKey).Append("=").Identifier(info.JoinTable, info.JoinRightKey);

            // Expand filter to be
            //   ContactCategory.CategoryId=@CategoryId AND (..additonal if specified..)
            filter = Filter.And(info.JoinTable + "." + info.JoinLeftKey + "=@" + info.JoinLeftKey, filter);
            join.Filter(filter);
            join.Order(order);
            join.Limit(limit);

            SQLiteCommand command = CreateCommand(join.ToString());
            AddParameter(command, info.JoinLeftKey, info.LeftKeyInfo.GetValue(left));

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
            s.Append("REPLACE INTO ").Identifier(info.JoinTable).Append("(");
            s.Identifier(info.JoinLeftKey).Append(","); 
            s.Identifier(info.JoinRightKey);
            s.Append(") VALUES (");
            s.Parameter(info.JoinLeftKey).Append(",");
            s.Parameter(info.JoinRightKey);
            s.Append(")");
            
            SQLiteCommand command = CreateCommand(s.ToString());
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
            
            SQLiteCommand command = CreateCommand(s.ToString());
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

            SQLiteCommand cmd = CreateCommand(info.UpdateCommandText);
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

            SQLiteCommand cmd = CreateCommand(info.DeleteCommandText);
            int i = 0;
            foreach (EntityMember m in info.KeyMembers)
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
