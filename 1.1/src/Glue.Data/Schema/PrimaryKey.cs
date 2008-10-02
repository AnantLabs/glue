using System;
using System.Data;
using System.Xml;

namespace Glue.Data.Schema
{
    /// <summary>
    /// PrimaryKey
    /// </summary>
    public class PrimaryKey : Key
    {
        internal PrimaryKey(Table table, XmlElement element) : base(table, element)
        {
        }

        public PrimaryKey(Table table, string name, string[] columns) : base(table, name, columns)
        {
        }

        public override bool IsEq(object obj)
        {
            return base.IsEq(obj) && obj is PrimaryKey;
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("primarykey");
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
