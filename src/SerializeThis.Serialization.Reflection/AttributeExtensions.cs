using SerializeThis.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SerializeThis.Serialization.Reflection
{
    public static class AttributeExtensions
    {
        /// <summary>
        /// It's an IList for indexing, but don't you dare add something.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IList<AttributeInfo> Map(this IEnumerable<Attribute> attributes)
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
