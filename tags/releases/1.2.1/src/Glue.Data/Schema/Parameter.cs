using System;
using System.Data;
using System.Xml;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Parameter
    /// </summary>
    public class Parameter: Scalar
    {
        private bool output;
        private bool result;

        internal Parameter(Database database, XmlElement element) : base(database, element)
        {
            output = Configuration.GetAttrBool(element, "output", false);
            result = Configuration.GetAttrBool(element, "result", false);
        }

        public Parameter(
            Database database,
            string name,
            DbType dataType, 
            string nativeType, 
            bool nullable, 
            int precision,
            int scale,
            int size,
            string defaultValue,
            string description,
            bool output,
            bool result
            ) : base(database, name, dataType, nativeType, nullable, precision, scale, size, defaultValue, description)
        {
            this.output = output;
            this.result = result;
        }

        public bool Output
        {
            get { return output; }
        }

        public bool Result
        {
            get { return result; }
        }

        public override void Write(XmlWriter writer)
        {
            writer.WriteStartElement("parameter");
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
            WriteAttribute(writer, "output", Output, false);
            WriteAttribute(writer, "result", Result, false);
            WriteAttribute(writer, "description", Description);
            writer.WriteEndElement();
        }
    }
}
