using System.Collections.Generic;

namespace SerializeThis.Serialization
{
    /// <summary>
    /// A complex or value type. Complex can be any of a few known collection types, or is treated as a class.
    /// </summary>
    public class TypeInfo
    {
        public string TypeName { get; set; }

        public TypeEnum Type { get; set; }

        public bool IsComplexType => Type == TypeEnum.ComplexType;

        public bool IsCollectionType => CollectionType.HasValue;

        public bool IsNullableValueType { get; set; }

        public bool IsEnum { get; set; }

        public CollectionType? CollectionType { get; set; }

        public IList<MemberInfo> Children { get; set; } = new List<MemberInfo>();
        
        public IList<MemberInfo> GenericParameters { get; set; } = new List<MemberInfo>();

        public IList<AttributeInfo> Attributes { get; set; } = new List<AttributeInfo>();
    }
}
