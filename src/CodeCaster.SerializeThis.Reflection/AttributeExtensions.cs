using CodeCaster.SerializeThis.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeCaster.SerializeThis.Reflection
{
    public static class AttributeExtensions
    {
        public static AttributeInfo[] Map(this IEnumerable<Attribute> attributes)
        {
            return attributes == null
                ? Array.Empty<AttributeInfo>()
                : attributes.Select(Map).ToArray();
        }

        public static AttributeInfo Map(Attribute a)
        {
            var attributeType = a.GetType();

            var info = new AttributeInfo
            {
                Name = attributeType.FullName,
                Properties = attributeType.GetProperties()
                                          .Select(p => new { p.Name, Value = p.GetValue(a) })
                                          .Where(p => p.Value != null)
                                          .ToDictionary(na => na.Name, na => na.Value),
            };

            return info;
        }
    }
}
