# Glue.Data: Using `List` and `Count` #

## List ##
The `List` methods in the `IDataProvider` interface return lists of objects. They are generated from the rows returned by a SELECT query. The List methods take care of constructing the query itself, using a Filter, Order and Limit you provide. For more complex queries using as many views, joins and inner selects as you like, you can pass in your own IDbCommand.

```

// List the customers that joined in the last month. 
Filter newFilter = new Filter("date > @0", DateTime.Now - 30);
IDbCommand cmd = provider.CreateCommand("SELECT * FROM CUSTOMER WHERE DATE > @date", "@date", DateTime.Now - 30);

// The following List statements are now equivalent:
List<Customer> newCustomers1 = provider.List<Customer>(newFilter, null, null);
List<Customer> newCustomers2 = provider.List<Customer>(cmd);
```

## Count ##

The `Count` methods count the number of objects. If filter is `null`, the total number of objects in the table is returned.

```
// Count the number of customers.
// Without using Count you would use this:
int count = provider.ExecuteScalarInt32("SELECT COUNT(*) FROM CUSTOMER");

// The same result using IDataProvider.Count:
int totalCustomerCount = provider.Count<Customer>(null);

// Counting only recent customers:
Filter newFilter = new Filter("date > @0", DateTime.Now - 30);
int newCustomerCount = provider.Count<Customer>(newFilter);

```