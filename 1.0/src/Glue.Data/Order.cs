using System;
using System.Text;

namespace Glue.Data
{
	/// <summary>
    /// An Order for data returned from a database query, corresponding to the 'ORDER BY' clause in SQL.
	/// </summary>
    /// <remarks>
    /// An Order can be constructed in the following ways:
    /// <code>
    /// Order o = new Order("-Name", "+Age");
    /// </code>
    /// <code>
    /// Order o = new Order("Name DESC", "Age ASC");
    /// </code>
    /// <code>
    /// Order o = new Order("-Name, +Age");    
    /// </code>
    /// An Order can also be constructed by casting: 
    /// <code>
    /// Order o = (Order)"-Name,+Age";
    /// </code>
    /// This is handy in List-methods:
    /// <code>
    /// List.All("-Name");
    /// </code>
    /// </remarks>
    public class Order
    {
        /// <summary>
        /// Returns empty Order. No ordering is done.
        /// </summary>
        public static Order Empty = new Order((string[])null);

        /// <summary>
        /// Create Order from string
        /// </summary>
        /// <param name="order">Ordering</param>
        /// <returns>New Order instance</returns>
        /// <remarks>
        /// An Order can be constructed in the following ways:
        /// <code>
        /// Order o = new Order("-Name", "+Age");
        /// </code>
        /// <code>
        /// Order o = new Order("Name DESC", "Age ASC");
        /// </code>
        /// <code>
        /// Order o = new Order("-Name, +Age");    
        /// </code>
        /// An Order can also be constructed by casting: 
        /// <code>
        /// Order o = (Order)"-Name,+Age";
        /// </code>
        /// This is handy in List-methods:
        /// <code>
        /// List.All("-Name");
        /// </code>
        /// </remarks>
        public static implicit operator Order(string order)
        {
            return new Order(order);
        }

        /// <summary>
        /// Returns the first non-empty Order in the argument list.
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public static Order Coalesce(params Order[] orders)
        {
            Order result = Order.Empty;
            foreach (Order o in orders)
                if (o != null && !o.IsEmpty)
                    return o;
            return result;
        }

        string[] terms;

        private Order()
        {
        }

        /// <summary>
        /// Create Order from string
        /// </summary>
        /// <param name="order"></param>
        /// <remarks>
        /// An Order can be constructed in the following ways:
        /// <code>
        /// Order o = new Order("-Name", "+Age");
        /// </code>
        /// <code>
        /// Order o = new Order("Name DESC", "Age ASC");
        /// </code>
        /// <code>
        /// Order o = new Order("-Name, +Age");    
        /// </code>
        /// An Order can also be constructed by casting: 
        /// <code>
        /// Order o = (Order)"-Name,+Age";
        /// </code>
        /// This is handy in List-methods:
        /// <code>
        /// List.All("-Name");
        /// </code>
        /// </remarks>
        public Order(string order) : this(order != null && order.Length > 0 ? order.Split(',') : null)
        {
        }

        /// <summary>
        /// Create Order from string
        /// </summary>
        /// <param name="order"></param>
        /// <remarks>
        /// An Order can be constructed in the following ways:
        /// <code>
        /// Order o = new Order("-Name", "+Age");
        /// </code>
        /// <code>
        /// Order o = new Order("Name DESC", "Age ASC");
        /// </code>
        /// <code>
        /// Order o = new Order("-Name, +Age");    
        /// </code>
        /// An Order can also be constructed by casting: 
        /// <code>
        /// Order o = (Order)"-Name,+Age";
        /// </code>
        /// This is handy in List-methods:
        /// <code>
        /// List.All("-Name");
        /// </code>
        /// </remarks>
        public static Order Create(string order)
        {
            return new Order(order);
        }

        /// <summary>
        /// Create new Order 
        /// </summary>
        /// <param name="args">Ordering arguments</param>
        /// <remarks>
        /// An Order can be constructed in the following ways:
        /// <code>
        /// Order o = new Order("-Name", "+Age");
        /// </code>
        /// <code>
        /// Order o = new Order("Name DESC", "Age ASC");
        /// </code>
        /// <code>
        /// Order o = new Order("-Name, +Age");    
        /// </code>
        /// An Order can also be constructed by casting: 
        /// <code>
        /// Order o = (Order)"-Name,+Age";
        /// </code>
        /// This is handy in List-methods:
        /// <code>
        /// List.All("-Name");
        /// </code>
        /// </remarks>
        public Order(string[] args)
        {
            int n = args != null ? args.Length : 0;
            terms = new string[n];
            for (int i = 0; i < n; i++)
                terms[i] = NormalizeTerm(args[i]);
        }

