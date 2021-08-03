namespace SerializeThis.Serialization
{
    public interface IClassInfoBuilder<T> : ITypeInfoProvider
    {
        MemberInfo GetMemberInfoRecursive(string objectName, T typeSymbol, object instance);
    }
}