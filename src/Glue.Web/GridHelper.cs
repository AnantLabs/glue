using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using Glue.Lib;
using Glue.Data;

namespace Glue.Web
{
    public class GridHelper 
    {
        public class Grid
        {
            public string  Id = null;
            public int     Page = 0;
            public IList   List = null;
            public int     TotalCount = 0;
            public int     PageSize = 20;
            public Order   Order = Order.Empty;
            public Limit   Limit = Limit.Unlimited;
            public Grid() {}
            public Grid(IDictionary bag)
            {
                Assign(bag);
            }
            public void Assign(IDictionary bag)
            {
                Page = NullConvert.ToInt32(bag["page"], Page);
                PageSize = NullConvert.ToInt32(bag["pagesize"], PageSize);
                Limit = new Limit(Page * PageSize, PageSize);
                Order = new Order(NullConvert.ToString(bag["order"], Order.ToString()));
            }
        }

        public static string GridPager(Grid grid)
        {
            StringBuilder s = new StringBuilder();
            s.Append("<div class=\"grid-pager\">\r\n");
            int n = ((grid.TotalCount-1) / grid.PageSize) + 1;
            if (n > 1)
                for (int i = 0; i < n; i++)
                    if (i == grid.Page)
                        s.Append("<span class=\"selected\">" + (i+1) + "</span>");
                    else
                        s.Append("<span>" + (i+1) + "</span>");
            s.Append("\r\n</div>\r\n");
            return s.ToString();
        }
    }
}