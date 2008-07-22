using System;
using System.Collections;

namespace Glue.Data.Mapping
{
    /// <summary>
    /// EntityColumn
    /// </summary>
    public class EntityColumn
    {
        public string Name;
        public Type Type;
        public bool Nullable;
        public bool GenericNullable;
        public string ConventionalNullValue;
        public int MaxLength;
    }

    /// <summary>
    /// EntityColumnList
    /// </summary>
    public class EntityColumnList : ArrayList
    {
        public EntityColumn Find(string name)
        {
            foreach (EntityColumn item in this)
                if (item != null && string.Compare(name, item.Name, true) == 0)
                    return item;
            return null;
        }
        public new EntityColumn this[int i]
        {
            get { return (EntityColumn)base[i]; }
        }
    }
}
