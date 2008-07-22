using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using Glue.Lib;
using Glue.Lib.Text;
using Glue.Data;
using Glue.Data.Mapping;

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

        public static string WriteGrid(Glue.Data.IDataProvider provider, Type type)
        {
            Entity entity = Entity.Obtain(type);
            HtmlBuilder s = new HtmlBuilder();
            
            s.Append("<table").Attr("class", "grid").Append(">").AppendLine();
            s.Append("  <thead>").AppendLine();
            s.Append("  <tr>").AppendLine();
            foreach (EntityMember column in EntityMemberList.Flatten(entity.AllMembers))
            {
                s.Append("    <th>").Append(column.Name).Append("</th>").AppendLine();
            }
            s.Append("  </tr>").AppendLine();
            s.Append("  </thead>").AppendLine();
            
            s.Append("  <tbody>").AppendLine();
            bool even = true;
            foreach (object instance in provider.List(type, null, null, null))
            {
                s.Append("    <tr").
                    Attr("class", even ? "even" : "odd").
                    Attr("href", "/entity/" + type.Name.ToLower() + "/edit/" + entity.KeyMembers[0].GetValue(instance)).
                    Append(">").
                    AppendLine();
                foreach (EntityMember column in EntityMemberList.Flatten(entity.AllMembers))
                {
                    s.Append("    <td>");
                    try 
                    { 
                        s.Append(column.GetValue(instance)); 
                    }
                    catch 
                    { 
                        s.Append("#ERR"); 
                    }
                    s.Append("</td>").AppendLine();
                }
                s.Append("  </tr>").AppendLine();
                even = !even;
            }
            s.Append("  </tbody>").AppendLine();
            s.Append("</table>").AppendLine();
            return s.ToString();
        }

        public static string WriteForm(Glue.Data.IDataProvider provider, Type type, object instance)
        {
            Entity entity = Entity.Obtain(type);
            HtmlBuilder s = new HtmlBuilder();
            s.Append("<table").Attr("class", "form").Append(">").AppendLine();
            foreach (EntityMember column in EntityMemberList.Flatten(entity.AllMembers))
            {
                s.Append("  <tr>").AppendLine();
                s.Append("    <th>").Append(column.Name).Append("</th>").AppendLine();
                s.Append("    <td>");
                string klass = "form-control-" + column.Type.Name.ToLower();
                string name = type.Name.ToLower() + "." + column.Name.ToLower();
                string id = type.Name.ToLower() + "_" + column.Name.ToLower();
                if (column.Foreign)
                {
                    s.Append("<select").
                        Attr("id", id).
                        Attr("name", name).
                        Attr("class", klass).
                        Append(">");
                    foreach (object other in provider.List(column.Type, null, null, null))
                        s.Append("<option>").Append(other).Append("</option>");
                    s.Append("</select>");
                }
                else
                {
                    object value;
                    try { value = column.GetValue(instance); }
                    catch { value = "#ERR"; }
                    
                    string disabled = column.AutoKey != null || column.Calculated != null ? " disabled" : null;
                    if (column.Type == typeof(Boolean))
                    {
                        s.Append("<input").
                            Attr("id", id).
                            Attr("name", name).
                            Attr("class", klass).
                            Attr("disabled", disabled).
                            Attr("type", "checkbox").
                            Append("/>");
                    }
                    else if (column.Type == typeof(DateTime))
                    {
                        s.Append("<input").
                            Attr("id", id).
                            Attr("name", name).
                            Attr("class", klass).
                            Attr("disabled", disabled).
                            Attr("type", "text").
                            Attr("value", "yyyy-mm-dd").
                            Append("/>");
                    }
                    else
                    {
                        s.Append("<input").
                            Attr("id", id).
                            Attr("name", name).
                            Attr("class", klass).
                            Attr("disabled", disabled).
                            Attr("type", "text").
                            Attr("value", value).
                            Append("/>");
                    }
                }
                s.Append("</td>");
                s.Append("</tr>").AppendLine();
            }
            s.Append("  <tr><th>&nbsp;</th><td><input type=\"submit\" value=\"Update\" /></td></tr>").AppendLine();
            s.Append("</table>").AppendLine();
            return s.ToString();
        }
    }
}