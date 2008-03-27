using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using System.Collections;
using Edf.Lib.Data;

namespace Edf.Lib.Data.Schema
{
	/// <summary>
	/// Static helper class.
	/// </summary>
	public class SchemaUtility
	{
        private SchemaUtility()
        {
        }

        /// <summary>
        /// Obtain a database object from url. Will throw a SecurityException on
        /// missing or wrong credentials.
        /// </summary>
        public static Database Open(string url, string username, string password)
        {
            ISchemaProvider provider = OpenSchemaProvider(url, username, password);
            return new Database(provider, "");
        }

        // Helpers
        public static Regex UrlPattern = new Regex(
            @"(?<scheme>\w+)\:\/\/
              (?<server>[\w\.]+)
              (?:\/\+(?<instance>\w+))?
              (?:\/(?<database>\w+))",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );

        /// <summary>
        /// Returns the provider type based on the url.
        /// sample:
        /// sql://calypso/Northwind
        /// </summary>
        public static Type GetProviderType(string url)
        {
            Match match = UrlPattern.Match(url);
            if (match.Success)
            {
                switch (match.Groups["scheme"].Value)
                {
                    case "sql":
                    case "mssql":
                        return typeof(Edf.Lib.Data.Providers.Sql.SqlSchemaProvider);
                    default:
                        return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a connection string based on url.
        /// TODO: should be moved to specific providers.
        /// </summary>
        private static string GetConnectionString(string url, string user, string pass)
        {
            Match match = UrlPattern.Match(url);
            if (match.Success)
            {
                string conn = "";
                conn += "server=" + match.Groups["server"] + ";";
                conn += "database=" + match.Groups["database"] + ";";
                if (user == null && pass == null)
                {
                    conn += "integrated security=sspi;";
                }
                else 
                {
                    if (user != null)
                        conn += "user id=" + user + ";";
                    if (pass != null)
                        conn += "password=" + pass + ";";
                }
                return conn;
            }
            return null;
        }

        /// <summary>
        /// TODO: should be moved to specific providers.
        /// </summary>
        private static ISchemaProvider OpenSchemaProvider(string url, string user, string pass)
        {
            Type type = GetProviderType(url);
            return Activator.CreateInstance(type, new object[] {GetConnectionString(url, user, pass)}) as ISchemaProvider; 
        }

        /// <summary>
        /// Generate schema xml. 
        /// </summary>
        public static void Schema(Database database, XmlWriter writer, string[] objects)
        {
            writer.WriteStartDocument();
            writer.WriteComment(string.Format(@"
Generator: {0}
Provider : {1}
Database : {2}
DateTime : {3}
",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                database.Provider.GetType(),
                "???uri???",
                DateTime.Now
                ));
            writer.WriteStartElement("database");
            writer.WriteAttributeString("name", database.Name);
            foreach (Table table in database.Tables)
            {
                if (objects == null || objects.Length == 0 || Edf.Lib.IO.WildCard.Matches(table.Name, objects))
                {
                    table.Write(writer);
                }
            }
            foreach (View view in database.Views)
            {
                if (objects == null || objects.Length == 0 || Edf.Lib.IO.WildCard.Matches(view.Name, objects))
                {
                    view.Write(writer);
                }
            }
            foreach (Procedure proc in database.Procedures)
            {
                if (objects == null || objects.Length == 0 || Edf.Lib.IO.WildCard.Matches(proc.Name, objects))
                {
                    proc.Write(writer);
                }
            }
        }

        public static void Script(Database database, TextWriter writer)
        {
            database.Provider.Script(database, writer);
        }

        public static void Export(Database database, XmlWriter writer, string[] objects)
        {
            writer.WriteStartDocument();
            writer.WriteComment(string.Format(@"
Generator: {0}
Provider : {1}
Database : {2}
DateTime : {3}
",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                database.Provider.GetType(),
                "??Uri??",
                DateTime.Now
                ));


            writer.WriteStartElement("database");
            writer.WriteAttributeString("name", database.Name);
            
            foreach (Table table in DetermineExportOrder(database))
            {
                if (objects == null || objects.Length == 0 || Edf.Lib.IO.WildCard.Matches(table.Name, objects))
                {
                    table.Export(writer);
                }
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
        
        private static Table[] DetermineExportOrder(Database database)
        {
            ArrayList remaining = new ArrayList(database.Tables);
            ArrayList ordered = new ArrayList();
            while (remaining.Count > 0)
                DetermineExportOrder((Table)remaining[0], remaining, ordered);
            return (Table[])ordered.ToArray(typeof(Table));
        }

        private static void DetermineExportOrder(Table table, ArrayList remaining, ArrayList ordered)
        {
            if (ordered.Contains(table))
            {
                // Already done
                return;
            }
            if (!remaining.Contains(table))
            {
                // TODO: Error circular reference
            }
            remaining.Remove(table);
            foreach (Key k in table.Keys)
            {
                ForeignKey fk = k as ForeignKey;
                if (fk != null && fk.ReferencedTable != null && fk.ReferencedTable != table)
                {
                    DetermineExportOrder(fk.ReferencedTable, remaining, ordered);
                }
            }
            ordered.Add(table);
        }

        public static void Import(Database database, XmlReader reader, string[] objects, ImportMode mode)
        {
            string name = null;
            int depth = -1;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        if ((reader.Depth == depth || -1 == depth) && "table" == reader.Name)
                        {
                            depth = reader.Depth;
                            while (reader.MoveToNextAttribute())
                                if (reader.Name == "name")
                                    name = reader.Value;
                        } 
                        else if (reader.Depth == depth + 1 && "rows" == reader.Name && !reader.IsEmptyElement)
                        {
                            if (objects == null || objects.Length == 0 || Edf.Lib.IO.WildCard.Matches(name, objects))
                            {
                                Table table = (Table)Database.Find(database.Tables, name);
                                if (table == null)
                                {
                                    // TODO: Create if necessary
                                    
                                }
                                if (table != null)
                                {
                                    table.Import(reader, mode);
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
	}
}
