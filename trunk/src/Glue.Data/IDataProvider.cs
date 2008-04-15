using System;
using System.Data;

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
        /// MappingProvider.Current.AddParameter(cmd, "Id", myGuid);
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
        /// MappingProvider.Current.AddParameters(cmd, "Id", myGuid, "Name", name);
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
        /// MappingProvider.Current.SetParameter(cmd, "Id", myGuid);
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
        /// MappingProvider.Current.SetParameters(cmd, "Id", myGuid, "Name", name);
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
        /// MappingProvider.Current.CreateCommand("SELECT * FROM User Where Name=@Name", "Name", name);
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
        /// MappingProvider.Current.CreateInsertCommand("User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateInsertCommand(string table, params object[] columnNameValueList);

        /// <summary>
        /// Create INSERT command and set up parameters
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// MappingProvider.Current.CreateInsertCommand("User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList);

        /// <summary>
        /// Create a REPLACE command and set up parameters.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// MappingProvider.Current.CreateReplaceCommand("User", "Id=@Id", "Id", 10, "Name", "John Doe", "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateReplaceCommand(string table, params object[] columnNameValueList);

        /// <summary>
        /// Create stored procedure command and initialize parameters.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <returns></returns>
        /// <example>
        /// MappingProvider.Current.CreateStoredProcedureCommand("FindUserByEmail", "Name", "john@doe");
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
        /// MappingProvider.Current.ExecuteNonQuery(
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
        /// IDbCommand command = MappingProvider.Current.CreateSelectCommand(
        ///     "Contacts",             // table 
        ///     "Id,DisplayName",       // columns
        ///     null,                   // filter
        ///     "-DisplayName,+Id",     // order
        ///     Limit.Range(100,110)    // limit
        /// );
        /// using (IDataReader reader = MappingProvider.Current.ExecuteReader(command))
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
        /// using (IDataReader reader = MappingProvider.Current.ExecuteReader("SELECT * FROM Contacts"))
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
        /// DateTime? dt = (DataTime?)MappingProvider.Current.ExecuteScalar(command);
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
        /// DateTime? dt = (DataTime?)MappingProvider.Current.ExecuteScalar("SELECT BirthDate FROM Contacts WHERE Id=@Id", "Id",10);
        /// </example>
        object ExecuteScalar(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Execute command returning an int value. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns single value (scalar).</returns>
        /// <example>
        /// int count = MappingProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
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
        /// int count = MappingProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
        /// </example>
        int ExecuteScalarInt32(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Execute command returning a string. No need to set Connection and Transaction 
        /// properties on the command. 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <returns>Returns string or null.</returns>
        /// <example>
        /// string name = MappingProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id",10");
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
        /// string name = MappingProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id",10");
        /// </example>
        string ExecuteScalarString(string commandText, params object[] paramNameValueList);
    }
}
