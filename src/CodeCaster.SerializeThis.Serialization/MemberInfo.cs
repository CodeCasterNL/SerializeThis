using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeCaster.SerializeThis.Serialization
{
    [DebuggerDisplay("{ToDebugString(),nq}")]
    public class MemberInfo
    {
        public string Name { get; set; }

        /// <summary>
        /// Property attributes.
        /// </summary>
        public ICollection<AttributeInfo> Attributes { get; set; } = Array.Empty<AttributeInfo>();

        public TypeInfo Class { get; set; }

        public string ToDebugString()
        {
            return $"{Class.TypeName} {Name}";
        }
    }
}
