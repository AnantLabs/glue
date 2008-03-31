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
    /// <summary>
    /// Summary description for SqlHelper.
    /// </summary>
    public class MySqlDataProvider : IDataProvider
    {
        private string connectionString;

        /// <summary>
        /// MySqlHelper
        /// </summary>
        public MySqlDataProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// MySqlHelper
        /// </summary>
        public MySqlDataProvider(string server, string database, string username, string password)
        {
            this.connectionString = "server=" + server + ";database=" + database + ";user id=" + username + ";password=" + password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected MySqlDataProvider(XmlNode node)
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
        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(this.connectionString);
        }

        public ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SetParameter
        /// </summary>
        public MySqlParameter SetParameter(MySqlCommand command, string name, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
                return command.Parameters.Add(name, v);
            MySqlParameter p = command.Parameters[i];
            p.Value = value;
            if (v.GetType() == typeof(byte[]))
                p.Size = ((byte[])v).Length;
            return p;
        }

        public MySqlParameter SetParameter(MySqlCommand command, string name, DbType type, object value)
        {
            object v = value == null ? DBNull.Value : value;
            int i = command.Parameters.IndexOf(name);
            if (i < 0)
            {
                MySqlParameter p = command.Parameters.Add(name, value);
                p.DbType = type;
                return p;
            }
            else
            {
                MySqlParameter p = command.Parameters[i];
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
                                    command.Parameters.Add("@" + rec.GetName(i), DBNull.Value);
                                else
                                    command.Parameters.Add("@" + rec.GetName(i), rec.GetValue(i));
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
                            command.Parameters.Add(name, DBNull.Value);
                        else
                            command.Parameters.Add(name, p);
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
            return CreateCommand(CreateConnection(), commandText, paramNameValueList);
        }
        
        /// <summary>
        /// CreateCommand
        /// </summary>
        public MySqlCommand CreateCommand(MySqlConnection connection, string commandText, params object[] paramNameValueList)
        {
            MySqlCommand command = new MySqlCommand(commandText, connection);
            SetParameters(command, paramNameValueList);
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
        /// CreateStoredProcedureCommand
        /// </summary>
        public MySqlCommand CreateStoredProcedureCommand(MySqlConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            throw new NotImplementedException();
        }

        string SqlName(string s)
        {
            if (s[0] != '`' && s.IndexOf(' ') < 0)
                return "`" + s + "`";
            else
                return s;
        }

        public MySqlCommand CreateSelectCommand(
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
        public MySqlCommand CreateSelectCommand(
            MySqlConnection connection, 
            string table, 
            string columns, 
            Filter constraint,
            Order order,
            Limit limit,
            params object[] paramNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            SetParameters(command, paramNameValueList);
            command.Connection = connection;

            constraint = Filter.Coalesce(constraint);
            order = Order.Coalesce(order);
            limit = Limit.Coalesce(limit);
            table = SqlName(table);
            
            StringBuilder s = new StringBuilder();
            s.Append("SELECT ");

            s.Append(columns);
            s.Append(" FROM ");
            s.Append(table);
            
            if (!constraint.IsEmpty)
                s.Append(" WHERE ").Append(constraint);

            if (!order.IsEmpty)
                s.Append(" ORDER BY ").Append(order);

            if (limit != null && !limit.IsUnlimited)
            {
                s.Append(" LIMIT " + limit.Index + "," + limit.Count);
            }
            /*if (limit.Index > 0 && limit.Count >= 0)
                s.Append(" LIMIT ").Append(limit.Index).Append(',').Append(limit.Count);
            else
                s.Append(" LIMIT ").Append(limit.Index);*/

            command.CommandText = s.ToString();
            command.Connection = connection;
            return command;
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public MySqlCommand CreateInsertCommand(string table, string columns)
        {
            return CreateInsertCommand(CreateConnection(), table, columns);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public MySqlCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return CreateInsertCommand(CreateConnection(), table, columnNameValueList);
        }

        /// <summary>
        /// CreateInsertCommand
        /// </summary>
        public MySqlCommand CreateInsertCommand(MySqlConnection connection, string table, params object[] columnNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("INSERT ");
            s.Append(SqlName(table));
            s.Append(" (");
            foreach (MySqlParameter param in command.Parameters)
            {
                s.Append(param.ParameterName.Substring(1));
                s.Append(',');
            }
            s.Length = s.Length - 1;
            s.Append(") VALUES (");
            foreach (MySqlParameter param in command.Parameters)
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
        public MySqlCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateUpdateCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        /// <summary>
        /// CreateUpdateCommand
        /// </summary>
        public MySqlCommand CreateUpdateCommand(MySqlConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("UPDATE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (MySqlParameter param in command.Parameters)
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

        public MySqlCommand CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return CreateReplaceCommand(CreateConnection(), table, constraint, columnNameValueList);
        }

        public MySqlCommand CreateReplaceCommand(MySqlConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            MySqlCommand command = new MySqlCommand();
            SetParameters(command, columnNameValueList);
            StringBuilder s = new StringBuilder();
            s.Append("REPLACE ");
            s.Append(SqlName(table));
            s.Append(" SET ");
            foreach (MySqlParameter param in command.Parameters)
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
        public int ExecuteNonQuery(MySqlCommand command)
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
        public MySqlDataReader ExecuteReader(MySqlCommand command)
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

        public int ExecuteScalarInt32(MySqlCommand command)
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

        IDbConnection Glue.Data.IDataProvider.CreateConnection()
        {
            return this.CreateConnection();
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, object value)
        {
            return this.SetParameter((MySqlCommand)command, name, value);
        }

        IDbDataParameter Glue.Data.IDataProvider.SetParameter(IDbCommand command, string name, DbType type, object value)
        {
            return this.SetParameter((MySqlCommand)command, name, type, value);
        }

        void Glue.Data.IDataProvider.SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            this.SetParameters((MySqlCommand)command, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand(commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateCommand(IDbConnection connection, string commandText, params object[] paramNameValueList)
        {
            return this.CreateCommand((MySqlConnection)connection, commandText, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand(table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateSelectCommand(IDbConnection connection, string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return this.CreateSelectCommand((MySqlConnection)connection, table, columns, constraint, order, limit, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand(table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateInsertCommand(IDbConnection connection, string table, params object[] columnNameValueList)
        {
            return this.CreateInsertCommand((MySqlConnection)connection, table, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateUpdateCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateUpdateCommand((MySqlConnection)connection, table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand(table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateReplaceCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return this.CreateReplaceCommand((MySqlConnection)connection, table, constraint, columnNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand(storedProcedureName, paramNameValueList);
        }

        IDbCommand Glue.Data.IDataProvider.CreateStoredProcedureCommand(IDbConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            return this.CreateStoredProcedureCommand((MySqlConnection)connection, storedProcedureName, paramNameValueList);
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

        /// <summary>
        /// Converts a native value to the SQL representation for this provider.
        /// </summary>
        string Glue.Data.IDataProvider.ToSql(object v)
        {
            if (v == null)
                throw new ArgumentException("Cannot convert null to a SQL constant.");
            Type t = v.GetType();
            if (t == typeof(String))
            {
                StringBuilder result = new StringBuilder();
                foreach (char c in (string)v)
                {
                    if ("\\\r\n\t\'\"%_".IndexOf(c) > -1) // needs escape?
                        result.Append('\\'); // escape it
                    result.Append(c);
                }
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


        #endregion
    }
}