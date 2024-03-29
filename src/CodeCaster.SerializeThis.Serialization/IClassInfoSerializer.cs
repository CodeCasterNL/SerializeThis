﻿namespace CodeCaster.SerializeThis.Serialization
{
    public interface IClassInfoSerializer
    {
        /// <summary>
        /// The file extension, without dot.
        /// </summary>
        string FileExtension { get; }
        
        /// <summary>
        /// What to show this in dialogs.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Generates initialization code for the given <paramref name="type"/></para>.
        /// </summary>
        string Serialize(ClassInfo type);
        
        /// <summary>
        /// Asks whether this serializer can handle the given type.
        /// </summary>
        bool CanSerialize(ClassInfo type);
    }
}
