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

            var type = GetSymbolType(typeSymbol, out var collectionType, out var isNullableValueType, out var isEnum, out var genericParameters);

            c = new Class
            {
                Type = type,
                CollectionType= collectionType,
                IsNullableValueType = isNullableValueType,
                IsEnum = isEnum,
                TypeName = typeName,
            };

            // Save it _before_ diving into children. 
            // TODO: will that work for a property of A.B.A.B? We need to pass typesSeen around recursively.
            typesSeen[typeName] = c;

            // TODO: handle _all_ generics. There can be a Foo<T1, T2> that (indirectly) inherits List<T2> and adds additional properties...
            // TODO: though for example JSON can't handle that, and do the C# object and collection initializer combine?
            if (c.CollectionType == CollectionType.Dictionary)
            {
                var keyValueType = GetDictionaryKeyType(typeSymbol);
                if (keyValueType?.Item1 != null && keyValueType.Item2 != null)
                {
                    c.GenericParameters.Add(keyValueType.Item1);
                    c.GenericParameters.Add(keyValueType.Item2);
                }
            }
            else if (c.CollectionType == CollectionType.Collection)
            {
                var collectionTypeParameter = GetCollectionTypeParameter(typeSymbol);
                if (collectionTypeParameter != null)
                {
                    c.GenericParameters.Add(collectionTypeParameter);
                }
            }
            else if (c.CollectionType == CollectionType.Array)
            {
                var arrayTypeParameter = GetArrayTypeParameter(typeSymbol);
                if (arrayTypeParameter != null)
                {
                    c.GenericParameters.Add(arrayTypeParameter);
                }
            }
            else if (c.Type == TypeEnum.ComplexType && !isNullableValueType)
            {
                foreach (var gp in genericParameters)
                {
                    c.GenericParameters.Add(gp);
                }
                
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

        /// <summary>
        /// For a T[] array, return the <see cref="ClassInfo"/> of T.
        /// </summary>
        private ClassInfo GetArrayTypeParameter(ITypeSymbol typeSymbol)
        {
            if (!(typeSymbol is IArrayTypeSymbol arrayType))
            {
                return null;
            }

            //arrayType.Sizes

            return GetMemberInfoRecursive("ArrayElementType", arrayType.ElementType);
        }

        /// <summary>
        /// For an ICollection{T}-implementing type, return the <see cref="ClassInfo"/> of T.
        /// </summary>
        private ClassInfo GetCollectionTypeParameter(ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iCollectionInterface = typeSymbol.GetICollectionTInterface();

            var collectionElementType = iCollectionInterface.TypeArguments.FirstOrDefault();

            if (collectionElementType != null)
            {
                return GetMemberInfoRecursive("CollectionElementType", collectionElementType);
            }

            return null;
        }

        /// <summary>
        /// For an IDictionary{TKey, TValue}, return the <see cref="ClassInfo"/> of TKey and TValue.
        /// </summary>
        private Tuple<ClassInfo, ClassInfo> GetDictionaryKeyType(ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iDictionarynInterface = typeSymbol.GetIDictionaryTKeyTValueInterface();

            var keyType = iDictionarynInterface.TypeArguments.FirstOrDefault();
            var valueType = iDictionarynInterface.TypeArguments.Skip(1).FirstOrDefault();

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

        private TypeEnum GetSymbolType(ITypeSymbol typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, out bool isEnum, out List<ClassInfo> typeParameters)
        {
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
            
            isEnum = namedTypeSymbol.IsEnum();
            isNullableValueType = typeSymbol.IsNullableType();

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            var isArray = typeSymbol.BaseType?.GetTypeName()== "System.Array";
            var isCollection = typeSymbol.SpecialType != SpecialType.System_String && typeSymbol.IsCollectionType();
            var isDictionary = isCollection && typeSymbol.IsDictionaryType();
            
            // TODO: we want to support most collection types, as quick starting point ICollection<T> was chosen to detect them.
            // TODO: the proper way would be to look for an Add() method or indexer property, but what would be the appropriate type in the first case?.
            // TODO: also, arrays can be jagged or multidimensional... what code to generate?
            collectionType = null;
            if (isDictionary)
            {
                collectionType = CollectionType.Dictionary;
            }
            else if (isArray)
            {
                collectionType = CollectionType.Array;
            }
            else if (isCollection)
            {
                collectionType = CollectionType.Collection;
            }

            typeParameters = new List<ClassInfo>();
            if (namedTypeSymbol?.TypeArguments.Any() == true)
            {
                foreach (var typeArg in namedTypeSymbol.TypeArguments)
                {
                    typeParameters.Add(GetMemberInfoRecursive(typeArg.Name, typeArg));
                }
            }

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
