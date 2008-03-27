using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Schema;

namespace Glue.Data.Providers.MySql
{
	/// <summary>
	/// MySqlProvider
	/// </summary>
	public class MySqlSchemaProvider : ISchemaProvider
	{
        string server = null;
        string username = null;
        string password = null;

        /// <summary>
        /// SqlSchemaProvider
        /// </summary>
        public MySqlSchemaProvider(string server, string username, string password)
        {
            this.server = server;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected MySqlSchemaProvider(XmlNode node)
        {
            this.server   = Configuration.GetAttr(node, "server");
            this.username = Configuration.GetAttr(node, "username", null);
            this.password = Configuration.GetAttr(node, "password", null);
        }

        /*
        /// <summary>
        /// Initialize the provider. Returns true if successful, false on invalid 
        /// credentials, throws an exceptionon all other errors.
        /// </summary>
        public bool Initialize(string connectionString)
        {
            this.connectionString = connectionString;
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (MySqlException e)
            {
                if (e.Number == 1045) // Access denied
                    return false;
                throw;
            }
            connection.Close();
            return true;
        }
        */
        
        /// <summary>
        /// Scheme identifier of this provider
        /// </summary>
        public string Scheme 
        { 
            get { return "mysql"; }
        }
        
        /// <summary>
        /// GetDatabase
        /// </summary>
        public Database GetDatabase(string name)
        {
            return new Database(this, name);
        }

        /// <summary>
        /// GetDatabase
        /// </summary>
        public Database[] GetDatabases()
        {
            using (MySqlDataReader reader = ExecuteReader(null, "SHOW DATABASES"))
            {
                ArrayList list = new ArrayList();
                while (reader.Read())
                    list.Add(new Database(this, (string)reader[0]));
                return (Database[])list.ToArray(typeof(Database));
            }
        }

        public string GetViewText(View view)
        {
            return null;
        }

        public Parameter[] GetParameters(Procedure procedure)
        {
            return new Parameter[0];
        }

        public string GetProcedureText(Procedure procedure)
        {
            return null;
        }

        public Trigger[] GetTriggers(Container container)
        {
            return new Trigger[0];
        }

        public Procedure[] GetProcedures(Database database)
        {
            return new Procedure[0];
        }

        public Table[] GetTables(Database database)
        {
            using (MySqlDataReader reader = ExecuteReader(database, "SHOW TABLES FROM `{0}`", database.Name))
            {
                ArrayList list = new ArrayList();
                while (reader.Read())
                {
                    list.Add(new Table(database, (string)reader[0]));
                }
                return (Table[])list.ToArray(typeof(Table));
            }
        }

        public Column[] GetColumns(Container container)
        {
            using (MySqlDataReader reader = ExecuteReader(
                       container.Database, 
                       "SHOW COLUMNS FROM `{0}`", 
                       container.Name))
            {
                ArrayList list = new ArrayList();
                while (reader.Read())
                {
                    string nativeType = reader["Type"].ToString();
                    DbType dataType;
                    int size;
                    NativeTypeToDataType(nativeType, out dataType, out size);
                    list.Add(new Column(
                        container, 
                        reader["Field"].ToString(),
                        dataType,
                        nativeType,
                        reader["Null"].ToString() == "YES",
                        0,
                        0,
                        size,
                        reader["Default"].ToString(),
                        "",
                        reader["Extra"].ToString()=="auto_increment",
                        false,
                        null));
                }
                return (Column[])list.ToArray(typeof(Column));
            }
        }

        public View[] GetViews(Database database)
        {
            return new View[0];
        }

        public Key[] GetKeys(Table table)
        {
            ArrayList list = new ArrayList();
            using (MySqlDataReader reader = ExecuteReader(
                       table.Database,
                       "SHOW KEYS FROM `{0}`",
                       table.Name))
            {
                Hashtable keys = new Hashtable();
                while (reader.Read())
                {
                    ArrayList cols = (ArrayList)keys[(string)reader["Key_name"]];
                    if (cols == null)
                        keys[(string)reader["Key_name"]] = cols = new ArrayList();
                    cols.Add((string)reader["Column_name"]);
                }
                foreach (string keyname in keys.Keys)
                {
                    ArrayList cols = (ArrayList)keys[keyname];
                    if (keyname == "PRIMARY")
                        list.Add(new PrimaryKey(table, keyname, (string[])cols.ToArray(typeof(string))));
                    // else
                        //list.Add(new Key(table, keyname, (string[])cols.ToArray(typeof(string))));
                }
            }
            return (Key[])list.ToArray(typeof(Key));
        }

        public Index[] GetIndexes(Container container)
        {
            ArrayList list = new ArrayList();
            using (MySqlDataReader reader = ExecuteReader(
                       container.Database,
                       "SHOW KEYS FROM `{0}`",
                       container.Name))
            {
                Hashtable keys = new Hashtable();
                while (reader.Read())
                {
                    Hashtable info = (Hashtable)keys[(string)reader["Key_name"]];
                    if (info == null)
                    {
                        keys[(string)reader["Key_name"]] = info = new Hashtable();
                        info["columns"] = new ArrayList();
                        info["unique"] = !Convert.ToBoolean(reader["Non_unique"]);
                        info["clustered"] = (string)reader["Key_name"] == "PRIMARY";
                    }
                    ((ArrayList)info["columns"]).Add((string)reader["Column_name"]);
                }
                foreach (string keyname in keys.Keys)
                {
                    Hashtable info = (Hashtable)keys[keyname];
                    list.Add(new Index(container, keyname, (bool)info["clustered"], keyname=="PRIMARY", (bool)info["unique"], (string[])((ArrayList)info["columns"]).ToArray(typeof(string))));
                }
            }
            return (Index[])list.ToArray(typeof(Index));
        }

        public void Import(Table table, IDataImporter reader, ImportMode mode)
        {
            Log.Info("Table: " + table.Name);

            StringBuilder sql = new StringBuilder();
            
            // Check on identity insert
            bool identity = false;
            foreach (Column c in table.Columns)
                if (c.Identity)
                    identity = true;
            if (identity)
                sql.AppendFormat("SET IDENTITY_INSERT `{0}` ON\r\n", table.Name);

            // for Mode=Incremental and Mode=Update:
            // if not exists
            //   insert
            if (mode == ImportMode.Incremental || mode == ImportMode.Update)
            {
                sql.AppendFormat("IF NOT EXISTS (SELECT `{0}` FROM `{1}` WHERE (", table.Columns[0].Name, table.Name);
                foreach (Column c in table.PrimaryKey.MemberColumns)
                    sql.AppendFormat("`{0}`=?{0} AND ", c.Name);
                sql.Length = sql.Length - 5;
                sql.Append("))\r\n    ");

                sql.AppendFormat("INSERT `{0}` (", table.Name);
                foreach (Column c in table.Columns)
                    if (!c.Computed)
                        sql.AppendFormat("`{0}`, ", c.Name);
                sql.Length = sql.Length - 2;
                sql.Append(") VALUES (");
                foreach (Column c in table.Columns)
                    if (!c.Computed)
                        sql.AppendFormat("?{0}, ", c.Name);
                sql.Length = sql.Length - 2;
                sql.Append(")\r\n");
            }

            // check if we can update any columns at all
            int updatecols = 0;
            foreach (Column c in table.Columns)
                if (Array.IndexOf(table.PrimaryKey.MemberColumns, c) < 0)
                    if (!c.Computed)
                        updatecols++;

            //bool skip = false;
            if (updatecols == 0)
            {
                // there are no updateable columns, so Mode=Refresh is useless
                //if (mode == ImportMode.Freshen)
                //    skip = true;
            }
            else
            {
                // if we're in Mode=Update we need the ELSE after the IF EXISTS clause
                if (mode == ImportMode.Update)
                    sql.Append("ELSE\r\n");

                // create the UPDATE WHERE statement
                if (mode == ImportMode.Update || mode == ImportMode.Freshen)
                {
                    sql.AppendFormat("UPDATE `{0}` SET ", table.Name);
                    foreach (Column c in table.Columns)
                        if (Array.IndexOf(table.PrimaryKey.MemberColumns, c) < 0)
                            if (!c.Computed)
                                sql.AppendFormat("`{0}`=?{0}, ", c.Name);
                    sql.Length = sql.Length - 2;
                    sql.Append(" WHERE (");
                    foreach (Column c in table.PrimaryKey.MemberColumns)
                        sql.AppendFormat("`{0}`=?{0} AND ", c.Name);
                    sql.Length = sql.Length - 5;
                    sql.Append(")\r\n");
                }
            }

            MySqlCommand command = CreateCommand(table.Database, sql.ToString());
            foreach (Column c in table.Columns)
                command.Parameters.Add(c.Name, ToMySqlDbType(c.DataType), c.Size);

            // Perform the actual import
            command.Connection.Open();
            try 
            {
                while (reader.ReadRow())
                {
                    for (int i = 0; i < reader.Columns.Length; i++)
                    {
                        object value = reader.GetValue(i);
                        command.Parameters[reader.Columns[i]].Value = value == null ? DBNull.Value : value;
                    }
                    try
                    {
                        int n = command.ExecuteNonQuery();
                        if (n > 0)
                            Console.WriteLine("  Importing: " + command.Parameters[0].Value.ToString());
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }

        /// <summary>
        /// Export
        /// </summary>
        public void Export(Container container, IDataExporter writer)
        {
            // Set column names and types
            string[] names = new string[container.Columns.Length];
            Type[] types = new Type[container.Columns.Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = container.Columns[i].Name;
                types[i] = container.Columns[i].SystemType;
            }

            // Export rows
            writer.WriteStart(container.Name, names, types);
            using (MySqlDataReader reader = ExecuteReader(container.Database, "SELECT * FROM `" + container.Name + "`"))
            {
                while (reader.Read())
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        object value = reader[names[i]];
                        writer.SetValue(i, value);
                    }
                    writer.WriteRow();
                }
            }
            writer.WriteEnd();
        }

        /// <summary>
        /// Script the database in native SQL
        /// </summary>
        public void Script(Database database, TextWriter writer)
        {
            if (database.Name != null && database.Name != "")
            {
                writer.WriteLine("/* DATABASE */");
                writer.WriteLine("CREATE DATABASE `{0}` IF NOT EXISTS `{0}`;", database.Name);
                writer.WriteLine("USE `{0}`;", database.Name);
                writer.WriteLine();
            }
            writer.WriteLine("/* TABLES */");
            writer.WriteLine();

            StringBuilder sql = new StringBuilder();
            foreach (Table table in database.Tables)
            {
                writer.WriteLine("SET FOREIGN_KEY_CHECKS=0;");
                sql.AppendFormat("CREATE TABLE `{0}` (\r\n", table.Name);
                foreach (Column c in table.Columns)
                {
                    if (!c.Computed)
                    {
                        sql.AppendFormat("  `{0}` {1}", c.Name, DataTypeToNativeType(c.DataType, c.Size), c.Size);
                        if (c.DefaultValue != null && c.DefaultValue.Length > 0)
                            sql.AppendFormat(" DEFAULT {0}", Filter.ToSql(c.DefaultValue));
                        if (c.Nullable)
                            sql.Append(" NULL");
                        else
                            sql.Append(" NOT NULL");
                        if (c.Identity)
                            sql.Append(" auto_increment");
                        sql.Append(", \r\n");
                    }
                    else
                    {
                        sql.AppendFormat("`{0}` AS {1}, \r\n", c.Name, c.Expression);
                    }
                }
                foreach (Key k in table.Keys)
                {
                    if (k is PrimaryKey)
                    {
                        PrimaryKey pk = k as PrimaryKey;
                        sql.AppendFormat("  PRIMARY KEY (", k.Name);
                        // TODO: IsClustered
                        foreach (Column c in pk.MemberColumns)
                            sql.AppendFormat("`{0}`, ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.Append("), \r\n");
                    }
                }
                foreach (Index index in table.Indexes)
                {
                    if (!index.IsPrimaryKey)
                    {
                        if (index.IsUnique)
                            sql.Append("  UNIQUE KEY ");
                        else
                            sql.Append("  KEY ");
                        sql.AppendFormat("`{0}` (", index.Name);
                        foreach (Column c in index.MemberColumns)
                            sql.AppendFormat("`{0}`, ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.Append("), \r\n");
                    }
                }
                sql.Length = sql.Length - 4;
                sql.Append("\r\n);");

                writer.WriteLine(sql.ToString());
                writer.WriteLine("SET FOREIGN_KEY_CHECKS=1;");
                writer.WriteLine();
                sql.Length = 0;
            }

            // Foreign keys
            writer.WriteLine();
            writer.WriteLine();
            foreach (Table table in database.Tables)
            {
                sql.AppendFormat("ALTER TABLE `{0}` ADD\r\n", table.Name);
                bool yep = false;
                foreach (Key k in table.Keys)
                {
                    if (k is ForeignKey)
                    {
                        yep = true;
                        ForeignKey fk = k as ForeignKey;
                        sql.AppendFormat("  CONSTRAINT `{0}` FOREIGN KEY (", k.Name);
                        foreach (Column c in fk.MemberColumns)
                            sql.AppendFormat("`{0}`, ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.AppendFormat(") REFERENCES `{0}` (", fk.ReferencedTable.Name);
                        foreach (Column c in fk.ReferencedColumns)
                            sql.AppendFormat("`{0}`, ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.Append("), \r\n");
                    }
                    else
                    {
                        // TODO: UNIQUE AND CHECK
                    }
                }
                if (yep)
                {
                    sql.Length = sql.Length - 4;
                    sql.Append("\r\n;");
                    writer.WriteLine(sql.ToString());
                    writer.WriteLine();
                }
                sql.Length = 0;
            }

            // Views
            writer.WriteLine();
            writer.WriteLine("/* VIEWS */");
            writer.WriteLine();
            
            /* StringCollection order = new StringCollection();
            foreach (View view in database.Views)
                order.Add(view.Name);
            //GetViewCreationOrder(database, view, order);

            foreach (string name in order)
            {
                View view = GetViewByName(database, name);
                writer.WriteLine("IF EXISTS (SELECT * FROM dbo.sysobjects WHERE name='{0}' AND xtype='V')", view.Name);
                writer.WriteLine("  DROP VIEW [" + view.Name + "]");
                writer.WriteLine("GO");
                writer.WriteLine();
                writer.WriteLine(view.Text);
                writer.WriteLine();
                writer.WriteLine("GO");
                writer.WriteLine();
                sql.Length = 0;
                foreach (Index index in view.Indexes)
                {
                    if (index.IsUnique)
                        sql.Append("CREATE UNIQUE ");
                    else
                        sql.Append("CREATE ");
                    if (index.IsClustered)
                        sql.Append("CLUSTERED ");
                    sql.Append("INDEX ");
                    sql.AppendFormat("`{0}` ON `{1}`(", index.Name, view.Name);
                    foreach (Column c in index.MemberColumns)
                        sql.AppendFormat("`{0}`, ", c.Name);
                    sql.Length = sql.Length - 2;
                    sql.Append(")\r\n");
                }
                if (sql.Length > 0)
                {
                    writer.Write(sql.ToString());
                    writer.WriteLine("GO");
                }
                writer.WriteLine();
                sql.Length = 0;
            } */

            writer.WriteLine();
            writer.WriteLine("/* PROCS */");
            writer.WriteLine();

            // Procedures
            foreach (Procedure proc in database.Procedures)
            {
                writer.WriteLine(proc.Text);
                writer.WriteLine();
            }
        }

        #region SQL command helpers

        private MySqlConnection CreateConnection(Database database)
        {
            string connectionString = "server=" + server + ";user id=" + username + ";password=" + password;
            if (database != null)
                connectionString += ";database=" + database.Name;
            return new MySqlConnection(connectionString);
        }

        private MySqlCommand CreateCommand(Database database, string commandText)
        {
            return new MySqlCommand(commandText, CreateConnection(database));
        }

        private MySqlCommand CreateCommand(Database database, string commandText, params object[] args)
        {
            return CreateCommand(database, string.Format(commandText, args));
        }

        private MySqlDataReader ExecuteReader(Database database, string commandText)
        {
            MySqlCommand command = new MySqlCommand(commandText, CreateConnection(database));
            command.Connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private MySqlDataReader ExecuteReader(Database database, string commandText, params object[] args)
        {
            return ExecuteReader(database, string.Format(commandText, args));
        }

        #endregion

        static System.Text.RegularExpressions.Regex regType = new System.Text.RegularExpressions.Regex(
            @"(?<type>\w+)\s*
            (?<size>\(\d+\))?\s*
            (?<sign>unsigned)?\s*
            (?<fill>zerofill)?",
            System.Text.RegularExpressions.RegexOptions.Compiled | 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
            System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace
            );

        private void NativeTypeToDataType(string spec, out DbType type, out int size)
        {
            System.Text.RegularExpressions.Match m = regType.Match(spec);
            if (!m.Success)
                throw new ArgumentException("Cannot determine MySqlDbType of: '" + spec + "'");
            string name = m.Groups[1].Value;
            size = m.Groups[2].Success ? int.Parse(m.Groups[2].Value.Trim('(',')')) : 0;
            bool unsigned = m.Groups[3].Value != "";
            bool zerofill = m.Groups[4].Value != "";
            switch (name.ToLower())
            {
                case "char":        type = DbType.StringFixedLength; return;
                case "varchar":     type = DbType.String; return;
                case "date":        type = DbType.Date; return;
                case "datetime":    type = DbType.DateTime; return;
                case "decimal": 
                case "dec":
                case "fixed":       type = DbType.Decimal; return;
                case "year":        type = DbType.Int32; return;
                case "time":        type = DbType.Time; return;
                case "timestamp":   type = DbType.DateTime; return;
                case "set":         type = DbType.String; return;
                case "enum":        type = DbType.String; return;
                case "bit":         
                case "bool":
                case "boolean":     type = DbType.Boolean; return;
                case "tinyint":     type = unsigned ? DbType.Byte : DbType.SByte; return;
                case "smallint":    type = unsigned ? DbType.UInt16 : DbType.Int16; return;
                case "mediumint":   type = unsigned ? DbType.UInt32 : DbType.Int32; return;
                case "int": 
                case "integer":     type = unsigned ? DbType.UInt32 : DbType.Int32; return;
                case "bigint":      type = unsigned ? DbType.UInt64 : DbType.Int64; return;
                case "float":       type = DbType.Single; return;
                case "double":      type = DbType.Double; return;
                case "real":        type = DbType.Double; return;
                case "blob":        type = DbType.Binary; return;
                case "text":        type = DbType.String; return;
                case "longblob":    type = DbType.Binary; return;
                case "longtext":    type = DbType.String; return;
                case "mediumblob":  type = DbType.Binary; return;
                case "mediumtext":  type = DbType.String; return;
                case "tinyblob":    type = DbType.Binary; return;
                case "tinytext":    type = DbType.String; return;
                default:
                    throw new ArgumentException("Unknown type: '" + spec + "'");
            }
        }

        string DataTypeToNativeType(DbType type, int size)
        {
            switch (type) 
            {
                case DbType.AnsiString:             
                case DbType.String:                 return size != 0 ? "varchar(" + size + ")" : "text";
                case DbType.AnsiStringFixedLength:  
                case DbType.StringFixedLength:      return "char(" + size + ")";
                case DbType.Decimal:                return "decimal"; 
                case DbType.Byte:                   return "tinyint unsigned";
                case DbType.SByte:                  return "tinyint";
                case DbType.Int16:                  return "smallint";
                case DbType.Int32:                  return "int";
                case DbType.Int64:                  return "bigint";
                case DbType.UInt16:                 return "smallint unsigned";
                case DbType.UInt32:                 return "int unsigned";
                case DbType.UInt64:                 return "bigint unsigned";
                case DbType.Boolean:                return "bit"; 
                case DbType.Single:                 return "float"; 
                case DbType.Double:                 return "double";
                case DbType.DateTime:               return "datetime";
                case DbType.Date:                   return "date";
                case DbType.Time:                   return "time";
                case DbType.Binary:                 return "blob";
            }
            throw new ArgumentException("Cannot get NativeType for DbType " + type);
        }

        static MySqlDbType ToMySqlDbType(DbType type)
        {
            switch (type)
            {
                case DbType.Guid:
                case DbType.AnsiString:
                case DbType.String:                 return MySqlDbType.VarChar; 
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:      return MySqlDbType.String;
                case DbType.Boolean:                return MySqlDbType.Byte;
                case DbType.Byte:                   return MySqlDbType.UByte;
                case DbType.SByte:                  return MySqlDbType.Byte;
                case DbType.Date:                   return MySqlDbType.Date; 
                case DbType.DateTime:               return MySqlDbType.Datetime; 
                case DbType.Time:                   return MySqlDbType.Time; 
                case DbType.Single:                 return MySqlDbType.Float; 
                case DbType.Double:                 return MySqlDbType.Double; 
                case DbType.Int16:                  return MySqlDbType.Int16;
                case DbType.UInt16:                 return MySqlDbType.UInt16;
                case DbType.Int32:                  return MySqlDbType.Int32;
                case DbType.UInt32:                 return MySqlDbType.UInt32;
                case DbType.Int64:                  return MySqlDbType.Int64;
                case DbType.UInt64:                 return MySqlDbType.UInt64;
                case DbType.Decimal:
                case DbType.Currency:               return MySqlDbType.Decimal;
                case DbType.Object:
                case DbType.VarNumeric: 
                case DbType.Binary:                 
                default:                            return MySqlDbType.Blob;
            }
        }
    }
}
