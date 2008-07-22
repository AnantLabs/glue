using System;
using System.Data;
using System.Xml;
using Glue.Lib;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Scalar
    /// </summary>
    public class Scalar : SchemaObject
    {
        private DbType dataType;
        private string nativeType;
        private bool nullable;
        private int precision;
        private int scale;
        private int size;
        private string defaultValue;
        private string description;
        
        internal Scalar(Database database, XmlElement element) : base(database, element)
        {
            dataType = (DbType)Configuration.GetAttrEnum(element, "datatype", typeof(DbType));
            nativeType = element.GetAttribute("type");
            nullable = Configuration.GetAttrBool(element, "nullable", false);
            precision = Configuration.GetAttrUInt(element, "precision", 0);
            scale = Configuration.GetAttrUInt(element, "scale", 0);
            size = Configuration.GetAttrUInt(element, "size", 0);
            defaultValue = Configuration.GetAttr(element, "default", null);
            description = Configuration.GetAttr(element, "description", null);
        }

        public Scalar(
            Database database,
            string name, 
            DbType dataType, 
            string nativeType, 
            bool nullable, 
            int precision,
            int scale,
            int size,
            string defaultValue,
            string description
            ) : 
            base(database, name)
        {
            this.dataType = dataType;
            this.nativeType = nativeType;
            this.nullable = nullable;
            this.precision = precision;
            this.scale = scale;
            this.size = size;
            this.defaultValue = defaultValue;
            this.description = description;
        }

        public override bool IsEq(object obj)
        {
            if (!base.IsEq(obj))                  
                return false;
            Scalar other = obj as Scalar;
            if (other == null)                      
                return false;
            if (DataType != other.DataType)         
                return false;
            if (HasSize != other.HasSize)           
                return false;
            if (HasSize && Size != other.Size)      
                return false;
            if (DefaultValue != other.DefaultValue) 
                return false;
            if (HasPrecision != other.HasPrecision) 
                return false;
            if (HasPrecision && precision != other.Precision) 
                return false;
            return true;
        }

        public DbType DataType
        {
            get { return dataType; }
        }
        
        public string NativeType
        {
            get { return nativeType; }
        }

        public Type SystemType
        {
            get { return Helper.GetSystemType(DataType); }
        }
        
        public bool Nullable
        {
            get { return nullable; }
        }
        
        public int Precision
        {
            get { return precision; }
        }
        
        public int Scale
        {
            get { return scale; }
        }
        
        public int Size
        {
            get { return size; }
        }
        
        public string DefaultValue
        {
            get { return defaultValue; }
        }
        
        public string Description
        {
            get { return description; }
        }

        public bool HasPrecision
        {
            get 
            {
                return 
                    dataType == DbType.Decimal ||
                    dataType == DbType.VarNumeric;
            }
        }

        public bool HasSize
        {
            get 
            {
                return 
                    dataType == DbType.String && size != 0 || 
                    dataType == DbType.AnsiString && size != 0 || 
                    dataType == DbType.Binary && size != 0 ||
                    dataType == DbType.StringFixedLength ||
                    dataType == DbType.AnsiStringFixedLength  ||
                    dataType == DbType.VarNumeric;
            }
        }
    }
}
