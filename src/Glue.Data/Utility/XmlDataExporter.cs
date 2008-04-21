using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Data;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Summary description for XmlDataExporter.
    /// </summary>
    public class XmlDataExporter : IDataExporter
    {
        XmlWriter writer;
        Hashtable lookup;
        string name;
        string[] names;
        Type[] types;
        object[] values;
        
        public XmlDataExporter(XmlWriter writer)
        {
            this.writer = writer;
        }

        public void WriteStart(string name, string[] columns, Type[] types)
        {
            this.name = name;
            this.names = (string[])columns.Clone();
            this.types = (Type[])types.Clone();
            this.values = new object[names.Length];
            this.lookup = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();
            for (int i = 0; i < names.Length; i++)
                this.lookup[names[i]] = i;
            // for (int i = 0; i < names.Length; i++)
            // writer.WriteLine(names[i] + ": " + types[i].ToString());
            // writer.WriteLine(".");
            writer.WriteStartElement("table");
            writer.WriteAttributeString("name", name);
        }

        public void WriteEnd()
        {
            writer.WriteEndElement();
        }

        public void SetValue(int index, object value)
        {
            if (value == null || value == DBNull.Value)
                values[index] = null;
            else
                values[index] = value;
        }

        public void SetValue(string name, object value)
        {
            SetValue((int)lookup[name], value);
        }

        public void WriteRow()
        {
            writer.WriteStartElement("row");
            for (int i = 0; i < names.Length; i++)
            {
                if (values[i] == null)
                    continue;
                
                Type type = types[i];
                if (type == typeof(string))
                {
                    writer.WriteElementString(names[i], (string)values[i]);
                }
                else if (type == typeof(bool))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((bool)values[i]));
                }
                else if (type == typeof(byte[]))
                {
                    byte[] v = (byte[])values[i];
                    writer.WriteStartElement(names[i]);
                    writer.WriteBinHex(v, 0, v.Length);
                    writer.WriteEndElement();
                }
                else if (type == typeof(DateTime))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((DateTime)values[i]));
                }
                else if (type == typeof(Guid))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((Guid)values[i]));
                }
                else if (type == typeof(Int64) || type == typeof(Int32) || type == typeof(Int16))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((Int64)values[i]));
                }
                else if (type == typeof(UInt64) || type == typeof(UInt32) || type == typeof(UInt16) || type == typeof(Byte))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((UInt64)values[i]));
                }
                else if (type == typeof(Double) || type == typeof(Single))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((Double)values[i]));
                }
                else if (type == typeof(Decimal))
                {
                    writer.WriteElementString(names[i], XmlConvert.ToString((Decimal)values[i]));
                }
                else
                {
                    throw new DataException("Unsupported: " + type);
                }
            }
            writer.WriteEndElement();
        }
    }
}
