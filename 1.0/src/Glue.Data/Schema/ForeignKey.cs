using System;
using System.Data;
using System.Xml;
using System.Collections;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /// <summary>
    /// ForeignKey
    /// </summary>
    public class ForeignKey : Key
    {
        string      referencedTableName;
        Table       referencedTable;
        string[]    referencedColumnNames;
        Column[]    referencedColumns;

        internal ForeignKey(Table table, XmlElement element) : base(table, element)
        {
            referencedTableName = element.GetAttribute("ref");
            
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./column"))
            {
                list.Add(e.GetAttribute("ref"));
            }
            referencedColumnNames = (string[])list.ToArray(typeof(string));
        }

        public ForeignKey(
            Table table, 
            string name, 
            string[] columns, 
            string referencedTableName, 
            string[] referencedColumnNames) : base(table, name, columns)
        {
            this.referencedTableName = referencedTableName;
            if (referencedColumnNames != null)
                this.referencedColumnNames = referencedColumnNames;
            else
                this.referencedColumnNames = new string[0];
        }
        
        public Table ReferencedTable
        {
            get 
            {
                if (referencedTable == null)
                {
                    referencedTable = (Table)Find(Database.Tables, referencedTableName);
                }
                return referencedTable;
            }
        }

        public Column[] ReferencedColumns
        {
            get 
            {
                if (referencedColumns == null)
                {
                    referencedColumns = new Column[referencedColumnNames.Length];
                    for (int i = 0; i < referencedColumnNames.Length; i++)
                    {
                        referencedColumns[i] = ReferencedTable.FindColumn(referencedColumnNames[i]);
                    }
                }
                return referencedColumns; 
            }
        }

        public override bool IsEq(object obj)
        {
            if (!base.IsEq(obj))                          
                return false;
            ForeignKey other = obj as ForeignKey;
            if (other == null)
                return false;
            if (referencedTableName != other.referencedTableName)
                return false;
            if (StringHelper.ExclusiveOr(referencedColumnNames, other.referencedColumnNames, true).Length != 0)
                return false;
            return true;
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("foreignkey");
            WriteAttribute(writer, "name", Name);
            WriteAttribute(writer, "ref", ReferencedTable.Name);
            int n = MemberColumns.Length;
            for (int i = 0; i < n; i++)
            {
                writer.WriteStartElement("column");
                WriteAttribute(writer, "name", MemberColumns[i].Name);
                WriteAttribute(writer, "ref", ReferencedColumns[i].Name);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
