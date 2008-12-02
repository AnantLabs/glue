using System;
using System.Collections;
using System.IO;
using System.Data;
using Glue.Data;

namespace Glue.Data.Schema
{
    /*
    Page
    Id: Int32
    Date: DateTime
    Title: String
    Summary: String
    Image: DbType.Binary
    .
    Id: 400
    Date: 2007-05-06
    Summary: Test\x10\x13
    Image: \xFD4F5F6G777
    .
    .
    */
    public class SimpleDataExporter : IDataExporter
    {
        TextWriter writer;
        Hashtable lookup;
        string[] names;
        Type[] types;
        object[] values;

        public SimpleDataExporter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void WriteStart(string name, string[] columns, Type[] types)
        {
            this.names = (string[])columns.Clone();
            this.types = (Type[])types.Clone();
            this.values = new object[names.Length];
            this.lookup = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();
            writer.WriteLine(name);
            for (int i = 0; i < names.Length; i++)
                this.lookup[names[i]] = i;
            for (int i = 0; i < names.Length; i++)
                writer.WriteLine(names[i] + ": " + types[i].ToString());
            writer.WriteLine(".");
        }

        public void WriteEnd()
        {
            writer.WriteLine(".");
        }

        public void SetValue(int index, object value)
        {
            values[index] = value;
        }

        public void SetValue(string name, object value)
        {
            SetValue((int)lookup[name], value);
        }

        public void WriteRow()
        {
            for (int i = 0; i < names.Length; i++)
                if (values[i] != null && values[i] != DBNull.Value)
                {
                    writer.Write(names[i]);
                    writer.Write(": ");
                    string s = Helper.SimpleEncode(types[i], values[i]);
                    writer.WriteLine(s);
                }
            writer.WriteLine(".");
            
            // Clear values for next row
            for (int i = 0; i < names.Length; i++)
                values[i] = null;
        }
    }
}
