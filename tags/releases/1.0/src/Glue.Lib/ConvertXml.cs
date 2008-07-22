using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Glue.Lib
{
    public class ConvertXml
    {


        /// <summary>
        /// Convert XmlNode to string
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>String</returns>
        /// <remarks>
        /// Converts XmlAttribute or XmlElement values to string. If the node is <code>null</code>, an exception is thrown.
        /// </remarks>
        public static string ToString(XmlNode node)
        {
            return node.Value;
        }

        /// <summary>
        /// Convert XmlNode to string
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="_default">Default value</param>
        /// <returns>String</returns>
        /// <remarks>
        /// Converts XmlAttribute or XmlElement values to string. If the node is <code>null</code>, the default value is returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// // element does not contain attribute "emptyattribute"
        /// string defaultValue = ConvertXml.ToString(element.Attributes["emptyattribute"], "default");
        /// // defaultValue = "default";
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// // element contains empty attribute "emptyattribute" 
        /// element.Attributes["emptyattribute"] = "";
        /// string defaultValue = ConvertXml.ToString(element.Attributes["emptyattribute"], "default");
        /// // defaultValue = "default";
        /// </code>
        /// </example>
        public static string ToString(XmlNode node, string _default)
        {
            if ((node == null) || (node.Value == null) || (node.Value == String.Empty))
                return _default;
            return node.Value;
        }

        /// <summary>
        /// Converts an XmlNode value to an int. If the conversion fails, an exception is thrown.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <returns>int</returns>
        /// <example>
        /// <code>
        /// int n = ConvertXml.ToInt32(element.Attributes["intsetting"]);
        /// </code>
        /// </example>
        public static Int32 ToInt32(XmlNode node)
        {
            return Convert.ToInt32(node.Value);
        }

        /// <summary>
        /// Converts an XmlNode value to an int. If the conversion fails, a default value is returned.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="_default">Default return value</param>
        /// <returns>int</returns>
        /// <example>
        /// <code>
        /// // element.Attributes["intsetting"] contains "This is no int..."
        /// int n = ConvertXml.ToInt32(element.Attributes["intsetting"], 0);
        /// // n = 0
        /// </code>
        /// </example>
        public static int ToInt32(XmlNode node, int _default)
        {
            try
            {
                return ToInt32(node);
            }
            catch
            {
                return _default;
            }
        }

        /// Wok a dok.
        public static Nullable<int> ToNullableInt32(XmlNode node)
        {
            return Convert.ToInt32(node.Value);
        }

        /// <summary>
        /// Converts an XmlNode value to an int. If the conversion fails, a default value is returned.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="_default">Default return value</param>
        /// <returns>int</returns>
        /// <example>
        /// <code>
        /// // element.Attributes["intsetting"] contains "This is no int..."
        /// int n = ConvertXml.ToInt32(element.Attributes["intsetting"], 0);
        /// // n = 0
        /// </code>
        /// </example>
        public static Nullable<int> ToNullableInt32(XmlNode node, Nullable<int> _default)
        {
            try
            {
                return ToNullableInt32(node);
            }
            catch
            {
                return _default;
            }
        }


        /// <summary>
        /// Converts an XmlNode value to a boolean. If the conversion fails, an exception is thrown.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <returns>bool</returns>
        /// <example>
        /// <code>
        /// bool b = ConvertXml.ToBoolean(element.Attributes["boolsetting"]);
        /// </code>
        /// </example>
        /// <remarks>
        /// Yes, 1, -1, true and on are converted to <code>true</code>. 
        /// No, 0, false and off are converted to <code>false</code>.
        /// The function is case-insensitive.
        /// </remarks>
        public static bool ToBoolean(XmlNode node)
        {
            return NullConvert.ToBoolean(node.Value, false);
            //switch (node.Value.ToLower())
            //{
            //    case "yes" :
            //    case "y":
            //    case "1":
            //    case "-1" :
            //    case "true" :
            //    case "t":
            //    case "on":
            //        return true;

            //    case "no" :
            //    case "n":
            //    case "0":
            //    case "false" :
            //    case "f":
            //    case "off":
            //        return false;

            //    default :
            //        throw new InvalidCastException("Cannot convert '" + node.Value + "' to Boolean.");
            //}
        }

        /// <summary>
        /// Converts an XmlNode value to a boolean. If the conversion fails, the default value is returned.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="_default">Default value</param>
        /// <returns>bool</returns>
        /// <example>
        /// <code>
        /// bool b = ConvertXml.ToBoolean(element.Attributes["boolsetting"]);
        /// </code>
        /// </example>
        /// <remarks>
        /// Yes, 1, -1, true and on are converted to <code>true</code>. 
        /// No, 0, false and off are converted to <code>false</code>.
        /// The function is case-insensitive.
        /// </remarks>
        public static bool ToBoolean(XmlNode node, bool _default)
        {
            return NullConvert.ToBoolean(node.Value, _default);
            //try
            //{
            //    return ToBoolean(node);
            //}
            //catch
            //{
            //    return _default;
            //}
        }

        /// <summary>
        /// Converts an XmlNode value to an enum. If the conversion fails, an exception is thrown.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="enumType">Enum type</param>
        /// <returns>enum</returns>
        /// <example>
        /// <code>
        /// MyEnum en = (MyEnum) ConvertXml.ToEnum(element.Attributes["enumsetting"], typeof(MyEnum));
        /// </code>
        /// </example>
        /// <remarks>
        /// The function is case-insensitive.
        /// </remarks>
        public static object ToEnum(XmlNode node, Type enumType)
        {
            return Enum.Parse(enumType, node.Value, true);
        }

        /// <summary>
        /// Converts an XmlNode value to an enum. If the conversion fails, the default value is returned.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="enumType">Enum type</param>
        /// <param name="_default">Default value</param>
        /// <returns>enum</returns>
        /// <example>
        /// <code>
        /// MyEnum en = (MyEnum) ConvertXml.ToEnum(element.Attributes["enumsetting"], typeof(MyEnum), MyEnum.Default);
        /// </code>
        /// </example>
        /// <remarks>
        /// The function is case-insensitive.
        /// </remarks>
        public static object ToEnum(XmlNode node, Type enumType, object _default)
        {
            try
            {
                return ToEnum(node, enumType);
            }
            catch
            {
                return _default;
            }
        }


        public static DateTime ToDateTime(XmlNode node)
        {
            return Convert.ToDateTime(node.Value);
        }

        public static DateTime ToDateTime(XmlNode node, DateTime _default)
        {
            DateTime retval = _default;
            try
            {
                retval = ToDateTime(node);
            }
            catch
            {
            }
            return retval;
        }

        public static Guid ToGuid(XmlNode node)
        {
            return new Guid(node.Value);
        }

        public static Guid ToGuid(XmlNode node, Guid _default)
        {
            Guid retval = _default;
            try
            {
                retval = ToGuid(node);
            }
            catch
            { 
            }
            return retval;
        }
    }
}
