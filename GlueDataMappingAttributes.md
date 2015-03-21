# The `[Column]` attribute #

The `[Column]` attribute tells Glue that a class member should be mapped to a column in the database's table. By default, every public class member is mapped to a column with the same name, so an empty `[Column`] is superfluous. To map a member to a differently named column, use: `[Column(Name = "AnotherName")]`. For string types, an optional MaxLength parameter may be given for the maximum length of the database column.

```

// The Customer column is called "CUST" in the database.
[Column(Name = "CUST")]
public string Name;

[Column(MaxLength = 1000)]
string Comment;

```

# Keys #

The `[Key]` attribute specifies that the corresponding column is (part of) the primary key.

The `[AutoKey]` attribute specifies that a class member is an auto key. Its value will be set by the database when the object is inserted.

# Excluding class members from the mapping #

Entities with an `[Exclude]` attribute will not be mapped to database columns (by default, all public members are mapped).

The `[Calculated]` attribute specifies that the database column is a calculated value. It will not be included in Inserts or Updates, but will be retrieved by Find and List.

# Example #

```
using System;
using Glue.Data;
using Glue.Data.Mapping;

/// <summary>
/// A Blog message
/// </summary>
[Table]
public class Message
{
    [AutoKey]
    public int Id;

    [Column(MaxLength = 10000)]
    public string Content;

    [Column(MaxLength = 100)]
    public string Author;

    public DateTime Published;

    public Message()
    {
        Published = DateTime.Now;
    }
}
```