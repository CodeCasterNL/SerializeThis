namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoBuilder<T>
    {
        MemberInfo GetMemberInfoRecursive(string objectName, T typeSymbol, object instance);
    }
}