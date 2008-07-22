using System;
using System.Xml;
using System.Data;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /*
    database
        tables
            table
                constraints
                    constraint
                clusteredindex
                columns
                    column
                foreignkeys
                    foreignkey
                indexes
                    index
                primarykey
                triggers
                    trigger
        views
            view
                columns
                clusteredindex
                indexes
                    index
                triggers
        
        procedures
            procedures
                parameters
                    parameter
        functions
            function
                parameters
                    parameter
    */                        

    /// <summary>
    /// SchemaObject
    /// </summary>
    public class SchemaObject
    {
        private object id;
        private Database database;
        protected string name;

        public static SchemaObject Find(SchemaObject[] list, string name)
        {
            foreach (SchemaObject obj in list)
                if (string.Compare(obj.Name, name, true) == 0)
                    return obj;
            return null;
        }

        protected SchemaObject(Database database, XmlElement element)
        {
            this.database = database;
            this.name = Configuration.GetAttr(element, "name", null);
        }

        public SchemaObject(Database database, string name)
        {
            this.database = database;
            this.name = name;
        }

        public virtual bool IsEq(object obj)
        {
            SchemaObject other = obj as SchemaObject;
            if (other == null) return false;
            if (other == this) return true;
            if (Name != other.Name) return false;
            return true;
        }

        public static bool IsEqList(SchemaObject[] list1, SchemaObject[] list2)
        {
            foreach (SchemaObject e1 in list1)
            {
                SchemaObject e2 = SchemaObject.Find(list2, e1.Name);
                if (e2 == null || !e2.IsEq(e1))
                    return false;
            }
            foreach (SchemaObject e2 in list2)
            {
                SchemaObject e1 = SchemaObject.Find(list1, e2.Name);
                if (e1 == null || !e1.IsEq(e2))
                    return false;
            }
            return true;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public Database Database
        {
            get { return database; }
        }

        public virtual void Write(XmlWriter writer)
        {
            writer.WriteStartElement(GetType().Name);
            writer.WriteAttributeString("name", this.Name);
            writer.WriteEndElement();
        }

        public override string ToString()
        {
            return name;
        }

        public object Id
        {
            get { return id; }
            set { id = value; }
        }

        protected void WriteAttribute(XmlWriter writer, string name, bool value, bool standard)
        {
            if (value != standard)
                writer.WriteAttributeString(name, value.ToString());
        }

        protected void WriteAttribute(XmlWriter writer, string name, int value, int standard)
        {
            if (value != standard)
                writer.WriteAttributeString(name, value.ToString());
        }
    
        protected void WriteAttribute(XmlWriter writer, string name, string value)
        {
            if (value != null && value.Length > 0)
                writer.WriteAttributeString(name, value);
        }

        protected void WriteAttribute(XmlWriter writer, string name, object value)
        {
            if (value != null)
                writer.WriteAttributeString(name, value.ToString());
        }
    }
}
