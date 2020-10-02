using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public class TypeSymbolParser
    {
        public ClassInfo GetMemberInfoRecursive(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            var memberInfo = GetMemberInfoRecursive(typeSymbol.GetTypeName(), typeSymbol);

            return memberInfo;
        }

        private ClassInfo GetMemberInfoRecursive(string name, ITypeSymbol typeSymbol)
        {
            var typesSeen = new Dictionary<string, Class>();

            // TODO: this will break. Include assembly name with type name?
            // TODO: this is already broken. We need to save "membername-typeInfo" tuples. Members can occur multiple times within the same or multiple types with different or equal names.
            string typeName = typeSymbol.GetTypeName();
            if (typesSeen.TryGetValue(typeName, out var c))
            {
                return new ClassInfo
                {
                    Name = name,
                    Class = c
                };
            }

            var type = GetSymbolType(typeSymbol, out var isCollection, out var isDictionary, out var isNullableValueType, out var isEnum);

            c = new Class
            {
                Type = type,
                IsCollection = isCollection,
                IsDictionary = isDictionary,
                IsNullableValueType = isNullableValueType,
                IsEnum = isEnum,
                TypeName = typeName,
            };

            // Save it _before_ diving into children. 
            // TODO: will that work for a property of A.B.A.B?
            typesSeen[typeName] = c;

            // TODO: handle _all_ generics. There can be a Foo<T1, T2> that (indirectly) inherits List<T2> and adds additional properties...
            // TODO: though for example JSON can't handle that, and does the C# object and collection initializer?
            if (c.IsDictionary)
            {
                var keyValueType = GetDictionaryKeyType(typeSymbol);
                if (keyValueType?.Item1 != null && keyValueType.Item2 != null)
                {
                    c.GenericParameters.Add(keyValueType.Item1);
                    c.GenericParameters.Add(keyValueType.Item2);
                }
            }
            else if (c.IsCollection)
            {
                var collectionType = GetCollectionType(typeSymbol);
                if (collectionType != null)
                {
                    c.GenericParameters.Add(collectionType);
                }
            }
            else if (c.Type == TypeEnum.ComplexType && !isNullableValueType)
            {
                foreach (var child in GetChildProperties(typeSymbol))
                {
                    c.Children.Add(child);
                }
            }

            return new ClassInfo
            {
                Name = name,
                Class = c
            };
        }

        private ClassInfo GetCollectionType(ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iCollectionInterface = typeSymbol.GetICollectionTInterface();

            var collectionElementType = iCollectionInterface.TypeArguments.FirstOrDefault();

            if (collectionElementType != null)
            {
                return GetMemberInfoRecursive("CollectionElementType", collectionElementType);
            }

            return null;
        }

        private Tuple<ClassInfo, ClassInfo> GetDictionaryKeyType(ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iCollectionInterface = typeSymbol.GetIDictionaryTKeyTValueInterface();

            var keyType = iCollectionInterface.TypeArguments.FirstOrDefault();
            var valueType = iCollectionInterface.TypeArguments.Skip(1).FirstOrDefault();

            if (keyType != null && valueType != null)
            {
                var keyInfo = GetMemberInfoRecursive("KeyType", keyType);
                var valueInfo = GetMemberInfoRecursive("ValueType", valueType);

                return Tuple.Create(keyInfo, valueInfo);
            }

            return null;
        }

        private IList<ClassInfo> GetChildProperties(ITypeSymbol typeSymbol)
        {
            var result = new List<ClassInfo>();

            // Walking up the inheritance tree. Root is System.Object without any more BaseTypes.
            if (typeSymbol.BaseType != null)
            {
                result.AddRange(GetChildProperties(typeSymbol.BaseType));
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member.Kind == SymbolKind.Property && member.DeclaredAccessibility == Accessibility.Public)
                {
                    if (member is IPropertySymbol memberTypeSymbol)
                    {
                        result.Add(GetMemberInfoRecursive(memberTypeSymbol.Name, memberTypeSymbol.Type));
                    }
                }
            }

            return result;
        }

        private TypeEnum GetSymbolType(ITypeSymbol typeSymbol, out bool isCollection, out bool isDictionary, out bool isNullableValueType, out bool isEnum)
        {
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;

            isEnum = namedTypeSymbol.IsEnum();
            isNullableValueType = typeSymbol.IsNullableType();

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            isCollection = typeSymbol.SpecialType != SpecialType.System_String && typeSymbol.IsCollectionType();

            isDictionary = isCollection && typeSymbol.IsDictionaryType();

            if (isNullableValueType)
            {
                var nullableType = namedTypeSymbol?.TypeArguments.FirstOrDefault();
                if (nullableType != null)
                {
                    var nullableTypeEnum = GetSymbolType(nullableType, out _, out _, out _, out _);
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
