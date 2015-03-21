# Connecting to MSSQL #

There are three ways to construct a SqlDataProvider.

## Connection string ##
`SqlDataProvider(string connectionString);`

With this constructor you pass the complete MSSQL connection string.
```
IDataProvider provider = 
    new SqlDataProvider("server=localhost;database=dev;user id=wdambrink;password=glu3");
```

## Server, database, username and password ##
`SqlDataProvider(string server, string database, string username, string password);`

## XML node ##
`public SqlDataProvider(XmlNode node);`

Constructs a SqlDataprovider with an XML node as a parameter. The node has to have either an attribute "connectionString", containing the complete connection string, or attributes for server, database, username and password.

```
<!-- This is an XML node in the app's configuration file -->
<dataprovider
    type="Glue.Data.Providers.Sql.SqlDataProvider"
    database="dev"
    username="wdambrink"
    server="glueproject.com"
    password="glu3"
/>
```