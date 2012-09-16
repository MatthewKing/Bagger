namespace SimpleTypeBuilder
{
    public interface IPropertyBag
    {
        object GetValue(string propertyName);

        void SetValue(string propertyName, object value);
    }
}
