namespace SimpleTypeBuilder.Tests
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
        public void AddProperty_ReturnsPropertyBagBuilderInstance()
        {
            PropertyBagBuilder builder = new PropertyBagBuilder("name");

            PropertyBagBuilder returnValue = builder.AddProperty("name", typeof(string));

            Assert.That(returnValue, Is.EqualTo(builder));
        }
    }
}
