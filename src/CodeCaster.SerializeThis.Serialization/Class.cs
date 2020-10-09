using System.Collections.Generic;

namespace CodeCaster.SerializeThis.Serialization
{
    public class Class
    {
        public TypeEnum Type { get; set; }

        public string TypeName { get; set; }

        public bool IsNullableValueType { get; set; }

        public CollectionType? CollectionType { get; set; }
        
        public bool IsEnum { get; set; }

        public IList<ClassInfo> Children { get; set; } = new List<ClassInfo>();
        
        public IList<ClassInfo> GenericParameters { get; set; } = new List<ClassInfo>();

        public bool IsComplexType => Type == TypeEnum.ComplexType;
    }
}
