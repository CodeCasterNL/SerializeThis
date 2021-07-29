namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoBuilder<T>
    {
        ClassInfo GetMemberInfoRecursive(T typeSymbol, object instance);
    }
}