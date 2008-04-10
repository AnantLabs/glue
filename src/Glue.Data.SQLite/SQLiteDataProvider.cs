using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Providers.SQLite
{
    /// <summary>
    /// Summary description for SqlHelper.
    /// </summary>
    public class SQLiteDataProvider : IDataProvider
    {
        protected string connectionString;
        protected SQLiteConnection connection;

        /// <summary>
        /// SQLiteHelper
        /// </summary>
        public SQLiteDataProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// SQLiteHelper
        /// </summary>
        public SQLiteDataProvider(string server, string database, string username, string password)
        {
            this.connectionString = "Data Source=" + database + "; Pooling=True; Version=3; UTF8Encoding=True;";
            //this.connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected SQLiteDataProvider(XmlNode node)
        {
            connectionString = Configuration.GetAttr(node, "connectionString", null);
            if (connectionString == null)
            {
                string server   = Configuration.GetAttr(node, "server");
                string database = Configuration.GetAttr(node, "database");
                string username = Configuration.GetAttr(node, "username", null);
                string password = Configuration.GetAttr(node, "password", null);
                this.connectionString = "Data Source=" + database;
                //connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
            }
        }

        protected SQLiteDataProvider(SQLiteConnection connection)
        {
            this.connection = connection;
            this.connection.Open();
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
        public SQLiteConnection CreateConnection()
        {
            if (this.connection != null)
                return this.connection;
            else
                return new SQLiteConnection(this.connectionString);
        }

        public ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SetParameter
        /// </summary>
        public SQLiteParameter SetParameter(SQLiteCommand command, string name, object value)
        {
            SQLiteParameter p;
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
            {
                //return command.Parameters[command.Parameters.Add(new SQLiteParameter(name, value))];
                p = new SQLiteParameter(name, value);
                command.Parameters.Add(p);
                return p;
            }
            p = command.Parameters[i];
            p.Value = value;
            if (v.GetType() == typeof(byte[]))
                p.Size = ((byte[])v).Length;
            return p;
        }

        public SQLiteParameter SetParameter(SQLiteCommand command, string name, DbType type, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
            {
                SQLiteParameter p = command.Parameters.Add(name, type);
                p.Value = value;
                return p;
            }
            else
            {
                SQLiteParameter p = command.Parameters[i];
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
        public void SetParameters(SQLiteCommand command, params object[] paramNameValueList)
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
                                    command.Parameters.Add(new SQLiteParameter("@" + rec.GetName(i), DBNull.Value));
                                else
                                    command.Parameters.Add(new SQLiteParameter("@" + rec.GetName(i), rec.GetValue(i)));
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
                            command.Parameters.Add(new SQLiteParameter(name, DBNull.Value));
                        else
                            command.Parameters.Add(new SQLiteParameter(name, p));
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
        public SQLiteCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return CreateCommand(CreateConnection(), commandText, paramNameValueList);
        }
        
        /// <summary>
        /// CreateCommand
        /// </summary>
        public SQLiteCommand CreateCommand(SQLiteConnection connection, string commandText, params object[] paramNameValueList)
        {
            SQLiteCommand command = new SQLiteCommand(commandText, connection);
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

        public SQLiteCommand CreateSelectCommand(
            string table, 
            string columns, 
            Filter constraint,
            Order order,
            Limit limit,
            params object[] paramNameValueList)
        {
            return CreateSelectCommand(CreateConnection(), table, columns, constraint, order, limit, paramNameValueList);
        }

        /// <summary>
        /// CreateSelectCommand
        /// </summary>
        public SQLiteCommand CreateSelectCommand(
            SQLiteConnection connection, 
            string table, 
            string columns, 
            Filter constraint,
            Order order,
            Limit limit,
            params object[] paramNameValueList)
        {
            SQLiteCommand command = new SQLiteCommand();
            SetParameters(command, paramNameValueList);
            command.Connection = connection;
            
            constraint = Filter.Coalesce(constraint);
            order = Order.Coalesce(order);
            limit = Limit.Coalesce(limit);
            table = SqlName(table);

            StringBuilder s = new StringBuilder();
            if (limit.Index == 0 && limit.Count >= 0)
                s.Append("SELECT TOP ").Append(limit.Count).Append(" ");
            else
                s.Append("SELECT ");

            s.Append(columns);
            s.Append(" FROM ");
            s.Append(table);
            
            if (!constraint.IsEmpty)
                s.Append(" WHERE ").Append(constraint);

            if (!order.IsEmpty)
                s.Append(" ORDER BY ").Append(order);

            if (limit.Index > 0 && limit.Count >= 0)
                s.Append(" LIMIT ").Append(limit.Index).Append(',').Append(limit.Count);
            else
                s.Append(" LIMIT ").Append(limit.Index);

            command.CommandText = s.ToString();
            command.Connection = connection;
            return command;
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public SQLiteCommand CreateInsertCommand(string table, string columns)
        {
            return CreateInsertCommand(CreateConnection(), table, columns);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public SQLiteCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return CreateInsertCommand(CreateConnection(), table, columnNameValueList);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public SQLiteCommand CreateInsertCommand(SQLiteConnection connection, string table, params object[] columnNameValueList)
        {
            SQLiteCommand command = new SQLiteCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("INSERT ");
            s.Append(SqlName(table));
            s.Append(" (");
            foreach (SQLiteParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(") VALUES (");
            foreach (SQLiteParameter param in command.Parameters)
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
        public SQLiteCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateUpdateCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public SQLiteCommand CreateUpdateCommand(SQLiteConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            SQLiteCommand command = new SQLiteCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("UPDATE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (SQLiteParameter param in command.Parameters)
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

        public SQLiteCommand CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateReplaceCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        public SQLiteCommand CreateReplaceCommand(SQLiteConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            SQLiteCommand command = new SQLiteCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("REPLACE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (SQLiteParameter param in command.Parameters)
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
        /// CreateStoredProcedureCommand
        /// </summary>
        public SQLiteCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CreateStoredProcedureCommand
        /// </summary>
        public SQLiteCommand CreateStoredProcedureCommand(SQLiteConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contains
        /// </summary>
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
        public int ExecuteNonQuery(SQLiteCommand command)
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
        public SQLiteDataReader ExecuteReader(SQLiteCommand command)
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
        public SQLiteDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return ExecuteReader(CreateCommand(commandText, paramNameValueList));            
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        public object ExecuteScalar(SQLiteCommand command)
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

        public int ExecuteScalarInt32(SQLiteCommand command)
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
        public string ExecuteScalarString(SQLiteCommand command)
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
            return this.SetParameter((SQLiteCommand)command, name, value);
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, DbType type, object value)
        {
            return this.SetParameter((SQLiteCommand)command, name, type, value);
        }

        void Glue.Data.IDataProvider.SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.SetParameters((SQLiteCommand)command, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand(commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(IDbConnection connection, string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand((SQLiteConnection)connection, commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand(table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(IDbConnection connection, string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand((SQLiteConnection)connection, table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand(table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(IDbConnection connection, string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand((SQLiteConnection)connection, table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand((SQLiteConnection)connection, table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand((SQLiteConnection)connection, table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand(storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(IDbConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand((SQLiteConnection)connection, storedProcedureName, paramNameValueList);
        }

        int Glue.Data.IDataProvider.ExecuteNonQuery(IDbCommand command)
        {
            return this.ExecuteNonQuery((SQLiteCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(IDbCommand command)
        {
            return this.ExecuteReader((SQLiteCommand)command);
        }

        IDataReader Glue.Data.IDataProvider.ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return this.ExecuteReader(commandText, paramNameValueList);
        }

        object Glue.Data.IDataProvider.ExecuteScalar(IDbCommand command)
        {
            return this.ExecuteScalar((SQLiteCommand)command);
        }

        int Glue.Data.IDataProvider.ExecuteScalarInt32(IDbCommand command)
        {
            return this.ExecuteScalarInt32((SQLiteCommand)command);
        }

        string Glue.Data.IDataProvider.ExecuteScalarString(IDbCommand command)
        {
            return this.ExecuteScalarString((SQLiteCommand)command);
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