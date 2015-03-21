Glue's object relation mapper maps SQL database tables to .Net classes. A class will normally correspond to a single database table, or sometimes to a database view. The public class members will map to the columns in that table. Glue's methods will create instances of your class from the rows returned by database queries, and vice versa.

# The Table attribute #

The `[Table]` attribute tells Glue how a class should be mapped.

If no `[Table]` attribute exists for a class, Glue will assume default values, e.g. that the table name is equal to the class name. We advise you to always include a `[Table]` attribute for every mapped class, though. It helps to warn the programmer that it is dangerous to make changes to the class members.

The parameters to the Table attribute are:
  * `Name`: Table name. If not set, it is assumed to be the same as the class name.
  * `Cached`: The complete table will be cached the first time it is loaded. It is stored in a hashtable by its primary key. Use the `IDataProvider.InvalidateCache()` method if you need to clear the cache. Useful for lookup tables.
  * `Prefix`: Columns in the database table have this prefix. The prefix will be prepended to the property name.
  * `Explicit`: If true, a public property in this class will only be mapped to a database column if it has a `[Column]` attribute. If false (the default), every public property will be mapped, unless it has an `[Exclude]` attribute.

# Example #

```
[Table(Name = "Users", Cached = false, Prefix = "Usr")]
public class User
{
        [Key]
        public int Id;
        public string Name;
        public string Username;
        public string Password;
        public bool Blocked;
}
```