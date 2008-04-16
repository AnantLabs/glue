using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace Glue.Data
{
	/// <summary>
	/// Summary description for IDataProvider.
	/// </summary>
	public interface IDataProvider : IDisposable 
	{
        /// <summary>
        /// Returns the ConnectionString for this DataProvider
        /// </summary>
        string ConnectionString { get; }

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
        IDataProvider Open();

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
        IDataProvider Open(IsolationLevel level);

        /// <summary>
        /// Close and commit any pending transactions
        /// </summary>
        void Close();

        /// <summary>
        /// Rollback transaction.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Add a single parameters on a IDbCommand using a name/value pair. 
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.AddParameter(cmd, "Id", myGuid);
        /// </example>
        /// </remarks>
        IDbDataParameter AddParameter(IDbCommand command, string name, object value);

        /// <summary>
        /// Add parameters on IDbCommand using a name/value parameter-collection.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="paramNameValueList">Parameters as name/ value pairs</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.AddParameters(cmd, "Id", myGuid, "Name", name);
        /// </example>
        /// </remarks>
        void AddParameters(IDbCommand command, params object[] paramNameValueList);

        /// <summary>
        /// Set a single parameters on a IDbCommand using a name/value pair. 
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.SetParameter(cmd, "Id", myGuid);
        /// </example>
        /// </remarks>
        IDbDataParameter SetParameter(IDbCommand command, string name, object value);

        /// <summary>
        /// Set parameters on IDbCommand using a name/value parameter-collection.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="paramNameValueList">Parameters as name/ value pairs</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// DataProvider.Current.SetParameters(cmd, "Id", myGuid, "Name", name);
        /// </example>
        /// </remarks>
        void SetParameters(IDbCommand command, params object[] paramNameValueList);

        /// <summary>
        /// Create command from command text and parameters
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="paramNameValueList">Parameters</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateCommand("SELECT * FROM User Where Name=@Name", "Name", name);
        /// </example>
        IDbCommand CreateCommand(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Create SELECT command
        /// </summary>
        IDbCommand CreateSelectCommand(string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList);

        /// <summary>
        /// Create INSERT-command
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateInsertCommand("User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateInsertCommand(string table, params object[] columnNameValueList);

        /// <summary>
        /// Create INSERT command and set up parameters
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateInsertCommand("User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList);

        /// <summary>
        /// Create a REPLACE command and set up parameters.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateReplaceCommand("User", "Id=@Id", "Id", 10, "Name", "John Doe", "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateReplaceCommand(string table, params object[] columnNameValueList);

        /// <summary>
        /// Create stored procedure command and initialize parameters.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// DataProvider.Current.CreateStoredProcedureCommand("FindUserByEmail", "Name", "john@doe");
        /// </example>
        IDbCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList);
        
        /// <summary>
        /// Execute non-query command. No need to set Connection and Transaction properties on the command.
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns number of rows affected (if applicable).</returns>
        /// <example>
        /// IDbCommand command = MyProvider.CreateCommand("UPDATE Contact SET Logins=Logins+1 WHERE Id=@Id", "Id", 20);
        /// MyProvider.ExecuteNonQuery(command);
        /// </example>
        int ExecuteNonQuery(IDbCommand command);

        /// <summary>
        /// Execute non-query command. No need to set Connection and Transaction properties on the command.
        /// </summary>
        /// <param name="command">Command object</param>
        /// <param name="columnNameValueList">Name / value pairs</param>
        /// <returns>Returns number of rows affected (if applicable).</returns>
        /// <example>
        /// DataProvider.Current.ExecuteNonQuery(
        ///     "UPDATE Contact SET DisplayName=@DisplayName WHERE Id=@Id", 
        ///     "Id", 10,                   // @Id => 10
        ///     "DisplayName", "John Doe"   // @DisplayName => "John Doe"
        /// );
        /// </example>
        int ExecuteNonQuery(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Execute command returning data in a IDataReader. No need to set Connection and Transaction 
        /// properties on the command. You are responsible for closing the IDataReader. Easiest way
        /// is with a "using" statement.
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns an open IDataReader</returns>
        /// <example>
        /// IDbCommand command = DataProvider.Current.CreateSelectCommand(
        ///     "Contacts",             // table 
        ///     "Id,DisplayName",       // columns
        ///     null,                   // filter
        ///     "-DisplayName,+Id",     // order
        ///     Limit.Range(100,110)    // limit
        /// );
        /// using (IDataReader reader = DataProvider.Current.ExecuteReader(command))
        ///     while (reader.Read())
        ///         Console.WriteLine(reader["Id"]);
        /// </example>
        IDataReader ExecuteReader(IDbCommand command);

        /// <summary>
        /// Execute command returning data in a IDataReader. No need to set Connection and Transaction 
        /// properties on the command. You are responsible for closing the IDataReader. Easiest way
        /// is with a "using" statement.
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="columnNameValueList">Name / value pairs</param>
        /// <returns>Returns an open IDataReader.</returns>
        /// <example>
        /// using (IDataReader reader = DataProvider.Current.ExecuteReader("SELECT * FROM Contacts"))
        ///     while (reader.Read())
        ///         Console.WriteLine(reader[0]);
        /// </example>
        IDataReader ExecuteReader(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Execute command returning scalar value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// DateTime? dt = (DataTime?)DataProvider.Current.ExecuteScalar(command);
        /// </example>
        object ExecuteScalar(IDbCommand command);

        /// <summary>
        /// Execute command returning scalar value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="columnNameValueList">Name / value pairs</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// DateTime? dt = (DataTime?)DataProvider.Current.ExecuteScalar("SELECT BirthDate FROM Contacts WHERE Id=@Id", "Id",10);
        /// </example>
        object ExecuteScalar(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Execute command returning an int value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// int count = DataProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
        /// </example>
        int ExecuteScalarInt32(IDbCommand command);

        /// <summary>
        /// Execute command returning an int value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="columnNameValueList">Name / value pairs</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// int count = DataProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
        /// </example>
        int ExecuteScalarInt32(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Execute command returning a string. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns string or null.</returns>
        /// <example>
        /// string name = DataProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id",10");
        /// </example>
        string ExecuteScalarString(IDbCommand command);

        /// <summary>
        /// Execute command returning a string. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="columnNameValueList">Name / value pairs</param>
        /// <returns>Returns string or null.</returns>
        /// <example>
        /// string name = DataProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id",10");
        /// </example>
        string ExecuteScalarString(string commandText, params object[] paramNameValueList);

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
