using System;
using System.Data;

namespace Glue.Data
{
	/// <summary>
	/// Summary description for IDataProvider.
	/// </summary>
	public interface IDataProvider
	{
        /// <summary>
        /// Returns the ConnectionString to this DataProvider
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Returns a new connection to this DataProvider
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();

        /// <summary>
        /// Create new UnitOfWork-instance with a specified IsolationLevel
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level</param>
        /// <returns>New UnitOfWork-instance</returns>
        UnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel);

        /// <summary>
        /// Set parameters on IDbCommand using a name/value parameter-collection.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="paramNameValueList">Parameters as name/ value pairs</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// MappingProvider.Current.SetParameters(cmd, "@Id", myGuid, "@Name", name);
        /// </example>
        /// </remarks>
        void SetParameters(IDbCommand command, params object[] paramNameValueList);

        /// <summary>
        /// Set a single parameters on a IDbCommand using a name/value pair.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// MappingProvider.Current.SetParameter(cmd, "@Id", myGuid);
        /// </example>
        /// </remarks>
        IDbDataParameter SetParameter(IDbCommand command, string name, object value);

        /// <summary>
        /// Set a single parameters on a IDbCommand using a name/value pair, and specify the type.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">Parameter type</param>
        /// <remarks>
        /// Usage looks like this:
        /// <example>
        /// MappingProvider.Current.SetParameter(cmd, "@Id", DbType.Guid, myGuid);
        /// </example>
        /// </remarks>
        IDbDataParameter SetParameter(IDbCommand command, string name, DbType type, object value);

        /// <summary>
        /// Create command from command text and parameters
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="paramNameValueList">Parameters</param>
        /// <returns></returns>
        /// <example>
        /// MappingProvider.Current.CreateCommand("SELECT * FROM User Where Name=@Name", "@Name", name);
        /// </example>
        IDbCommand CreateCommand(string commandText, params object[] paramNameValueList);

        /// <summary>
        /// Create command from command text and parameters
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="paramNameValueList">Parameters</param>
        /// <param name="connection">Connection</param>
        /// <returns></returns>
        /// <example>
        /// MappingProvider.Current.CreateCommand(connection, "SELECT * FROM User Where Name=@Name", "@Name", name);
        /// </example>
        IDbCommand CreateCommand(IDbConnection connection, string commandText, params object[] paramNameValueList);
        

        IDbCommand CreateSelectCommand(IDbConnection connection, string table, string columns, Filter constraint, Order order, Limit limit, params object[] paramNameValueList);
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
        /// Create INSERT-command
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="columnNameValueList">Name/ value pairs</param>
        /// <param name="connection">Connection</param>
        /// <returns></returns>
        /// <example>
        /// MappingProvider.Current.CreateInsertCommand(connection, "User", "Name", name, "DateOfBirth", dateOfBirth);
        /// </example>
        IDbCommand CreateInsertCommand(IDbConnection connection, string table, params object[] columnNameValueList);


        IDbCommand CreateUpdateCommand(string table, Filter constraint, params object[] columnNameValueList);
        IDbCommand CreateUpdateCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList);

        IDbCommand CreateReplaceCommand(string table, Filter constraint, params object[] columnNameValueList);
        IDbCommand CreateReplaceCommand(IDbConnection connection, string table, Filter constraint, params object[] columnNameValueList);

        IDbCommand CreateStoredProcedureCommand(string storedProcedureName, params object[] paramNameValueList);
        IDbCommand CreateStoredProcedureCommand(IDbConnection connection, string storedProcedureName, params object[] paramNameValueList);
        
        int ExecuteNonQuery(IDbCommand command);
        int ExecuteNonQuery(string commandText, params object[] paramNameValueList);

        IDataReader ExecuteReader(IDbCommand command);
        IDataReader ExecuteReader(string commandText, params object[] paramNameValueList);

        object ExecuteScalar(IDbCommand command);
        object ExecuteScalar(string commandText, params object[] paramNameValueList);

        int ExecuteScalarInt32(IDbCommand command);
        int ExecuteScalarInt32(string commandText, params object[] paramNameValueList);

        string ExecuteScalarString(IDbCommand command);
        string ExecuteScalarString(string commandText, params object[] paramNameValueList);

		string ToSql(object v);

        /// <summary>
        /// Get the SchemaProvider for the current data provider.
        /// </summary>
        /// <returns></returns>
        ISchemaProvider GetSchemaProvider();
    }
}
