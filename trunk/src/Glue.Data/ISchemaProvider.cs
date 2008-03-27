using System;
using System.IO;
using System.Xml;
using Glue.Data.Schema;

namespace Glue.Data
{
    /// <summary>
    /// ISchemaProvider
    /// </summary>
    public interface ISchemaProvider
    {
        Database    GetDatabase(string name);
        Database[]  GetDatabases();
        Table[]     GetTables(Database database);
        View[]      GetViews(Database database);
        Procedure[] GetProcedures(Database database);
        Column[]    GetColumns(Container container);
        Index[]     GetIndexes(Container container);
        Trigger[]   GetTriggers(Container container);
        Key[]       GetKeys(Table table);
        string      GetViewText(View view);
        Parameter[] GetParameters(Procedure procedure);
        string      GetProcedureText(Procedure procedure);
        void        Export(Container container, IDataExporter writer);
        void        Import(Table table, IDataImporter reader, ImportMode mode);
        void        Script(Database database, TextWriter writer);
        string      Scheme { get; }
    }

    /*
    public class ColumnInfo
    {
        public string   Name;
        public string   NativeType;
        public Type     RuntimeType;
        public DbType   DataType;
        public int      Size;
        public int      Precision;
        public bool     Nullable;
        public bool     Identity;
        public string   Default;
        public string   Expression;
        public string   Constraint;
    }
    
    class KeyInfo
    {
        public string  Name;
        public bool Primary;
        public string Columns[];
        public string ForeignTable;
        public string[] ForeignColumns;
    }
    
    class IndexInfo
    {
        public string Name;
        public bool Primary;
        public bool Clustered;
        public string Columns[];
        public int Order[];
    }
    
    ISchemaProvider provider = GetSchemaProvider();
    provider.AddTable("Log",
        ColumnInfo.NewInt32("Id", false, null, true, true),
        ColumnInfo.NewString("Name", false, 100),
        ColumnInfo.NewString("Title", false, 150)
        );
    provider.ChangeColumn(ColumnInfo.NewInt64("Id", false, null, true));
    
    
    
    public interface ISchemaProvider2
    {
        string[]        GetTableNames();
        string[]        GetViewNames();
        string[]        GetColumnNames(string table);
        ColumnInfo      GetColumn(string table, string name);
        string[]        GetIndexNames(string table);
        IndexInfo       GetIndex(string table, string name);
        string[]        GetKeyNames(string table);
        KeyInfo         GetKey(string table, string name);
        string[]        GetProcedureNames();
        string          GetProcedure(string name);
        
        void            AddTable(string table);
        void            AddColumn(string table, string name, ColumnInfo info);
        void            DeleteColumn(string name);
        void            RenameColumn(string table, string name, string newname);
        void            MoveColumn(string table, string name, string before);
        void            DeleteKey(string name);
        void            AddKey(string table, string name, KeyInfo info);
        void            AddForeignKey(string table, string name, ForeignKeyInfo info);
        void            DeleteForeignKey(string table, string name);
    }
    */  
}
