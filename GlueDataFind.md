# Using Find, FindByFilter and Exists #

## Find ##

`IDataProvider.Find()` attempts to find a single object by its primary key(s).

```
_user = User.Find(UserId);
```

## FindByFilter ##

You can also search for objects using a Filter. Only the first object that satisfies the conditions in the Filter is returned.

```
Customer fred = provider.FindByFilter<Customer>(new Filter("name='Fred'"));
```

## Exists ##

Tests if given object exists

```
bool Exists(object obj);
```