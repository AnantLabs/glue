using System;
using System.Data;
using System.Xml;
using System.Collections;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Table
    /// </summary>
    public class Table : Container
    {
        private Constraint[] constraints;
        private Key[] keys;
        private Key primaryKey;

        internal Table(Database database, XmlElement element) : base(database, element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./keys/primarykey"))
            {
                list.Add(new PrimaryKey(this, e));
            }
            foreach (XmlElement e in element.SelectNodes("./keys/foreignkey"))
            {
                list.Add(new ForeignKey(this, e));
            }
            foreach (XmlElement e in element.SelectNodes("./keys/key"))
            {
                list.Add(new Key(this, e));
            }
            keys = (Key[])list.ToArray(typeof(Key));
        }

        public Table(Database database, string name) : base(database, name)
        {
        }

        public Constraint[] Constraints
        {
            get 
            { 
                if (constraints == null)
                    constraints = new Constraint[0];
                return constraints; 
            }
        }
        
        public Key[] Keys
        {
            get 
            { 
                if (keys == null)
                    keys = Database.Provider.GetKeys(this);
                return keys; 
            }
        }
        
        public Key PrimaryKey
        {
            get 
            { 
                if (primaryKey == null && Keys.Length > 0)
                    primaryKey = Keys[0] as PrimaryKey;
                return primaryKey; 
            }
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("table");
            WriteAttribute(writer, "name", Name);
            
            WriteChildren(writer);
            
            writer.WriteEndElement();
        }

        protected void WriteChildren(XmlWriter writer)
        {
            writer.WriteStartElement("columns");
            foreach (Column column in Columns)
                column.Write(writer);
            writer.WriteEndElement();
            
            writer.WriteStartElement("keys");
            foreach (Key key in Keys)
                key.Write(writer);
            writer.WriteEndElement();
            
            writer.WriteStartElement("constraints");
            foreach (Constraint constraint in Constraints)
                constraint.Write(writer);
            writer.WriteEndElement();
            
            writer.WriteStartElement("indexes");
            foreach (Index index in Indexes)
                index.Write(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("triggers");
            foreach (Trigger trigger in Triggers)
                trigger.Write(writer);
            writer.WriteEndElement();
        }
    }
}
