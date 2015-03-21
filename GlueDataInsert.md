# Using Insert, Update, Save and Delete #

This section discusses some IDataProvider methods to store and delete objects to a database.

## Insert ##

Use `Insert` to store a new object in the database.

```
Customer fred = new Customer("Fred");
provider.Save(fred);
```

## Update ##

Use `Update` to store an object that already exists in the database.
```
Customer fred = Customer.FindByName("Fred");
fred.age = 32;
provider.Update(fred);
```

## Save ##

`Save` either inserts or updates a given object.

```
provider.Save(customer);
```

## Delete ##

To delete an object from the database, all you have to do is call IDataProvider.Delete().

```
Custumer c = Customer.FindById(101);
provider.Delete(c);
```

Delete() will use the mapping for the object's class to find out what row in what database table to delete. If you do not have an object instance, you can delete objects by type and primary key(s).

```
// Delete customer 101
void Delete<Customer>(101);
```

To delete many objects, optionally restricted by a Filter, use `void DeleteAll<T>(Filter filter);` .