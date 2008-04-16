using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.Sql
{
    public class SqlDataProviderOBSOLETE : IDataProvider
    {
        string _connectionString;
        SqlConnection _connection;
        SqlTransaction _transaction;

        /// <summary>
        /// SqlHelper
        /// </summary>
        public SqlDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// SqlDataProvider
        /// </summary>
        public SqlDataProvider(string server, string database, string username, string password)
        {
            _connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected SqlDataProvider(XmlNode node)
        {
            _connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (_connectionString == null)
            {
                string server   = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                if (username != null)
                    _connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
                else
                    _connectionString = "server=" + server + ";database=" + database + ";integrated security=true";
            }
        }

        /// <summary>
        /// Creates a QueryBuilder
        /// </summary>
        protected QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder('@', '[', ']');
        }

        /// <summary>
        /// Copy constructor for opening sessions and transactions
        /// </summary>
        protected SqlDataProvider(SqlDataProvider provider)
        {
            _connectionString = provider._connectionString;
        }

        /// <summary>
        /// Create a copy of current instance. Derived classes should
        /// override this to create a copy of their own.
        /// </summary>
        /// <returns></returns>
        protected virtual SqlDataProvider Copy()
        {
            return new SqlDataProvider(this);
        }

        /// <summary>
        /// Open session
        /// </summary>
        public SqlDataProvider Open()
        {
            return Open(IsolationLevel.Unspecified);
        }

        public SqlDataProvider Open(IsolationLevel level)
        {
            SqlDataProvider copy = Copy();
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
                return "'" + ((String)v).Replace("'", "''") + "'";
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
            return new QueryBuilder('@', '[', ']');
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
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(this._connectionString);
        }

        public SqlConnection GetConnection()
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
        public SqlParameter AddParameter(SqlCommand command, string name, object value)
        {
            SqlParameter parameter = new SqlParameter(name, value ?? DBNull.Value);
            command.Parameters.Add(parameter);
            if (value is byte[])
                parameter.Size = ((byte[])value).Length;
            return parameter;
        }

        /// <summary>
        /// SetParameter
        /// </summary>
        public SqlParameter SetParameter(SqlCommand command, string name, object value)
        {
            int index = command.Parameters.IndexOf(name);
            SqlParameter parameter;
            if (index < 0)
            {
                parameter = new SqlParameter(name, value ?? DBNull.Value);
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
        /// CollectParameters
        /// </summary>
        internal IEnumerable CollectParameters(params object[] paramNameValueList)
        {
            if (paramNameValueList == null)
                yield break;
            int state = 0;
            string name = null;
            foreach (object p in paramNameValueList)
            {
                switch (state)
                {
                    case 0:
                        if (p == null)
                            throw new ApplicationException("Null value encountered, expected parameter name string.");
                        if (p is string)
                        {
                            name = (string)p;
                            state = 1;
                        }
                        else if (p is IDataRecord)
                        {
                            IDataRecord record = (IDataRecord)p;
                            for (int i = 0; i < record.FieldCount; i++)
                                yield return new SqlParameter(record.GetName(i), record.GetValue(i) ?? DBNull.Value);
                        }
                        else if (p is object[])
                        {
                            yield return CollectParameters((object[])p);
                        }
                        else
                        {
                            throw new ApplicationException("Expected parameter name or IDataRecord");
                        }
                        break;
                    case 1:
                        yield return new SqlParameter(name, p ?? DBNull.Value);
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
        /// AddParameters
        /// </summary>
        public void AddParameters(SqlCommand command, params object[] paramNameValueList)
        {
            foreach (SqlParameter parameter in CollectParameters(paramNameValueList))
                command.Parameters.Add(parameter);
        }

        /// <summary>
        /// SetParameters
        /// </summary>
        public void SetParameters(SqlCommand command, params object[] paramNameValueList)
        {
            foreach (SqlParameter parameter in CollectParameters(paramNameValueList))
            {
                int index = command.Parameters.IndexOf(parameter.ParameterName);
                if (index >= 0)
                    command.Parameters.RemoveAt(index);
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// CreateCommand
        /// </summary>
        public SqlCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand(commandText, GetConnection());
            AddParameters(command, paramNameValueList);
            return command;
        }

        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public SqlCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand(storedProcedureName, GetConnection());
            command.CommandType = CommandType.StoredProcedure;
            AddParameters(command, paramNameValueList);
            return command;
        }

        /// <summary>
        /// CreateSelectCommand
        /// ASSUMPTION: The order clause contains the primary key (or keys in case of a combined primary key)
        /// </summary>
        public SqlCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            QueryBuilder s = CreateQueryBuilder();

            constraint = Filter.Coalesce(constraint);
            order = Order.Coalesce(order);
            limit = Limit.Coalesce(limit);

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

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public SqlCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            SqlCommand command = new SqlCommand();
            AddParameters(command, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("INSERT INTO ").Identifier(table);
            s.Append("(").ColumnList(command.Parameters).Append(") VALUES ");
            s.Append("(").ParameterList(command.Parameters).Append(")");
            command.CommandText = s.ToString();
            return command;
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public SqlCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            SqlCommand command = new SqlCommand();
            AddParameters(command, columnNameValueList);
            QueryBuilder s = CreateQueryBuilder();
            s.Append("UPDATE ").Identifier(table).Append(" SET ");
            s.ColumnAndParameterList(command.Parameters, "=", ",");
            s.Filter(constraint);
            command.CommandText = s.ToString();
            command.Connection = GetConnection();
            return command;
        }

        /// <summary>
        /// CreateReplaceCommand
        /// </summary>
        public SqlCommand CreateReplaceCommand(string table, params object[] columnNameValueList)
        {
            throw new NotImplementedException();

            //SqlCommand command = new SqlCommand();
            //AddParameters(command, columnNameValueList);
            //QueryBuilder s = CreateQueryBuilder();
            //s.Append("UPDATE ").Identifier(table).Append(" SET ");
            //s.ColumnAndParameterList(command.Parameters, "=", ",");
            //s.Filter(constraint);
            //s.AppendLine();
            //s.Append("IF @@ROWCOUNT=0 ");
            //s.Append("INSERT INTO ").Identifier(table);
            //s.Append("(").ColumnList(command.Parameters).Append(") VALUES ");
            //s.Append("(").ParameterList(command.Parameters).Append(")");
            //s.AppendLine();
            //command.CommandText = s.ToString();
            //return command;
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        public int ExecuteNonQuery(SqlCommand command)
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
        public SqlDataReader ExecuteReader(SqlCommand command)
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
        public SqlDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return ExecuteReader(CreateCommand(commandText, paramNameValueList));            
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        public object ExecuteScalar(SqlCommand command)
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
        public int ExecuteScalarInt32(SqlCommand command)
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
        public string ExecuteScalarString(SqlCommand command)
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
            return Open();
        }

        IDataProvider IDataProvider.Open(IsolationLevel level)
        {
            return Open(level);
        }

        IDbDataParameter Glue.Data.IDataProvider.AddParameter(IDbCommand command, string name, object value)
        {
            return this.AddParameter((SqlCommand)command, name, value);
        }

        void Glue.Data.IDataProvider.AddParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.AddParameters((SqlCommand)command, paramNameValueList);
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, object value)
        {
            return this.SetParameter((SqlCommand)command, name, value);
        }

        void Glue.Data.IDataProvider.SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.SetParameters((SqlCommand)command, paramNameValueList);
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
            return this.ExecuteNonQuery((SqlCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(IDbCommand command)
        {
            return this.ExecuteReader((SqlCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return this.ExecuteReader(commandText, paramNameValueList);
        }

        object Glue.Data.IDataProvider.ExecuteScalar(IDbCommand command)
        {
            return this.ExecuteScalar((SqlCommand)command);
        }

        int Glue.Data.IDataProvider.ExecuteScalarInt32(IDbCommand command)
        {
            return this.ExecuteScalarInt32((SqlCommand)command);
        }

        string Glue.Data.IDataProvider.ExecuteScalarString(IDbCommand command)
        {
            return this.ExecuteScalarString((SqlCommand)command);
        }

        #endregion
    }
}
