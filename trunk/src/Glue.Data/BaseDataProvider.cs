using System;
using System.Xml;
using System.Data;
using System.Collections;
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

        public virtual int ExecuteNonQuery(IDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = GetConnection();
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();
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
    }
}
