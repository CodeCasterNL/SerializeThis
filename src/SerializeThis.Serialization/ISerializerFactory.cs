namespace SerializeThis.Serialization
{
    public interface ISerializerFactory
    {
        IClassInfoSerializer GetSerializer(object serializer, IPropertyValueProvider valueProvider);
    }
}
