using System;
using System.Data;
using System.Xml;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Constraint
    /// </summary>
    public class Constraint : SchemaObject
    {
        protected Table table;

        internal Constraint(Table table, XmlElement element) : base(table.Database, element)
        {
            this.table = table;
        }

        public Constraint(Table table, string name) : base(table.Database, name)
        {
            this.table = table;
        }

        public Table Table
        {
            get { return table; }
        }
    }
}
