using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Glue.Lib;

namespace Glue.Data.Providers.Sql
{
    /// <summary>
    /// Summary description for SqlHelper.
    /// </summary>
    public class SqlDataProvider : IDataProvider
    {
        string connectionString;
        string server;
        string database;
        string username;
        string password;

        /// <summary>
        /// SqlHelper
        /// </summary>
        public SqlDataProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// SqlHelper
        /// </summary>
        public SqlDataProvider(string server, string database, string username, string password)
        {
            this.connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
            this.server = server;
            this.database = database;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected SqlDataProvider(XmlNode node)
        {
            connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (connectionString == null)
            {
                server   = Configuration.GetAttr(node, "server");
                database = Configuration.GetAttr(node, "database");
                username = Configuration.GetAttr(node, "username", null);
                password = Configuration.GetAttr(node, "password", null);
                if (username != null)
                    connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
                else
                    connectionString = "server=" + server + ";database=" + database + ";integrated security=true";
            }
        }

        /// <summary>
        /// CreateConnection
        /// </summary>
        public string ConnectionString
        {
            get { return connectionString; }
        }

        /// <summary>
        /// CreateConnection
        /// </summary>
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(this.connectionString);
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

        public ISchemaProvider GetSchemaProvider()
        {
            return new SqlSchemaProvider(server, username, password);
        }

        /// <summary>
        /// SetParameter
        /// </summary>
        public SqlParameter SetParameter(SqlCommand command, string name, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
                return command.Parameters.AddWithValue(name, v);
            SqlParameter p = command.Parameters[i];
            p.Value = value;
            if (v.GetType() == typeof(byte[]))
                p.Size = ((byte[])v).Length;
            return p;
        }

        public SqlParameter SetParameter(SqlCommand command, string name, DbType type, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
            {
                SqlParameter p = command.Parameters.AddWithValue(name, value);
                p.DbType = type;
                return p;
            }
            else
            {
                SqlParameter p = command.Parameters[i];
                p.DbType = type;
                p.Value = value;
                if (type == DbType.Binary)
                    p.Size = ((byte[])v).Length;
                return p;
            }
        }

        /// <summary>
        /// SetParameters
        /// </summary>
        public void SetParameters(SqlCommand command, params object[] paramNameValueList)
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
                                command.Parameters.RemoveAt("@" + name.Substring(1));
                            } 
                            else
                            {
                                if (name[0] != '@')
                                    name = "@" + name;
                                state = 1;
                            }
                        } 
                        else if (p is IDataRecord)
                        {
                            IDataRecord rec = (IDataRecord)p;
                            for (int i = 0; i < rec.FieldCount; i++)
                            {
                                if (rec[i] == null || rec[i] == DBNull.Value)
                                    command.Parameters.AddWithValue("@" + rec.GetName(i), DBNull.Value);
                                else
                                    command.Parameters.AddWithValue("@" + rec.GetName(i), rec.GetValue(i));
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
        public SqlCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return CreateCommand(CreateConnection(), commandText, paramNameValueList);
        }
        
        /// <summary>
        /// CreateCommand
        /// </summary>
        public SqlCommand CreateCommand(SqlConnection connection, string commandText, params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand(commandText, connection);
            
            SetParameters(command, paramNameValueList);
            return command;
        }

        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public SqlCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return CreateStoredProcedureCommand(CreateConnection(), storedProcedureName, paramNameValueList);
        }

        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public SqlCommand CreateStoredProcedureCommand(SqlConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand(storedProcedureName, connection);
            
            command.CommandType = CommandType.StoredProcedure;
            SetParameters(command, paramNameValueList);
            return command;
        }

        string SqlName(string s)
        {
            if (s[0] != '[' && s.IndexOf(' ') < 0)
                return "[" + s + "]";
            else
                return s;
        }

        /// <summary>
        /// CreateSelectCommand
        /// </summary>
        public SqlCommand CreateSelectCommand(SqlConnection connection, string table, string columns, Filter constraint, Order order, params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand();
            SetParameters(command, paramNameValueList);
            command.Connection = connection;
            
            command.CommandText = "SELECT " + columns + " FROM " + SqlName(table);
            if (constraint != null && !constraint.IsEmpty)
                command.CommandText = command.CommandText + " WHERE " + constraint;
            if (order != null && !order.IsEmpty)
                command.CommandText = command.CommandText + " ORDER BY " + order;
            return command;
        }

