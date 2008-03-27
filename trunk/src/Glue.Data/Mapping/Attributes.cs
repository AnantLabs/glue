using System;

namespace Glue.Data.Mapping
{
    /// <summary>
    /// MappingOptions
    /// </summary>
    public enum MappingOptions
    {
        None,
        PrefixedColumns,
        PrefixedKeys
    }

    /// <summary>
    /// TableAttribute
    /// </summary>
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string Name;

        /// <summary>
        /// Cache table?
        /// </summary>
        public bool Cached;

        /// <summary>
        /// Table prefix
        /// </summary>
        public string Prefix;

        /// <summary>
        /// Explicit
        /// </summary>
        public bool Explicit;

        /// <summary>
        /// Name of mappingprovider configuration element
        /// </summary>
        public string MappingProvider = "dataprovider";
    }

    /// <summary>
    /// ColumnAttribute
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public string Name;
        public bool Nullable;
        public int MaxLength;
        public ColumnAttribute()
        {
        }
        public ColumnAttribute(string name)
        {
            Name = name;
        }
        public ColumnAttribute(bool nullable)
        {
            Nullable = nullable;
        }
        public ColumnAttribute(string name, bool nullable)
        {
            Name = name;
            Nullable = nullable;
        }
    }

    public class NullableAttribute : ColumnAttribute
    {
        public NullableAttribute() : base(true)
        {
        }
    }


    /// <summary>
    /// ExcludeAttribute
    /// </summary>
    public class ExcludeAttribute : Attribute
    {
    }

    /// <summary>
    /// KeyAttribute
    /// </summary>
    public class KeyAttribute : Attribute
    {
    }

    /// <summary>
    /// AutoKeyAttribute
    /// </summary>
    public class AutoKeyAttribute : KeyAttribute
    {
    }

    /// <summary>
    /// CalculatedAttribute
    /// </summary>
    public class CalculatedAttribute : Attribute
    {
    }

    /// <summary>
    /// AutoFindAttribute
    /// </summary>
    public class AutoFindAttribute : Attribute
    {
    }
}
