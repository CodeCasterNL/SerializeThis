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
            // TODO: meh.
            var iCollectionInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => i.GetTypeName().StartsWith("System.Collections.Generic.ICollection<"));
            return iCollectionInterface;
        }

        public static bool IsNullableType(this ITypeSymbol typeSymbol)
        {
            var named = typeSymbol as INamedTypeSymbol;
            if (named == null)
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

        public static bool IsCollectionType(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetICollectionTInterface() != null;

            // TODO: store known collection collection somewhere with their interface info, so we know which properties to ignore (Item[], Syncroot, Count, AllKeys, ...).
            //string[] knownCollectionInterfaces =
            //{
            //    "System.Collections.IList",
            //    "System.Collections.ICollection",
            //    "System.Collections.IEnumerable",
            //    "System.Collections.Generic.IList`1",
            //    "System.Collections.Generic.ICollection`1",
            //    "System.Collections.Generic.IEnumerable`1",
            //    "System.Collections.Generic.IReadOnlyList`1",
            //    "System.Collections.Generic.IReadOnlyCollection`1",
            //};

            // TODO: do we also want to treat dictionaries differently? Or let the serializer deal with that?
            //string[] knownDictionaryMemberTypes =
            //{
            //    "System.Tuple<,>",
            //    "System.Collections.Generic.KeyValuePair<,>",
            //};

            // TODO: refactor somehow.
            //return typeSymbol.AllInterfaces.Any(i => knownCollectionInterfaces.Any(c => c == i.GetTypeName()));
        }
    }
}
