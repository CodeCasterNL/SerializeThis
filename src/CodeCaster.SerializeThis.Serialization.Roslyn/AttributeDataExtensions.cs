using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public static class AttributeDataExtensions
    {
        public static AttributeInfo[] Map(this ImmutableArray<AttributeData> attributes)
        {
            return attributes == null
                ? Array.Empty<AttributeInfo>()
                : attributes.Select(a => new AttributeInfo
                {
                    Name = a.AttributeClass.GetTypeName(withGenericParameterNames: true),
                    ConstructorArguments = a.ConstructorArguments.Select(ca => ca.Value).ToArray(),
                    Properties = a.NamedArguments.ToDictionary(na => na.Key, na => na.Value.Value),
                })
                .ToArray();
        }
    }
}
