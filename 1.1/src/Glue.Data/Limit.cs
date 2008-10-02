using System;

namespace Glue.Data
{
    /// <summary>
    /// Specifies a range of rows to limit the result set of a query. A Limit contains an Index (the first row 
    /// to be returned) and a Count (the maximum number of rows). A Count of -1 means "unlimited".
    /// For methods that take a Limit as an argument (like those in IDataProvider), passing 'null' is equivalent
    /// to passing Limit.Unlimited.
    /// </summary>
    public class Limit 
    {
        /// <summary>
        /// The row number of the first row in the result set.
        /// </summary>
        public readonly int Index = 0;

        /// <summary>
        /// The maximum number of returned rows. Set to -1 for an unlimited number of rows.
        /// </summary>
        public readonly int Count = -1;

        /// <summary>
        /// Create new Limit
        /// </summary>
        /// <param name="index">Index row</param>
        /// <param name="count">Number of rows to return</param>
        public Limit(int index, int count)
        {
            this.Index = index;
            this.Count = count;
        }

        public override string ToString()
        {
            return string.Concat(Index, " +", Count);
        }
        
        /// <summary>
        /// True if Limit is set to Unlimited.
        /// </summary>
        public bool IsUnlimited
        {
            get { return Index == 0 && Count == -1; }
        }

        /// <summary>
        /// Limit to return all rows.
        /// </summary>
        public static readonly Limit Unlimited = new Limit(0, -1);

        /// <summary>
        /// Limit to return only the first row from a result set.
        /// </summary>
        public static readonly Limit One = new Limit(0, 1);
        
        /// <summary>
        /// Create new Limit
        /// </summary>
        /// <param name="index">Index row</param>
        /// <param name="count">Number of rows to return</param>
        /// <returns>New Limit instance</returns>
        public static Limit Create(int index, int count)
        {
            return new Limit(index, count);
        }
        
        /// <summary>
        /// Returns the first non-empty Limit in the argument list.
        /// </summary>
        /// <param name="limits">Limits</param>
        /// <returns>First non-empty Limit in the argument list</returns>
        public static Limit Coalesce(params Limit[] limits)
        {
            Limit result = Limit.Unlimited;
            foreach (Limit limit in limits)
                if (limit != null)
                    return limit;
            return result;
        }

        /// <summary>
        /// Limit to return the top 'count' rows.
        /// </summary>
        /// <param name="count">Number of rows to return</param>
        /// <returns>New Limit-instance</returns>
        public static Limit Top(int count)
        {
            return new Limit(0, count);
        }

        /// <summary>
        /// A Limit to return rows between rows 'from' and 'to'
        /// </summary>
        /// <param name="from">First row in the result set</param>
        /// <param name="to">First row after (and not included in) the result set</param>
        /// <returns>New Limit-instance</returns>
        public static Limit Range(int from, int to)
        {
            return new Limit(from, from < to ? to - from : 0);
        }
    }
}
