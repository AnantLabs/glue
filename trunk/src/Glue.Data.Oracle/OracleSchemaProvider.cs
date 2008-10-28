﻿using System;
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

        public static IList<Column> GetColumns(OracleDataProvider provider, string table)
        {
            List<Column> columns = new List<Column>();
            using (System.Data.IDataReader reader = provider.ExecuteReader("select * from sys.all_tab_columns where owner='TEST' and table_name='" + table + "' order by column_id"))
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
                            output.Write("'" + data + "'");
                        else if (col.Type == "DATE")
                            output.Write("to_date('" + data + "','YYYY-MM-DD HH24:MI:SS')");
                        else
                            output.Write(data);
                    }
                    output.WriteLine(");");
                }
        }

    }
}