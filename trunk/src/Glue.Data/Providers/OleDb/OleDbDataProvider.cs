using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Glue.Lib;

namespace Glue.Data.Providers.OleDb
{
    /// <summary>
    /// Summary description for OleDbHelper.
    /// </summary>
    public class OleDbDataProvider : IDataProvider
    {
        private string connectionString;
        
        /// <summary>
        /// OleDbHelper
        /// </summary>
        public OleDbDataProvider(string connectionString)
        {
            this.connectionString = connectionString;
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

        /// <summary>
        /// OleDbHelper
        /// </summary>
        public OleDbDataProvider(string server, string database, string username, string password)
        {
            this.connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected OleDbDataProvider(XmlNode node)
        {
            connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (connectionString == null)
            {
                string server   = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
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
        public OleDbConnection CreateConnection()
        {
            return new OleDbConnection(this.connectionString);
        }

        public ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SetParameter
        /// </summary>
        public OleDbParameter SetParameter(OleDbCommand command, string name, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
                return command.Parameters.AddWithValue(name, v);
            OleDbParameter p = command.Parameters[i];
            p.Value = value;
            if (v.GetType() == typeof(byte[]))
                p.Size = ((byte[])v).Length;
            return p;
        }

        public OleDbParameter SetParameter(OleDbCommand command, string name, DbType type, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
            {
                OleDbParameter p = command.Parameters.AddWithValue(name, value);
                p.DbType = type;
                return p;
            }
            else
            {
                OleDbParameter p = command.Parameters[i];
                p.DbType = type;
                p.Value = value;
                if (type == DbType.Binary)
                    p.Size = ((byte[])v).Length;
                return p;
            }
        }

        /// <summary>
        /// CreateParameters
        /// </summary>
        public void SetParameters(OleDbCommand command, params object[] paramNameValueList)
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

        string SqlName(string s)
        {
            if (s[0] != '[' && s.IndexOf(' ') < 0)
                return "[" + s + "]";
            else
                return s;
        }

        /// <summary>
        /// CreateCommand
        /// </summary>
        public OleDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return CreateCommand(CreateConnection(), commandText, paramNameValueList);
        }
        
        /// <summary>
        /// CreateCommand
        /// </summary>
        public OleDbCommand CreateCommand(OleDbConnection connection, string commandText, params object[] paramNameValueList)
        {
            OleDbCommand command = new OleDbCommand(commandText, connection);
            SetParameters(command, paramNameValueList);

            return command;
        }

        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public OleDbCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return CreateStoredProcedureCommand(CreateConnection(), storedProcedureName, paramNameValueList);
        }

        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public OleDbCommand CreateStoredProcedureCommand(OleDbConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            OleDbCommand command = new OleDbCommand(storedProcedureName, connection);
            command.CommandType = CommandType.StoredProcedure;
            SetParameters(command, paramNameValueList);
            
            return command;
        }

        /// <summary>
        /// CreateSelectCommand
        /// </summary>
        public OleDbCommand CreateSelectCommand(string table, string columns, Order order, Filter constraint, params object[] paramNameValueList)
        {
            return CreateSelectCommand(CreateConnection(), table, columns, order, constraint, paramNameValueList);
        }

        /// <summary>
        /// CreateSelectCommand
        /// </summary>
        public OleDbCommand CreateSelectCommand(OleDbConnection connection, string table, string columns, Order order, Filter constraint, params object[] paramNameValueList)
        {
            OleDbCommand command = new OleDbCommand();
            SetParameters(command, paramNameValueList);
            command.Connection = connection;
            
            table = SqlName(table);
            command.CommandText = "SELECT " + columns + " FROM " + table;
            if (constraint != null && !constraint.IsEmpty)
                command.CommandText = command.CommandText + " WHERE " + constraint;
            if (order != null && !order.IsEmpty)
                command.CommandText = command.CommandText + " ORDER BY " + order;
            return command;
        }

        /// <summary>
        /// CreatePagedCommand
        /// </summary>
        public OleDbCommand CreateSelectCommand(
            OleDbConnection connection, 
            string table, 
            string columns, 
            Filter constraint,
            Order order,
            Limit limit,
            params object[] paramNameValueList)
        {
            OleDbCommand command = new OleDbCommand();
            SetParameters(command, paramNameValueList);
            command.Connection = connection;

            StringBuilder s = new StringBuilder();
            
            constraint = Filter.Coalesce(constraint);
            order = Order.Coalesce(order);
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
                /*if (nolock)
                    s.Append(" WITH (NOLOCK) ");*/
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
            /*if (nolock)
                s.Append(" WITH (NOLOCK) ");*/
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
        public OleDbCommand CreateSelectCommand(
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
        
        /*
        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public OleDbCommand CreateInsertCommand(string table, string columns)
        {
            return CreateInsertCommand(CreateConnection(), table, columns);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public OleDbCommand CreateInsertCommand(OleDbConnection connection, string table, string columns)
        {
            OleDbCommand command = new OleDbCommand();
            StringBuilder OleDb = new StringBuilder();
            OleDb.Append("INSERT [");
            OleDb.Append(table);
            OleDb.Append("] (");
            string[] cols = columns.Split(',');
            foreach (string col in cols)
            {
                OleDb.Append(col);
                OleDb.Append(',');
            }
            OleDb.Length = OleDb.Length - 1;
            OleDb.Append(") VALUES (");
            foreach (string col in cols)
            {
                OleDb.Append('@');
                OleDb.Append(col);
                OleDb.Append(',');
            }
            OleDb.Length = OleDb.Length - 1;
            OleDb.Append(')');
            command.CommandText = OleDb.ToString();
            command.Connection = connection;
            return command;
        }
        */

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public OleDbCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return CreateInsertCommand(CreateConnection(), table, columnNameValueList);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public OleDbCommand CreateInsertCommand(OleDbConnection connection, string table, params object[] columnNameValueList)
        {
            OleDbCommand command = new OleDbCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder OleDb = new StringBuilder();
            OleDb.Append("INSERT ");
            OleDb.Append(SqlName(table));
            OleDb.Append(" (");
            foreach (OleDbParameter param in command.Parameters)
            {
                OleDb.Append(param.ParameterName.Substring(1));
                OleDb.Append(',');
            }
            OleDb.Length = OleDb.Length - 1;
            OleDb.Append(") VALUES (");
            foreach (OleDbParameter param in command.Parameters)
            {
                OleDb.Append(param.ParameterName);
                OleDb.Append(',');
            }
            OleDb.Length = OleDb.Length - 1;
            OleDb.Append(')');
            command.CommandText = OleDb.ToString();
            command.Connection = connection;

            return command;
        }

        /*
        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public OleDbCommand CreateUpdateCommand(string table, string columns)
        {
            return CreateUpdateCommand(CreateConnection(), table, columns);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public OleDbCommand CreateUpdateCommand(OleDbConnection connection, string table, string columns)
        {
            OleDbCommand command = new OleDbCommand();
            StringBuilder OleDb = new StringBuilder();
            OleDb.Append("UPDATE ");
            OleDb.Append(SqlName(table));
            OleDb.Append(" SET ");
            string[] cols = columns.Split(',');
            foreach (string col in cols)
            {
                OleDb.Append(col);
                OleDb.Append("=@");
                OleDb.Append(col);
                OleDb.Append(',');
            }
            OleDb.Length = OleDb.Length - 1;
            command.CommandText = OleDb.ToString();
            command.Connection = connection;
            return command;
        }
        */

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public OleDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateUpdateCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public OleDbCommand CreateUpdateCommand(OleDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            OleDbCommand command = new OleDbCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder OleDb = new StringBuilder();
            OleDb.Append("UPDATE ");
            OleDb.Append(SqlName(table));
            OleDb.Append(" SET ");
            foreach (OleDbParameter param in command.Parameters)
            {
                OleDb.Append(param.ParameterName.Substring(1));
                OleDb.Append('=');
                OleDb.Append(param.ParameterName);
                OleDb.Append(',');
            }
            OleDb.Length = OleDb.Length - 1;
            command.CommandText = OleDb.ToString();
            command.Connection = connection;

            return command;
        }


        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public OleDbCommand CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateReplaceCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public OleDbCommand CreateReplaceCommand(OleDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            OleDbCommand command = new OleDbCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            
            s.Append("UPDATE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (OleDbParameter param in command.Parameters)
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
            foreach (OleDbParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(") VALUES (");
            foreach (OleDbParameter param in command.Parameters)
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
        public int ExecuteNonQuery(OleDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = CreateConnection();
            else if (command.Connection.State == ConnectionState.Closed)
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
        public OleDbDataReader ExecuteReader(OleDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = CreateConnection();
            else if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();
            else
                leaveOpen = true;

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
        public OleDbDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return ExecuteReader(CreateCommand(commandText, paramNameValueList));            
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        public object ExecuteScalar(OleDbCommand command)
        {
            bool leaveOpen = false;
            if (command.Connection == null)
                command.Connection = CreateConnection();
            else if (command.Connection.State == ConnectionState.Closed)
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

        public int ExecuteScalarInt32(OleDbCommand command)
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
        public string ExecuteScalarString(OleDbCommand command)
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
            return this.SetParameter((OleDbCommand)command, name, value);
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, DbType type, object value)
        {
            return this.SetParameter((OleDbCommand)command, name, type, value);
        }

        void Glue.Data.IDataProvider.SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.SetParameters((OleDbCommand)command, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand(commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(IDbConnection connection, string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand((OleDbConnection)connection, commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand(storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(IDbConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand((OleDbConnection)connection, storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(IDbConnection connection, string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand((OleDbConnection)connection, table, columns, constraint, order, limit, paramNameValueList);
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
            return this.CreateInsertCommand((OleDbConnection)connection, table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand(table, constraint, columnNameValueList);
        }
        
        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand((OleDbConnection)connection, table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand((OleDbConnection)connection, table, constraint, columnNameValueList);
        }

        int Glue.Data.IDataProvider.ExecuteNonQuery(IDbCommand command)
        {
            return this.ExecuteNonQuery((OleDbCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(IDbCommand command)
        {
            return this.ExecuteReader((OleDbCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return this.ExecuteReader(commandText, paramNameValueList);
        }

        object Glue.Data.IDataProvider.ExecuteScalar(IDbCommand command)
        {
            return this.ExecuteScalar((OleDbCommand)command);
        }

        int Glue.Data.IDataProvider.ExecuteScalarInt32(IDbCommand command)
        {
            return this.ExecuteScalarInt32((OleDbCommand)command);
        }

        string Glue.Data.IDataProvider.ExecuteScalarString(IDbCommand command)
        {
            return this.ExecuteScalarString((OleDbCommand)command);
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