
namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoSerializer
    {
        string Extension { get; }

        string Serialize(ClassInfo type);
    }
}
