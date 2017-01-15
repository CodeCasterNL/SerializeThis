using System.Collections.Generic;

namespace CodeCaster.SerializeThis.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public class Class
    {
        public string Name { get; set; }

        public TypeEnum Type { get; set; }

        public bool IsNullableValueType { get; set; }

        public bool IsCollection { get; set; }

        public bool IsEnum { get; set; }

        public IList<Class> Children { get; set; }

        public bool IsComplexType => Type == TypeEnum.ComplexType;
    }
}