        /// <summary>
        /// Create new Order 
        /// </summary>
        /// <param name="args">Ordering arguments</param>
        /// <remarks>
        /// An Order can be constructed in the following ways:
        /// <code>
        /// Order o = new Order("-Name", "+Age");
        /// </code>
        /// <code>
        /// Order o = new Order("Name DESC", "Age ASC");
        /// </code>
        /// <code>
        /// Order o = new Order("-Name, +Age");    
        /// </code>
        /// An Order can also be constructed by casting: 
        /// <code>
        /// Order o = (Order)"-Name,+Age";
        /// </code>
        /// This is handy in List-methods:
        /// <code>
        /// List.All("-Name");
        /// </code>
        /// </remarks>
        public static Order Create(string[] args)
        {
            return new Order(args);
        }

        /// <summary>
        /// True if the Order is empty, i.e. does no ordering at all.
        /// </summary>
        public bool IsEmpty
        {
            get { return terms == null || terms.Length == 0; }
        }

        /// <summary>
        /// The number of columns in the Order clause.
        /// </summary>
        public int Count
        {
            get { return terms.Length; }
        }

        public string this[int i]
        {
            get { return terms[i]; }
        }

        public bool Contains(string term)
        {
            return IndexOf(term) >= 0;
        }

        public int IndexOf(string term)
        {
            term = NormalizeTerm(term);
            for (int i = 0; i < terms.Length; i++)
                if (string.Compare(terms[i], 1, term, 1, int.MaxValue, true) == 0)
                    return i;
            return -1;
        }

        public int GetDirection(int i)
        {
            return terms[i][0] == '-' ? -1 : +1; 
        }

        public int GetDirection(string term)
        {
            int i = IndexOf(term);
            if (i < 0)
                return 0;
            return terms[i][0] == '-' ? -1 : +1; 
        }

        public Order Remove(int index)
        {
            return new Order(ArrDel(terms, index));
        }
        
        public Order Append(string term)
        {
            return Insert(terms.Length, term);
        }

        public Order Insert(int index, string term)
        {
            string[] newterms = ArrIns(terms, index);
            newterms[index] = NormalizeTerm(term);
            return new Order(newterms);
        }
        
        public Order Inverse()
        {
            Order result = new Order();
            result.terms = new string[this.terms.Length];
            for (int i = 0; i < result.terms.Length; i++)
                result.terms[i] = (this.terms[i][0] == '+' ? '-' : '+') + this.terms[i].Substring(1);
            return result;
        }

        public Order Toggle(string term)
        {
            term = NormalizeTerm(term);
            int index = IndexOf(term);
            Order result = new Order();
            if (index < 0)
            {
                result.terms = new string[this.terms.Length + 1];
                result.terms[0] = "+" + term.Substring(1);
                Array.Copy(this.terms, 0, result.terms, 1, this.terms.Length);
            }
            else
            {
                result.terms = new string[this.terms.Length];
                result.terms[0] = (this.terms[index][0] == '+' ? '-' : '+') + this.terms[index].Substring(1);
                if (index > 0)
                    Array.Copy(this.terms, 0, result.terms, 1, index);
                if (index + 1 < this.terms.Length)
                    Array.Copy(this.terms, index + 1, result.terms, index + 1, this.terms.Length - index - 1);
            }
            return result;
        }

        public override string ToString()
        {
            int n = terms.Length;
            if (n == 0)
                return "";

            StringBuilder s = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                if (i > 0)
                    s.Append(',');
                s.Append(terms[i].Substring(1));
                if (terms[i][0] == '-')
                    s.Append(" desc");
            }
            return s.ToString();
        }

        public string ToSql()
        {
            if (IsEmpty)
                return "";
            else
                return "ORDER BY " + ToString();
        }

        private string NormalizeTerm(string s)
        {
            s = s.Trim();
            if (s[0] == '+' || s[0] == '-')
                return s;

            string a = s.ToLower();
            if (a.EndsWith(" asc"))
                return '+' + s.Substring(0, s.Length - 4);

            if (a.EndsWith(" desc"))
                return '-' + s.Substring(0, s.Length - 5);
            
            return '+' + s;
        }

        private string[] ArrIns(Array list, int index)
        {
            string[] result = new string[list.Length + 1];
            if (index > 0)
                Array.Copy(list, 0, result, 0, index);
            result[index] = null;
            if (index < list.Length)
                Array.Copy(list, index, result, index + 1, list.Length - index);
            return result;
        }

        private string[] ArrDel(string[] list, int index)
        {
            string[] result = new string[list.Length - 1];
            Array.Copy(list, 0, result, 0, index);
            if (index < result.Length)
                Array.Copy(list, index + 1, result, index, result.Length - index);
            return result;
        }

    }
}
