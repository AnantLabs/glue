using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        /// Open connection and return a cloned provider associated with this connection.
        /// </summary>
        /// <example>
        /// using (IDataProvider provider = Provider.Current.Open()) {
        ///     provider.ExecuteNonQuery("update Contacts set Login=Login+1 where Id=@Id", "Id",10);
        ///     ...
        ///     provider.ExecuteNonQuery( ... );
        /// }
        /// </example>
        new IMappingProvider Open();

        /// <summary>
        /// Open a transaction and return a cloned provider associated with this transaction.
        /// </summary>
        /// <example>
        /// using (IDataProvider provider = Provider.Current.Open(IsolationLevel.ReadCommitted)) {
        ///     provider.ExecuteNonQuery("update Account set Saldo=Saldo+@Amount where Id=@Id", "Id", 10, Amount,99);
        ///     provider.ExecuteNonQuery("update Account set Saldo=Saldo-@Amount where Id=@Id", "Id", 11, Amount,99);
        ///     // Calling Commit is optional, because the "using" statement will make 
        ///     // sure the Close method is called, which will also commit any pending 
        ///     // transaction.
        ///     provider.Commit(); 
        /// }
        /// </example>
        new IMappingProvider Open(IsolationLevel level);

        /// <summary>
        /// Find object by its primary key(s)
        /// </summary>
        object Find(Type type, params object[] keys);

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        object FindByFilter(Type type, Filter filter);

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        object FindByFilter(Type type, Filter filter, Order order);

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        object FindByFilter(string table, Type type, Filter filter);

        /// <summary>
        /// Search for first object which satisfies given conditions.
        /// </summary>
        object FindByFilter(Type type, IDbCommand command);

        /// <summary>
        /// Return objects of given type. Parameters filter, order and limit can be null.
        /// </summary>
        Array List(Type type, Filter filter, Order order, Limit limit);
        
        /// <summary>
        /// Return objects of given type. Parameters filter, order and limit can be null.
        /// </summary>
        Array List(string table, Type type, Filter filter, Order order, Limit limit);

        /// <summary>
        /// Return objects of given type. Parameters filter, order and limit can be null.
        /// </summary>
        Array List(Type type, IDbCommand command);

        /// <summary>
        /// Store (insert or update) given object.
        /// </summary>
        void Save(object obj);

        /// <summary>
        /// Insert given object.
        /// </summary>
        void Insert(object obj);

        /// <summary>
        /// Update given object.
        /// </summary>
        void Update(object obj);

        /// <summary>
        /// Delete given object.
        /// </summary>
        void Delete(object obj);

        /// <summary>
        /// Delete object by primary key(s).
        /// </summary>
        void Delete(Type type, params object[] keys);

        /// <summary>
        /// Delete all objects satisfying given filter.
        /// </summary>
        void DeleteAll(Type type, Filter filter);

        /// <summary>
        /// Determine number of objects satisfying given filter.
        /// </summary>
        int Count(Type type, Filter filter);

        /// <summary>
        /// List all associated (right-side) objects for given instance (left-side) 
        /// in a many-to-many relationship. Explicitly specify the joining table.
        /// </summary>
        Array ListManyToMany(object left, Type right, string jointable);

        /// <summary>
        /// List all associated (right-side) objects for given instance (left-side) 
        /// in a many-to-many relationship. Explicitly specify the joining table.
        /// Filter, order and limit can be null.
        /// </summary>
        Array ListManyToMany(object left, Type right, string jointable, Filter filter, Order order, Limit limit);

        /// <summary>
        /// Create an association between left and right object in a 
        /// many-to-many relationship. Explicitly specify the joining table.
        /// </summary>
        void AddManyToMany(object left, object right, string jointable);

        /// <summary>
        /// Delete an association between left and right object in a 
        /// many-to-many relationship. Explicitly specify the joining table.
        /// </summary>
        void DelManyToMany(object left, object right, string jointable);

        /// <summary>
        /// Creates a dictionary of key-entity pairs for a given type. 
        /// </summary>
        IDictionary Map(Type type, Filter filter, Order order);
        /// <summary>
        /// Creates a dictionary of key-value pairs where the keys and values are taken from two columns in a table.
        /// </summary>
        IDictionary Map(string table, string key, string value, Filter filter, Order order);

        
        T Find<T>(params object[] keys);
        T FindByFilter<T>(Filter filter);
        T FindByFilter<T>(Filter filter, Order order);
        T FindByFilter<T>(string table, Filter filter);
        T FindByFilter<T>(IDbCommand command);
        IList<T> List<T>(Filter filter, Order order, Limit limit);
        IList<T> List<T>(string table, Filter filter, Order order, Limit limit);
        IList<T> List<T>(IDbCommand command);
        void Delete<T>(params object[] keys);
        void DeleteAll<T>(Filter filter);
        int Count<T>(Filter filter);
    }
}
