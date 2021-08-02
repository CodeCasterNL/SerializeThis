using System.Collections.Generic;

namespace SerializeThis.Serialization
{
    public class AttributeInfo
    {
        public string Name { get; set; }

        public object[] ConstructorArguments { get; set; }

        public Dictionary<string, object> Properties { get; set; }
    }
}
