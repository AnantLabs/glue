using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Providers.MySql
{
    public class MySqlDataProviderOBSOLETE : IDataProvider
    {
        string _connectionString;
        MySqlConnection _connection;
        MySqlTransaction _transaction;

        /// <summary>
        /// MySqlHelper
        /// </summary>
        public MySqlDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// MySqlHelper
        /// </summary>
        public MySqlDataProvider(string server, string database, string username, string password)
        {
            _connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected MySqlDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server   = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                _connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
            }
        }

        /// <summary>
        /// Copy constructor for opening sessions and transactions
        /// </summary>
        protected MySqlDataProvider(MySqlDataProvider provider)
        {
            _connectionString = provider._connectionString;
        }

        /// <summary>
        /// Create a copy of current instance. Derived classes should
        /// override this to create a copy of their own.
        /// </summary>
        /// <returns></returns>
        protected virtual MySqlDataProvider Copy()
        {
            return new MySqlDataProvider(this);
        }

        /// <summary>
        /// Open session
        /// </summary>
        public MySqlDataProvider Open()
        {
            return Open(IsolationLevel.Unspecified);
        }

        public MySqlDataProvider Open(IsolationLevel level)
        {
            MySqlDataProvider copy = Copy();
            copy.OpenInternal(level);
            return copy;
        }

        protected virtual void OpenInternal(IsolationLevel level)
        {
            _connection = CreateConnection();
            _connection.Open();
            if (level != IsolationLevel.Unspecified)
                _transaction = _connection.BeginTransaction(level);
        }

        public void Cancel()
        {
            if (_connection != null)
                if (_transaction != null) {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }
        }

        public void Close()
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

        /// <summary>
        /// Converts a native value to the SQL representation for this provider.
        /// </summary>
        string ToSql(object v)
        {
            if (v == null)
                throw new ArgumentException("Cannot convert null to a SQL constant.");
            Type t = v.GetType();
            if (t == typeof(String))
            {
                StringBuilder result = new StringBuilder();
                result.Append('\'');
                foreach (char c in (string)v)
                {
                    if ("\\\r\n\t\'\"%_".IndexOf(c) > -1) // needs escape?
                        result.Append('\\'); // escape it
                    result.Append(c);
                }
                result.Append('\'');
                return result.ToString();
            }
            if (t == typeof(Boolean))
                return (Boolean)v ? "1" : "0";
            if (t == typeof(Char))
                return (Char)v == '\'' ? "''''" : "'" + (Char)v + "'";
            if (t == typeof(Int32))
                return ((Int32)v).ToString();
            if (t == typeof(Byte))
                return ((Byte)v).ToString();
            if (t.IsPrimitive)
                return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(Guid))
                return "'{" + ((Guid)v).ToString("D") + "}'";
            if (t == typeof(DateTime))
                return "'" + ((DateTime)v).ToString("yyyy'-'MM'-'dd HH':'mm':'ss':'fff") + "'";
            throw new ArgumentException("Cannot convert type " + t + " to a SQL constant.");
        }

        /// <summary>
        /// Create a QueryBuilder
        /// </summary>
        protected QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('?', '`', '`');
        }

        /// <summary>
        /// ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// CreateConnection
        /// </summary>
        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(this._connectionString);
        }

        public MySqlConnection GetConnection()
        {
            return _connection ?? CreateConnection();
        }

        public ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// AddParameter
        /// </summary>
        public MySqlParameter AddParameter(MySqlCommand command, string name, object value)
        {
            MySqlParameter parameter = new MySqlParameter(name, value ?? DBNull.Value);
            if (value is byte[])
                parameter.Size = ((byte[])value).Length;
            command.Parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// SetParameter
        /// </summary>
        public MySqlParameter SetParameter(MySqlCommand command, string name, object value)
        {
            int index = command.Parameters.IndexOf(name);
            MySqlParameter parameter;
            if (index < 0)
            {
                parameter = new MySqlParameter(name, value ?? DBNull.Value);
                command.Parameters.Add(parameter);
            }
            else
            {
                parameter = command.Parameters[index];
                parameter.Value = value ?? DBNull.Value;
            }
            if (value is byte[])
                parameter.Size = ((byte[])value).Length;
            return parameter;
        }

        /// <summary>
        /// SetParameters
        /// </summary>
        public void AddParameters(MySqlCommand command, params object[] paramNameValueList)
        {
            SetParameters(command, paramNameValueList);
        }

        /// <summary>
        /// SetParameters
        /// </summary>
        public void SetParameters(MySqlCommand command, params object[] paramNameValueList)
        {
            if (paramNameValueList == null)
                return;
            int state = 0; 
            string name = null;
            foreach (object p in paramNameValueList)
            {
                switch (state)
                {
                    case 0:
                        if (p == null)
                            throw new ApplicationException("Null value encountered, expected parameter name string.");
                        if (p.GetType() == typeof(string))
                        {
                            name = (string)p;
                            if (name[0] == '-')
                            {
                                command.Parameters.RemoveAt("?" + name.Substring(1));
                            } 
                            else
                            {
                                if (name[0] != '?')
                                    name = "?" + name;
                                state = 1;
                            }
                        } 
                        else if (p is IDataRecord)
                        {
                            IDataRecord rec = (IDataRecord)p;
                            for (int i = 0; i < rec.FieldCount; i++)
                            {
                                if (rec[i] == null || rec[i] == DBNull.Value)
                                    command.Parameters.AddWithValue("?" + rec.GetName(i), DBNull.Value);
                                else
                                    command.Parameters.AddWithValue("?" + rec.GetName(i), rec.GetValue(i));
                            }
                        }
                        else if (p.GetType() == typeof(object[]))
                        {
                            SetParameters(command, (object[])p);
                        }
                        else 
                        {
                            throw new ApplicationException("Expected parameter name or Row object");
                        }
                        break;
                    case 1:
                        if (p == null)
                            command.Parameters.AddWithValue(name, DBNull.Value);
                        else
                            command.Parameters.AddWithValue(name, p);
                        name = null;
                        state = 0;
                        break;
                }
            }
            if (state == 1)
            {
                throw new ApplicationException("Unexpected end of parameterlist.");
            }
        }

        /// <summary>
        /// CreateCommand
        /// </summary>
        public MySqlCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            MySqlCommand command = new MySqlCommand(commandText, GetConnection());
            AddParameters(command, paramNameValueList);
            return command;
        }
        
        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public MySqlCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CreateSelectCommand
        /// </summary>
        public MySqlCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            AddParameters(command, paramNameValueList);

            QueryBuilder s = CreateQueryBuilder();
            s.Append("SELECT ");
            s.Append(columns);
            s.Append(" FROM ");
            s.Identifier(table);
            s.Filter(constraint);
            s.Order(order);
            s.Limit(limit);

            command.CommandText = s.ToString();
            return command;
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public MySqlCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            AddParameters(command, columnNameValueList);

            QueryBuilder s = CreateQueryBuilder();
            s.Append("INSERT INTO ");
            s.Identifier(table);
            s.Append(" (");
            s.ColumnList(command.Parameters);
            s.Append(") VALUES (");
            s.ParameterList(command.Parameters);
            s.Append(")");
            
            command.CommandText = s.ToString();
            return command;
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public MySqlCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            AddParameters(command, columnNameValueList);

            QueryBuilder s = CreateQueryBuilder();
            s.Append("UPDATE ");
            s.Identifier(table);
            s.Append(" SET ");
            s.ColumnAndParameterList(command.Parameters, "=", ",");
            s.Filter(constraint);
            
            command.CommandText = s.ToString();
            return command;
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public MySqlCommand CreateReplaceCommand(string table, params object[] columnNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            AddParameters(command, columnNameValueList);

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

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        public int ExecuteNonQuery(MySqlCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = GetConnection();
            command.Transaction = _transaction;
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
        /// ExecuteNonQuery
        /// </summary>
        public int ExecuteNonQuery(string commandText, params object[] paramNameValueList)
        {
            return ExecuteNonQuery(CreateCommand(commandText, paramNameValueList));            
        }

        /// <summary>
        /// ExecuteReader
        /// </summary>
        public MySqlDataReader ExecuteReader(MySqlCommand command)
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

        /// <summary>
        /// ExecuteReader
        /// </summary>
        public MySqlDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return ExecuteReader(CreateCommand(commandText, paramNameValueList));            
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        public object ExecuteScalar(MySqlCommand command)
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
        
        /// <summary>
        /// ExecuteScalar
        /// </summary>
        public object ExecuteScalar(string commandText, params object[] paramNameValueList)
        {
            return ExecuteScalar(CreateCommand(commandText, paramNameValueList));
        }

        /// <summary>
        /// ExecuteScalarInt32
        /// </summary>
        public int ExecuteScalarInt32(MySqlCommand command)
        {
            return Convert.ToInt32(ExecuteScalar(command));
        }

        /// <summary>
        /// ExecuteScalarInt32
        /// </summary>
        public int ExecuteScalarInt32(string commandText, params object[] paramNameValueList)
        {
            return Convert.ToInt32(ExecuteScalar(commandText, paramNameValueList));
        }

        /// <summary>
        /// ExecuteScalar returns string or null if DBNull
        /// </summary>
        public string ExecuteScalarString(MySqlCommand command)
        {
            return NullConvert.ToString(ExecuteScalar(command));
        }
        
        /// <summary>
        /// ExecuteScalar returns string or null if DBNull
        /// </summary>
        public string ExecuteScalarString(string commandText, params object[] paramNameValueList)
        {
            return NullConvert.ToString(ExecuteScalar(commandText, paramNameValueList));
        }
 
        #region IDataProvider Members

        IDataProvider IDataProvider.Open()
        {
            return ((MySqlDataProvider)this).Open(IsolationLevel.Unspecified);
        }

        IDataProvider IDataProvider.Open(IsolationLevel level)
        {
            return Open(level);
        }

        IDbDataParameter Glue.Data.IDataProvider.AddParameter(IDbCommand command, string name, object value)
        {
            return this.AddParameter((MySqlCommand)command, name, value);
        }

        void Glue.Data.IDataProvider.AddParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.AddParameters((MySqlCommand)command, paramNameValueList);
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, object value)
        {
            return this.SetParameter((MySqlCommand)command, name, value);
        }

        void Glue.Data.IDataProvider.SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.SetParameters((MySqlCommand)command, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand(commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand(storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand(table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand(table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(string table, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand(table, columnNameValueList);
        }

        int Glue.Data.IDataProvider.ExecuteNonQuery(IDbCommand command)
        {
            return this.ExecuteNonQuery((MySqlCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(IDbCommand command)
        {
            return this.ExecuteReader((MySqlCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return this.ExecuteReader(commandText, paramNameValueList);
        }

        object Glue.Data.IDataProvider.ExecuteScalar(IDbCommand command)
        {
            return this.ExecuteScalar((MySqlCommand)command);
        }

        int Glue.Data.IDataProvider.ExecuteScalarInt32(IDbCommand command)
        {
            return this.ExecuteScalarInt32((MySqlCommand)command);
        }

        string Glue.Data.IDataProvider.ExecuteScalarString(IDbCommand command)
        {
            return this.ExecuteScalarString((MySqlCommand)command);
        }

        #endregion
    }
}