using System;
using System.Xml;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    public abstract class BaseDataProvider : IDataProvider
    {
        protected string _connectionString;
        protected IDbConnection _connection;
        protected IDbTransaction _transaction;

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        protected BaseDataProvider()
        {
        }

        public BaseDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected BaseDataProvider(BaseDataProvider provider)
        {
            _connectionString = provider._connectionString;
        }

        protected abstract object Copy();

        #region Sessions, Transactions

        protected virtual void InternalOpen(IsolationLevel level)
        {
            _connection = CreateConnection();
            _connection.Open();
            if (level != IsolationLevel.Unspecified)
                _transaction = _connection.BeginTransaction(level);
        }

        public IDataProvider Open()
        {
            return Open(IsolationLevel.Unspecified);
        }

        public IDataProvider Open(IsolationLevel level)
        {
            BaseDataProvider copy = (BaseDataProvider)Copy();
            copy.InternalOpen(level);
            return copy;
        }

        public virtual void Cancel()
        {
            if (_connection != null)
                if (_transaction != null)
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }
        }

        public virtual void Close()
        {
            if (_connection != null)
            {
                if (_transaction != null)
                {
                    _transaction.Commit();
                    _transaction.Dispose();
                }
                _connection.Close();
                _transaction = null;
                _connection = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Commands, Parameters

        public abstract IDbConnection CreateConnection();

        protected IDbConnection GetConnection()
        {
            return _connection == null ? CreateConnection() : _connection;
        }

        public virtual IDbDataParameter AddParameter(IDbCommand command, string name, object value)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            if (value is byte[])
                parameter.Size = ((byte[])value).Length;
            else if (value is char[])
                parameter.Size = ((char[])value).Length;
            command.Parameters.Add(parameter);
            return parameter;
        }

        public virtual IDbDataParameter SetParameter(IDbCommand command, string name, object value)
        {
            IDbDataParameter parameter;
            int index = command.Parameters.IndexOf(name);
            if (index < 0)
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = name;
                command.Parameters.Add(parameter);
            }
            else
                parameter = (IDbDataParameter)command.Parameters[index];
            parameter.Value = value ?? DBNull.Value;
            if (value is byte[])
                parameter.Size = ((byte[])value).Length;
            else if (value is char[])
                parameter.Size = ((char[])value).Length;
            return parameter;
        }

        protected IEnumerable CollectParameters(object[] bag)
        {
            if (bag == null)
                yield break;

            DictionaryEntry entry;
            string name = null;
            foreach (object item in bag)
            {
                if (name == null)
                {
                    if (item == null)
                        throw new ApplicationException("Null value encountered, expected parameter name string.");

                    if (item is string)
                    {
                        name = (string)item;
                    }
                    else if (item is IDataRecord)
                    {
                        IDataRecord record = (IDataRecord)item;
                        for (int i = 0; i < record.FieldCount; i++)
                        {
                            entry.Key = record.GetName(i);
                            entry.Value = record.GetValue(i);
                            yield return entry;
                        }
                    }
                    else if (item is object[])
                    {
                        foreach (DictionaryEntry child in CollectParameters((object[])item))
                            yield return child;
                    }
                    else
                    {
                        throw new ApplicationException("Expected parameter name or IDataRecord");
                    }
                }
                else
                {
                    entry.Key = name;
                    entry.Value = item;
                    name = null;
                    yield return entry;
                }
            }

            if (name != null)
                throw new ApplicationException("Unexpected end of parameterlist.");

        }

        public void AddParameters(IDbCommand command, params object[] paramNameValueList)
        {
            foreach (DictionaryEntry entry in CollectParameters(paramNameValueList))
                AddParameter(command, (string)entry.Key, entry.Value);
        }

        public void SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            foreach (DictionaryEntry entry in CollectParameters(paramNameValueList))
                SetParameter(command, (string)entry.Key, entry.Value);
        }

        protected abstract QueryBuilder CreateQueryBuilder();

        public abstract IDbCommand CreateCommand(string commandText, params object[] paramNameValueList);

        public virtual IDbCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            QueryBuilder s = CreateQueryBuilder();
            s.Append(" SELECT ");
            s.Append(columns);
            s.Append(" FROM ");
            s.Identifier(table);
            s.Filter(constraint);
            s.Order(order);
            s.Limit(limit);
            return CreateCommand(s.ToString(), paramNameValueList);
        }

        public virtual IDbCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            IDbCommand command = CreateCommand(null, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("INSERT INTO ").Identifier(table);
            s.Append(" (").ColumnList(command.Parameters).Append(") VALUES");
            s.Append(" (").ParameterList(command.Parameters).Append(")");
            command.CommandText = s.ToString();
            return command;
        }

        public virtual IDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            IDbCommand command = CreateCommand(null, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("UPDATE ").Identifier(table).Append(" SET ");
            s.ColumnAndParameterList(command.Parameters, "=", ",");
            s.Filter(constraint);
            command.CommandText = s.ToString();
            return command;
        }

        public virtual IDbCommand CreateReplaceCommand(string table, params object[] columnNameValueList)
        {
            IDbCommand command = CreateCommand(null, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("REPLACE INTO ");
            s.Identifier(table);
            s.Append(" (");
            s.ColumnList(command.Parameters);
            s.Append(") VALUES (");
            s.ParameterList(command.Parameters);
            s.Append(")");
            command.CommandText = s.ToString();
            return command;
        }

        public virtual IDbCommand CreateStoredProcedureCommand(string sproc, params object[] paramNameValueList)
        {
            IDbCommand command = CreateCommand(sproc, paramNameValueList);
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }
        
        #endregion

        #region Execute methods

        public virtual int ExecuteNonQuery(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = GetConnection();
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();
            else
                leaveOpen = true;
            try
            {
                return command.ExecuteNonQuery();
            }
            finally
            {
                if (!leaveOpen)
                    command.Connection.Close();
            }
        }

        public int ExecuteNonQuery(string commandText, params object[] paramNameValueList)
        {
            return ExecuteNonQuery(CreateCommand(commandText, paramNameValueList));
        }

        public virtual IDataReader ExecuteReader(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = GetConnection();
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();
            else
                leaveOpen = true;
            try
            {
                return command.ExecuteReader(leaveOpen ? CommandBehavior.Default : CommandBehavior.CloseConnection);
            }
            catch
            {
                if (!leaveOpen)
                    command.Connection.Close();
                throw;
            }
        }

        public IDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return ExecuteReader(CreateCommand(commandText, paramNameValueList));
        }

        public virtual object ExecuteScalar(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = GetConnection();
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();
            else
                leaveOpen = true;
            try
            {
                return command.ExecuteScalar();
            }
            finally
            {
                if (!leaveOpen)
                    command.Connection.Close();
            }
        }

        public object ExecuteScalar(string commandText, params object[] paramNameValueList)
        {
            return ExecuteScalar(CreateCommand(commandText, paramNameValueList));
        }

        public int ExecuteScalarInt32(IDbCommand command)
        {
            return Convert.ToInt32(ExecuteScalar(command));
        }

        public int ExecuteScalarInt32(string commandText, params object[] paramNameValueList)
        {
            return Convert.ToInt32(ExecuteScalar(CreateCommand(commandText, paramNameValueList)));
        }

        public string ExecuteScalarString(IDbCommand command)
        {
            return NullConvert.ToString(ExecuteScalar(command));
        }

        public string ExecuteScalarString(string commandText, params object[] paramNameValueList)
        {
            return NullConvert.ToString(ExecuteScalar(CreateCommand(commandText, paramNameValueList)));
        }

        #endregion

        #region Mapping 

        /// <summary>
        /// Override this in derived class
        /// </summary>
        /// <returns></returns>
        protected internal abstract Accessor CreateAccessor(Type type);

        public virtual object Find(Type type, params object[] keys)
        {
            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);

            if (info.Table.Cached)
            {
                if (info.Cache == null)
                    info.Cache = Map(type, null, null);
                return info.Cache[keys[0]];
            }

            if (acc.FindCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("SELECT ");
                s.ColumnList(EntityMemberList.Flatten(info.AllMembers));
                s.Append(" FROM ");
                s.Identifier(info.Table.Name);
                s.Append(" WHERE ");
                s.ColumnAndParameterList(info.KeyMembers, "=", " AND ");

                acc.FindCommandText = s.ToString();
                Log.Debug("Find SQL: " + acc.FindCommandText);
            }

            IDbCommand command = CreateCommand(acc.FindCommandText);
            int i = 0;
            foreach (EntityMember m in info.KeyMembers)
                AddParameter(command, m.Column.Name, keys[i++]);

            using (IDataReader reader = ExecuteReader(command))
                if (reader.Read())
                    return acc.Accessor.CreateFromReaderFixed(reader, 0);
                else
                    return null;
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

        public Array List(Type type, Filter filter, Order order, Limit limit)
        {
            return List(null, type, filter, order, limit);
        }

        public virtual Array List(string table, Type type, Filter filter, Order order, Limit limit)
        {
            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);

            if (table == null)
                table = info.Table.Name;

            // Make sure the order by clause contains all key members
            order = Order.Coalesce(order);
            foreach (EntityMember m in info.KeyMembers)
                if (!order.Contains(m.Column.Name))
                    order = order.Append(m.Column.Name);

            // Create column list
            QueryBuilder columns = CreateQueryBuilder().ColumnList(EntityMemberList.Flatten(info.AllMembers));

            // Get results
            IDbCommand command = CreateSelectCommand(table, columns.ToString(), filter, order, limit);
            using (IDataReader reader = ExecuteReader(command))
            {
                return acc.Accessor.ListFromReaderFixed(reader).ToArray(type);
            }
        }

        public Array List(Type type, IDbCommand command)
        {
            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);
            using (IDataReader reader = ExecuteReader(command))
            {
                return acc.Accessor.ListFromReaderDynamic(reader, Limit.Unlimited).ToArray(type);
            }
        }

        public Array ListManyToMany(object left, Type right, string jointable)
        {
            return ListManyToMany(left, right, jointable, null, null, null);
        }

        public virtual Array ListManyToMany(object left, Type right, string jointable, Filter filter, Order order, Limit limit)
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
            join.Identifier(info.RightTable);
            join.Append(" INNER JOIN ");
            join.Identifier(info.JoinTable);
            join.Append(" ON ").Identifier(info.RightTable, info.RightKey).Append("=").Identifier(info.JoinTable, info.JoinRightKey);

            // Expand filter to be
            //   ContactCategory.CategoryId=@CategoryId AND (..additonal if specified..)
            filter = Filter.And(info.JoinTable + "." + info.JoinLeftKey + "=@" + info.JoinLeftKey, filter);

            // Be sure the order contains the primary key of the item sought, in this example
            //   Contact.ContactId
            if (!order.Contains(info.RightKey))
                order = order.Append(info.RightTable + "." + info.RightKey);

            // Create command
            QueryBuilder columns = CreateQueryBuilder();
            columns.ColumnList(info.RightTable, EntityMemberList.Flatten(info.RightInfo.AllMembers), ",").ToString();

            IDbCommand command = CreateSelectCommand(
                join.ToString(), columns.ToString(), filter, order, limit,
                info.JoinLeftKey, info.LeftKeyInfo.GetValue(left)
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
            s.Append("REPLACE INTO ").Identifier(info.JoinTable).Append("(");
            s.Identifier(info.JoinLeftKey).Append(",");
            s.Identifier(info.JoinRightKey);
            s.Append(") VALUES (");
            s.Parameter(info.JoinLeftKey).Append(",");
            s.Parameter(info.JoinRightKey);
            s.Append(")");

            IDbCommand command = CreateCommand(s.ToString());
            AddParameter(command, info.JoinLeftKey, info.LeftKeyInfo.GetValue(left));
            AddParameter(command, info.JoinRightKey, info.RightKeyInfo.GetValue(right));
            ExecuteNonQuery(command);
        }

        /// <summary>
        /// Remove a n:m relation between object left and object right. Explicitly specify the
        /// joining table.
        /// </summary>
        public virtual void DelManyToMany(object left, object right, string jointable)
        {
            ManyToManyInfo info = new ManyToManyInfo(left.GetType(), right.GetType(), jointable);

            QueryBuilder s = CreateQueryBuilder();
            s.Append(" DELETE FROM ").Identifier(info.JoinTable);
            s.Append(" WHERE ").Identifier(info.JoinLeftKey).Append("=").Parameter(info.JoinLeftKey);
            s.Append(" AND ").Identifier(info.JoinRightKey).Append("=").Parameter(info.JoinRightKey);

            IDbCommand command = CreateCommand(s.ToString());
            AddParameter(command, info.JoinLeftKey, info.LeftKeyInfo.GetValue(left));
            AddParameter(command, info.JoinRightKey, info.RightKeyInfo.GetValue(right));
            ExecuteNonQuery(command);
        }

        public virtual void Insert(object obj)
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
                    s.AppendLine("SELECT @@IDENTITY");
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

        public void Update(object obj)
        {
            Type type = obj.GetType();
            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);

            if (acc.UpdateCommandText == null)
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
                acc.UpdateCommandText = s.ToString();
                Log.Debug("Update SQL: " + acc.UpdateCommandText);
            }

            IDbCommand cmd = CreateCommand(acc.UpdateCommandText);
            acc.Accessor.AddParametersToCommandFixed(obj, cmd);
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
            Entity info = Entity.Obtain(obj.GetType());
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
            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);

            if (acc.DeleteCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("DELETE FROM ");
                s.Append(info.Table.Name);
                s.AppendLine(" WHERE ");
                s.ColumnAndParameterList(info.KeyMembers, "=", " AND ");

                acc.DeleteCommandText = s.ToString();
                Log.Debug("Delete SQL: " + acc.DeleteCommandText);
            }

            IDbCommand cmd = CreateCommand(acc.DeleteCommandText);
            int i = 0;
            foreach (EntityMember m in info.KeyMembers)
                AddParameter(cmd, m.Column.Name, keys[i++]);
            ExecuteNonQuery(cmd);
            info.Cache = null; // invalidate cache if present
        }

        public void DeleteAll(Type type, Filter filter)
        {
            Entity info = Entity.Obtain(type);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("DELETE FROM ");
            s.Identifier(info.Table.Name);
            s.Filter(filter);

            ExecuteNonQuery(s.ToString());
            info.Cache = null; // invalidate cache if present
        }

        public int Count(Type type, Filter filter)
        {
            Entity info = Entity.Obtain(type);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT COUNT(*) FROM ");
            s.Identifier(info.Table.Name);
            s.Filter(filter);

            return Convert.ToInt32(ExecuteScalar(s.ToString()));
        }

        public IDictionary Map(Type type, Filter filter, Order order)
        {
            OrderedDictionary result = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            Entity info = Entity.Obtain(type);
            AccessorInfo acc = AccessorInfo.Obtain(this, type);

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
                    object instance = acc.Accessor.CreateFromReaderFixed(reader, 0);
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

        #endregion

        #region Generic methods

        public T Find<T>(params object[] keys)
        {
            return (T)Find(typeof(T), keys);
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

        #endregion
    }
}
