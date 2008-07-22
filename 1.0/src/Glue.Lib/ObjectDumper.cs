using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Glue.Lib
{
    public class ObjectDumper
    {

        public static void Write(object instance)
        {
            Write(Console.Out, instance, 0);
        }

        public static void Write(object instance, int depth)
        {
            Write(Console.Out, instance, depth);
        }

        public static void Write(TextWriter output, object instance, int depth)
        {
            ObjectDumper dumper = new ObjectDumper(depth);
            dumper.WriteObject(null, instance);
            Console.Write(dumper.ToString());
        }

        private StringBuilder builder = new StringBuilder();

        int pos;
        int level;
        int depth;

        public override string ToString()
        {
            return builder.ToString();
        }

        private ObjectDumper(int depth)
        {
            this.depth = depth;
        }

        private void Write(string s)
        {
            if (s != null)
            {
                builder.Append(s);
                pos += s.Length;
            }
        }

        private void WriteIndent()
        {
            builder.Append(' ', level * 3);
        }

        private void WriteLine()
        {
            builder.AppendLine();
            pos = 0;
        }

        private void WriteTab()
        {
            Write("\t");
        }

        private void WriteObject(string prefix, object o)
        {

            if (o == null || o is ValueType || o is string)
            {
                WriteIndent();
                Write(prefix);
                WriteValue(o);
                WriteLine();
            }
            else if (o is IEnumerable)
            {
                foreach (object element in (IEnumerable)o)
                {
                    if (element is IEnumerable && !(element is string))
                    {
                        WriteIndent();
                        Write(prefix);
                        Write("…");
                        WriteLine();
                        if (level < depth)
                        {
                            level++;
                            WriteObject(prefix, element);
                            level--;
                        }
                    }
                    else
                    {
                        WriteObject(prefix, element);
                    }
                }
            }
            else if (o is System.Data.IDataRecord)
            {
                System.Data.IDataRecord r = (System.Data.IDataRecord)o;
                WriteIndent();
                Write(prefix);
                for (int i = 0; i < r.FieldCount; i++)
                {
                    if (i > 0)
                        WriteTab();
                    Write(r.GetName(i));
                    Write("=");
                    WriteValue(r.GetValue(i));
                }
                WriteLine();
            }
            else
            {
                MemberInfo[] members = o.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                WriteIndent();
                Write(prefix);
                bool propWritten = false;
                foreach (MemberInfo m in members)
                {
                    FieldInfo f = m as FieldInfo;
                    PropertyInfo p = m as PropertyInfo;
                    if (f != null || p != null)
                    {
                        if (propWritten)
                        {
                            WriteTab();
                        }
                        else
                        {
                            propWritten = true;
                        }
                        Write(m.Name);
                        Write("=");
                        Type t = f != null ? f.FieldType : p.PropertyType;
                        if (t.IsValueType || t == typeof(string))
                        {
                            WriteValue(f != null ? f.GetValue(o) : p.GetValue(o, null));
                        }
                        else
                        {
                            if (typeof(IEnumerable).IsAssignableFrom(t))
                            {
                                Write("…");
                            }
                            else
                            {
                                Write("{ }");
                            }
                        }
                    }
                }
                if (propWritten) WriteLine();
                if (level < depth)
                {
                    foreach (MemberInfo m in members)
                    {
                        FieldInfo f = m as FieldInfo;
                        PropertyInfo p = m as PropertyInfo;
                        if (f != null || p != null)
                        {
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if (!(t.IsValueType || t == typeof(string)))
                            {
                                object value = f != null ? f.GetValue(o) : p.GetValue(o, null);
                                if (value != null)
                                {
                                    level++;
                                    WriteObject(m.Name + ": ", value);
                                    level--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(object o)
        {
            if (o == null)
            {
                Write("null");
            }
            else if (o is DateTime)
            {
                Write(((DateTime)o).ToShortDateString());
            }
            else if (o is ValueType || o is string)
            {
                Write(o.ToString());
            }
            else if (o is IEnumerable)
            {
                Write("…");
            }
            else
            {
                Write("{ }");
            }
        }
    }
}