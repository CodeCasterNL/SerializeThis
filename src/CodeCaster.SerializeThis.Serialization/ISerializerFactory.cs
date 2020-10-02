namespace CodeCaster.SerializeThis.Serialization
{
    public interface ISerializerFactory
    {
        IClassInfoSerializer GetSerializer(object serializer);
    }
}