        /// <summary>
        /// CreatePagedCommand
        /// ASSUMPTION: The order clause contains the primary key (or keys in case of a combined primary key)
        /// </summary>
        public SqlCommand CreateSelectCommand(
            SqlConnection connection, 
            string table, 
            string columns, 
            Filter constraint,
            Order order,
            Limit limit,
            params object[] paramNameValueList)
        {
            SqlCommand command = new SqlCommand();
            SetParameters(command, paramNameValueList);
            command.Connection = connection;

            StringBuilder s = new StringBuilder();

            order = Order.Coalesce(order);
            constraint = Filter.Coalesce(constraint);
            limit = Limit.Coalesce(limit);

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
                
                // Declare variables for all ordering members
                int i = 0;
                for (i = 0; i < order.Count; i++)
                    s.Append("DECLARE @start_" + order[i].Substring(1) + " sql_variant\r\n");
                
                // Create the first select
                s.Append("SET ROWCOUNT " + limit.Index + "\r\n");
                s.Append("SELECT ");
                for (i = 0; i < order.Count; i++)
                {
                    if (i > 0)
                        s.Append(",");
                    s.Append("@start_" + order[i].Substring(1) + "=" + order[i].Substring(1));
                }
                s.Append(" FROM ");
                s.Append(SqlName(table));
                if (nolock)
                    s.Append(" WITH (NOLOCK) ");
                if (!constraint.IsEmpty)
                {
                    s.Append(" WHERE ");
                    s.Append(constraint);
                }
                s.Append(" ORDER BY ");
                s.Append(order);
                s.Append("\r\n");

                // Now adapt the constraint for use in the subsequent select.
                Filter outside = null;
                for (i = order.Count-1; i >= 0; i--)
                {
                    string col = order[i].Substring(1);
                    if (outside != null)
                        outside = Filter.And("(" + col + " IS NULL) AND (@start_" + col + " IS NULL) OR (" + col + "=@start_" + col + ")", outside);
                    
                    if (order.GetDirection(i) > 0)
                        outside = Filter.Or("NOT(" + col + " IS NULL) AND (@start_" + col + " IS NULL) OR (" + col + ">@start_" + col + ")", outside);
                    else
                        outside = Filter.Or("(" + col + " IS NULL) AND NOT(@start_" + col + " IS NULL) OR (" + col + "<@start_" + col + ")", outside);
                }
                constraint = Filter.And(constraint, outside);
            }

            if (limit.Count >= 0)
            {
                s.Append("SET ROWCOUNT " + limit.Count + "\r\n");
            }

            s.Append("SELECT ");
            s.Append(columns);
            s.Append(" FROM ");
            s.Append(SqlName(table));
            if (nolock)
                s.Append(" WITH (NOLOCK) ");
            if (!constraint.IsEmpty)
            {
                s.Append(" WHERE ");
                s.Append(constraint);
            }
            if (!order.IsEmpty)
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
            
            command.CommandText = s.ToString();
            
