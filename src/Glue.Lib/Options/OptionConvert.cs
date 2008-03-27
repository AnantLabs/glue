using System;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml;

namespace Glue.Lib.Options
{
    /// <summary>
	/// Summary description for OptionConverter.
	/// </summary>
    public class OptionConvert
    { 
        static Hashtable typeMemberLookup = new Hashtable();

        /// <summary>
        /// Assign a NameValueCollection of arguments to an object instance.
        /// </summary>
        public static void Assign(
            object instance, 
            string[] arguments
            )
        {
            Assign(instance, Get(arguments), OptionConvertFlags.Normal);
        }

        /// <summary>
        /// Assign a NameValueCollection of arguments to an object instance.
        /// </summary>
        public static void Assign(
            object instance, 
            string[] arguments,
            OptionConvertFlags flags
        )
        {
            Assign(instance, Get(arguments), flags);
        }

        /// <summary>
        /// Assign a NameValueCollection of arguments to an object instance.
        /// </summary>
        public static void Assign(
            object instance, 
            NameValueCollection arguments
            )
        {
            Assign(instance, arguments, OptionConvertFlags.Normal);
        }

        /// <summary>
        /// Assign a NameValueCollection of arguments to an object instance.
        /// </summary>
        public static void Assign(
            object instance, 
            NameValueCollection arguments, 
            OptionConvertFlags flags
            )
        {
            // Get type of instance
            Type type = instance.GetType();

            Hashtable memberLookup = (Hashtable)typeMemberLookup[type];
            if (memberLookup == null)
            {
                // Prepare collections of attributes
                memberLookup = new Hashtable(
                    System.Collections.CaseInsensitiveHashCodeProvider.Default,
                    System.Collections.CaseInsensitiveComparer.Default
                    );

                // Walk members on the type
                foreach (MemberInfo mi in type.GetMembers())
                {
                    OptionAttribute at = GetOptionAttribute(mi);
                    if (mi.MemberType == MemberTypes.Property || mi.MemberType == MemberTypes.Field ||
                        mi.MemberType == MemberTypes.Method && (flags & OptionConvertFlags.IncludeMethods) != 0 && 
                        at != null)
                    {
                        if (at != null)
                        {
                            if (at.Name != null && at.Name.Length > 0)
                            {
                                memberLookup.Add(at.Name, mi);
                                if (at.Short != 0)
                                    memberLookup.Add(at.Short.ToString(), mi);
                            } 
                            else if (at.Short != 0)
                            {
                                memberLookup.Add(at.Short.ToString(), mi);
                            } 
                            else if (at is AnonymousOptionAttribute)
                            {
                                memberLookup.Add("", mi); // anonymous options (like filenames)
                            } 
                            else if (at is AnyOptionAttribute)
                            {
                                memberLookup.Add("|", mi); // any option (-p:5 -x:6 -z:6)
                            }
                        }
                        else if ((flags & OptionConvertFlags.AttributedOnly) == 0)
                        {
                            memberLookup.Add(mi.Name, mi);
                        }
                    }
                }
                typeMemberLookup[type] = memberLookup;
            }

            // Walk all name-value pairs
            foreach (string key in arguments.Keys)
            {
                MemberInfo mi = (MemberInfo)memberLookup[key == null ? "" : key];
                
                // If not found, look for a "any" option
                if (mi == null) 
                {
                    mi = (MemberInfo)memberLookup["|"];
                }
                if (mi != null)
                {
                    AssignMember(instance, mi, arguments.GetValues(key), flags);
                }
                else  if ((flags & OptionConvertFlags.IgnoreUnknown) == 0)
                {
                    throw new OptionException("Unknown option: " + key);
                }
            }

            if ((flags & OptionConvertFlags.EatOptions) != 0)
            {
                // TODO: Eat options
            }

            // TODO: Check for missing required arguments
        }

        /// <summary>
        /// Convert argument string to NameValueCollection
        /// </summary>
        public static NameValueCollection Get(string arguments)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert argument strings to NameValueCollection
        /// </summary>
        public static NameValueCollection Get(string[] arguments)
        {
            if (arguments == null)
            {
                return new NameValueCollection();
            }
            NameValueCollection args = new NameValueCollection(arguments.Length);
            foreach (string arg in arguments)
            {
                if (arg != null && arg.Length > 0 && (arg[0] == '/' || arg[0] == '-')) 
                {
                    // Named option
                    int j = arg.IndexOf(':');
                    if (j < 0)
                        args.Add(arg.Substring(1), "");
                    else
                        args.Add(arg.Substring(1, j-1), arg.Substring(j+1));
                }
                else
                {
                    // Unnamed option
                    args.Add(null, arg);
                }
            }
            return args;
        }

