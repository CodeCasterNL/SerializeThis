using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public class TypeSymbolParser
    {
        private readonly Dictionary<string, Class> _typesSeen = new Dictionary<string, Class>();

        public Class GetMemberInfoRecursive(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            Class memberInfo = GetMemberInfoRecursive(typeSymbol.Name, typeSymbol);

            return memberInfo;
        }

        private Class GetMemberInfoRecursive(string name, ITypeSymbol typeSymbol)
        {
            // TODO: this will break. Include assembly name with type name?
            // TODO: this is already broken. We need to save "membername-typeInfo" tuples. Members can occur multiple times within the same or multiple types with different or equal names.
            string typeName = typeSymbol.GetTypeName();
            Class existing;
            if (_typesSeen.TryGetValue(typeName, out existing))
            {
                return existing;
            }

            bool isCollection;
            bool isNullableValueType;
            bool isEnum;
            var type = GetSymbolType(typeSymbol, out isCollection, out isNullableValueType, out isEnum);

            existing = new Class
            {
                Name = name,
                Type = type,
                IsCollection = isCollection,
                IsNullableValueType = isNullableValueType,
                IsEnum = isEnum,
                TypeName = typeName,
            };

            // Save it _before_ diving into children. 
            // TODO: will that work for a property of A.B.A.B?
            _typesSeen[typeName] = existing;

            if (existing.IsCollection)
            {
                // A collection's first child is its collection type.
                var collectionType = GetCollectionType(typeSymbol);
                existing.Children = collectionType != null ? new List<Class> { collectionType } : new List<Class>();
            }
            else if (existing.Type == TypeEnum.ComplexType && !isNullableValueType)
            {
                existing.Children = GetChildProperties(typeSymbol);
            }

            return existing;
        }

        private Class GetCollectionType(ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iCollectionInterface = typeSymbol.GetICollectionTInterface();

            var collectionElementType = iCollectionInterface.TypeArguments.FirstOrDefault();

            if (collectionElementType != null)
            {
                return GetMemberInfoRecursive("CollectionElement", collectionElementType);
            }

            return null;
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

            isEnum = namedTypeSymbol.IsEnum();
            isNullableValueType = typeSymbol.IsNullableType();

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            isCollection = typeSymbol.SpecialType != SpecialType.System_String && typeSymbol.IsCollectionType();

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
    }
}
