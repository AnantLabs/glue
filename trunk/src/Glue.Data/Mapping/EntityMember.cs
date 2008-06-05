using System;
using System.Collections;
using System.Data;
using System.Reflection;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Mapping
{
    /// <summary>
    /// EntityMember
    /// </summary>
    public class EntityMember
    {
        public Entity Root;
        public string Name;
        public Type Type;
        public PropertyInfo Property;
        public FieldInfo Field;
        public EntityColumn Column;
        public EntityMemberList Children;
        public bool Aggregated;
        public bool Foreign;
        public KeyAttribute Key;
        public AutoKeyAttribute AutoKey;
        public CalculatedAttribute Calculated;
        public int MaxLength   { get { return Column.MaxLength; } }
        public bool Required   { get { return !Column.Nullable; } }
        public string Pattern  { get { return ""; } }

        public object GetValue(object instance)
        {
            if (Field != null)
                return Field.GetValue(instance);
            else 
                return  Property.GetValue(instance, null);
        }

        public void SetValue(object instance, object value)
        {
            if (Field != null)
                Field.SetValue(instance, value);
            else 
                Property.SetValue(instance, value, null);
        }

        public void Load(object instance, IDataReader from, string[] fields)
        {
            if (Field != null)
                Field.SetValue(instance, from[Column.Name]);
            else
                Property.SetValue(instance, from[Column.Name], null);
        }

        public void Load(object instance, IDataReader from, ref int index)
        {
            object value = null;
            if (DBNull.Value == value)
                value = null;
            if (Field != null)
            {
                Field.SetValue(instance, value);
                return;
            }
            if (Property != null)
            {
                Property.SetValue(instance, value, null);
                return;
            }
            index++;
        }
    }

    /// <summary>
    /// EntityMemberList
    /// </summary>
    public class EntityMemberList : ArrayList
    {
        public EntityMember Find(string name)
        {
            foreach (EntityMember item in this)
                if (item != null && string.Compare(name, item.Name, true) == 0)
                    return item;
            return null;
        }
        
        public EntityMember FindByColumnName(string name)
        {
            if (name == null || name.Length == 0)
                return null;
            if (name[0] == '-' || name[0] == '+')
                name = name.Substring(1);
            foreach (EntityMember item in this)
                if (item != null && string.Compare(name, item.Column.Name, true) == 0)
                    return item;
            return null;
        }

        public new EntityMember this[int i]
        {
            get { return (EntityMember)base[i]; }
        }
        
        public static EntityMemberList Union(EntityMemberList a, EntityMemberList b)
        {
            EntityMemberList r = new EntityMemberList();
            foreach (EntityMember item in a)
                r.Add(item);
            foreach (EntityMember item in b)
                if (r.Find(item.Name) == null)
                    r.Add(item);
            return r;
        }
        
        public static EntityMemberList Subtract(EntityMemberList a, EntityMemberList b)
        {
            EntityMemberList r = new EntityMemberList();
            foreach (EntityMember item in a)
                if (b.Find(item.Name) == null)
                    r.Add(item);
            return r;
        }
        
        public static EntityMemberList Subtract(EntityMemberList a, EntityMemberList b, EntityMemberList c)
        {
            EntityMemberList r = new EntityMemberList();
            foreach (EntityMember item in a)
                if (b.Find(item.Name) == null && c.Find(item.Name) == null)
                    r.Add(item);
            return r;
        }
        
        public static EntityMemberList Flatten(EntityMemberList list)
        {
            EntityMemberList r = new EntityMemberList();
            Flatten(r, list);
            return r;
        }
        
        static void Flatten(EntityMemberList result, EntityMemberList list)
        {
            foreach (EntityMember item in list)
                if (item.Children == null || item.Children.Count == 0)
                    result.Add(item);
                else
                    Flatten(result, item.Children);
        }
    }
}
