using System;
using System.Data;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Schema
{
    public class Helper
    {
        public static Type GetSystemType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:                 return typeof(String);
                case DbType.AnsiStringFixedLength:      return typeof(String);
                case DbType.Binary:                     return typeof(byte[]);
                case DbType.Boolean:                    return typeof(bool);
                case DbType.Byte:                       return typeof(byte);
                case DbType.Currency:                   return typeof(Decimal);
                case DbType.Date:                       return typeof(DateTime);
                case DbType.DateTime:                   return typeof(DateTime);
                case DbType.Decimal:                    return typeof(Decimal);
                case DbType.Double:                     return typeof(double);
                case DbType.Guid:                       return typeof(Guid);
                case DbType.Int16:                      return typeof(Int16);
                case DbType.Int32:                      return typeof(Int32);
                case DbType.Int64:                      return typeof(Int64);
                case DbType.Object:                     return typeof(object);
                case DbType.SByte:                      return typeof(SByte);
                case DbType.Single:                     return typeof(Single);
                case DbType.String:                     return typeof(String);
                case DbType.StringFixedLength:          return typeof(String);
                case DbType.Time:                       return typeof(DateTime);
                case DbType.UInt16:                     return typeof(UInt16);
                case DbType.UInt32:                     return typeof(UInt32);
                case DbType.UInt64:                     return typeof(UInt64);
                case DbType.VarNumeric:                 return typeof(byte[]);
            }
            throw new ArgumentException("Unknown type " + dbType);
        }

        public static bool IsIntegerType(Type type)
        {
            return type == typeof(Int32) || type == typeof(Int64) || type == typeof(Int16) || type == typeof(SByte) ||
                   type == typeof(UInt32) || type == typeof(UInt64) || type == typeof(UInt16) || type == typeof(Byte);
        }

        public static bool IsRealType(Type type)
        {
            return type == typeof(Double) || type == typeof(Single) || type == typeof(Decimal);
        }

        public static DbType GetDataType(Type systemType)
        {
            if (systemType == typeof(String))
                return DbType.String;
            else if (systemType == typeof(Byte[]))
                return DbType.Binary;
            else if (systemType == typeof(Boolean))
                return DbType.Boolean;
            else if (systemType == typeof(SByte))
                return DbType.SByte;
            else if (systemType == typeof(Int16))
                return DbType.Int16;
            else if (systemType == typeof(Int32))
                return DbType.Int32;
            else if (systemType == typeof(Int64))
                return DbType.Int64;
            else if (systemType == typeof(Byte))
                return DbType.Byte;
            else if (systemType == typeof(UInt16))
                return DbType.UInt16;
            else if (systemType == typeof(UInt32))
                return DbType.UInt32;
            else if (systemType == typeof(UInt64))
                return DbType.UInt64;
            else if (systemType == typeof(DateTime))
                return DbType.DateTime;
            else if (systemType == typeof(Decimal))
                return DbType.Decimal;
            else if (systemType == typeof(Single))
                return DbType.Single;
            else if (systemType == typeof(Double))
                return DbType.Double;
            else if (systemType == typeof(Guid))
                return DbType.Guid;
            else
                throw new ArgumentException("Unknown type " + systemType);
        }

        public static string SimpleEncode(Type type, object value)
        {
            if (value == null || value == DBNull.Value)
                return "";
            if (type == null)
                type = value.GetType();
            if (type == typeof(string))
                return '"' + StringHelper.EscapeCStyle((string)value) + '"';
            else if (Helper.IsIntegerType(type))
                return Convert.ToString(value);
            else if (Helper.IsRealType(type))
                return Convert.ToString(value);
            else if (type == typeof(bool))
                return Convert.ToString(value);
            else if (type == typeof(byte[]))
                return string.Concat("#", BinHexEncoding.Encode((byte[])value));
            else if (type == typeof(DateTime))
                return ((DateTime)value).ToString("s");
            else if (type == typeof(Guid))
                return ((Guid)value).ToString("B");
            else 
                return '"' + StringHelper.EscapeCStyle(value.ToString()) + '"';
        }

        private static System.Text.RegularExpressions.Regex regexDate = 
            new System.Text.RegularExpressions.Regex("[0-9][0-9][0-9][0-9][-/][0-9][0-9][-/][0-9][0-9].*", 
            System.Text.RegularExpressions.RegexOptions.Compiled);
        private static System.Text.RegularExpressions.Regex regexReal = 
            new System.Text.RegularExpressions.Regex("[0-9]*\\.[0-9]+.*", 
            System.Text.RegularExpressions.RegexOptions.Compiled);
        
        private static string[] dateTimeFormats = {"r", "s", "u", "yyyyMMddTHHmmss"};

        public static object SimpleDecode(Type type, string s)
        {
            if (s == null || s == "")
                return null;
            if (type == null)
            {
                if (s[0] == '"') 
                    type = typeof(String);
                else if (regexDate.IsMatch(s))
                    type = typeof(DateTime);
                else if (regexReal.IsMatch(s))
                    type = typeof(Double);
                else if (s[0] >= '0' && s[0] <= '9')
                    type = typeof(Int32);
                else if (s[0] == '{')
                    type = typeof(Guid);
                else if (s[0] == '#')
                    type = typeof(byte[]);
            }
            if (type == typeof(string))
                return StringHelper.UnEscapeCStyle(s.Substring(1, s.Length-2));
            if (type == typeof(Guid))
                return new Guid(s);
            if (type == typeof(DateTime))
                return DateTime.ParseExact(s, dateTimeFormats, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
            if (type == typeof(Boolean))
                return NullConvert.ToBoolean(s, false);
            if (type == typeof(Int32))
                return Convert.ToInt32(s);
            if (type == typeof(Int64))
                return Convert.ToInt64(s);
            if (type == typeof(Int16))
                return Convert.ToInt16(s);
            if (type == typeof(SByte))
                return Convert.ToSByte(s);
            if (type == typeof(UInt32))
                return Convert.ToUInt32(s);
            if (type == typeof(UInt64))
                return Convert.ToUInt64(s);
            if (type == typeof(Byte))
                return Convert.ToByte(s);
            if (type == typeof(UInt16))
                return Convert.ToUInt16(s);
            if (type == typeof(Double))
                return Convert.ToDouble(s);
            if (type == typeof(Single))
                return Convert.ToSingle(s);
            if (type == typeof(Decimal))
                return Convert.ToDecimal(s);
            if (type == typeof(byte[]))
                return BinHexEncoding.Decode(s.ToCharArray(1, s.Length-1));
            throw new ArgumentException("Cannot convert value", "s");
        }
    }
}
