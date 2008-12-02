using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;

namespace Glue.Lib.Text.Template
{
    public class Runtime
    {
        public static object Add(object x, object y)
        {
            if (x is Int32 && y is Int32)
                return (Int32)x + (Int32)y;
            if (x is Double && y is Double)
                return (Double)x + (Double)y;
            if (x is String || y is String)
                return String.Concat(x, y);
            if ((x is Double || x is Single || x is Decimal) &&
                (y is Double || y is Single || y is Decimal))
                return Convert.ToDouble(x) + Convert.ToDouble(y);
            if ((x is Int64 || x is Int32 || x is Int16) &&
                (y is Int64 || y is Int32 || y is Int16))
                return Convert.ToInt32(x) + Convert.ToInt32(y);
            return String.Concat(x, y);
        }

        public static object Sub(object x, object y)
        {
            if (x is Int32 && y is Int32)
                return (Int32)x - (Int32)y;
            if (x is Double && y is Double)
                return (Double)x - (Double)y;
            if ((x is Double || x is Single || x is Decimal) &&
                (y is Double || y is Single || y is Decimal))
                return Convert.ToDouble(x) - Convert.ToDouble(y);
            if ((x is Int64 || x is Int32 || x is Int16) &&
                (y is Int64 || y is Int32 || y is Int16))
                return Convert.ToInt32(x) - Convert.ToInt32(y);
            return "?";
        }

        public static object Div(object x, object y)
        {
            if (x is Int32 && y is Int32)
                return (Int32)x / (Int32)y;
            if (x is Double && y is Double)
                return (Double)x / (Double)y;
            if ((x is Double || x is Single || x is Decimal) &&
                (y is Double || y is Single || y is Decimal))
                return Convert.ToDouble(x) / Convert.ToDouble(y);
            if ((x is Int64 || x is Int32 || x is Int16) &&
                (y is Int64 || y is Int32 || y is Int16))
                return Convert.ToInt32(x) / Convert.ToInt32(y);
            return "?";
        }

        public static object Mul(object x, object y)
        {
            if (x is Int32 && y is Int32)
                return (Int32)x * (Int32)y;
            if (x is Double && y is Double)
                return (Double)x * (Double)y;
            if ((x is Double || x is Single || x is Decimal) &&
                (y is Double || y is Single || y is Decimal))
                return Convert.ToDouble(x) * Convert.ToDouble(y);
            if ((x is Int64 || x is Int32 || x is Int16) &&
                (y is Int64 || y is Int32 || y is Int16))
                return Convert.ToInt32(x) * Convert.ToInt32(y);
            return "?";
        }

        public static object Mod(object x, object y)
        {
            if (x is Int32 && y is Int32)
                return (Int32)x % (Int32)y;
            if ((x is Double || x is Single || x is Decimal) &&
                (y is Double || y is Single || y is Decimal))
                return Convert.ToDouble(x) % Convert.ToDouble(y);
            if ((x is Int64 || x is Int32 || x is Int16) &&
                (y is Int64 || y is Int32 || y is Int16))
                return Convert.ToInt32(x) % Convert.ToInt32(y);
            return "?";
        }

        public static object EQ(object x, object y)
        {
            if (x is IComparable)
                return (x as IComparable).Equals(y);
            if (y is IComparable)
                return (y as IComparable).Equals(x);
            return x == y;
        }

        public static object NE(object x, object y)
        {
            return !(bool)EQ(x,y);
        }

        public static object LT(object x, object y)
        {
            if (x is IComparable)
                return (x as IComparable).CompareTo(y) < 0;
            if (y is IComparable)
                return (y as IComparable).CompareTo(x) > 0;
            return false;
        }

        public static object LE(object x, object y)
        {
            return !(bool)GT(x,y);
        }

        public static object GT(object x, object y)
        {
            if (x is IComparable)
                return (x as IComparable).CompareTo(y) > 0;
            if (y is IComparable)
                return (y as IComparable).CompareTo(x) < 0;
            return false;
        }

        public static object GE(object x, object y)
        {
            return !(bool)LT(x,y);
        }

        public static object And(object a, object b)
        {
            return Test(a) && Test(b);
        }

        public static object Or(object a, object b)
        {
            return Test(a) || Test(b);
        }

        public static bool Test(object test)
        {
            if (test == null || test == DBNull.Value)
                return false;
            if (test is Boolean)
                return (Boolean)test;
            if (test is Int32 && (Int32)test == 0)
                return false;
            if (test is Double && (Double)test == 0)
                return false;
            if (test is String && ((String)test).Length == 0)
                return false;
            return true;
        }

        public static IDictionary Bag(params object[] namevals)
        {
            HybridDictionary bag = new HybridDictionary(false);
            for (int i = 0; i < namevals.Length - 1; i += 2)
                bag[namevals[i]] = namevals[i+1];
            return bag;
        }

        public static object IIf(bool test, object yes, object no)
        {
            return test ? yes : no;
        }
        
        public static object IIf(bool test, string yes, string no)
        {
            return test ? yes : no;
        }
        
        public static IEnumerable Range(object begin, object end)
        {
            return new Range((int)begin, (int)end);
        }

        public static string Join(string[] list)
        {
            if (list == null)
                return "";
            return string.Join(",", list);
        }

        public static object[] Split(string s)
        {
            if (s == null)
                return new object[] {};
            return s.Split(',');
        }

