using System;
using System.Data;
using System.Xml;
using System.Collections;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Procedure
    /// </summary>
    public class Procedure : SchemaObject
    {
        private Parameter[] parameters;
        private string text;

        internal Procedure(Database database, XmlElement element) : base(database, element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./parameters/parameter"))
            {
                list.Add(new Parameter(database, e));
            }
            parameters = (Parameter[])list.ToArray(typeof(Parameter));

            XmlNode node = element.SelectSingleNode("text");
            text = node == null ? "" : node.InnerText;
        }

        public Procedure(Database database, string name) : base(database, name)
        {
        }
        
        public Parameter[] Parameters
        {
            get 
            { 
                if (parameters == null)
                    parameters = Database.Provider.GetParameters(this);
                return parameters; 
            }
        }
        
        public string Text
        {
            get 
            { 
                if (text == null)
                    text = Database.Provider.GetProcedureText(this).Trim();
                return text; 
            }
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("procedure");
            WriteAttribute(writer, "name", Name);
            
            writer.WriteStartElement("parameters");
            foreach (Parameter parameter in Parameters)
                parameter.Write(writer);
            writer.WriteEndElement();
            
            writer.WriteStartElement("text");
            writer.WriteCData(Text);
            writer.WriteEndElement();
            
            writer.WriteEndElement();
        }
    }
}
