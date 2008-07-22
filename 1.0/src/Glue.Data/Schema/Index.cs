using System;
using System.Data;
using System.Xml;
using System.Collections;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Index
    /// </summary>
    public class Index : SchemaObject
    {
        Container parent;
        bool isClustered;
        bool isPrimaryKey;
        bool isUnique;
        string[] memberColumnNames;
        Column[] memberColumns;
        
        internal Index(Container parent, XmlElement element) : base(parent.Database, element)
        {
            this.parent = parent;
            this.isClustered = Configuration.GetAttrBool(element, "clustered", false);
            this.isPrimaryKey = Configuration.GetAttrBool(element, "primarykey", false);
            this.isUnique = Configuration.GetAttrBool(element, "unique", false);
            
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./column"))
            {
                list.Add(e.GetAttribute("name"));
            }
            memberColumnNames = (string[])list.ToArray(typeof(string));
        }

        public Index(
            Container parent, 
            string name, 
            bool isClustered, 
            bool isPrimaryKey, 
            bool isUnique, 
            string[] memberColumnNames
            ) 
            : base(parent.Database, name)
        {
            this.parent = parent;
            this.isClustered = isClustered;
            this.isPrimaryKey = isPrimaryKey;
            this.isUnique = isUnique;

            if (memberColumnNames != null)
                this.memberColumnNames = memberColumnNames;
            else
                this.memberColumnNames = new string[0];
        }

        public bool IsClustered
        {
            get {return isClustered; }
        }
        
        public bool IsPrimaryKey
        {
            get {return isPrimaryKey; }
        }
        
        public bool IsUnique
        {
            get {return isUnique; }
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
                        memberColumns[i] = parent.FindColumn(memberColumnNames[i]);
                    }
                }
                return memberColumns; 
            }
        }

        public override bool IsEq(object obj)
        {
            if (!base.IsEq(obj))                          
                return false;
            Index other = obj as Index;
            if (other == null)                              
                return false;
            if (MemberColumns.Length != other.MemberColumns.Length)
                return false;
            for (int i = 0; i < MemberColumns.Length; i++)
                if (MemberColumns[i].Name != other.MemberColumns[i].Name)
                    return false;
            return true;
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("index");
            WriteAttribute(writer, "name", Name);

            WriteAttribute(writer, "primarykey", IsPrimaryKey, false);
            WriteAttribute(writer, "unique", IsUnique, false);
            WriteAttribute(writer, "clustered", IsClustered, false);

            foreach (Column column in MemberColumns)
            {
                writer.WriteStartElement("column");
                WriteAttribute(writer, "name", column.Name);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
