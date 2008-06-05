using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    public abstract class BaseDataProvider : IDataProvider
    {
        protected string _connectionString;
        protected IDbConnection _connection;
        protected IDbTransaction _transaction;

        /// <summary>
        /// Returns the ConnectionString for this DataProvider
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Protected default constructor.
        /// </summary>
        protected BaseDataProvider()
        {
        }

        /// <summary>
        /// Protected copy constructor. Needed for Open() methods.
        /// </summary>
        protected BaseDataProvider(BaseDataProvider provider)
        {
            _connectionString = provider._connectionString;
        }

        /// <summary>
        /// Initialize the DataProvider with given connection string.
        /// </summary>
        public BaseDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Create a copy of this connection. Override this in your derived provider.
        /// Only copy the connection string, do not copy connection and transaction fields.
        /// Needed for Open() methods.
        /// </summary>
        protected abstract object Copy();

        public abstract ISchemaProvider GetSchemaProvider();

        #region Sessions, Transactions

        /// <summary>
        /// Internal open function. 
        /// </summary>
        protected virtual void InternalOpen(IsolationLevel level)
        {
            _connection = CreateConnection();
            _connection.Open();
            if (level != IsolationLevel.Unspecified)
                _transaction = _connection.BeginTransaction(level);
        }

        /// <summary>
        /// Open connection and return a cloned provider associated with this connection.
        /// </summary>
        /// <example>
        /// using (IDataProvider provider = Provider.Current.Open()) {
        ///     provider.ExecuteNonQuery("update Contacts set Login=Login+1 where Id=@Id", "Id",10);
        ///     ...
        ///     provider.ExecuteNonQuery( ... );
        /// }
        /// </example>
        public IDataProvider Open()
        {
            return Open(IsolationLevel.Unspecified);
        }

        /// <summary>
        /// Open connection and return a cloned provider associated with this connection.
        /// </summary>
        /// <example>
        /// using (IDataProvider provider = Provider.Current.Open()) {
        ///     provider.ExecuteNonQuery("update Contacts set Login=Login+1 where Id=@Id", "Id",10);
        ///     ...
        ///     provider.ExecuteNonQuery( ... );
        /// }
        /// </example>
        public IDataProvider Open(IsolationLevel level)
        {
            BaseDataProvider copy = (BaseDataProvider)Copy();
            copy.InternalOpen(level);
            return copy;
        }

        /// <summary>
        /// Rollback transaction.
        /// </summary>
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

        /// <summary>
        /// Close and commit any pending transactions
        /// </summary>
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

        /// <summary>
        /// Close and commit any pending transactions
        /// </summary>
        void IDisposable.Dispose()
        {
            Close();
        }

        #endregion

        #region Commands, Parameters

        /// <summary>
        /// Create a provider specific connection. Override this in your derived provider.
        /// </summary>
        public abstract IDbConnection CreateConnection();

        /// <summary>
        /// Get an existing connection or create a new one.
        /// </summary>
        protected IDbConnection GetConnection()
        {
            return _connection == null ? CreateConnection() : _connection;
        }

        /// <summary>
        /// Collect parameter names and values from a array of objects.
        /// </summary>
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
                        throw new DataException("Null value encountered, expected parameter name string.");

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
                        throw new DataException("Expected parameter name or IDataRecord");
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
                throw new DataException("Unexpected end of parameterlist.");
        }

        /// <summary>
        /// Add a single parameters on a IDbCommand using a name/value pair. 
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.AddParameter(cmd, "Id", myGuid);
        /// </example>
        /// </remarks>
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

        /// <summary>
        /// Set a single parameters on a IDbCommand using a name/value pair. 
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.SetParameter(cmd, "Id", myGuid);
        /// </example>
        /// </remarks>
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

        /// <summary>
        /// Set parameters on IDbCommand using a name/value parameter-collection.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="paramNameValueList">Parameters as name/ value pairs</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.SetParameters(cmd, "Id", myGuid, "Name", name);
        /// </example>
        /// </remarks>
        public void AddParameters(IDbCommand command, params object[] paramNameValueList)
        {
            foreach (DictionaryEntry entry in CollectParameters(paramNameValueList))
                AddParameter(command, (string)entry.Key, entry.Value);
        }

        /// <summary>
        /// Set parameters on IDbCommand using a name/value parameter-collection.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="paramNameValueList">Parameters as name/ value pairs</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.SetParameters(cmd, "Id", myGuid, "Name", name);
        /// </example>
        /// </remarks>
        public void SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            foreach (DictionaryEntry entry in CollectParameters(paramNameValueList))
                SetParameter(command, (string)entry.Key, entry.Value);
        }

        /// <summary>
        /// Create a QueryBuilder helper specific to the SQL dialect of
        /// this provider.
        /// </summary>
        protected abstract QueryBuilder CreateQueryBuilder();

        /// <summary>
        /// Create command from command text and parameters
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="paramNameValueList">Parameters</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateCommand("SELECT * FROM User Where Name=@Name", "Name", name);
        /// </example>
        public abstract IDbCommand CreateCommand(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Create SELECT command
        /// </summary>
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

        /// <summary>
        /// Create INSERT-command
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateInsertCommand("User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        public virtual IDbCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            IDbCommand command = CreateCommand(null, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("INSERT INTO ").Identifier(table);
            s.Append(" (").Columns(command.Parameters).Append(") VALUES");
            s.Append(" (").Parameters(command.Parameters).Append(")");
            command.CommandText = s.ToString();
            return command;
        }

        /// <summary>
        /// Create UPDATE command and set up parameters
        /// </summary>
        /// <example>
        /// DataProvider.Current.CreateInsertCommand("User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        public virtual IDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            IDbCommand command = CreateCommand(null, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("UPDATE ").Identifier(table).Append(" SET ");
            s.ColumnsParameters(command.Parameters, "=", ",");
            s.Filter(constraint);
            command.CommandText = s.ToString();
            return command;
        }

        /// <summary>
        /// Create stored procedure command and initialize parameters.
        /// </summary>
        /// <example>
        /// DataProvider.Current.CreateStoredProcedureCommand("FindUserByEmail", "Name", "john@doe");
        /// </example>
        public virtual IDbCommand CreateStoredProcedureCommand(string sproc, params object[] paramNameValueList)
        {
            IDbCommand command = CreateCommand(sproc, paramNameValueList);
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }
        
        #endregion

        #region Execute methods

        /// <summary>
        /// Execute non-query command. No need to set Connection and Transaction properties on the command.
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns number of rows affected (if applicable).</returns>
        /// <example>
        /// IDbCommand command = MyProvider.CreateCommand("UPDATE Contact SET Logins=Logins+1 WHERE Id=@Id", "Id", 20);
        /// MyProvider.ExecuteNonQuery(command);
        /// </example>
        public virtual int ExecuteNonQuery(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
            {
                command.Connection = GetConnection();
                command.Transaction = _transaction;
            }
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

        /// <summary>
        /// Execute non-query command. No need to set Connection and Transaction properties on the command.
        /// Returns number of rows affected (if applicable).
        /// </summary>
        /// <example>
        /// DataProvider.Current.ExecuteNonQuery(
        ///     "UPDATE Contact SET DisplayName=@DisplayName WHERE Id=@Id", 
        ///     "Id", 10,                   // @Id => 10
        ///     "DisplayName", "John Doe"   // @DisplayName => "John Doe"
        /// );
        /// </example>
        public int ExecuteNonQuery(string commandText, params object[] paramNameValueList)
        {
            return ExecuteNonQuery(CreateCommand(commandText, paramNameValueList));
        }

        /// <summary>
        /// Execute command returning data in a IDataReader. No need to set Connection and Transaction 
        /// properties on the command. You are responsible for closing the IDataReader. Easiest way
        /// is with a "using" statement.
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns an open IDataReader</returns>
        /// <example>
        /// IDbCommand command = DataProvider.Current.CreateSelectCommand(
        ///     "Contacts",             // table 
        ///     "Id,DisplayName",       // columns
        ///     null,                   // filter
        ///     "-DisplayName,+Id",     // order
        ///     Limit.Range(100,110)    // limit
        /// );
        /// using (IDataReader reader = DataProvider.Current.ExecuteReader(command))
        ///     while (reader.Read())
        ///         Console.WriteLine(reader["Id"]);
        /// </example>
        public virtual IDataReader ExecuteReader(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
            {
                command.Connection = GetConnection();
                command.Transaction = _transaction;
            }
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

        /// <summary>
        /// Execute command returning data in a IDataReader. No need to set Connection and Transaction 
        /// properties on the command. You are responsible for closing the IDataReader. Easiest way
        /// is with a "using" statement.
        /// Returns an open IDataReader.
        /// </summary>
        /// <example>
        /// using (IDataReader reader = DataProvider.Current.ExecuteReader("SELECT * FROM Contacts"))
        ///     while (reader.Read())
        ///         Console.WriteLine(reader[0]);
        /// </example>
        public IDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return ExecuteReader(CreateCommand(commandText, paramNameValueList));
        }

        /// <summary>
        /// Execute command returning scalar value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// DateTime? dt = (DataTime?)DataProvider.Current.ExecuteScalar(command);
        /// </example>
        public virtual object ExecuteScalar(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
            {
                command.Connection = GetConnection();
                command.Transaction = _transaction;
            }
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

        /// <summary>
        /// Execute command returning scalar value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// Returns single value (scalar).
        /// </summary>
        /// <example>
        /// DateTime? dt = (DataTime?)DataProvider.Current.ExecuteScalar("SELECT BirthDate FROM Contacts WHERE Id=@Id", "Id",10);
        /// </example>
        public object ExecuteScalar(string commandText, params object[] paramNameValueList)
        {
            return ExecuteScalar(CreateCommand(commandText, paramNameValueList));
        }

        /// <summary>
        /// Execute command returning an int value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// int count = DataProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
        /// </example>
        public int ExecuteScalarInt32(IDbCommand command)
        {
            return Convert.ToInt32(ExecuteScalar(command));
        }

        /// <summary>
        /// Execute command returning an int value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// Returns single int value.
        /// </summary>
        /// <example>
        /// int count = DataProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
        /// </example>
        public int ExecuteScalarInt32(string commandText, params object[] paramNameValueList)
        {
            return Convert.ToInt32(ExecuteScalar(CreateCommand(commandText, paramNameValueList)));
        }

        /// <summary>
        /// Execute command returning a string. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns string or null.</returns>
        /// <example>
        /// string name = DataProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id",10");
        /// </example>
        public string ExecuteScalarString(IDbCommand command)
        {
            return NullConvert.ToString(ExecuteScalar(command));
        }

        /// <summary>
        /// Execute command returning a string. No need to set Connection and Transaction 
        /// properties on the command. 
        /// Returns string or null.
        /// </summary>
        /// <example>
        /// string name = DataProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id",10");
        /// </example>
        public string ExecuteScalarString(string commandText, params object[] paramNameValueList)
        {
            return NullConvert.ToString(ExecuteScalar(CreateCommand(commandText, paramNameValueList)));
        }

        #endregion

        #region Mapping 

        /// <summary>
        /// Create an Accessor class for high performance reading and writing objects 
        /// from and to the database.Override this in derived class.
        /// </summary>
        protected internal abstract Accessor CreateAccessor(Type type);

        /// <summary>
        /// Invalidate cached objects.
        /// </summary>
        protected void InvalidateCache(Type type)
        {
            // TODO: Caching per connectionstring or per provider
        }

        /// <summary>
        /// Find object by its primary key(s). Returns null if not found.
        /// </summary>
        public virtual object Find(Type type, params object[] keys)
        {
            Accessor info = Accessor.Obtain(this, type);

            if (info.Entity.Table.Cached)
            {
                // TODO: Cache per connectionstring
                // if (info.Cache == null)
                //    info.Entity.Cache = Map(type, null, null);
                // return info.Entity.Cache[keys[0]];
            }

            if (info.FindCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("SELECT ");
                s.Columns(EntityMemberList.Flatten(info.Entity.AllMembers));
                s.Append(" FROM ");
                s.Identifier(info.Entity.Table.Name);
                s.Append(" WHERE ");
                s.ColumnsParameters(info.Entity.KeyMembers, "=", " AND ");

                info.FindCommandText = s.ToString();
                Log.Debug("Find SQL: " + info.FindCommandText);
            }

            IDbCommand command = CreateCommand(info.FindCommandText);
            int i = 0;
            foreach (EntityMember m in info.Entity.KeyMembers)
                AddParameter(command, m.Column.Name, keys[i++]);

            using (IDataReader reader = ExecuteReader(command))
                if (reader.Read())
                    return info.CreateFromReaderFixed(reader, 0);
                else
                    return null;
        }

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        public object FindByFilter(Type type, Filter filter)
        {
            return FindByFilter(type, filter, null);
        }

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        public object FindByFilter(Type type, Filter filter, Order order)
        {
            Array list = List(type, filter, order, Limit.One);
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        public object FindByFilter(string table, Type type, Filter filter)
        {
            return FindByFilter(table, type, filter, null);
        }

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        public object FindByFilter(string table, Type type, Filter filter, Order order)
        {
            Array list = List(table, type, filter, null, Limit.One);
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        public object FindByFilter(Type type, IDbCommand command)
        {
            Array list = List(type, command);
            if (list != null && list.Length > 0)
                return list.GetValue(0);
            else
                return null;
        }

        /// <summary>
        /// Return objects of given type. Parameters filter, order and limit can be null.
        /// </summary>
        public Array List(Type type, Filter filter, Order order, Limit limit)
        {
            return List(null, type, filter, order, limit);
        }

        /// <summary>
        /// Return objects of given type. Parameters filter, order and limit can be null.
        /// </summary>
        public virtual Array List(string table, Type type, Filter filter, Order order, Limit limit)
        {
            Accessor info = Accessor.Obtain(this, type);

            if (table == null)
                table = info.Entity.Table.Name;

            // Make sure the order by clause contains all key members
            order = Order.Coalesce(order);
            foreach (EntityMember m in info.Entity.KeyMembers)
                if (!order.Contains(m.Column.Name))
                    order = order.Append(m.Column.Name);

            // Create column list
            QueryBuilder columns = CreateQueryBuilder().Columns(
                EntityMemberList.Flatten(info.Entity.AllMembers)
                );

            // Get results
            IDbCommand command = CreateSelectCommand(table, columns.ToString(), filter, order, limit);
            using (IDataReader reader = ExecuteReader(command))
            {
                return info.ListFromReaderFixed(reader).ToArray(type);
            }
        }

        /// <summary>
        /// Return objects of given type selected by command.
        /// </summary>
        public Array List(Type type, IDbCommand command)
        {
            Accessor info = Accessor.Obtain(this, type);
            using (IDataReader reader = ExecuteReader(command))
            {
                return info.ListFromReaderDynamic(reader, Limit.Unlimited).ToArray(type);
            }
        }

        /// <summary>
        /// Insert given object.
        /// </summary>
        public virtual void Insert(object obj)
        {
            Type type = obj.GetType();
            Accessor info = Accessor.Obtain(this, type);

            if (info.InsertCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("INSERT INTO ");
                s.Identifier(info.Entity.Table.Name);
                s.Append(" (");
                // Obtain a flattened list of all columns excluding 
                // automatic ones (autoint, calculated fields)
                EntityMemberList columns = EntityMemberList.Subtract(
                    EntityMemberList.Flatten(info.Entity.AllMembers),
                    EntityMemberList.Flatten(info.Entity.AutoMembers)
                    );
                s.Columns(columns);
                s.AppendLine(")");
                s.Append(" VALUES (");
                s.Parameters(columns);
                s.AppendLine(")");
                if (info.Entity.AutoKeyMember != null)
                {
                    s.Next();
                    s.SelectIdentity();
                    s.Next();
                }
                info.InsertCommandText = s.ToString();
            }

            IDbCommand command = CreateCommand(info.InsertCommandText);
            info.AddParametersToCommandFixed(obj, command);

            object autokey = ExecuteScalar(command);
            if (info.Entity.AutoKeyMember != null)
            {
                info.Entity.AutoKeyMember.SetValue(obj, Convert.ToInt32(autokey));
            }

            InvalidateCache(type);
        }

        /// <summary>
        /// Update given object.
        /// </summary>
        public void Update(object obj)
        {
            Type type = obj.GetType();
            Accessor info = Accessor.Obtain(this, type);

            if (info.UpdateCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("UPDATE ");
                s.Identifier(info.Entity.Table.Name);
                s.AppendLine(" SET ");
                // set all non-key, non-auto members
                EntityMemberList cols = EntityMemberList.Flatten(EntityMemberList.Subtract(info.Entity.AllMembers, info.Entity.KeyMembers, info.Entity.AutoMembers));
                s.ColumnsParameters(cols, "=", ",");
                s.AppendLine();
                s.AppendLine(" WHERE ");
                s.ColumnsParameters(info.Entity.KeyMembers, "=", " AND ");
                info.UpdateCommandText = s.ToString();
                Log.Debug("Update SQL: " + info.UpdateCommandText);
            }

            IDbCommand cmd = CreateCommand(info.UpdateCommandText);
            info.AddParametersToCommandFixed(obj, cmd);
            ExecuteNonQuery(cmd);

            InvalidateCache(type);
        }

        /// <summary>
        /// Save (insert or update) given object.
        /// </summary>
        public void Save(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete given object.
        /// </summary>
        public void Delete(object obj)
        {
            Type type = obj.GetType();
            Accessor info = Accessor.Obtain(this, type);

            EntityMemberList keys = info.Entity.KeyMembers;
            object[] values = new object[keys.Count];
            for (int i = 0; i < keys.Count; i++)
                values[i] = keys[i].GetValue(obj);

            Delete(type, values);
        }

        /// <summary>
        /// Delete object by primary key(s).
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <param name="keys">Keys</param>
        public void Delete(Type type, params object[] keys)
        {
            Accessor info = Accessor.Obtain(this, type);

            if (info.DeleteCommandText == null)
            {
                QueryBuilder s = CreateQueryBuilder();
                s.Append("DELETE FROM ");
                s.Append(info.Entity.Table.Name);
                s.AppendLine(" WHERE ");
                s.ColumnsParameters(info.Entity.KeyMembers, "=", " AND ");

                info.DeleteCommandText = s.ToString();
                Log.Debug("Delete SQL: " + info.DeleteCommandText);
            }

            IDbCommand cmd = CreateCommand(info.DeleteCommandText);
            int i = 0;
            foreach (EntityMember m in info.Entity.KeyMembers)
                AddParameter(cmd, m.Column.Name, keys[i++]);
            ExecuteNonQuery(cmd);
            InvalidateCache(type);
        }

        /// <summary>
        /// Delete all objects satisfying given filter.
        /// </summary>
        public void DeleteAll(Type type, Filter filter)
        {
            Accessor info = Accessor.Obtain(this, type);

            QueryBuilder s = CreateQueryBuilder();
            s.Append("DELETE FROM ");
            s.Identifier(info.Entity.Table.Name);
            s.Filter(filter);

            ExecuteNonQuery(s.ToString());
            InvalidateCache(type);
        }

        /// <summary>
        /// Determine number of objects satisfying given filter.
        /// </summary>
        public int Count(Type type, Filter filter)
        {
            Accessor info = Accessor.Obtain(this, type);

            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT COUNT(*) FROM ");
            s.Identifier(info.Entity.Table.Name);
            s.Filter(filter);

            return Convert.ToInt32(ExecuteScalar(s.ToString()));
        }

        /// <summary>
        /// List all associated (right-side) objects for given instance (left-side) 
        /// in a many-to-many relationship. Explicitly specify the joining table.
        /// </summary>
        public Array ListManyToMany(object left, Type right, string jointable)
        {
            return ListManyToMany(left, right, jointable, null, null, null);
        }

        /// <summary>
        /// List all associated (right-side) objects for given instance (left-side) 
        /// in a many-to-many relationship. Explicitly specify the joining table.
        /// Filter, order and limit can be null.
        /// </summary>
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

            // Create column list
            QueryBuilder columns = CreateQueryBuilder();
            columns.Columns(info.RightTable, EntityMemberList.Flatten(info.RightInfo.AllMembers), ",").ToString();

            // Create command
            IDbCommand command = CreateSelectCommand(
                join.ToString(), columns.ToString(), filter, order, limit,
                info.JoinLeftKey, info.LeftKeyInfo.GetValue(left)
                );

            // Get right-hand side objects
            Accessor accessor = Accessor.Obtain(this, info.RightType);
            using (IDataReader reader = ExecuteReader(command))
            {
                return accessor.ListFromReaderFixed(reader).ToArray(info.RightType);
            }
        }

        /// <summary>
        /// Create an association between left and right object in a 
        /// many-to-many relationship. Explicitly specify the joining table.
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
        /// Delete an association between left and right object in a 
        /// many-to-many relationship. Explicitly specify the joining table.
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

        /// <summary>
        /// Creates a dictionary of key-entity pairs for a given type. 
        /// </summary>
        public IDictionary Map(Type type, Filter filter, Order order)
        {
            OrderedDictionary result = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            Accessor info = Accessor.Obtain(this, type);

            if (info.Entity.KeyMembers.Count != 1)
                throw new InvalidOperationException("Entity should have precisely one key column: " + info.Entity.Type.ToString() + " - " + info.Entity.Table.Name);
            EntityMember key = info.Entity.KeyMembers[0];

            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT ");
            s.Columns(EntityMemberList.Flatten(info.Entity.AllMembers));
            s.Append(" FROM ");
            s.Identifier(info.Entity.Table.Name);
            s.Filter(filter);
            s.Order(order);

            Log.Debug("Map SQL: " + s);
            using (IDataReader reader = ExecuteReader(s.ToString()))
                while (reader.Read())
                {
                    object instance = info.CreateFromReaderFixed(reader, 0);
                    object keyvalue = key.Field.GetValue(instance);
                    result.Add(keyvalue, instance);
                }
            return result;
        }

        /// <summary>
        /// Creates a dictionary of key-value pairs where the keys and values are taken from two columns in a table.
        /// </summary>
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

        /// <summary>
        /// Find object by its primary key(s). Returns null if not found.
        /// </summary>
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