            return command;
        }

        /// <summary>
        /// CreatePagedCommand
        /// </summary>
        public SqlCommand CreateSelectCommand(
            string table, 
            string columns, 
            Filter constraint,
            Order order,
            Limit limit,
            params object[] paramNameValueList)
        {
            return CreateSelectCommand(
                CreateConnection(),
                table,
                columns,
                constraint,
                order,
                limit,
                paramNameValueList);
        }
        
        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public SqlCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return CreateInsertCommand(CreateConnection(), table, columnNameValueList);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public SqlCommand CreateInsertCommand(SqlConnection connection, string table, params object[] columnNameValueList)
        {
            SqlCommand command = new SqlCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("INSERT ");
            s.Append(SqlName(table));
            s.Append(" (");
            foreach (SqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(") VALUES (");
            foreach (SqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName);
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(')');
            command.CommandText = s.ToString();
            command.Connection = connection;

            return command;
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public SqlCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateUpdateCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public SqlCommand CreateUpdateCommand(SqlConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            SqlCommand command = new SqlCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("UPDATE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (SqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append('=');
                s.Append(param.ParameterName);
                s.Append(',');
            }
            s.Length = s.Length - 1;
            if (constraint != null && !constraint.IsEmpty)
                s.Append(" WHERE " + constraint);
            command.CommandText = s.ToString();
            command.Connection = connection;

            return command;
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public SqlCommand CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateReplaceCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public SqlCommand CreateReplaceCommand(SqlConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            SqlCommand command = new SqlCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            
            s.Append("UPDATE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (SqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append('=');
                s.Append(param.ParameterName);
                s.Append(',');
            }
            s.Length = s.Length - 1;
            if (constraint != null && !constraint.IsEmpty)
                s.Append(" WHERE " + constraint);
            
            s.Append(" IF @@ROWCOUNT=0 ");
            s.Append(" INSERT ");
            s.Append(SqlName(table));
            s.Append(" (");
            foreach (SqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(") VALUES (");
            foreach (SqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName);
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(')');

            command.CommandText = s.ToString();
            command.Connection = connection;

            return command;
        }

        private bool Contains(string[] strings, string value)
        {
            foreach (string s in strings)
                if (string.Compare(s, value, true) == 0)
                    return true;
            return false;
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        public int ExecuteNonQuery(SqlCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
            {
                command.Connection = CreateConnection();
                command.Connection.Open();
                leaveOpen = false;
            }
            else if (command.Connection.State == ConnectionState.Closed)
            {
                command.Connection.Open();
                leaveOpen = false;
            }
            else
            {
                leaveOpen = true;
            }

            try 
            {
                return command.ExecuteNonQuery();
            }
            /*
            catch
            {
                foreach (SqlParameter p in command.Parameters)
                    Log.Debug("Parameter: " + p.ParameterName + "(" + p.SqlDbType + ")=" + (Convert.IsDBNull(p.Value) ? "NULL" : Convert.ToString(p.Value)));
                throw;
            }
            */
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
            {
                command.Connection = CreateConnection();
                command.Connection.Open();
                leaveOpen = false;
            }
            else if (command.Connection.State == ConnectionState.Closed)
            {
                command.Connection.Open();
                leaveOpen = false;
            }
            else
            {
                leaveOpen = true;
            }

            try 
            {
                CommandBehavior behavior = 
                    leaveOpen ? CommandBehavior.Default : CommandBehavior.CloseConnection;
                return command.ExecuteReader(behavior);
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
            {
                command.Connection = CreateConnection();
                command.Connection.Open();
                leaveOpen = false;
            }
            else if (command.Connection.State == ConnectionState.Closed)
            {
                command.Connection.Open();
                leaveOpen = false;
            }
            else
            {
                leaveOpen = true;
            }

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

        public int ExecuteScalarInt32(SqlCommand command)
        {
            return Convert.ToInt32(ExecuteScalar(command));
        }

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

        IDbConnection Glue.Data.IDataProvider.CreateConnection()
        {
            return this.CreateConnection();
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, object value)
        {
            return this.SetParameter((SqlCommand)command, name, value);
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, DbType type, object value)
        {
            return this.SetParameter((SqlCommand)command, name, type, value);
        }

        void Glue.Data.IDataProvider.SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.SetParameters((SqlCommand)command, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand(commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(IDbConnection connection, string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand((SqlConnection)connection, commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand(storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(IDbConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand((SqlConnection)connection, storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(IDbConnection connection, string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand((SqlConnection)connection, table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand(table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand(table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(IDbConnection connection, string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand((SqlConnection)connection, table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand((SqlConnection)connection, table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand((SqlConnection)connection, table, constraint, columnNameValueList);
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

        /// <summary>
        /// Converts a native value to the SQL representation for this provider.
        /// </summary>
        string Glue.Data.IDataProvider.ToSql(object v)
        {
            if (v == null)
                throw new ArgumentException("Cannot convert null to a SQL constant.");
            Type t = v.GetType();
            if (t == typeof(String))
                return "'" + ((String)v).Replace("'","''") + "'";
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

        #endregion
    }
}