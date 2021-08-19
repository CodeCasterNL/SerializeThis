using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;

namespace SerializeThis.Serialization.Roslyn
{
    public static class AttributeDataExtensions
    {
        public static IList<AttributeInfo> Map(this ImmutableArray<AttributeData> attributes)
        {
            return attributes == null
                ? Array.Empty<AttributeInfo>()
                : attributes.Select(a => new AttributeInfo
                {
                    Name = a.AttributeClass.GetTypeName(withGenericParameterNames: true),
                    ConstructorArguments = a.ConstructorArguments.Select(GetValue).ToArray(),
                    Properties = a.NamedArguments.ToDictionary(na => na.Key, na => GetValue(na.Value)),
                })
                .ToArray();
        }

        private static object GetValue(TypedConstant typedConstant)
        {
            switch (typedConstant.Kind)
            {
                case TypedConstantKind.Array:
                    return typedConstant.Values.Select(GetValue).ToArray();

                case TypedConstantKind.Primitive:
                case TypedConstantKind.Enum:
                    return typedConstant.Value;

                case TypedConstantKind.Type:
                    return typedConstant.Value is ITypeSymbol typeSymbol
                        ? $"typeof({typeSymbol.GetTypeName()})"
                        : typedConstant.Value?.GetType().Name;

                case TypedConstantKind.Error:
                default:
                    return null;
            }
        }
    }
}
