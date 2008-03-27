using System;

namespace Glue.Data
{
    /// <summary>
    /// Specifies a range of rows to limit the result set of a query.
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

        public Limit(int index, int count)
        {
            this.Index = index;
            this.Count = count;
        }
        public override string ToString()
        {
            return string.Concat(Index, " +", Count);
        }
        
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
        
        public static Limit New(int index, int count)
        {
            return new Limit(index, count);
        }
        
        /// <summary>
        /// Returns the first non-empty Limit in the argument list.
        /// </summary>
        /// <param name="limits"></param>
        /// <returns></returns>
        public static Limit Coalesce(params Limit[] limits)
        {
            Limit result = Limit.Unlimited;
            foreach (Limit limit in limits)
                if (limit != null)
                    return limit;
            return result;
        }

        public static Limit Top(int count)
        {
            return new Limit(0, count);
        }

        /// <summary>
        /// A Limit to return rows between rows 'from' and 'to'
        /// </summary>
        /// <param name="from">First row in the result set</param>
        /// <param name="to">First row after (and not included in) the result set</param>
        /// <returns></returns>
        public static Limit Range(int from, int to)
        {
            return new Limit(from, from < to ? to - from : 0);
        }
    }
}
