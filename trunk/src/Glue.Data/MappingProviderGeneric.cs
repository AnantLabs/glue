using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// A MappingProvider for a generic type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MappingProvider<T>
    {
        #region MappingProvider

        /// <summary>
        /// Return the MappingProvider for this class
        /// </summary>
        /// <remarks>
        /// The MappingProvider can to be declared in the Table-attribute.

        /// If the MappingProvider is not declared, the default MappingProvider is initialized
        /// from the element "dataprovider" (which, obviously, has to be there...).
        /// </remarks>
        /// <example>
        /// [Table(MappingProvider="dataprovider-account")]
        /// public class Account : ActiveRecord<Account>
        /// {
        /// [...]
        /// }
        /// </example>
        public static IMappingProvider Provider
        {
            get { return Glue.Data.MappingProvider.Get(typeof(T)); }
        }

        #endregion 

        #region Find

        /// <summary>
        /// Find a record by ID(s)
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>Instance of object</returns>
        /// <remarks>
        /// Multiple keys can be used as well, as long as they are used in the order of declaration.
        /// <example>
        /// Account = Account.Find(accountId);
        /// Transaction = AccountTransaction.Find(transactionId, accountId);
        /// </example>
        /// </remarks>
        public static T Find(params object[] keys)
        {
            return (T)Provider.Find(typeof(T), keys);
        }

        /// <summary>
        /// Find a record by Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns>New instance</returns>
        /// <remarks>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.FindByFilter(Filter.Create("item_id=@0", uri));
        /// </example>
        /// </remarks>
        public static T FindByFilter(Filter filter)
        {
            return (T)Provider.FindByFilter(typeof(T), filter);
        }

        /// <summary>
        /// Find a record by Filter, ordered by Order
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="order">Order</param>
        /// <returns>New instance</returns>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.FindByFilter(Filter.Create("item_id=@0", uri), new Order("URI DESC"));
        /// </example>
        public static T FindByFilter(Filter filter, Order order)
        {
            return (T)Provider.FindByFilter(typeof(T), filter, order);
        }

        /// <summary>
        /// Find a record by Filter in a specific table (such as a view)
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="table">Table (or view)</param>
        /// <returns>New instance</returns>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.FindByFilter("vwWoorden", Filter.Create("item_id=@0", uri));
        /// </example>
        public static T FindByFilter(string table, Filter filter)
        {
            return (T)Provider.FindByFilter(table, typeof(T), filter);
        }

        /// <summary>
        /// Find a record by using a specific IDbCommand
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <returns>New instance</returns>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.FindByFilter(command);
        /// </example>
        public static T FindByFilter(IDbCommand command)
        {
            return (T)Provider.FindByFilter(typeof(T), command);
        }

        #endregion

        #region List

        /// <summary>
        /// List records by Filter, ordered by Order and limited to Limit
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="order">Order</param>
        /// <param name="limit">Limit</param>
        /// <returns>New instance</returns>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.List(Filter.Create("item_id=@0", uri), new Order("URI DESC"), new Limit(0, 10));
        /// </example>
        public static IList<T> List(Filter filter, Order order, Limit limit)
        {
            return (IList<T>)Provider.List(typeof(T), filter, order, limit);
        }

        /// <summary>
        /// List records by Filter, ordered by Order and limited to Limit, from a specific table or view
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="order">Order</param>
        /// <param name="limit">Limit</param>
        /// <param name="table">Table (or view)</param>
        /// <returns>New instance</returns>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.List("vwWoord", Filter.Create("item_id=@0", uri), new Order("URI DESC"), new Limit(0, 10));
        /// </example>
        public static IList<T> List(string table, Filter filter, Order order, Limit limit)
        {
            return (IList<T>)Provider.List(table, typeof(T), filter, order, limit);
        }

        /// <summary>
        /// List records by using a specific IDbCommand
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <returns>New instance</returns>
        /// <example>
        /// TypeProvider&lt;Woord&gt;.List(command);
        /// </example>
        public static IList<T> List(IDbCommand command)
        {
            return (IList<T>)Provider.List(typeof(T), command);
        }

        #endregion

        #region Delete

        public static void Delete(params object[] keys)
        {
            Provider.Delete(typeof(T), keys);
        }

        public static void DeleteAll(Filter filter)
        {
            Provider.DeleteAll(typeof(T), filter);
        }

        #endregion

        #region Count

        public static int Count(Filter filter)
        {
            return Provider.Count(typeof(T), filter);
        }

        #endregion

        #region ManyToMany

        public static Array ListManyToMany(object left, Type right)
        {
            return Provider.ListManyToMany(left, right);
        }

        public static Array ListManyToMany(object left, Type right, Filter filter, Order order, Limit limit)
        {
            return Provider.ListManyToMany(left, right, filter, order, limit);
        }

        public static Array ListManyToMany(Type left, object right)
        {
            return Provider.ListManyToMany(left, right);
        }

        public static Array ListManyToMany(Type left, object right, Filter filter, Order order, Limit limit)
        {
            return Provider.ListManyToMany(left, right, filter, order, limit);
        }

        public static void AddManyToMany(object left, object right)
        {
            Provider.AddManyToMany(left, right);
        }

        public static void DelManyToMany(object left, object right)
        {
            Provider.DelManyToMany(left, right);
        }

        public static System.Collections.IDictionary Map(Type type, string key, string value, Filter filter, Order order)
        {
            return Provider.Map(type, key, value, filter, order);
        }

        #endregion

        #region IDataProvider Members

        public static string ConnectionString
        {
            get { return Provider.ConnectionString; }
        }

        public static IDbConnection CreateConnection()
        {
            return Provider.CreateConnection();
        }

        public static UnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel)
        {
            return Provider.CreateUnitOfWork(isolationLevel);
        }

        public static void SetParameters(IDbCommand command, params object[] paramNameValueList)
        {
            Provider.SetParameters(command, paramNameValueList);
        }

        public static IDbDataParameter SetParameter(IDbCommand command, string name, object value)
        {
            return Provider.SetParameter(command, name, value);
        }

        public static IDbDataParameter SetParameter(IDbCommand command, string name, DbType type, object value)
        {
            return Provider.SetParameter(command, name, type, value);
        }

        public static IDbCommand CreateCommand(string commandText, params object[] paramNameValueList)
        {
            return Provider.CreateCommand(commandText, paramNameValueList);
        }

        public static IDbCommand CreateCommand(IDbConnection connection, string commandText, params object[] paramNameValueList)
        {
            return Provider.CreateCommand(connection, commandText, paramNameValueList);
        }

        public static IDbCommand CreateSelectCommand(IDbConnection connection, string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return Provider.CreateSelectCommand(connection, table, columns, constraint, order, limit, paramNameValueList);
        }

        public static IDbCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList)
        {
            return Provider.CreateSelectCommand(table, columns, constraint, order, limit, paramNameValueList);
        }

        public static IDbCommand CreateInsertCommand(string table, params object[] columnNameValueList)
        {
            return Provider.CreateInsertCommand(table, columnNameValueList);
        }

        public static IDbCommand CreateInsertCommand(IDbConnection connection, string table, params object[] columnNameValueList)
        {
            return Provider.CreateInsertCommand(connection, table, columnNameValueList);
        }

        public static IDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return Provider.CreateUpdateCommand(table, constraint, columnNameValueList);
        }

        public static IDbCommand CreateUpdateCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return Provider.CreateUpdateCommand(connection, table, constraint, columnNameValueList);
        }

        public static IDbCommand CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList)
        {
            return Provider.CreateReplaceCommand(table, constraint, columnNameValueList);
        }

        public static IDbCommand CreateReplaceCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList)
        {
            return Provider.CreateReplaceCommand(connection, table, constraint, columnNameValueList);
        }

        public static IDbCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList)
        {
            return Provider.CreateStoredProcedureCommand(storedProcedureName, paramNameValueList);
        }

        public static IDbCommand CreateStoredProcedureCommand(IDbConnection connection, string storedProcedureName, params object[] paramNameValueList)
        {
            return Provider.CreateStoredProcedureCommand(connection, storedProcedureName, paramNameValueList);
        }

        public static int ExecuteNonQuery(IDbCommand command)
        {
            return Provider.ExecuteNonQuery(command);
        }

        public static int ExecuteNonQuery(string commandText, params object[] paramNameValueList)
        {
            return Provider.ExecuteNonQuery(commandText, paramNameValueList);
        }

        public static IDataReader ExecuteReader(IDbCommand command)
        {
            return Provider.ExecuteReader(command);
        }

        public static IDataReader ExecuteReader(string commandText, params object[] paramNameValueList)
        {
            return Provider.ExecuteReader(commandText, paramNameValueList);
        }

        public static object ExecuteScalar(IDbCommand command)
        {
            return Provider.ExecuteScalar(command);
        }

        public static object ExecuteScalar(string commandText, params object[] paramNameValueList)
        {
            return Provider.ExecuteScalar(commandText, paramNameValueList);
        }

        public static int ExecuteScalarInt32(IDbCommand command)
        {
            return Provider.ExecuteScalarInt32(command);
        }

        public static int ExecuteScalarInt32(string commandText, params object[] paramNameValueList)
        {
            return Provider.ExecuteScalarInt32(commandText, paramNameValueList);
        }

        public static string ExecuteScalarString(IDbCommand command)
        {
            return Provider.ExecuteScalarString(command);
        }

        public static string ExecuteScalarString(string commandText, params object[] paramNameValueList)
        {
            return Provider.ExecuteScalarString(commandText, paramNameValueList);
        }

        public static string ToSql(object v)
        {
            return Provider.ToSql(v);
        }

        #endregion
    }
}
