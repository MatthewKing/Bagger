namespace SimpleTypeBuilder
{
    /// <summary>
    /// Represents a property bag that can get and set properties by name.
    /// </summary>
    public interface IPropertyBag
    {
        /// <summary>
        /// Returns the value of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the specified property.</returns>
        object GetValue(string propertyName);

        /// <summary>
        /// Sets the value of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the specified property.</param>
        void SetValue(string propertyName, object value);
    }
}
