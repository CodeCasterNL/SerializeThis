using System;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.Reflection
{
    public class ClassInfoBuilder : IClassInfoBuilder
    {
        public ClassInfo BuildObjectTree(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var type = value.GetType();

            return new ClassInfo
            {
                Name = "value",
                Class = new Class
                {
                    TypeName = type.FullName
                }
            };
        }
    }
}
