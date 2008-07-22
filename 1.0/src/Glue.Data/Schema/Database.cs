using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Database
    /// </summary>
    public class Database : SchemaObject
    {
        private ISchemaProvider provider;
        private Table[] tables;
        private View[] views;
        private Procedure[] procedures;

        /// <summary>
        /// Use Database.Open instead
        /// </summary>
        public Database(ISchemaProvider provider, string name) : base(null, name)
        {
            this.provider = provider;
        }

        /// <summary>
        /// Use Database.Open instead
        /// </summary>
        public Database(XmlElement element) : base(null, element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("table"))
            {
                list.Add(new Table(this, e));
            }
            tables = (Table[])list.ToArray(typeof(Table));
            
            list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("view"))
            {
                list.Add(new View(this, e));
            }
            views = (View[])list.ToArray(typeof(View));

            list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("procedure"))
            {
                list.Add(new Procedure(this, e));
            }
            procedures = (Procedure[])list.ToArray(typeof(Procedure));
        }

        public ISchemaProvider Provider
        {
            get { return provider; }
            set { provider = value; }
        }
        
        public Table[] Tables
        {
            get 
            { 
                if (tables == null)
                    tables = Provider.GetTables(this);
                return tables;
            }
        }
        
        public View[] Views
        {
            get 
            { 
                if (views == null)
                    views = Provider.GetViews(this);
                return views;
            }
        }

        public Procedure[] Procedures
        {
            get 
            { 
                if (procedures == null)
                    procedures = Provider.GetProcedures(this);
                return procedures;
            }
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("database");
            WriteAttribute(writer, "name", Name);
            WriteAttribute(writer, "type", Provider.Scheme);
            // TODO: Remove attribute OR provide connection string through provider
            // WriteAttribute(writer, "connectionstring", ConnectionString);
            
            foreach (Table table in Tables)
                table.Write(writer);
            
            foreach (View view in Views)
                view.Write(writer);
            
            foreach (Procedure procedure in Procedures)
                procedure.Write(writer);
            
            writer.WriteEndElement();
        }
    }
}
