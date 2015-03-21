# Connecting to PostgreSQL #

The classes needed to connect to Oracle are compiled in Glue.Data.PostgreSQL.dll - don't forget to reference in in your project. <br />
There are three ways to construct a PostgreSQLDataProvider.

## Connection string ##
`PostgreSQLDataProvider(string connectionString);`

With this constructor you pass the complete connection string.
```
IDataProvider provider = 
    new PostgreSQLDataProvider("host=glueproject.com port=5432 dbname=dev user=postgres password=glu3");
```

## Server, database, username and password ##
`PostgreSQLDataProvider(string server, string database, string username, string password);`

## XML node ##
`public PostgreSQLDataProvider(XmlNode node);`

Constructs a PostgreSQLDataProvider with an XML node as a parameter. The node has to have either an attribute "connectionString", containing the complete connection string, or attributes for server, database, username, and password.

```
<!-- This is an XML node in the app's configuration file -->
<dataprovider
    type="Glue.Data.Providers.Oracle.PostgreSQLDataProvider"
    database="dev"
    username="wdambrink"
    server="glueproject.com"
    password="glu3"
/>
```