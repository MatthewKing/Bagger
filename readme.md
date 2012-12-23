Bagger
======

This is just a quick project I threw together while learning how `Reflection.Emit` works. It is not really meant for actual use; it's just something I made for learning purposes.

What can it do?
---------------

Bagger facilitates the runtime construction of semi-dynamic, kinda-type-safe property bag types, using `Reflection.Emit`. These can then be used for data binding, etc.

**Dynamically creating a type**:

```csharp
PropertyBagBuilder builder = new PropertyBagBuilder();
builder.AddProperty("Name", typeof(string));
builder.AddProperty("Value1", typeof(int));
builder.AddProperty("Value2", typeof(int));
Type type = builder.Build();
```

**Instantiating the type**:

These types can be instantiated as usual...

```
object instance = Activator.CreateInstance(type);
```

**Accessing properties**:

All types created by `PropertyBagBuilder` also implement the `IPropertyBag` interface, so you can easily access the properties by name.

```csharp
IPropertyBag bag = Activator.CreateInstance(type) as IPropertyBag;
string name = (string)bag.GetValue("Name");
bag.SetValue("Value1", 1);
```

Copyright
---------
Copyright Matthew King 2012.

License
-------
Bagger is licensed under the [Boost Software License](http://www.boost.org/users/license.html). Refer to license.txt for more information.