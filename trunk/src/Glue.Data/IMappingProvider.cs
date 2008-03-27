using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Reflection;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// IMappingProvider
    /// </summary>
    public interface IMappingProvider : IDataProvider
    {
        object Find(Type type, params object[] keys);
        object FindByFilter(Type type, Filter filter);
        object FindByFilter(Type type, Filter filter, Order order);
        object FindByFilter(string table, Type type, Filter filter);
        object FindByFilter(Type type, IDbCommand command);
        Array  List(Type type, Filter filter, Order order, Limit limit);
        Array  List(string table, Type type, Filter filter, Order order, Limit limit);
        Array  List(Type type, IDbCommand command);
        void   Save(object obj);
        void   Insert(object obj);
        void   Insert(UnitOfWork unitOfWork, object obj);
        void   Update(object obj);
        void   Update(UnitOfWork unitOfWork, object obj);
        void   Delete(object obj);
        void   Delete(UnitOfWork unitOfWork, object obj);
        void   Delete(Type type, params object[] keys);
        void   DeleteAll(Type type, Filter filter);
        int    Count(Type type, Filter filter);
        Array  ListManyToMany(object left, Type right);
        Array  ListManyToMany(object left, Type right, Filter filter, Order order, Limit limit);
        Array  ListManyToMany(Type left, object right);
        Array  ListManyToMany(Type left, object right, Filter filter, Order order, Limit limit);
        void   AddManyToMany(object left, object right);
        void   DelManyToMany(object left, object right);
        IDictionary Map(Type type, string key, string value, Filter filter, Order order);
    }
}
