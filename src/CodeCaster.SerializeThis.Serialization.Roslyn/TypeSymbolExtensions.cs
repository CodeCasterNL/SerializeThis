using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public static class TypeSymbolExtensions
    {
        public static bool IsEnum(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol?.EnumUnderlyingType != null;
        }

        public static INamedTypeSymbol GetICollectionTInterface(this ITypeSymbol typeSymbol)
        {
            return GetInterface(typeSymbol, x => x.IsCollectionInterfaceType());
        }

        public static INamedTypeSymbol GetIDictionaryTKeyTValueInterface(this ITypeSymbol typeSymbol)
        {
            return GetInterface(typeSymbol, x => x.IsDictionaryInterfaceType());
        }

        public static bool IsNullableType(this ITypeSymbol typeSymbol)
        {
            if (!(typeSymbol is INamedTypeSymbol named))
            {
                return false;
            }

            return typeSymbol.TypeKind == TypeKind.Struct && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
        }

        public static string GetTypeName(this ITypeSymbol typeSymbol, bool withGenericParameterNames = false)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    genericsOptions: withGenericParameterNames ? SymbolDisplayGenericsOptions.IncludeTypeParameters : SymbolDisplayGenericsOptions.None,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            );

            return typeSymbol.ToDisplayString(symbolDisplayFormat);
        }

        public static bool IsDictionaryType(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetIDictionaryTKeyTValueInterface() != null;
        }

        public static bool IsCollectionType(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetICollectionTInterface() != null;
        }

        private static INamedTypeSymbol GetInterface(ITypeSymbol typeSymbol, Func<ITypeSymbol, bool> testFunction)
        {
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && testFunction(namedTypeSymbol))
            {
                return namedTypeSymbol;
            }

            var iCollectionInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => testFunction(i));
            return iCollectionInterface;
        }

        public static bool IsArray(this ITypeSymbol typeSymbol)
        {
            // TODO: meh.
            return typeSymbol.BaseType?.GetTypeName() == TypeNameConstants.Array;
        }

        private static bool IsCollectionInterfaceType(this ITypeSymbol typeSymbol)
        {
            // TODO: meh.
            return typeSymbol.GetTypeName().StartsWith(TypeNameConstants.CollectionInterface)
                   && typeSymbol is INamedTypeSymbol n && n.TypeArguments.Length == 1;
        }

        private static bool IsDictionaryInterfaceType(this ITypeSymbol typeSymbol)
        {
            // TODO: meh.
            return typeSymbol.GetTypeName().StartsWith(TypeNameConstants.DictionaryInterface)
                   && typeSymbol is INamedTypeSymbol n && n.TypeArguments.Length == 2;
        }

        public static string GetArgOrNamedProperty(this AttributeData attribute, int? constructorArgumentIndex, string propertyName)
        {
            if (constructorArgumentIndex.HasValue && attribute.ConstructorArguments.Length == constructorArgumentIndex + 1)
            {
                return attribute.ConstructorArguments[0].Value?.ToString();
            }

            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                foreach (var namedArg in attribute.NamedArguments)
                {
                    if (namedArg.Key == propertyName)
                    {
                        return namedArg.Value.Value?.ToString();
                    }
                }
            }

            return null;
        }
    }
}
