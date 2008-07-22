using System;
using System.Data;
using System.Xml;
using System.Collections;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Key
    /// </summary>
    public class Key : Constraint
    {
        string[] memberColumnNames;
        Column[] memberColumns;
        
        internal Key(Table table, XmlElement element) : base(table, element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./column"))
            {
                list.Add(e.GetAttribute("name"));
            }
            memberColumnNames = (string[])list.ToArray(typeof(string));
        }

        public Key(Table table, string name, string[] columns) : base(table, name)
        {
            if (columns == null)
                this.memberColumnNames = new string[0];
            else
                this.memberColumnNames = columns;
        }
        
        public Column[] MemberColumns
        {
            get 
            {
                if (memberColumns == null)
                {
                    memberColumns = new Column[memberColumnNames.Length];
                    for (int i = 0; i < memberColumnNames.Length; i++)
                    {
                        memberColumns[i] = this.table.FindColumn(memberColumnNames[i]);
                    }
                }
                return memberColumns; 
            }
        }

        public override bool IsEq(object obj)
        {
            if (!base.IsEq(obj))                          
                return false;
            Key other = obj as Key;
            if (other == null)                              
                return false;
            if (StringHelper.ExclusiveOr(memberColumnNames, other.memberColumnNames, true).Length != 0)
                return false;
            return true;
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("key");
            WriteAttribute(writer, "name", Name);
            foreach (Column c in MemberColumns)
            {
                writer.WriteStartElement("column");
                WriteAttribute(writer, "name", c.Name);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
