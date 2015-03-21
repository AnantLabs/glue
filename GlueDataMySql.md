# Connecting to MySQL #

The classes needed to connect to MySql are compiled in Glue.Data.MySql.dll - don't forget to reference in in your project. There are three ways to construct a MySqlDataProvider.

## Connection string ##
`MySqlDataProvider(string connectionString);`

With this constructor you pass the complete MySQL connection string.
```
IDataProvider provider = 
    new MySqlDataProvider("server=localhost;database=dev;user id=wdambrink;password=glu3");
```

## Server, database, username and password ##
`MySqlDataProvider(string server, string database, string username, string password);`

## XML node ##
`public MySqlDataProvider(XmlNode node);`

Constructs a MySqlDataProviderwith an XML node as a parameter. The node has to have either an attribute "connectionString", containing the complete connection string, or attributes for server, database, username and password.

```
<!-- 
     This is an XML node in the app's configuration file.

     Please note not only the type, but also the assembly of the MySqlDataProvider
     need to be added:
    
     type="[...],Glue.Data.MySql"

-->
<dataprovider
    type="Glue.Data.Providers.MySql.MySqlDataProvider,Glue.Data.MySql"
    database="dev"
    username="wdambrink"
    server="glueproject.com"
    password="glu3"
/>
```
