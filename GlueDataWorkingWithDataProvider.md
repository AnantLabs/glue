# Initializing from a config file #

The connection settings should be set in an XML config file to keep your connection settings configurable. This example shows how to store DataProvider settings in a standard Web.config file.

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <section name="settings" type="Glue.Lib.Configuration, glue.lib" />
    </configSections>

    <settings>	
        <!-- Database connection to the Content-database -->
        <dataprovider
            type="Glue.Data.Providers.Sql.SqlDataProvider"
            database="dev"
            username="jzoef"
            server="glueproject.com"
            password="glu3"
        />
    </settings>
</configuration>
```
_Example: Web.config_

# Store an IDataProvider as a global static variable #

This has the advantage that the database connection stays open during the life of your program. Also, the different classes in your model can share the same DataProvider, which means they also share the connection.

This example shows how to load a DataProvider in one line of code, using Glue.Lib.Configuration.

```
namespace SampleApplication.Model
{
    public class Global
    {
        public static IDataProvider DataProvider = (IDataProvider)Configuration.Get("dataprovider");
    }
}
```

# Extend the default behaviour with class methods #

We recommend that you use the DataProvider only from your mapped classes. This allows you to keep the class logic tied to your class, which makes it easier to respond to changes in the model or the database schema.<br />
You can also use these class methods for
  * **Input validation:** For example, check that an e-mail address is valid
  * **Custom code on update or delete:** For example, set a 'ModifiedOn' variable before an update.

The code below shows an extended Delete() method that clears a user's messages before deleting the user itself. A shortcut method "FindByName" is added for the common task of finding the user by its username, otherwise done using e.g. `Global.DataProvider.FindByFilter<User>(...)`

```
[Table]
class User
{
    public int Id;
    public string UserName;
    public bool IsAdmin;
    // ...


    /// Delete a user
    public void Delete()
    {
        // Business logic dictates that admin users may not be deleted
        if (IsAdmin)
            throw new Exception("Super users may not be deleted!");

        // The "Message" table has a foreign key relation "UserId".
        // We need to delete all the user's messages first.
        Global.DataProvider.DeleteAll<Message>(new Filter("UserId=@0", Id));

        Global.DataProvider.Delete(this);
    }

    /// Find a user by userid.
    public static User Find(int userId)
    {
        return Global.DataProvider.Find<User>(userId);
    }

    /// Return the user with username 'name'.
    public static User FindByName(string name)
    {
        Filter f = new Filter("name=@0", name);
        return Global.DataProvider.FindByFilter<User>(f);
    }

}
```