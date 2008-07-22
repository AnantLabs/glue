using System;
using System.Data;
using System.Xml;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Column
    /// </summary>
    public class Column : Scalar
    {
        private Container parent;
        private bool identity;
        private bool computed;
        private string expression;

        internal Column(Container parent, XmlElement element) : base(parent.Database, element)
        {
            this.parent = parent;
            identity = Configuration.GetAttrBool(element, "identity", false);
            computed = Configuration.GetAttrBool(element, "computed", false);
            expression = Configuration.GetAttr(element, "expression", null);
        }

        public Column(
            Container parent,
            string name, 
            DbType dataType, 
            string nativeType, 
            bool nullable, 
            int precision,
            int scale,
            int size,
            string defaultValue,
            string description,
            bool identity,
            bool computed,
            string expression
            ) : 
            base(parent.Database, name, dataType, nativeType, nullable, precision, scale, size, defaultValue, description)
        {
            this.parent = parent;
            this.identity = identity;
            this.computed = computed;
            this.expression = expression;
        }

        public Container Parent
        {
            get { return parent; }
        }

        public bool Identity
        {
            get { return identity; }
        }

        public bool Computed
        {
            get { return computed; }
        }

        public string Expression
        {
            get { return expression; }
        }

        public override bool IsEq(object obj)
        {
            if (!base.IsEq(obj))                          
                return false;
            Column other = obj as Column;
            if (other == null)                              
                return false;
            if (Identity != other.Identity)                 
                return false;
            if (Computed != other.Computed)                 
                return false;
            if (Computed && Expression != other.Expression) 
                return false;
            return true;
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("column");
            WriteAttribute(writer, "name", Name);
            WriteAttribute(writer, "type", NativeType);
            WriteAttribute(writer, "datatype", DataType);
            if (HasPrecision)
            {
                WriteAttribute(writer, "precision", Precision, 0);
                WriteAttribute(writer, "scale", Scale, 0);
            }
            if (HasSize)
            {
                WriteAttribute(writer, "size", Size, 0);
            }
            WriteAttribute(writer, "nullable", Nullable, false);
            WriteAttribute(writer, "default", DefaultValue);
            WriteAttribute(writer, "identity", Identity, false);
            WriteAttribute(writer, "computed", Computed, false);
            if (Computed)
            {
                WriteAttribute(writer, "expression", Expression);
            }
            WriteAttribute(writer, "description", Description);
            writer.WriteEndElement();
        }
    }
}
