using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Glue.Data;
using Glue.Data.Schema;

namespace Glue.Data.Providers.Schema.SQLite
{
	/// <summary>
	/// SQLiteProvider
	/// </summary>
	public class SQLiteSchemaProvider : ISchemaProvider
	{
        private string name = null;
        private string connectionString = null;
        
        /// <summary>
        /// SQLiteProvider
        /// </summary>
        public SQLiteSchemaProvider()
		{
		}
	
        #region ISchemaProvider Members

        /// <summary>
        /// Initialize the provider. Returns true if successful, false on invalid 
        /// credentials, throws an exceptionon all other errors.
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
            get { return "mysql"; }
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
            using (SQLiteDataReader reader = ExecuteReader(
                       "SHOW TABLES FROM {0}",
                       database.Name))
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
            using (SQLiteDataReader reader = ExecuteReader(
                       "SHOW COLUMNS FROM {0}",
                       container.Name))
            {
                ArrayList list = new ArrayList();
                while (reader.Read())
                {
                    string nativeType = reader["Type"].ToString();
                    DbType dataType = DbType.String; // TODO
                    int size = 0; // TODO
                    // NativeTypeToDataType(nativeType, out dataType, out size);
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
            using (SQLiteDataReader reader = ExecuteReader(
                       "SHOW KEYS FROM {0}",
                       table.Name))
            {
                string last = null;
                ArrayList cols = new ArrayList();
                while (reader.Read())
                {
                    if (last != null && last != (string)reader["Key_name"])
                    {
                        if (last == "PRIMARY")
                            list.Add(new PrimaryKey(table, last, (string[])cols.ToArray(typeof(string))));
                        else
                            list.Add(new Key(table, last, (string[])cols.ToArray(typeof(string))));
                        cols.Clear();
                    }
                    else
                    {
                        cols.Add((string)reader["Column_name"]);
                    }
                    last = (string)reader["Key_name"];
                }
                if (last != null && last != (string)reader["Key_name"])
                {
                    if (last == "PRIMARY")
                        list.Add(new PrimaryKey(table, last, (string[])cols.ToArray(typeof(string))));
                    else
                        list.Add(new Key(table, last, (string[])cols.ToArray(typeof(string))));
                    cols.Clear();
                }
            }
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
    }
}
