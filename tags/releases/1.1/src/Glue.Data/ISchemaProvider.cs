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
}
