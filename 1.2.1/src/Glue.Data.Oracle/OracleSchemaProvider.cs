using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Providers.Oracle
{
    public class OracleSchemaProvider
    {
        public class Column
        {
            public string Name;
            public string Type;
            public int Size;
            public bool Nullable;
        }

        public static IDataReader ExecuteCursorProc(OracleDataProvider provider, string package, string name, IDictionary args)
        {
            System.Data.OracleClient.OracleCommand command = (System.Data.OracleClient.OracleCommand)provider.CreateStoredProcedureCommand(package + "." + name);

            using (System.Data.IDataReader cols = provider.ExecuteReader("select * from sys.user_arguments where object_name='" + name + "' and package_name='" + package + "' order by position"))
                while (cols.Read())
                {
                    System.Data.OracleClient.OracleParameter parm;
                    if ("" + cols["DATA_TYPE"] == "REF CURSOR")
                    {
                        parm = new OracleParameter("" + cols["ARGUMENT_NAME"], OracleType.Cursor);
                        parm.OracleType = OracleType.Cursor;
                        parm.Direction = ParameterDirection.Output;
                    }
                    else
                    {
                        parm = new OracleParameter("" + cols["ARGUMENT_NAME"], args[cols["ARGUMENT_NAME"]]);
                        parm.Direction = System.Data.ParameterDirection.Input;
                    }
                    command.Parameters.Add(parm);
                }
            return provider.ExecuteReader(command);
        }

        public static IList<Column> GetColumns(OracleDataProvider provider, string table)
        {
            List<Column> columns = new List<Column>();
            using (System.Data.IDataReader reader = provider.ExecuteReader("select * from sys.user_tab_columns where table_name='" + table + "' order by column_id"))
            while (reader.Read())
            {
                Column c = new Column();
                c.Name = "" + reader["COLUMN_NAME"];
                c.Type = "" + reader["DATA_TYPE"];
                c.Size = Convert.ToInt32(reader["DATA_LENGTH"]);
                c.Nullable = NullConvert.ToBoolean(reader["NULLABLE"], false);
                columns.Add(c);
            }
            return columns;
        }

        public static void GenerateInsertScript(OracleDataProvider provider, TextWriter output, string table)
        {
            IList<Column> columns = GetColumns(provider, table);
            using (System.Data.IDataReader reader = provider.ExecuteReader("select * from " + table))
                while (reader.Read())
                {
                    bool first = true;
                    output.Write("INSERT INTO " + table + " (");
                    foreach (Column col in columns)
                    {
                        if (!first)
                            output.Write(", ");
                        first = false;
                        output.Write(col.Name);
                    }
                    output.Write(")");
                    output.Write("  VALUES (");
                    first = true;
                    foreach (Column col in columns)
                    {
                        if (!first)
                            output.Write(", ");
                        first = false;
                        object data = reader[col.Name];
                        if (data == DBNull.Value)
                            output.Write("null");
                        else if (col.Type == "NVARCHAR" || col.Type == "NVARCHAR2" || col.Type == "VARCHAR" || col.Type == "CHAR" || col.Type == "VARCHAR2")
                            output.Write(EscapeString(data.ToString()));
                        else if (col.Type == "DATE")
                            output.Write("to_date('" + Convert.ToDateTime(data).ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS')");
                        else
                            output.Write(data);
                    }
                    output.WriteLine(");");
                }
        }

        public static string EscapeString(string s)
        {
            StringBuilder d = new StringBuilder(s.Length);
            int mode = 0;
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '&':
                    case '\'':
                    case '\n':
                    case '\r':
                    case '\t':
                    case '\\':
                        if (mode == 1) // in string
                            d.Append("'||");
                        else if (mode == 2) // in escape
                            d.Append("||");
                        d.Append("chr(" + (int)s[i] + ")");
                        mode = 2;
                        break;
                    default:
                        if (mode == 0) // beginning
                            d.Append('\'');
                        if (mode == 2) // in escape
                            d.Append("||'");
                        mode = 1; // in string
                        d.Append(s[i]);
                        break;
                }
            }
            if (mode == 1)
                d.Append('\'');
            return d.ToString();
        }
    }
}
