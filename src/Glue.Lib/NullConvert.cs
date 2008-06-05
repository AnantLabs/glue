using System;
using System.ComponentModel;

namespace Glue.Lib
{
	/// <summary>
	/// 
	/// </summary>
	public static class NullConvert
	{
        /// <summary>
        /// Checks if value is null or empty.
        /// Returns true if v is null or DBNull, or empty string or empty GUID.
        /// Returns false otherwise.
        /// </summary>
        public static bool IsNoE(object v)
        {
            if (v == null || v == DBNull.Value)
                return true;
            if (v is string)
                return string.Empty.Equals(v);
            if (v is Guid)
                return Guid.Empty.Equals(v);
            return false;
        }

        /// <summary>
        /// Returns true if v is null or DBNull, or empty string or empty GUID.
        /// Returns false otherwise.
        /// </summary>
        public static bool IsNullOrEmpty(object v)
        {
            if (v == null || v == DBNull.Value)
                return true;
            if (v is string)
                return string.Empty.Equals(v);
            if (v is Guid)
                return Guid.Empty.Equals(v);
            return false;
        }

        /// <summary>
        /// Returns the first non-null value in the argument list.
        /// </summary>
        public static object Coalesce(params object[] args)
        {
            if (args == null)
                return null;
            for (int i = 0; i < args.Length; i++)
                if (!IsNullOrEmpty(args[i]))
                    return args[i];
            return null;
        }

        /// <summary>
        /// Returns null if v is null or DBNull.
        /// </summary>
        public static string Coalesce(params string[] args)
        {
            if (args == null)
                return null;
            for (int i = 0; i < args.Length; i++)
                if (!IsNullOrEmpty(args[i]))
                    return args[i];
            return null;
        }
        /*
        /// <summary>
        /// Truncates a string to max length
        /// </summary>
        public static string Truncate(string s, int maxlength)
        {
            if (s == null || s.Length <= maxlength)
                return s;
            else
                return s.Substring(0, maxlength);
        }
        */
        /// <summary>
        /// Returns null if v is null or DBNull.
        /// </summary>
        public static object ToObject(object v)
        {
            if (v == null || v == DBNull.Value)
                return null;
            return (Object)v;
        }

