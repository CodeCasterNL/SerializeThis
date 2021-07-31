namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoBuilder<T>
    {
        ClassInfo GetMemberInfoRecursive(string objectName, T typeSymbol, object instance);
    }
}