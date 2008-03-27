using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using System.Collections;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Static helper class.
    /// </summary>
    public class Management
    {
        // Helpers
        public static Regex UrlPattern = new Regex(
            @"(?<scheme>\w+)\:\/\/
              (?<server>[\w\.]+(\/\w+)?)
              (?:\/(?<database>\w+)?)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );

        public static Regex FileUrlPattern = new Regex(
            @"(?<scheme>file)\:\/\/
              (?<path>.+)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );

        /// <summary>
        /// Returns the provider scheme based on the url.
        /// sample:
        /// sql://calypso/Northwind => sql
        /// </summary>
        public static string GetSchemePart(string url)
        {
            Match match = UrlPattern.Match(url);
            if (match.Success)
                return match.Groups["scheme"].Value;
            else
                return null;
        }

        /// <summary>
        /// Returns the server based on the url.
        /// sample:
        /// sql://calypso/Northwind => server
        /// </summary>
        public static string GetServerPart(string url)
        {
            Match match = UrlPattern.Match(url);
            if (match.Success)
                return match.Groups["server"].Value;
            return null;
        }

        /// <summary>
        /// Returns the server based on the url.
        /// sample:
        /// sql://calypso/Northwind => Northwind
        /// </summary>
        public static string GetDatabasePart(string url)
        {
            Match match = UrlPattern.Match(url);
            if (match.Success)
                return match.Groups["database"].Value;
            return null;
        }

        /// <summary>
        /// Returns the server based on the url.
        /// sample:
        /// sql://calypso/Northwind => Northwind
        /// </summary>
        public static string GetPathPart(string url)
        {
            Match match = FileUrlPattern.Match(url);
            if (match.Success)
                return match.Groups["path"].Value;
            return null;
        }

        /// <summary>
        /// Open database schema. Either from live server, (e.g. sql://calypso/Intranet_1_0 or from file file://myschema.schema) 
        /// </summary>
        public static Database OpenDatabase(string url, string user, string pass)
        {
            string scheme = Management.GetSchemePart(url);
            string server = Management.GetServerPart(url);
            if (scheme == "file")
                return LoadSchemaFromXml(Applet.OpenXml(Management.GetPathPart(url)));
            ISchemaProvider provider = CreateSchemaProvider(scheme, server, user, pass);
            return provider.GetDatabase(GetDatabasePart(url));
        }

        /// <summary>
        /// Open database from configuration file (typically web.config with dataprovider element).
        /// </summary>
        public static Database OpenDatabaseFromConfiguration(string elementname)
        {
            elementname = NullConvert.Coalesce(elementname, "dataprovider");
            // <dataprovider
            //  type="Edf.Lib.Data.Providers.Sql.SqlMappingProvider"
            //  database="database_name"
            //  username="glue_user"
            //  server="server_name"
            //  password="7njd34a"
            // />
            // Get DataProvider first, then ask for corresponding SchemaProvider, then open database
            IDataProvider dataprovider = (IDataProvider)Configuration.Get(elementname);
            ISchemaProvider schemaprovider = dataprovider.GetSchemaProvider();
            Database database = schemaprovider.GetDatabase(Configuration.GetElement(elementname).GetAttribute("database"));
            return database;
        }

        /// <summary>
        /// Scheme can be sql, mysql, oledb
        /// </summary>
        public static ISchemaProvider CreateSchemaProvider(string scheme, string server, string user, string pass)
        {
            string name = "edf.lib.data.providers." + scheme + "." + scheme + "SchemaProvider";
            Type type = Configuration.FindType(name);
            if (type == null)
            {
                name = name + ",edf.lib." + scheme;
                type =  Configuration.FindType(name);
            }
            if (type == null)
                throw new DataException("Cannot find schema provider for: " + scheme);
            return Activator.CreateInstance(type, new object[] {server, user, pass}) as ISchemaProvider; 
        }

        public static Database LoadSchemaFromXml(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            return new Database(doc.DocumentElement);
        }

        public static void SaveSchemaToXml(Database database, XmlWriter writer)
        {
            Schema(database, writer, null);
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
                database.Name,
                DateTime.Now
                ));
            writer.WriteStartElement("database");
            writer.WriteAttributeString("name", database.Name);
            writer.WriteAttributeString("type", database.Provider.Scheme);
            foreach (Table table in database.Tables)
            {
                if (objects == null || objects.Length == 0 || Glue.Lib.IO.WildCard.Matches(table.Name, objects))
                {
                    table.Write(writer);
                }
            }
            foreach (View view in database.Views)
            {
                if (objects == null || objects.Length == 0 || Glue.Lib.IO.WildCard.Matches(view.Name, objects))
                {
                    view.Write(writer);
                }
            }
            foreach (Procedure proc in database.Procedures)
            {
                if (objects == null || objects.Length == 0 || Glue.Lib.IO.WildCard.Matches(proc.Name, objects))
                {
                    proc.Write(writer);
                }
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public static void Script(Database database, TextWriter writer)
        {
            database.Provider.Script(database, writer);
        }

        public static void Export(Database database, IDataExporter writer, string[] objects)
        {
            foreach (Table table in DetermineExportOrder(database))
            {
                if (objects == null || objects.Length == 0 || Glue.Lib.IO.WildCard.Matches(table.Name, objects))
                {
                    database.Provider.Export(table, writer);
                }
            }
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

        public static void Import(Database database, IDataImporter reader, string[] objects, ImportMode mode)
        {
            while (reader.ReadStart())
            {
                Table table = (Table)Database.Find(database.Tables, reader.Name);
                if (table != null) 
                    database.Provider.Import(table, reader, mode);
            }
        }
    }
}