using System;

namespace Glue.Data
{
	/// <summary>
	/// A filter for data returned from a database query, corresponding to the 'WHERE' clause in SQL.
	/// </summary>
    public class Filter
    {
        string expression;

        private Filter()
        {
        }

        /// <summary>
        /// Returns new Filter where and fills parameters in expression with placeholders.
        /// </summary>
        /// <remarks>
        /// <code>
        ///   Filter f = new Filter("City=@0 AND Age=@1 AND BirthDate>@2", "Amsterdam", 20, DateTime.Now)
        /// </code>
        /// returns
        /// <code>
        ///   City='Amsterdam' and Age=20 and BirthDate>'2005-11-19 15:54:00'
        /// </code>
        /// Strings will be correctly quoted, dates will have ISO representation, so they'll
        /// always work.
        /// </remarks>
        public Filter(string expr, params object[] parms)
        {
            // check if we have parameters
            if ((parms != null) && (parms.Length > 0))
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                int last = 0;
                int i = 0;
                int n = expr.Length;
                while (i < n)
                {
                    if (expr[i] == '@')
                    {
                        int index = 0;
                        int j = i;
                        while (++j < n && expr[j] >= '0' && expr[j] <= '9')
                            index = index * 10 + (byte)expr[j] - (byte)'0';
                        if (j == i + 1)
                        {
                            if (j >= n || expr[j] != '@')
                                throw new ArgumentException("Expected parameter number or extra '@' after '@'");
                            s.Append(expr, last, i - last);
                            j++;
                        }
                        else
                        {
                            s.Append(expr, last, i - last);
                            s.Append(ToSql(parms[index]));
                        }
                        i = last = j;
                    }
                    else
                    {
                        i++;
                    }
                }
                if (last < n)
                    s.Append(expr, last, n - last);

                this.expression = s.ToString();
            }
            else
            {
                this.expression = expr;
            }
        }

        public override string ToString()
        {
            return expression;
        }
        
        public bool IsEmpty
        {
            get { return expression == null || expression.Length == 0; }
        }
        
        public string ToSql()
        {
            if (IsEmpty)
                return string.Empty;
            else
                return "WHERE " + ToString();
        }
        
        public static implicit operator Filter(string expression)
        {
            return new Filter(expression);
        }

        /// <summary>
        /// Returns the first non-empty Filter from the argument list.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static Filter Coalesce(params Filter[] filters)
        {
            Filter result = Filter.Empty;
            foreach (Filter f in filters)
                if (f != null && !f.IsEmpty)
                    return f;
            return result;
        }

        public static readonly Filter Empty = new Filter(null);

        /// <summary>
        /// Returns new Filter where and fills parameters in expression with placeholders.
        /// </summary>
        /// <remarks>
        /// <code>
        ///   Filter f = new Filter("City=@0 AND Age=@1 AND BirthDate>@2", "Amsterdam", 20, DateTime.Now)
        /// </code>
        /// returns
        /// <code>
        ///   City='Amsterdam' and Age=20 and BirthDate>'2005-11-19 15:54:00'
        /// </code>
        /// Strings will be correctly quoted, dates will have ISO representation, so they'll
        /// always work.
        /// </remarks>
        public static Filter Create(string expr, params object[] parms)
        {
            return new Filter(expr, parms);
        }

        /// <summary>
        /// Convert value to safe SQL string. 
        /// </summary>
        public static string ToSql(object v)
        {
            if (v == null)
                throw new ArgumentException("Cannot convert null to a SQL constant.");
            Type t = v.GetType();
            if (t == typeof(String))
                return "'" + ((String)v).Replace("'","''") + "'";
            if (t == typeof(Boolean))
                return (Boolean)v ? "1" : "0";
            if (t == typeof(Char))
                return (Char)v == '\'' ? "''''" : "'" + (Char)v + "'";
            if (t == typeof(Int32))
                return ((Int32)v).ToString();
            if (t == typeof(Byte))
                return ((Byte)v).ToString();
            if (t.IsPrimitive)
                return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(Guid))
                return "'{" + ((Guid)v).ToString("D") + "}'";
            if (t == typeof(DateTime))
                return "'" + ((DateTime)v).ToString("yyyy'-'MM'-'dd HH':'mm':'ss':'fff") + "'";
            throw new ArgumentException("Cannot convert type " + t + " to a SQL constant.");
        }

        // public static Filter And(string op1, Filter op2) { return Filter.And((Filter)op1, op2); }
        // public static Filter Or(string op1, Filter op2) { return Filter.Or((Filter)op1, op2); }

        /// <summary>
        /// Combines two Filters with a logical AND.
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <returns></returns>
        public static Filter And(Filter op1, Filter op2)
        {
            if (op2 == null || op2.IsEmpty)
                return op1;
            if (op1 == null || op1.IsEmpty)
                return op2;
            return new Filter("(" + op1.expression + ") AND (" + op2.expression + ")");
        }
        
        /// <summary>
        /// Combines two Filters with a logical OR.
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <returns></returns>
        public static Filter Or(Filter op1, Filter op2)
        {
            if (op2 == null || op2.IsEmpty)
                return op1;
            if (op1 == null || op1.IsEmpty)
                return op2;
            return new Filter("(" + op1.expression + ") OR (" + op2.expression + ")");
        }
    }
}
