# Executing custom queries #

The IDataProvider interface has several methods that take a custom query string.

The Glue methods in this section accept a "commandText" string, which is the query's text with optional placeholders for parameters, and a "paramNameValueList", which is a list of placeholder names and values for each placeholder. This makes it easy for you to add parameters to your query and still stay clear from SQL injection vulnerabilities.

## ExecuteScalarInt32 ##
`int ExecuteScalarInt32(string commandText, params object[] paramNameValueList);`

Executes a command returning an int value.

```
int count = DataProvider.Current.ExecuteScalarInt32("SELECT COUNT(*) FROM Contacts");
```

## ExecuteScalarString ##
`string ExecuteScalarString(string commandText, params object[] paramNameValueList);`

Executes a command returning a string value.

```
string name = DataProvider.Current.ExecuteScalarInt32("SELECT Name FROM Contacts WHERE Id=@Id", "Id", 10");
```

## ExecuteScalar ##
`object ExecuteScalar(string commandText, params object[] paramNameValueList);`

Executes a command returning any scalar value.

```
DateTime? dt = (DateTime?)DataProvider.Current.ExecuteScalar("SELECT BirthDate FROM Contacts WHERE Id=@Id", "Id", 10);
```

## ExecuteNonQuery ##
`int ExecuteNonQuery(string commandText, params object[] paramNameValueList);`

The ExecuteNonQuery method is used to execute a custom non-query command like that does not return any data, like an UPDATE or a DELETE. This method returns number of rows affected (if applicable).

```
DataProvider.Current.ExecuteNonQuery(
        "UPDATE Contact SET DisplayName=@DisplayName WHERE Id=@Id", 
        "Id", 10,                   // @Id => 10
        "DisplayName", "John Doe"   // @DisplayName => "John Doe"
    );

```

# See also #

For queries that return more complicated data (i.e. rows with more than one column) see: [Working with DataReaders (IDbDataReader)](GlueDataExecuteReader.md)

Another way is to use IDbCommands, which has more flexibility in adding parameters. See: [Working with database commands (IDbCommand)](GlueDataCommand.md)