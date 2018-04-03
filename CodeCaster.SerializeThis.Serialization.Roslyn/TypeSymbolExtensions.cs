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

        public static string GetTypeName(this ITypeSymbol typeSymbol)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters
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

        private static bool IsCollectionInterfaceType(this ITypeSymbol arg)
        {
            // TODO: meh.
            return arg.GetTypeName().StartsWith("System.Collections.Generic.ICollection<");
        }

        private static bool IsDictionaryInterfaceType(this ITypeSymbol arg)
        {
            // TODO: meh.
            return arg.GetTypeName().StartsWith("System.Collections.Generic.IDictionary<");
        }
    }
}
