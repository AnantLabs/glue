using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// Helper class for creating SQL queries. Used by DataProvider classes.
    /// </summary>
    public class QueryBuilder
    {
        public char IdentifierStart;
        public char IdentifierEnd;
        public char ParameterMarker;
        public string CommandSeparator;
        public string Identity;
        
        StringBuilder inner;

        public QueryBuilder(char parameterMarker, char identifierStart, char identifierEnd, string commandSeparator, string identity)
        {
            IdentifierEnd = identifierEnd;
            IdentifierStart = identifierStart;
            ParameterMarker = parameterMarker;
            CommandSeparator = commandSeparator;
            Identity = identity;
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
            if (name[0] != IdentifierStart)
                inner.Append(IdentifierStart).Append(name).Append(IdentifierEnd);
            else
                inner.Append(name);
            return this;
        }

        public QueryBuilder Identifier(string name, params string[] rest)
        {
            inner.Append(IdentifierStart).Append(name).Append(IdentifierEnd);
            foreach (string next in rest)
                if (name[0] != IdentifierStart)
                    inner.Append('.').Append(IdentifierStart).Append(next).Append(IdentifierEnd);
                else
                    inner.Append(name);
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

        public QueryBuilder SelectIdentity()
        {
            inner.Append("SELECT ").Append(Identity);
            return this;
        }

        public QueryBuilder Next()
        {
            inner.Append(CommandSeparator);
            return this;
        }

        public QueryBuilder Columns(IDataParameterCollection list)
        {
            return Columns(null, list, ",");
        }

        public QueryBuilder Columns(IDataParameterCollection list, string separator)
        {
            return Columns(null, list, separator);
        }

        public QueryBuilder Columns(string table, IDataParameterCollection list, string separator)
        {
            bool first = true;
            foreach (IDbDataParameter p in list)
            {
                if (!first) inner.Append(separator);
                if (table != null)
                    Identifier(table, p.ParameterName);
                else
                    Identifier(p.ParameterName);
                first = false;
            }
            return this;
        }

        public QueryBuilder Columns(EntityMemberList list)
        {
            return Columns(null, list, ",");
        }

        public QueryBuilder Columns(EntityMemberList list, string separator)
        {
            return Columns(null, list, separator);
        }

        public QueryBuilder Columns(string table, EntityMemberList list, string separator)
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

        public QueryBuilder Parameters(EntityMemberList list)
        {
            return Parameters(list, ",");
        }

        public QueryBuilder Parameters(EntityMemberList list, string separator)
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

        public QueryBuilder Parameters(IDataParameterCollection list)
        {
            return Parameters(list, ",");
        }

        public QueryBuilder Parameters(IDataParameterCollection list, string separator)
        {
            bool first = true;
            foreach (IDbDataParameter p in list)
            {
                if (!first) inner.Append(separator);
                Parameter(p.ParameterName);
                first = false;
            }
            return this;
        }

        public QueryBuilder ColumnsParameters(EntityMemberList list, string op, string separator)
        {
            return ColumnsParameters(null, list, op, separator);
        }

        public QueryBuilder ColumnsParameters(string table, EntityMemberList list, string op, string separator)
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

        public QueryBuilder ColumnsParameters(IDataParameterCollection list, string op, string separator)
        {
            return ColumnsParameters(null, list, op, separator);
        }

        public QueryBuilder ColumnsParameters(string table, IDataParameterCollection list, string op, string separator)
        {
            bool first = true;
            foreach (IDbDataParameter p in list)
            {
                if (!first) inner.Append(separator);
                if (table != null)
                    Identifier(table, p.ParameterName);
                else
                    Identifier(p.ParameterName);
                inner.Append(op);
                Parameter(p.ParameterName);
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
