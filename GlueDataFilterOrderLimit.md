# Filter, Order and Limit #

Many IDataProvider methods take a Filter, Order and/or Limit as parameter. They are used to construct the WHERE, ORDER BY and LIMIT clauses (or their equivalents) in the query language.

## Filter ##

A filter is used to restrict the results returned from a database query. It corresponds to the 'WHERE' clause in SQL.

```
// A simple filter: (in SQL you would write: WHERE SIZE > 10)
Filter big = new Filter("SIZE > 10");

// A filter takes numbered parameters @0, @1, @2, ...
Filter heavy = new Filter("WEIGHT > @0", 100);
```

Sometimes it is useful to combine Filters with `Filter.And` or `Filter.Or`. For example, when you want to restrict an already existing Filter.

```
Filter bigAndHeavy = Filter.And(big, heavy);
```

## Order ##

`Order` defines a sorting order for data returned from a database query. It corresponds to the 'ORDER BY' clause in SQL.

```
// An Order can be constructed in the following ways:
Order o1 = new Order("-Name", "+Age");
Order o2 = new Order("Name DESC", "Age ASC");
Order o3 = new Order("-Name, +Age");    

/// An Order can also be constructed by casting: 
Order o = (Order)"-Name,+Age";
// This is handy in List-methods:
List.All("-Name");
```

## Limit ##

A Limit specifies a range of rows to limit the result set of a query. A Limit contains an Index (the first row to be returned) and a Count (the maximum number of rows). A Count of -1 means "unlimited". For methods that take a Limit as an argument, passing `null` is equivalent to passing Limit.Unlimited.

```
// Putting it all together, if 50 customers fit on a page, which customers are on 
// page 32 if you list the big and heavy customers, sorted by name?

int pageSize = 50;
int page = 32;

Order name = new Order("lastname");
Limit page32 = new Limit(pageSize * page, pageSize);

IList<Customer> customersOnPage32 = provider.List<Customer>(bigAndHeavy, name, page32);
```