        /// <summary>
        /// </summary>
        public static string GetSingleValue(NameValueCollection options, string key)
        {
            string[] v = options.GetValues(key);
            if (v == null || v.Length == 0)
                return null;
            else
                return v[0];
        }

        /// <summary>
        /// Convert argument strings to NameValueCollection
        /// </summary>
        public static NameValueCollection Subset(NameValueCollection options, string prefix)
        {
            NameValueCollection subset = new NameValueCollection(options.Count);
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                string k = options.GetKey(i);
                if (k == null || !k.ToLower().StartsWith(prefix))
                    continue;
                k = k.Substring(prefix.Length);
                string[] values = options.GetValues(i);
                if (values == null || values.Length == 0)
                {
                    subset.Add(k, null);
                }
                else
                {
                    foreach (string v in values)
                        subset.Add(k, v);
                }
            }
            return subset;
        }

        /// <summary>
        /// Convert argument strings to NameValueCollection
        /// </summary>
        public static string ToString(NameValueCollection options)
        {
            return string.Join(" ", ToStringArray(options));
        }

        /// <summary>
        /// Convert argument strings to NameValueCollection
        /// </summary>
        public static string[] ToStringArray(NameValueCollection options)
        {
            ArrayList list = new ArrayList();
            foreach (string key in options.Keys)
            {
                string[] values = options.GetValues(key);
                if (values == null || values.Length == 0)
                {
                    list.Add("-" + key);
                }
                else
                {
                    foreach (string value in options.GetValues(key))
                    {
                        if (key == null || key.Length == 0)
                            list.Add(value);
                        else
                            list.Add("-" + key + ":" + value);
                    }
                }
            }
            return (string[])list.ToArray(typeof(string));
        }

        /// <summary>
        /// Load arguments from file to NameValueCollection
        /// </summary>
        public static NameValueCollection Load(string path)
        {
            using (TextReader reader = new StreamReader(path))
            {
                return Load(reader);
            }
        }

        /// <summary>
        /// Load arguments from file to NameValueCollection, and reads
        /// rest of the file into rest.
        /// </summary>
        public static NameValueCollection Load(string path, out string rest)
        {
            using (TextReader reader = new StreamReader(path))
            {
                NameValueCollection options = Load(reader, out rest);
                rest += "\n" + reader.ReadToEnd().Replace("\r\n", "\n").Replace("\r", "\n");
                return options;
            }
        }

        public static NameValueCollection Load(TextReader reader)
        {
            string next;
            return Load(reader, out next);
        }

        static Regex mimeHeader = new Regex(@"^(?://|\#|\')?\s*([a-zA-Z\-][a-zA-Z0-9_\-]*)\s*\:\s*(.*)");

