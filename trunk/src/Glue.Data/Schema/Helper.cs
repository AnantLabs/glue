using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using System.Collections;
using Glue.Data;

/*
namespace Glue.Data.Schema
{
    /// <summary>
    /// Static helper class.
    /// </summary>
    public class Helper
    {
        // Helpers
        public static Regex UrlPattern = new Regex(
            @"(?<scheme>\w+)\:\/\/
              (?<server>[\w\.]+(\/\w+)?)
              (?:\/(?<database>\w+))",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );

        /// <summary>
        /// Scheme can be sql, mysql, oledb
        /// </summary>
        public static ISchemaProvider CreateSchemaProvider(string scheme, string server, string user, string pass)
        {
            string name = "edf.lib.data.providers." + scheme + "SchemaProvider";
            Type type = Configuration.FindType(name);
            if (type == null)
            {
                name = name + ",edf.lib." + scheme + ".dll";
                type =  Configuration.FindType(name);
            }
            if (type == null)
                throw new DataException("Cannot find schema provider for: " + scheme);
            return Activator.CreateInstance(type, new object[] {server, user, pass}) as ISchemaProvider; 
        }

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
    }
}
*/