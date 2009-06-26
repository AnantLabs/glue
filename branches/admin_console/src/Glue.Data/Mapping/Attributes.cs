using System;

namespace Glue.Data.Mapping
{
    /// <summary>
    /// Properties for the database table.
    /// </summary>
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Table name. If not set, it is assumed to be the same as the class name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Cache table?
        /// The complete table will be cached the first time it is loaded. It is stored in a hashtable by its 
        /// primary key. Use the IDataProvider.InvalidateCache() method if you need to clear the cache.
        /// Useful for lookup tables.
        /// </summary>
        public bool Cached;

        /// <summary>
        /// Columns in the database table have this prefix. The prefix will be prepended to the property name.
        /// </summary>
        public string Prefix;

        /// <summary>
        /// If true, a public property in this class will only be mapped to a database column if it has
        /// a [Column] attribute.
        /// If false, every public property will be mapped, unless it has an [Exclude] attribute.
        /// </summary>
        public bool Explicit;

        /// <summary>
        /// Name of the dataprovider configuration element. The default is 'dataprovider'.
        /// </summary>
        public string DataProvider = "dataprovider";
    }

    /// <summary>
    /// Properties for the database column. 
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

    //[Obsolete("Nullable attribute is obsolete; please use a nullable type (like int?, DateTime? etc.)")]
    public class NullableAttribute : ColumnAttribute
    {
        public NullableAttribute() : base(true)
        {
        }
    }


    /// <summary>
    /// Entities with an [Exclude] attribute will not be mapped to database columns.
    /// </summary>
    public class ExcludeAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute specifies that the corresponding column is (part of) the primary key.
    /// </summary>
    public class KeyAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that a class member is an auto key. Its value will be set by the database when the object is
    /// inserted.
    /// </summary>
    public class AutoKeyAttribute : KeyAttribute
    {
    }

    /// <summary>
    /// Specifies that the database column is a calculated value. It will not be included in Inserts or Updates,
    /// but will be retrieved by Find and List.
    /// </summary>
    public class CalculatedAttribute : Attribute
    {
    }
}