        /// <summary>
        /// Returns null if v is null or DBNull, converts to string otherwise.
        /// </summary>
        public static String ToString(object v)
        {
            if (v == null || v == DBNull.Value)
                return null;
            return Convert.ToString(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to string otherwise.
        /// </summary>
        public static String ToString(object v, string defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToString(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Int16 otherwise.
        /// </summary>
        public static Int16 ToInt16(object v, Int16 defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToInt16(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Int32 otherwise.
        /// </summary>
        public static Int32 ToInt32(object v, Int32 defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToInt32(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Int64 otherwise.
        /// </summary>
        public static Int64 ToInt64(object v, Int64 defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToInt64(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Bool otherwise.
        /// </summary>
        public static Boolean ToBoolean(object v, Boolean defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            if (v is Boolean)
                return (Boolean)v;
            if (v is SByte || v is Int16 || v is Int32 || v is Int64)
                return ((Int64)v) != 0;
            if (v is Byte || v is UInt16 || v is UInt32 || v is UInt64)
                return ((UInt64)v) != 0;
            string s = v as string;
            if (s == null)
                return Convert.ToBoolean(v);
            s = s.ToLower();
            if (s == "1" || s == "-1" || s == "true" || s == "yes" || s == "on" || s == "t" || s == "y")
                return true;
            if (s == "0" || s == "false" || s == "no" || s == "off" || s == "f" || s == "n")
                return false;
            throw new InvalidCastException("Cannot convert " + v + "(" + v.GetType() + ") to Boolean.");
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Bool otherwise.
        /// </summary>
        public static Boolean ToBoolean(object v, int defaultValue)
        {
            return ToBoolean(v, defaultValue != 0);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Byte otherwise.
        /// </summary>
        public static Byte ToByte(object v, Byte defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToByte(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to DateTime otherwise.
        /// </summary>
        public static DateTime ToDateTime(object v, DateTime defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToDateTime(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Single otherwise.
        /// </summary>
        public static Single ToSingle(object v, Single defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToSingle(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Double otherwise.
        /// </summary>
        public static Double ToDouble(object v, Double defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToDouble(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Decimal otherwise.
        /// </summary>
        public static Decimal ToDecimal(object v, Decimal defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToDecimal(v);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to Guid otherwise.
        /// </summary>
        public static Guid ToGuid(object v, Guid defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            string s = v as String;
            if (s != null)
                return new Guid(s);
            return (Guid)v;
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to given enumType otherwise.
        /// </summary>
        public static object ToEnum(object v, object defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Enum.Parse(defaultValue.GetType(), v.ToString(), true);
        }
    
        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to given enumType otherwise.
        /// </summary>
        public static object ToEnum(Type enumType, object v, object defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Enum.Parse(enumType, v.ToString(), true);
        }

        /// <summary>
        /// Returns defaultValue is v is null or DBNull, converts to char otherwise.
        /// </summary>
        public static char ToChar(object v, char defaultValue)
        {
            if (v == null || v == DBNull.Value)
                return defaultValue;
            return Convert.ToChar(v);
        }

        /// <summary>
        /// Special version for generic nullable types, which don't handle
        /// DBNull at all.
        /// </summary>
        public static T To<T>(object value)
        {
            if (value == null || value == DBNull.Value)
                return default(T);

            Type type = typeof(T);
            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];

            // Can we cast directly, for instance Int to Int, or Int to Int? (Nullable<Int>)?
            if (type == value.GetType()) 
                return (T)value;
            else
            {
                try
                {
                    // Casting an Int32 to an Int16? (Nullable<Int16>) doesn't work, 
                    // although casting an Int32 to an Int16 (normal short) works fine.
                    // Same goes for conversion from string to char etc. 
                    
                    // So we need some special conversions...
                    if (type == typeof(bool)) 
                    {
                        bool v = Convert.ToBoolean(value);
                        return (T)(object) v;
                    }
                    else if (type == typeof(short))
                    {
                        short v = Convert.ToInt16(value);
                        return (T)(object)v;
                    }
                    else if (type == typeof(int))
                    {
                        int v = Convert.ToInt32(value);
                        return (T)(object) v;
                    }
                    else if (type == typeof(long))
                    {
                        long v = Convert.ToInt64(value);
                        return (T)(object)v;
                    }
                    else if (type == typeof(double))
                    {
                        double v = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                        return (T)(object)v;
                    }
                    else if (type == typeof(float))
                    {
                        float v = (float)Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                        return (T)(object)v;
                    }
                    else if (type == typeof(Guid))
                    {
                        Guid v = (Guid) (value);
                        return (T)(object)v;
                    }
                    else if (type == typeof(DateTime))
                    {
                        DateTime v = Convert.ToDateTime(value, System.Globalization.CultureInfo.InvariantCulture);
                        return (T)(object)v;
                    }
                    else if (type == typeof(decimal))
                    {
                        decimal v = Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
                        return (T)(object)v;
                    }
                    else if (type == typeof(char))
                    {
                        char v = Convert.ToChar(value);
                        return (T)(object)v;
                    }
                    else
                        return (T)value; 
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException("Error converting type '" + value.GetType().ToString()
                        + "' to '" + typeof(T).ToString() + "'.", e);

                }
            }
        }

        /// <summary>
        /// Returns DBNull if v is null, or v otherwise
        /// </summary>
        public static object From(object v)
        {
            if (v == null)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is null;
        /// </summary>
        public static object From(string v)
        {
            if (v == null)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is null, or v otherwise
        /// </summary>
        public static object From(Array v)
        {
            if (v == null)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Int16 v, Int16 nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }
        
        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Int32 v, Int32 nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }
        
        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Int64 v, Int64 nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }
        
        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Boolean v, Boolean nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }
        
        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Byte v, Byte nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }
        
        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(DateTime v, DateTime nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Single v, Single nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Double v, Double nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Decimal v, Decimal nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(Guid v, Guid nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Returns DBNull if v is conventional null value, or v otherwise
        /// </summary>
        public static object From(char v, char nullval)
        {
            if (v == nullval)
                return DBNull.Value;
            return v;
        }

        /// <summary>
        /// Special version for generic nullable types, which don't handle
        /// DBNull at all.
        /// </summary>
        public static object From<T>(T value)
        {
            if (value == null)
                return DBNull.Value;
            return value;
        }
    }
}
