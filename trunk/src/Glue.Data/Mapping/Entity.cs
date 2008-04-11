using System;
using System.Collections;
using System.Data;
using System.Reflection;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Mapping
{
    /// <summary>
    /// Entity
    /// </summary>
    public class Entity
    {
        public EntityMemberList AllMembers;
        public EntityMemberList KeyMembers;
        public EntityMemberList AutoMembers;
        public EntityMember AutoKeyMember;
        public Type Type;
        public TableAttribute Table;
        public string FindCommandText;
        public string InsertCommandText;
        public string UpdateCommandText;
        public string DeleteCommandText;
        public string ReplaceCommandText;
        public Accessor Accessor;
        public IDictionary Cache;

        public Entity(Type type)
        {
            this.Type = type;

            AllMembers = new EntityMemberList();
            KeyMembers = new EntityMemberList();
            AutoMembers = new EntityMemberList();
            
            Table = (TableAttribute)GetAttribute(Type, typeof(TableAttribute));
            if (Table == null)
                Table = new TableAttribute();
            if (Table.Name == null)
                Table.Name = Type.Name;
            
            foreach (MemberInfo member in ListRelevantFieldsAndProperties(Type, Table.Explicit))
            {
                EntityMember em = CreateEntityMember(this, member, Table.Prefix);

                AllMembers.Add(em);
                if (em.Key != null)
                {
                    KeyMembers.Add(em);
                }
                if (em.AutoKey != null)
                {
                    AutoKeyMember = em;
                    AutoMembers.Add(em);
                }
                else if (em.Calculated != null)
                {
                    AutoMembers.Add(em);
                }
            }
        }

        static Hashtable entityCache = new Hashtable();

        public static Entity Obtain(Type type)
        {
            try
            {
                Entity info = (Entity)entityCache[type];
                if (info == null)
                {
                    entityCache[type] = -1; // means busy, will throw exception if recursion occurs.
                    info = new Entity(type);
                    entityCache[type] = info;
                }
                return info;
            }
            catch (InvalidCastException)
            {
                throw new DataException("Cannot obtain entity information for " + type + ". This may be caused by recursion.");
            }
        }

        public static IList ListRelevantFieldsAndProperties(Type type, bool xplicit)
        {
            ArrayList list = new ArrayList();
            foreach (MemberInfo member in type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (GetAttribute(member, typeof(ExcludeAttribute)) != null)
                    continue;

                if (member is FieldInfo)
                {
                    FieldInfo field = member as FieldInfo;
                    if (GetAttribute(member, typeof(ColumnAttribute)) == null) // No explicit column specified
                        if (!field.IsPublic)
                            continue;
                        else if (xplicit) 
                            continue; // Public field but no column because not explicitly specified
                }
                else if (member is PropertyInfo)
                {
                    PropertyInfo prop = member as PropertyInfo;
                    if (GetAttribute(member, typeof(ColumnAttribute)) == null) // No explicit column specified
                        if (prop.GetGetMethod() == null) // Does not have public get?
                            continue;
                        else if (xplicit)
                            continue; // Public property but no column because not explicitly specified
                    if (prop.GetSetMethod(true) == null)
                        continue;
                }
                else
                {
                    continue;
                }
                list.Add(member);
            }
            return list;
        }

        static EntityMember CreateEntityMember(Entity root, MemberInfo member, string prefix)
        {
            EntityMember em = new EntityMember();
            em.Name = member.Name;
            em.Root = root;
            em.Property = member as PropertyInfo;
            em.Field = member as FieldInfo;
            em.Type = GetFieldOrPropertyType(member);;

            ColumnAttribute ca = GetAttribute(member, typeof(ColumnAttribute)) as ColumnAttribute;
                
            em.Key = GetAttribute(member, typeof(KeyAttribute)) as KeyAttribute;
            em.AutoKey = GetAttribute(member, typeof(AutoKeyAttribute)) as AutoKeyAttribute;
            em.Calculated = GetAttribute(member, typeof(CalculatedAttribute)) as CalculatedAttribute;

            if (em.Type == typeof(string) ||
                em.Type == typeof(bool) ||
                em.Type == typeof(int) ||
                em.Type == typeof(long) ||
                em.Type == typeof(double) ||
                em.Type == typeof(float) ||
                em.Type == typeof(Guid) ||
                em.Type == typeof(DateTime) ||
                em.Type == typeof(decimal) ||
                em.Type == typeof(char) ||
//[NET20
                em.Type == typeof(bool?) ||
                em.Type == typeof(int?) ||
                em.Type == typeof(long?) ||
                em.Type == typeof(double?) ||
                em.Type == typeof(float?) || 
                em.Type == typeof(Guid?) ||
                em.Type == typeof(DateTime?) ||
                em.Type == typeof(decimal?) ||
                em.Type == typeof(char?) ||
//NET20]
                em.Type.IsEnum)
            {
                EntityColumn ec = new EntityColumn();
                if (ca != null && ca.Name != null)
                    ec.Name = prefix + ca.Name;
                else
                    ec.Name = prefix + member.Name;
                
                if (ca != null)
                {
                    ec.Nullable = ca.Nullable;
                    ec.MaxLength = ca.MaxLength;
                }

                if (em.Type == typeof(bool?))
                {
                    ec.Type = typeof(bool);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(int?))
                {
                    ec.Type = typeof(int);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(long?))
                {
                    ec.Type = typeof(long);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(double?))
                {
                    ec.Type = typeof(double);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(Guid?))
                {
                    ec.Type = typeof(Guid);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(DateTime?))
                {
                    ec.Type = typeof(DateTime);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(Decimal?))
                {
                    ec.Type = typeof(Decimal);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(float?))
                {
                    ec.Type = typeof(float);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else if (em.Type == typeof(char?))
                {
                    ec.Type = typeof(char);
                    ec.GenericNullable = ec.Nullable = true;
                }
                else
                {
                ec.Type = em.Type;
                
                if (ec.Type == typeof(string))
                    ec.Nullable = true;
                else if (ec.Type == typeof(DateTime))
                    ec.ConventionalNullValue = "DateTime.MinValue";
                else if (ec.Type == typeof(Guid))
                    ec.ConventionalNullValue = "Guid.Empty";
                else if (ec.Type == typeof(string))
                    ec.ConventionalNullValue = null;
                else if (ec.Type == typeof(bool)) 
                    ec.ConventionalNullValue = "false";
                else if (ec.Type == typeof(char))
                    ec.ConventionalNullValue = "' '";
                else
                    ec.ConventionalNullValue = "0";
                }
                em.Column = ec;
            }
            else if (GetAttribute(em.Type, typeof(TableAttribute)) != null)
            {
                Entity foreign = Obtain(em.Type);
                em.Foreign = true;
                EntityColumn ec = new EntityColumn();
                if (ca != null && ca.Name != null)
                    ec.Name = prefix + ca.Name;
                else
                    ec.Name = prefix + foreign.KeyMembers[0].Column.Name;
                ec.Type = foreign.KeyMembers[0].Column.Type;
                em.Column = ec;
            }
            else if (em.Type.IsClass)
            {
                em.Aggregated = true;
                em.Children = new EntityMemberList();
                string child_prefix;
                if (ca != null && ca.Name != null)
                    child_prefix = prefix + ca.Name;
                else
                    child_prefix = prefix + member.Name;
                foreach (MemberInfo child in ListRelevantFieldsAndProperties(em.Type, false))
                {
                    em.Children.Add(CreateEntityMember(root, child, child_prefix));
                }
            }
            else
            {
                throw new InvalidOperationException("Member " + em.Name + " on " + root.Type + " has a type which can't be mapped.");        
            }
            return em;
        }

        public static Type GetFieldOrPropertyType(MemberInfo info)
        {
            if (info is FieldInfo)
                return (info as FieldInfo).FieldType;
            if (info is PropertyInfo)
                return (info as PropertyInfo).PropertyType;
            throw new ArgumentException();
        }

        public static Attribute GetAttribute(Type type, Type attributeType)
        {
            object[] attr = type.GetCustomAttributes(attributeType, false);
            return attr == null || attr.Length == 0 ? null : attr[0] as Attribute;
        }
        
        public static Attribute GetAttribute(MemberInfo member, Type attributeType)
        {
            object[] attr = member.GetCustomAttributes(attributeType, false);
            return attr == null || attr.Length == 0 ? null : attr[0] as Attribute;
        }
    }
}
