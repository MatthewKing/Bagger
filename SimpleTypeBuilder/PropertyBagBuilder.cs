namespace SimpleTypeBuilder
{
    using System;
    using System.Collections.Generic;

    public sealed class PropertyBagBuilder
    {
        private readonly string name;

        private readonly IList<PropertyDefinition> properties;

        public PropertyBagBuilder()
            : this("DynamicType" + Guid.NewGuid().ToString("N").ToUpper()) { }

        public PropertyBagBuilder(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name", "name should not be null.");
            if (name.Length == 0)
                throw new ArgumentException("name should not be an empty string.", "name");

            this.name = name;
            this.properties = new List<PropertyDefinition>();
        }

        public PropertyBagBuilder AddProperty(string name, Type type)
        {
            if (name == null)
                throw new ArgumentNullException("name", "name should not be null.");
            if (name.Length == 0)
                throw new ArgumentException("name should not be an empty string.", "name");
            if (type == null)
                throw new ArgumentNullException("type", "type should not be null.");

            PropertyDefinition property = new PropertyDefinition(name, type);

            this.properties.Add(property);

            return this;
        }

        private Type Build()
        {
            // ToDo: Add implementation :)

            return null;
        }
    }
}
