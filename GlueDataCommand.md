Many IDataProvider methods can take an IDbCommand to execute. You can create these commands with IDataProvider methods or with standard System.Data methods, and then set the SQL parameters later. Using this multi-step process is usually not what you want, but it can sometimes help to optimise your program if you run the same IDbCommand many times, to prepare a command and then update the parameter values in between calls.

# Creating a IDbCommand with Glue #

With IDataProvider.CreateCommand you can create an IDbCommand from a SQL string and named parameters.

```
// Create an IDbCommand with parameters
provider.CreateCommand("SELECT * FROM User Where Name=@Name", "Name", name);

// Create an IDbCommand first, then set the parameters later.
IDbCommand cmd = provider.CreateCommand("SELECT * FROM User Where Name=@Name AND Age=@Age");
provider.AddParameters(cmd, "Name", name, "Age", age);
users = provider.List<User>(cmd);

// Execute the same command but with different parameters
provider.SetParameter(cmd, "Name", anotherName);
users = provider.List<User>(cmd);

```

# DataProvider methods that use IDbCommands #

```

// Returns the number of rows affected, if applicable.
int ExecuteNonQuery(IDbCommand command)

IDataReader ExecuteReader(IDbCommand command)

object ExecuteScalar(IDbCommand command)

int ExecuteScalarInt32(IDbCommand command)

string ExecuteScalarString(IDbCommand command)

T Find<T>(IDbCommand command)

IList<T> List<T>(IDbCommand command)

```

# See Also #

The [Glue API documentation](http://www.glueproject.com/api) page on IDataProvider can be found here:

http://www.glueproject.com/api/html/AllMembers_T_Glue_Data_IDataProvider.htm