        public static IEnumerator GetEnumerator(object instance)
        {
            if (instance == null)
                return _emptyArgs.GetEnumerator();
            if (instance is IEnumerable)
                return (instance as IEnumerable).GetEnumerator();
            if (instance is System.Data.IDataReader)
                return new DataReaderEnumerator(instance as System.Data.IDataReader);
            return _emptyArgs.GetEnumerator();
        }
        
        static object[] _emptyArgs = new object[] {};

        public static object Get(object instance, string member, object[] args)
        {
            if (instance == null)
                return null;
            
            if (args == null)
                args = _emptyArgs;
            
            if (instance is StringTemplate)
            {
                object val = ((StringTemplate)instance).Get(member);
                if (val != null)
                    return val;
            }
            
            if (instance is IDictionary)
            {
                object val = ((IDictionary)instance)[member];
                if (val != null)
                    return val;
            }
            
            if (instance is NameValueCollection)
            {
                string val = ((NameValueCollection)instance)[member];
                if (val != null)
                    return val;
            }

            if (instance is System.Data.IDataRecord)
            {
                try   { return ((System.Data.IDataRecord)instance)[member]; }
                catch {}
            }
            
            Type type = instance as Type;
            if (type != null)
                instance = null;
            else
                type = instance.GetType();
            
            BindingFlags flags = 
                BindingFlags.IgnoreCase | BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | 
                BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.GetField | 
                BindingFlags.Public | 
                BindingFlags.Static | BindingFlags.Instance;
            if (ContainsMember(type, member, flags))
                try
                {
                    return type.InvokeMember(member, flags, null, instance, args);
                }
                catch (TargetInvocationException e)
                {
                    throw new TargetInvocationException("Error invoking: " + member + " on " + type + ": " + e.InnerException.Message, e.InnerException);
                }
            return null;
        }

        public static object GetWithBag(object instance, string member, IDictionary bag)
        {
            MethodInfo method = instance.GetType().GetMethod(member, BindingFlags.Instance | BindingFlags.Public);
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                if (bag.Contains(parameters[i].Name))
                    arguments[i] = bag[parameters[i].Name];
                else
                    arguments[i] = parameters[i].DefaultValue;
            return Get(instance, member, arguments);
        }

        public static void Set(object instance, string member, object value, object[] args)
        {
            if (instance == null)
                return;
            if (args == null)
                args = _emptyArgs;

            if (instance is StringTemplate)
            {
                (instance as StringTemplate).Set(member, value);
                return;
            }

            if (instance is IDictionary)
            {
                (instance as IDictionary)[member] = value;
                return;
            }
            
            if (instance is NameValueCollection)
            {
                (instance as NameValueCollection)[member] = Convert.ToString(value);
                return;
            }

            Type type = instance as Type;
            if (type != null)
                instance = null;
            else
                type = instance.GetType();

            BindingFlags flags = 
                BindingFlags.IgnoreCase | BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | 
                BindingFlags.InvokeMethod | BindingFlags.SetProperty | BindingFlags.SetField | 
                BindingFlags.Public | 
                BindingFlags.Static | BindingFlags.Instance;
            if (ContainsMember(type, member, flags))
                try
                {
                    type.InvokeMember(member, flags, null, instance, args);
                    return;
                }
                catch (TargetInvocationException)
                {
                }
            return;
        }

        public static object Invoke(object instance, string member, object[] args)
        {
            if (instance == null)
                return null;
            if (args == null)
                args = _emptyArgs;

            Type type = instance as Type;
            if (type != null)
                instance = null;
            else
                type = instance.GetType();

            BindingFlags flags = 
                BindingFlags.IgnoreCase | BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | 
                BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.GetField | 
                BindingFlags.Public | 
                BindingFlags.Static | BindingFlags.Instance;
            if (ContainsMember(type, member, flags))
                try
                {
                    return type.InvokeMember(member, flags, null, instance, args);
                }
                catch (TargetInvocationException)
                {
                }
            return null;
        }

        static bool ContainsMember(Type type, string member, BindingFlags bindingFlags)
        {
            MemberInfo[] members = type.FindMembers(
                MemberTypes.Method | MemberTypes.Property | MemberTypes.Field, 
                BindingFlags.IgnoreCase | bindingFlags,
                new MemberFilter(FindMemberByName), 
                member);
            if (members == null || members.Length == 0)
                return false;
            return true;
        }

        static bool FindMemberByName(MemberInfo info, Object data)
        {
            return (string.Compare(info.Name, (string)data, true) == 0);
        }
    }

    public class Range : IEnumerable
    {
        class RangeEnumerator : IEnumerator
        {
            int _begin;
            int _end;
            int _cur;
            public RangeEnumerator(int begin, int end)
            {
                _begin = begin;
                _end = end;
                _cur = begin - 1;
            }
            public void Reset()
            {
                _cur = _begin - 1;
            }
            public object Current
            {
                get { return _cur; }
            }
            public bool MoveNext()
            {
                _cur++;
                return (_cur <= _end);
            }
        }

        int _begin;
        int _end;
        public Range(int begin, int end)
        {
            _begin = begin;
            _end = end;
        }
        public IEnumerator GetEnumerator()
        {
            return new RangeEnumerator(_begin, _end);
        }
        public override string ToString()
        {
            return string.Concat("[", _begin, "..", _end, "]");
        }
    }

    public class DataReaderEnumerator: IEnumerator
    {
        System.Data.IDataReader _reader;
        
        public DataReaderEnumerator(System.Data.IDataReader reader)
        {
            _reader = reader;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public object Current
        {
            get { return _reader; }
        }

        public bool MoveNext()
        {
            return _reader.Read();
        }
    }
}