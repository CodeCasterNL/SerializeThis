namespace SerializeThis.Serialization
{
    public interface ITypeInfoProvider
    {
        TypeInfo GetTypeInfo(string typeName);
    }
}