namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoBuilder
    {
        ClassInfo BuildObjectTree(object value);
    }
}