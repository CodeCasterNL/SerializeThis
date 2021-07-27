using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public class TypeSymbolParser
    {
        // TODO: this makes it pretty much not thread safe, might we ever need that, store the class/info stuff in a more stateful object?
        private readonly Dictionary<string, Class> _typesSeen = new Dictionary<string, Class>();

        public ClassInfo GetMemberInfoRecursive(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            _typesSeen.Clear();
            var memberInfo = GetMemberInfoRecursive(typeSymbol.GetTypeName(), typeSymbol);
            return memberInfo;
        }

        private ClassInfo GetMemberInfoRecursive(string name, ITypeSymbol typeSymbol)
        {
            var returnValue = new ClassInfo
            {
                Name = name
            };

            // TODO: this will break. Include assembly name with type name?
            var fullTypeName = typeSymbol.GetTypeName(withGenericParameterNames: true);
            if (_typesSeen.TryGetValue(fullTypeName, out var c))
            {
                returnValue.Class = c;
                return returnValue;
            }

            // Save it _before_ diving into members and type parameters. 
            var typeName = typeSymbol.GetTypeName();
            c = new Class
            {
                TypeName = typeName
            };
            _typesSeen[fullTypeName] = c;

            var type = GetSymbolType(typeSymbol, out var collectionType, out var isNullableValueType, out var isEnum, out var genericParameters);

            c.Type = type;
            c.CollectionType = collectionType;
            c.IsNullableValueType = isNullableValueType;
            c.IsEnum = isEnum;

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

            returnValue.Class = c;
            return returnValue;
        }

        /// <summary>
        /// Considers attributes.
        /// </summary>
        private string GetPropertyNameFromContext(string name, ITypeSymbol typeSymbol, ImmutableArray<AttributeData> attributes)
        {
            foreach (var attr in attributes)
            {
                var attributeName = attr.AttributeClass.GetTypeName(withGenericParameterNames: true);

                switch (attributeName)
                {
                    case "Newtonsoft.Json.JsonPropertyAttribute":
                        return attr.GetArgOrNamedProperty(0, "PropertyName") ?? name;

                    case "System.Text.Json.JsonPropertyNameAttribute":
                        return attr.GetArgOrNamedProperty(0, "Name") ?? name;

                    // [DataMember] only applies inside a class marked with [DataContract].
                    case "System.Runtime.Serialization.DataMemberAttribute":
                        if (typeSymbol.GetAttributes().Any(a => a.AttributeClass.GetTypeName() == "System.Runtime.Serialization.DataContractAttribute"))
                        {
                            return attr.GetArgOrNamedProperty(null, "Name") ?? name;
                        }
                        break;
                    default: 
                        continue;
                }

            }

            return name;
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

            if (arrayType.Rank > 1)
            {
                // TODO: proper T[,] support.
                return GetMemberInfoRecursive("ArrayElementType", arrayType.ElementType);
            }

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
                        var name = GetPropertyNameFromContext(memberTypeSymbol.Name, typeSymbol, memberTypeSymbol.GetAttributes());
                        result.Add(GetMemberInfoRecursive(name, memberTypeSymbol.Type));
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

            var isArray = typeSymbol.IsArray();

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            var isCollection = typeSymbol.SpecialType != SpecialType.System_String && typeSymbol.IsCollectionType();

            var isDictionary = isCollection && typeSymbol.IsDictionaryType();
            if (isDictionary)
            {
                // And don't count dictionaries as collections.
                isCollection = false;
            }

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
