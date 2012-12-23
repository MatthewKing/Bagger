namespace SimpleTypeBuilder
{
    using System;

    /// <summary>
    /// Defines a property that will be included in a dynamically generated IPropertyBag.
    /// </summary>
    internal sealed class PropertyDefinition
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PropertyDefinition class.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Type of the property.</param>
        public PropertyDefinition(string name, Type type)
        {
            if (name == null)
                throw new ArgumentNullException("name", "name should not be null.");
            if (name.Length == 0)
                throw new ArgumentException("name should not be an empty string.", "name");
            if (type == null)
                throw new ArgumentNullException("type", "type should not be null.");

            this.Name = name;
            this.Type = type;
        }
    }
}
