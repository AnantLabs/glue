using System;
using System.Collections;
using System.IO;
using System.Data;
using Glue.Data;

namespace Glue.Data.Schema
{
	/// <summary>
	/// Summary description for SimpleDataExporter.
	/// </summary>
	public class SimpleDataImporter : IDataImporter
	{
        TextReader reader;
        string name;
        string[] columns = {};
        Type[] types = {};
        Hashtable lookup;
        object[] values;

		public SimpleDataImporter(TextReader reader)
		{
            this.reader = reader;
        }

        public bool ReadStart()
        {
            string line = reader.ReadLine();
            if (line != null)
            {
                ArrayList col = new ArrayList();
                ArrayList typ = new ArrayList();
                this.name = line;
                line = reader.ReadLine();
                while (line != null && line != ".")
                {
                    string[] s = line.Split(':');
                    col.Add(s[0].Trim());
                    typ.Add(Type.GetType(s[1].Trim(), true, true));
                    line = reader.ReadLine();
                }
                this.columns = (string[])col.ToArray(typeof(string));
                this.types = (Type[])typ.ToArray(typeof(Type));
                this.lookup = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();
                for (int i = 0; i < this.columns.Length; i++)
                    this.lookup[this.columns[i]] = i;
                this.values = new object[this.columns.Length];
            }
            return (line != null);
        }

        public bool ReadRow()
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = null;
            string line = reader.ReadLine();
            if (line == ".")
                return false;
            while (line != null && line != ".")
            {
                string[] s = line.Split(new char[] {':'}, 2);
                int i = (int)lookup[s[0].Trim()];
                values[i] = Helper.SimpleDecode(types[i], s[1].Trim());
                line = reader.ReadLine();
            }
            return line != null;
        }

        public object GetValue(int index)
        {
            return values[index];
        }

        public object GetValue(string name)
        {
            return values[(int)lookup[name]];
        }
        
        public string Name 
        {
            get { return name; }
        }

        public string[] Columns
        {
            get { return columns; }
        }

        public Type[] Types
        {
            get { return types; }
        }

    }
}
