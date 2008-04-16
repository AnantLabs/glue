using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Glue.Data;
using Glue.Data.Schema;

namespace Glue.Data.Providers.SQLite
{
	/// <summary>
	/// SQLiteProvider
	/// </summary>
	public class SQLiteSchemaProvider : ISchemaProvider
	{
        private string name = null;
        private string connectionString = null;
        private SQLiteDataProvider2 provider;
        
        ///// <summary>
        ///// SQLiteProvider
        ///// </summary>
        //public SQLiteSchemaProvider()
        //{
        //}

        public SQLiteSchemaProvider(SQLiteDataProvider2 provider)
        {
            this.provider = provider;
        }

	
        #region ISchemaProvider Members

        /// <summary>
        /// Initialize the provider. Returns true if successful, false on invalid 
        /// credentials, throws an exception on all other errors.
        /// </summary>
        public bool Initialize(string connectionString)
        {
            this.connectionString = connectionString;
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (SQLiteException)
            {
                return false;
            }
            connection.Close();
            return true;
        }
        
        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Scheme identifier of this provider
        /// </summary>
        public string Scheme 
        { 
            get { return "sqlite"; }
        }
        
        /// <summary>
        /// GetDatabase
        /// </summary>
        public Database GetDatabase(string name)
        {
            // ignore name argument
            return new Database(this, this.name);
        }

        /// <summary>
        /// GetDatabase
        /// </summary>
        public Database[] GetDatabases()
        {
            return new Database[] { GetDatabase(null) };
        }

        public string GetViewText(View view)
        {
            return null;
        }

        public Index[] GetIndexes(Container container)
        {
            // TODO:  Add SQLiteProvider.GetIndexes implementation
            return new Index[0];
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
            using (IDataReader reader = provider.ExecuteReader(
                "SELECT name FROM sqlite_master WHERE type='table'", null))
            {
                ArrayList list = new ArrayList();
                while (reader.Read())
                {
                    list.Add(new Table(database, (string)reader[0]));
                }
                return (Table[])list.ToArray(typeof(Table));
            }

            //using (SQLiteDataReader reader = ExecuteReader(
            //           "SHOW TABLES FROM {0}",
            //           database.Name))
            //{
            //    ArrayList list = new ArrayList();
            //    while (reader.Read())
            //    {
            //        list.Add(new Table(database, (string)reader[0]));
            //    }
            //    return (Table[])list.ToArray(typeof(Table));
            //}
        }

        public Column[] GetColumns(Container container)
        {
            using (IDataReader reader = provider.ExecuteReader(
                "pragma table_info(" + container.Name + ")", null))
            {
                ArrayList list = new ArrayList();

                while (reader.Read())
                {
                    string type = (string)reader["type"];
                    object o = reader["dflt_value"];
                    string dflt = o.GetType() == typeof(DBNull)? null: (string)o;

                    Column c = new Column(
                        container,
                        (string)reader["name"],
                        GetDataType(type),
                        type,
                        (long)reader["notnull"] != 99, // nullable
                        0, // precision
                        0, // scale
                        0, // size
                        dflt, // string default value
                        null, // string description
                        false, // bool identity??
                        false, // bool computed
                        null // string expression
                    );
                    list.Add(c);
                }
                return (Column[])list.ToArray(typeof(Column));
            }

            //using (SQLiteDataReader reader = ExecuteReader(
            //           "SHOW COLUMNS FROM {0}",
            //           container.Name))
            //{
            //    ArrayList list = new ArrayList();
            //    while (reader.Read())
            //    {
            //        string nativeType = reader["Type"].ToString();
            //        DbType dataType = DbType.String; // TODO
            //        int size = 0; // TODO
            //        // NativeTypeToDataType(nativeType, out dataType, out size);
            //        list.Add(new Column(
            //            container, 
            //            reader["Field"].ToString(),
            //            dataType,
            //            nativeType,
            //            reader["Null"].ToString() == "YES",
            //            0,
            //            0,
            //            size,
            //            reader["Default"].ToString(),
            //            "",
            //            reader["Extra"].ToString()=="auto_increment",
            //            false,
            //            null));
            //    }
            //    return (Column[])list.ToArray(typeof(Column));
            //}
        }

        public View[] GetViews(Database database)
        {
            return new View[0];
        }

        public void Import(Table table, IDataImporter reader, ImportMode mode)
        {
        }

        public void Export(Container container, IDataExporter writer)
        {
            // TODO:  Add SQLiteProvider.Export implementation
        }

        public Key[] GetKeys(Table table)
        {
            ArrayList list = new ArrayList();

            // primary key
            IDataReader reader = provider.ExecuteReader("pragma table_info(" + table.Name + ")", null);
            //Console.WriteLine("Finding Primary key...");
            List<string> PK_columns = new List<string>();
            while (reader.Read())
            {
                // Add all columns who have a value of '1' in column 'pk'.
                if ((long)reader["pk"] == 1)
                {
                    //Console.WriteLine(reader["name"].ToString() + reader["pk"].ToString());
                    PK_columns.Add(reader["name"].ToString());
                }
            }
            if (PK_columns.Count > 0)
                list.Add(new Key(table, "PRIMARY_KEY", PK_columns.ToArray()));

            // TODO: Add other keys
            // use pragma index_list and pragma index_info


            return (Key[])list.ToArray(typeof(Key));
        }

        public void Import(Table table, XmlReader reader, ImportMode mode)
        {
            // TODO:  Add SQLiteProvider.Import implementation
            throw new NotImplementedException("Import not implemented.");
        }

        public void Script(Database database, TextWriter writer)
        {
            // TODO:  Add SQLiteProvider.Import implementation
            throw new NotImplementedException("Import not implemented.");
        }

        #endregion

        private SQLiteCommand CreateCommand(string commandText)
        {
            return new SQLiteCommand(commandText, new SQLiteConnection(connectionString));
        }

        private SQLiteCommand CreateCommand(string commandText, params object[] args)
        {
            return CreateCommand(string.Format(commandText, args));
        }

        private SQLiteDataReader ExecuteReader(string commandText)
        {
            SQLiteCommand command = new SQLiteCommand(commandText, new SQLiteConnection(connectionString));
            command.Connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private SQLiteDataReader ExecuteReader(string commandText, params object[] args)
        {
            return ExecuteReader(string.Format(commandText, args));
        }

        private DbType GetDataType(string s)
        {
            // TODO detect type from string, "INTEGER", "VARCHAR(30)", etc
            return DbType.Object;
        }
    }
}
