namespace SimpleTypeBuilder
{
    using System;

    internal sealed class PropertyDefinition
    {
        public string Name { get; private set; }

        public Type Type { get; private set; }

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
