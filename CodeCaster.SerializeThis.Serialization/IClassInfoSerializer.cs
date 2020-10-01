namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoSerializer
    {
        /// <summary>
        /// The file extension, without dot.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Generates initialization code for the given <paramref name="type"/></para>.
        /// </summary>
        string Serialize(ClassInfo type);
    }
}
