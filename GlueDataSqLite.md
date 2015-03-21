# Connecting to SQLite #

There are three ways to construct a SQLiteDataProvider.

## Connection string ##
`SQLiteDataProvider(string connectionString);`

With this constructor you pass the complete SQLite connection string. You can create a new SQLite database by putting in "New=True".
```
IDataProvider provider = new SQLiteDataProvider("Data Source=" + dbPath + ";New=True");
```

## Server, database, username and password ##
`SQLiteDataProvider(string server, string database, string username, string password);`

This is a default constructor. It works, but only the "database" is used.

## XML node ##
`public SQLiteDataProvider(XmlNode node);`

Constructs a SQLiteDataprovider with an XmlNode as a parameter. The node has to have either an attribute "connectionString", containing the complete connection string, or an attribute "database", with a path to a database file.

```
<!-- 
     This is an XML node in the app's configuration file.

     Please note not only the type, but also the assembly of the SQLiteDataProvider
     need to be added:
    
     type="[...],Glue.Data.SQLite"

-->
<dataprovider 
      type="Glue.Data.Providers.SQLite.SQLiteDataProvider,Glue.Data.SQLite" 
      database="App_Data/databasefile.db3"
/>
```
