using System;

namespace CodeCaster.SerializeThis.Serialization
{
    public class ClassInfo
    {
        public string Name { get; set; }

        /// <summary>
        /// Property attributes.
        /// </summary>
        public AttributeInfo[] Attributes { get; set; } = Array.Empty<AttributeInfo>();

        public Class Class { get; set; }
    }
}
