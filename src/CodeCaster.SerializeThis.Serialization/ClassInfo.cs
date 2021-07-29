using System;
using System.Collections.Generic;

namespace CodeCaster.SerializeThis.Serialization
{
    public class ClassInfo
    {
        public string Name { get; set; }

        /// <summary>
        /// Property attributes.
        /// </summary>
        public ICollection<AttributeInfo> Attributes { get; set; } = Array.Empty<AttributeInfo>();

        public Class Class { get; set; }
    }
}
