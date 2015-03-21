# Connecting to Oracle #

The classes needed to connect to Oracle are compiled in Glue.Data.Oracle.dll - don't forget to reference in in your project. <br />
There are three ways to construct a OracleDataProvider.

## Connection string ##
`OracleDataProvider(string connectionString);`

With this constructor you pass the complete connection string.
```
IDataProvider provider = 
    new OracleDataProvider("Data Source =(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))" + 
            "(CONNECT_DATA=(SID=dev)));Unicode=True; User Id=wdambrink; Password=glu3;");

```

## Server, database, username and password ##
`OracleDataProvider(string server, string database, string username, string password);`

## XML node ##
`public OracleDataProvider(XmlNode node);`

Constructs a OracleDataProvider with an XML node as a parameter. The node has to have either an attribute "connectionString", containing the complete connection string, or attributes for server, database, username, password, and port (optional).

```
<!-- This is an XML node in the app's configuration file -->
<dataprovider
    type="Glue.Data.Providers.Oracle.OracleDataProvider"
    database="dev"
    port="1521" 
    username="wdambrink"
    server="glueproject.com"
    password="glu3"
/>
```