        public static NameValueCollection Load(TextReader reader, out string next)
        {
            NameValueCollection options = new NameValueCollection();
            string line = reader.ReadLine();
            bool htmlComment = false;
            if (line == "<!--")
            {
                line = reader.ReadLine();
                htmlComment = true;
            }
            while (line != null)
            {
                Match match = mimeHeader.Match(line);
                if (!match.Success)
                    break;
                string name = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
                        value += "\n" + line.TrimStart(' ','\t');
                    else
                        break;
                    line = reader.ReadLine();
                }
                options.Add(name, value);
            }
            if (htmlComment && line == "-->")
            {
                line = reader.ReadLine();
            }
            next = line;
            return options;
        }

        /// <summary>
        /// Get associated OptionAttribute for given member
        /// </summary>
        public static OptionAttribute GetOptionAttribute(MemberInfo mi)
        {
            object[] ats = mi.GetCustomAttributes(typeof(OptionAttribute), true);
            if (ats != null && ats.Length > 0)
                return ats[0] as OptionAttribute;
            return null;
        }

        static void AssignMember(
            object instance, 
            MemberInfo mi, 
            string[] values, 
            OptionConvertFlags flags
            )
        {
            // Subroutine invokation
            if (mi is MethodInfo)
            {
                try 
                {
                    (mi as MethodInfo).Invoke(instance, null);
                }
                catch (Exception e)
                {
                    if (e.InnerException is OptionException)
                        throw e.InnerException;
                    if ((flags & OptionConvertFlags.IgnoreConvertErrors) == 0)
                    {
                        OptionAttribute at = OptionConvert.GetOptionAttribute(mi);
                        throw new OptionException("Error invoking: " + mi == null ? mi.Name : at.Name, e.InnerException);
                    }
                }
                return;
            }

            // Get type of field or property
            PropertyInfo pi = mi as PropertyInfo;
            FieldInfo fi = mi as FieldInfo;
            Type forptype = null;
            if (pi != null)
                forptype = pi.PropertyType;
            else if (fi != null)
                forptype = fi.FieldType;
            else
                throw new ArgumentException("Member must be a field or property");

            // Get converter
            TypeConverter converter = TypeDescriptor.GetConverter(forptype);

            AssignMember(instance, mi, pi, fi, forptype, converter, values, flags);
        }

        static void AssignMember(
            object instance, 
            MemberInfo mi, 
            PropertyInfo pi, 
            FieldInfo fi, 
            Type forptype, 
            TypeConverter converter, 
            string[] values, 
            OptionConvertFlags flags
            )
        {
            try 
            {
                if (values == null || values.Length == 0 || values.Length == 1 && values[0] == null)
                {
                    // Try if it's a boolean flag and switch it
                    if (forptype == typeof(bool))
                    {
                        if (fi != null)
                            fi.SetValue(instance, true);
                            // fi.SetValue(instance, !(bool)fi.GetValue(instance));
                        if (pi != null)
                            pi.SetValue(instance, true, null);
                            // pi.SetValue(instance, !(bool)pi.GetValue(instance, null), null);
                    }
                    return;
                }
                if (forptype.IsEnum)
                {
                    // It's an enumeration, handle these with standard function
                    object v = Enum.Parse(forptype, string.Join(",", values));
                    if (fi != null)
                        fi.SetValue(instance, v);
                    if (pi != null)
                        pi.SetValue(instance, v, null);
                    return;
                }
                if (values.Length == 1)
                {
                    object v = null;
                    if (forptype == typeof(bool))
                    {
                        // It's a bool, handle the variants
                        string s = values[0].ToLower();
                        if (s == "" || s == "+" || s == "yes" || s == "true" || s == "1")
                            v = true;
                        else if (s == "-" || s == "no" || s == "false" || s == "0")
                            v = false;
                    }
                    else if (converter.CanConvertFrom(typeof(string)))
                    {
                        // Eventually convert it with standard converter
                        v = converter.ConvertFromString(values[0]);
                    }
                    if (v != null)
                    {
                        if (fi != null)
                            fi.SetValue(instance, v);
                        if (pi != null)
                            pi.SetValue(instance, v, null);
                        return;
                    }
                }
                if (values.Length > 0)
                {
                    // Almost exhausted, so try if this is an array, list or dictionary
                
                    object obj = null;
                    if (fi != null)
                        obj = fi.GetValue(instance);
                    if (pi != null)
                        obj = pi.GetValue(instance, null);

                    // TODO: Check for too many arguments

                    NameValueCollection nvc = obj as NameValueCollection;
                    if (nvc != null)
                    {
                        foreach (string val in values)
                        {
                            string[] v = val.Split(new char[]{'='}, 2);
                            if (v.Length == 2)
                                nvc.Add(v[0], v[1]);
                        }
                        return;
                    }
            
                    StringDictionary sd = obj as StringDictionary;
                    if (sd != null)
                    {
                        foreach (string val in values)
                        {
                            string[] v = val.Split(new char[]{'='}, 2);
                            if (v.Length == 2)
                                sd.Add(v[0], v[1]);
                        }
                        return;
                    }
            
                    IDictionary d = obj as IDictionary;
                    if (d != null)
                    {
                        foreach (string val in values)
                        {
                            string[] v = val.Split(new char[]{'='}, 2);
                            if (v.Length == 2)
                                d.Add(v[0], v[1]);
                        }
                        return;
                    }

                    IList l = obj as IList;
                    if (l != null)
                    {
                        foreach (string val in values)
                            l.Add(val);
                        return;
                    }

                    // No array-like property found on the instance, 
                    // so try if there's a single value property, to
                    // wich we can assign value[0] to.

                    object single_val = null;
                    if (forptype == typeof(bool))
                    {
                        // It's a bool, handle the variants
                        string s = values[0].ToLower();
                        if (s == "" || s == "+" || s == "yes" || s == "true" || s == "1")
                            single_val = true;
                        else if (s == "-" || s == "no" || s == "false" || s == "0")
                            single_val = false;
                    }
                    else if (converter.CanConvertFrom(typeof(string)))
                    {
                        // Eventually convert it with standard converter
                        single_val = converter.ConvertFromString(values[0]);
                    }

                    if (single_val != null)
                    {
                        if (fi != null)
                            fi.SetValue(instance, single_val);
                        if (pi != null)
                            pi.SetValue(instance, single_val, null);
                        return;
                    }
            
                    if ((flags & OptionConvertFlags.IgnoreConvertErrors) == 0)
                    {
                        OptionAttribute at = OptionConvert.GetOptionAttribute(mi);
                        throw new ArgumentException("No conversion available for: " + at == null ? mi.Name : at.Name);
                    }
                }
            }
            catch (Exception e)
            {
                if ((flags & OptionConvertFlags.IgnoreConvertErrors) == 0)
                {
                    OptionAttribute at = OptionConvert.GetOptionAttribute(mi);
                    throw new OptionException("Error converting option: " + at == null ? mi.Name : at.Name, e);
                }
            }
        }
    }
}
