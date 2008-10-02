using System;
using System.Data;
using System.Xml;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Trigger
    /// </summary>
    public class Trigger : SchemaObject
    {
        Container parent;

        internal Trigger(Container parent, XmlElement element) : base(parent.Database, element)
        {
            // TODO:
            this.parent = parent;
        }

        public Trigger(Container parent, string name) : base(parent.Database, name)
        {
            // TODO:
            this.parent = parent;
        }
    }
}
