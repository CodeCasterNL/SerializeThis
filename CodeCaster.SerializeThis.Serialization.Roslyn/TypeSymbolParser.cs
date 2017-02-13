using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public class TypeSymbolParser
    {
        public Class GetMemberInfoRecursive(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            Class memberInfo = GetMemberInfoRecursive(typeSymbol.Name, typeSymbol);

            return memberInfo;
        }

        private Class GetMemberInfoRecursive(string name, ITypeSymbol typeSymbol)
        {
            bool isCollection;
            bool isNullableValueType;
            bool isEnum;
            var type = GetSymbolType(typeSymbol, out isCollection, out isNullableValueType, out isEnum);

            var thisClass = new Class
            {
                Name = name,
                Type = type,
                IsCollection = isCollection,
                IsNullableValueType = isNullableValueType,
                IsEnum = isEnum,
            };

            if (thisClass.IsCollection)
            {
                // A collection's first child is its collection type.
                var collectionType = GetCollectionType(thisClass, typeSymbol);
                thisClass.Children = collectionType != null ? new List<Class> { collectionType } : new List<Class>();
            }
            else if (thisClass.Type == TypeEnum.ComplexType && !isNullableValueType)
            {
                thisClass.Children = GetChildProperties(typeSymbol);
            }

            return thisClass;
        }

        private Class GetCollectionType(Class thisClass, ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iCollectionInterface = GetICollectionInterface(typeSymbol);

            var collectionElementType = iCollectionInterface.TypeArguments.FirstOrDefault();

            if (collectionElementType != null)
            {
                return GetMemberInfoRecursive("?", collectionElementType);
            }

            return null;
        }

        private INamedTypeSymbol GetICollectionInterface(ITypeSymbol typeSymbol)
        {
            var iCollectionInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => GetClrName(i) == "System.Collections.Generic.ICollection`1");
            return iCollectionInterface;
        }

        private IList<Class> GetChildProperties(ITypeSymbol typeSymbol)
        {
            var result = new List<Class>();

            // Walking up the inheritance tree. Root is System.Object without any more BaseTypes.
            if (typeSymbol.BaseType != null)
            {
                result.AddRange(GetChildProperties(typeSymbol.BaseType));
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member.Kind == SymbolKind.Property && member.DeclaredAccessibility == Accessibility.Public)
                {
                    var memberTypeSymbol = member as IPropertySymbol;
                    if (memberTypeSymbol != null)
                    {
                        result.Add(GetMemberInfoRecursive(memberTypeSymbol.Name, memberTypeSymbol.Type));
                    }
                }
            }

            return result;
        }

        private TypeEnum GetSymbolType(ITypeSymbol typeSymbol, out bool isCollection, out bool isNullableValueType, out bool isEnum)
        {
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;

            isEnum = IsEnum(namedTypeSymbol);
            isNullableValueType = IsNullableType(typeSymbol);

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            isCollection = typeSymbol.SpecialType != SpecialType.System_String && IsCollectionType(ref typeSymbol);

            if (isNullableValueType)
            {
                var nullableType = namedTypeSymbol?.TypeArguments.FirstOrDefault();
                if (nullableType != null)
                {
                    bool ignored;
                    var nullableTypeEnum = GetSymbolType(nullableType, out ignored, out ignored, out ignored);
                    return nullableTypeEnum;
                }
            }

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Byte:
                    return TypeEnum.Byte;
                case SpecialType.System_Boolean:
                    return TypeEnum.Boolean;
                case SpecialType.System_String:
                    return TypeEnum.String;
                case SpecialType.System_DateTime:
                    return TypeEnum.DateTime;
                case SpecialType.System_Int16:
                    return TypeEnum.Int16;
                case SpecialType.System_Int32:
                    return TypeEnum.Int32;
                case SpecialType.System_Int64:
                    return TypeEnum.Int64;
                case SpecialType.System_Single:
                    return TypeEnum.Float16;
                case SpecialType.System_Double:
                    return TypeEnum.Float32;
                case SpecialType.System_Decimal:
                    return TypeEnum.Decimal;
            }

            return TypeEnum.ComplexType;
        }

        private bool IsEnum(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol?.EnumUnderlyingType != null;
        }

        private bool IsNullableType(ITypeSymbol typeSymbol)
        {
            var named = typeSymbol as INamedTypeSymbol;
            if (named == null)
            {
                return false;
            }

            return typeSymbol.TypeKind == TypeKind.Struct && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
        }

        private bool IsCollectionType(ref ITypeSymbol typeSymbol)
        {
            // TODO: store known collection collection somewhere with their interface info, so we know which properties to ignore (Item[], Syncroot, Count, AllKeys, ...).
            string[] knownCollectionInterfaces =
            {
                "System.Collections.IList",
                "System.Collections.ICollection",
                "System.Collections.IEnumerable",
                "System.Collections.Generic.IList`1",
                "System.Collections.Generic.ICollection`1",
                "System.Collections.Generic.IEnumerable`1",
                "System.Collections.Generic.IReadOnlyList`1",
                "System.Collections.Generic.IReadOnlyCollection`1",
            };

            // TODO: do we also want to treat dictionaries differently? Or let the serializer deal with that?
            string[] knownDictionaryMemberTypes =
            {
                "System.Tuple<,>",
                "System.Collections.Generic.KeyValuePair<,>",
            };

            return typeSymbol.AllInterfaces.Any(i => knownCollectionInterfaces.Any(c => c == GetClrName(i)));
        }

        private string GetClrName(INamedTypeSymbol namedTypeSymbol)
        {
            string typeNamespace = GetNamespace(namedTypeSymbol.ContainingNamespace);
            return typeNamespace + namedTypeSymbol.MetadataName;
        }

        private string GetNamespace(INamespaceSymbol ns)
        {
            string result = "";
            while (!string.IsNullOrWhiteSpace(ns.Name))
            {
                result = ns.Name + "." + result;
                ns = ns.ContainingNamespace;
            }
            return result;
        }
    }
}
