using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data.Mapping
{
    public class QueryBuilder
    {
        public char IdentifierStart;
        public char IdentifierEnd;
        public char ParameterMarker;
        StringBuilder inner;

        public QueryBuilder(char parameterMarker, char identifierStart, char identifierEnd)
        {
            IdentifierEnd = identifierEnd;
            IdentifierStart = identifierStart;
            ParameterMarker = parameterMarker;
            inner = new StringBuilder();
        }

        public QueryBuilder Append(string text)
        {
            inner.Append(text);
            return this;
        }

        public QueryBuilder AppendLine()
        {
            inner.AppendLine();
            return this;
        }

        public QueryBuilder AppendLine(string text)
        {
            inner.AppendLine(text);
            return this;
        }

        public QueryBuilder Identifier(string name)
        {
            inner.Append(IdentifierStart).Append(name).Append(IdentifierEnd);
            return this;
        }

        public QueryBuilder Identifier(string name, params string[] rest)
        {
            inner.Append(IdentifierStart).Append(name).Append(IdentifierEnd);
            foreach (string next in rest)
                inner.Append('.').Append(IdentifierStart).Append(next).Append(IdentifierEnd);
            return this;
        }

        public QueryBuilder Parameter(string name)
        {
            inner.Append(ParameterMarker).Append(name);
            return this;
        }

        public QueryBuilder Filter(Filter filter)
        {
            if (filter != null && !filter.IsEmpty)
                inner.Append(" WHERE ").Append(filter);
            return this;
        }

        public QueryBuilder Order(Order order)
        {
            if (order != null && !order.IsEmpty)
                inner.Append(" ORDER BY ").Append(order);
            return this;
        }

        public QueryBuilder Limit(Limit limit)
        {
            if (limit != null && !limit.IsUnlimited)
                inner.Append(" LIMIT " + limit.Index + "," + limit.Count);
            return this;
        }

        public QueryBuilder ColumnList(EntityMemberList list)
        {
            return ColumnList(null, list, ",");
        }

        public QueryBuilder ColumnList(EntityMemberList list, string separator)
        {
            return ColumnList(null, separator);
        }

        public QueryBuilder ColumnList(string table, EntityMemberList list, string separator)
        {
            bool first = true;
            foreach (EntityMember m in list)
                if (m.Column != null)
                {
                    if (!first) inner.Append(separator);
                    if (table != null)
                        Identifier(table, m.Column.Name);
                    else
                        Identifier(m.Column.Name);
                    first = false;
                }
            return this;
        }

        public QueryBuilder ParameterList(EntityMemberList list)
        {
            return ParameterList(list, ",");
        }

        public QueryBuilder ParameterList(EntityMemberList list, string separator)
        {
            bool first = true;
            foreach (EntityMember m in list)
                if (m.Column != null)
                {
                    if (!first) inner.Append(separator);
                    Parameter(m.Column.Name);
                    first = false;
                }
            return this;
        }

        public QueryBuilder ColumnAndParameterList(EntityMemberList list, string op, string separator)
        {
            return ColumnAndParameterList(null, list, op, separator);
        }

        public QueryBuilder ColumnAndParameterList(string table, EntityMemberList list, string op, string separator)
        {
            bool first = true;
            foreach (EntityMember m in list)
                if (m.Column != null)
                {
                    if (!first) inner.Append(separator);
                    if (table != null)
                        Identifier(table, m.Column.Name);
                    else
                        Identifier(m.Column.Name);
                    inner.Append(op);
                    Parameter(m.Column.Name);
                    first = false;
                }
            return this;
        }

        public override string ToString()
        {
            return inner.ToString();
        }
    }
}
