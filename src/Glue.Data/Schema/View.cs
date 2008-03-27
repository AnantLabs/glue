using System;
using System.Data;
using System.Xml;

namespace Glue.Data.Schema
{
    /// <summary>
    /// View
    /// </summary>
    public class View : Container
    {
        private Index clusteredIndex = null; 
        private string text;

        internal View(Database database, XmlElement element) : base(database, element)
        {
            XmlNode node = element.SelectSingleNode("text");
            text = node == null ? "" : node.InnerText;
        }

        public View(Database database, string name) : base(database, name)
        {
        }

        public Index ClusteredIndex
        {
            get 
            { 
                // TODO:
                return clusteredIndex; 
            }
        }
        
        public string Text
        {
            get 
            { 
                if (text == null)
                    text = Database.Provider.GetViewText(this).Trim();
                return text; 
            }
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("view");
            WriteAttribute(writer, "name", Name);
            
            writer.WriteStartElement("columns");
            foreach (Column column in Columns)
                column.Write(writer);
            writer.WriteEndElement();
            
            writer.WriteStartElement("indexes");
            foreach (Index index in Indexes)
                index.Write(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("triggers");
            foreach (Trigger trigger in Triggers)
                trigger.Write(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("text");
            writer.WriteCData(Text);
            writer.WriteEndElement();
            
            writer.WriteEndElement();
        }
    }
}
