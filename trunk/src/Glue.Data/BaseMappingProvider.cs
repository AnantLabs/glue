using System;
using System.Xml;
using System.Data;
using System.Collections;
using System.Text;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    public abstract class BaseMappingProvider : BaseDataProvider, IMappingProvider
    {
        public BaseMappingProvider(string connectionString)
            : base(connectionString)
        {
        }

        public new IMappingProvider Open()
        {
            return (IMappingProvider)base.Open();
        }

        public new IMappingProvider Open(IsolationLevel level)
        {
            return (IMappingProvider)base.Open(level);
        }

        public object Find(Type type, params object[] keys)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object FindByFilter(Type type, Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object FindByFilter(Type type, Filter filter, Order order)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object FindByFilter(string table, Type type, Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object FindByFilter(Type type, IDbCommand command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Array List(Type type, Filter filter, Order order, Limit limit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Array List(string table, Type type, Filter filter, Order order, Limit limit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Array List(Type type, IDbCommand command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Save(object obj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Insert(object obj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Update(object obj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Delete(object obj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Delete(Type type, params object[] keys)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void DeleteAll(Type type, Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count(Type type, Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Array ListManyToMany(object left, Type right, string jointable)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Array ListManyToMany(object left, Type right, string jointable, Filter filter, Order order, Limit limit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddManyToMany(object left, object right, string jointable)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void DelManyToMany(object left, object right, string jointable)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDictionary Map(Type type, Filter filter, Order order)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDictionary Map(string table, string key, string value, Filter filter, Order order)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public T Find<T>(params object[] keys)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public T FindByFilter<T>(Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public T FindByFilter<T>(Filter filter, Order order)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public T FindByFilter<T>(string table, Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public T FindByFilter<T>(IDbCommand command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public System.Collections.Generic.IList<T> List<T>(Filter filter, Order order, Limit limit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public System.Collections.Generic.IList<T> List<T>(string table, Filter filter, Order order, Limit limit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public System.Collections.Generic.IList<T> List<T>(IDbCommand command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Delete<T>(params object[] keys)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void DeleteAll<T>(Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count<T>(Filter filter)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
