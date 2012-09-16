namespace SimpleTypeBuilder.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    internal sealed class PropertyDefinitionTests
    {
        [Test]
        public void Constructor_NameIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new PropertyDefinition(null, typeof(string)),
                Throws.TypeOf<ArgumentNullException>()
                      .And.Message.Contains("name should not be null."));
        }

        [Test]
        public void Constructor_NameIsEmpty_ThrowsArgumentException()
        {
            Assert.That(() => new PropertyDefinition(String.Empty, typeof(string)),
                Throws.TypeOf<ArgumentException>()
                      .And.Message.Contains("name should not be an empty string."));
        }

        [Test]
        public void Constructor_TypeIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new PropertyDefinition("name", null),
                Throws.TypeOf<ArgumentNullException>()
                      .And.Message.Contains("type should not be null."));
        }
    }
}
