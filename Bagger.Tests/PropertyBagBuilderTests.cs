namespace Bagger.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    internal sealed class PropertyBagBuilderTests
    {
        [Test]
        public void Constructor_WithoutNameParameter_SetsNameToArbitraryNonNullValue()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder();

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo fieldInfo = typeof(PropertyBagBuilder).GetField("name", flags);
            string name = fieldInfo.GetValue(builder) as string;

            Assert.That(name, Is.Not.Null);
        }

        [Test]
        public void Constructor_NameIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new PropertyBagBuilder(null),
                Throws.TypeOf<ArgumentNullException>()
                      .And.Message.Contains("name should not be null."));
        }

        [Test]
        public void Constructor_NameIsEmpty_ThrowsArgumentException()
        {
            Assert.That(() => new PropertyBagBuilder(String.Empty),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains("name should not be an empty string."));
        }

        [Test]
        public void AddProperty_NameIsNull_ThrowsArgumentNullException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");

            Assert.That(() => builder.AddProperty(null, typeof(string)),
                Throws.TypeOf<ArgumentNullException>()
                      .And.Message.Contains("name should not be null."));
        }

        [Test]
        public void AddProperty_NameIsEmpty_ThrowsArgumentException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");

            Assert.That(() => builder.AddProperty(String.Empty, typeof(string)),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains("name should not be an empty string."));
        }

        [Test]
        public void AddProperty_TypeIsNull_ThrowsArgumentNullException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");

            Assert.That(() => builder.AddProperty("name", null),
                Throws.TypeOf<ArgumentNullException>()
                      .And.Message.Contains("type should not be null."));
        }

        [Test]
        public void AddProperty_PropertyAlreadyExists_ThrowsArgumentException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Assert.That(() => builder.AddProperty("Property", typeof(string)),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains("Property 'Property' is already defined."));
        }

        [Test]
        public void AddProperty_TypeHasAlreadyBeenBuilt_ThrowsInvalidOperationException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");

            builder.Build();

            Assert.That(() => builder.AddProperty("name", typeof(string)),
                Throws.TypeOf<InvalidOperationException>()
                      .And.Message.Contains("Unable to add properties once a type has already been built."));
        }

        [Test]
        public void AddProperty_ReturnsPropertyBagBuilderInstance()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");

            PropertyBagBuilder returnValue = builder.AddProperty("name", typeof(string));

            Assert.That(returnValue, Is.EqualTo(builder));
        }

        [Test]
        public void CreatedType_ContainsExpectedProperties()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property1", typeof(string));
            builder.AddProperty("Property2", typeof(int));

            Type type = builder.Build();

            Assert.That(type.GetProperty("Property1"), Is.Not.Null);
            Assert.That(type.GetProperty("Property1").PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(type.GetProperty("Property2"), Is.Not.Null);
            Assert.That(type.GetProperty("Property2").PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void CreatedType_CanGetAndSetPropertyAsExpected()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            PropertyInfo property = type.GetProperty("Property");
            object instance = Activator.CreateInstance(type);

            property.SetValue(instance, "Value", null);
            object value = property.GetValue(instance, null);
            Assert.That(value, Is.EqualTo("Value"));
        }

        [Test]
        public void CreatedType_SetPropertyToIncompatibleType_ThrowsArgumentException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            PropertyInfo property = type.GetProperty("Property");
            object instance = Activator.CreateInstance(type);

            string message = "Object of type 'System.Object' cannot be converted to type 'System.String'.";
            Assert.That(() => property.SetValue(instance, new object(), null),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains(message));
        }

        [Test]
        public void CreatedType_ImplementsIPropertyBag()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            Type type = builder.Build();

            Assert.That(typeof(IPropertyBag).IsAssignableFrom(type), Is.True);
        }

        [Test]
        public void CreatedType_AsIPropertyBag_GetValue_PropertyNameIsNotValid_ThrowsArgumentException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            IPropertyBag bag = Activator.CreateInstance(type) as IPropertyBag;

            string message = "propertyName must refer to a parameter that exists on this type.";
            Assert.That(() => bag.GetValue("Name"),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains(message));
        }

        [Test]
        public void CreatedType_AsIPropertyBag_GetValue_ReturnsPropertyValue()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            IPropertyBag bag = Activator.CreateInstance(type) as IPropertyBag;

            PropertyInfo propertyInfo = type.GetProperty("Property");
            propertyInfo.SetValue(bag, "expected_value", null);

            Assert.That(bag.GetValue("Property"), Is.EqualTo("expected_value"));
        }

        [Test]
        public void CreatedType_AsIPropertyBag_SetValue_PropertyNameIsNotValid_ThrowsArgumentException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            IPropertyBag bag = Activator.CreateInstance(type) as IPropertyBag;

            string message = "propertyName must refer to a parameter that exists on this type.";
            Assert.That(() => bag.SetValue("Name", new object()),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains(message));
        }

        [Test]
        public void CreatedType_AsIPropertyBag_SetValue_ObjectIsOfInvalidType_ThrowsInvalidCastException()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            IPropertyBag bag = Activator.CreateInstance(type) as IPropertyBag;

            string message = "Unable to cast object of type 'System.Object' to type 'System.String'.";
            Assert.That(() => bag.SetValue("Property", new object()),
                Throws.TypeOf<InvalidCastException>()
                      .And.Message.Contains(message));
        }

        [Test]
        public void CreatedType_AsIPropertyBag_SetValue_SetsPropertyValue()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");
            builder.AddProperty("Property", typeof(string));

            Type type = builder.Build();
            IPropertyBag bag = Activator.CreateInstance(type) as IPropertyBag;
            bag.SetValue("Property", "expected_value");

            PropertyInfo propertyInfo = type.GetProperty("Property");
            Assert.That(propertyInfo.GetValue(bag, null), Is.EqualTo("expected_value"));
        }
    }
}
