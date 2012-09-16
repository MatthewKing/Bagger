SimpleTypeBuilder
-----------------

This is just a quick project I threw together while learning how `Reflection.Emit` works.

What can it do?
===============

SimpleTypeBuilder facilitates the runtime construction of semi-dynamic, type-safe property bag types, using `Reflection.Emit`. These can then be used for data binding, etc.

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

License and copyright
=====================

Copyright © Matthew King.

Available under the [Boost Software License](http://www.boost.org/users/license.html).