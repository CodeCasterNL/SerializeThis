using System;
using System.Collections.Generic;
using System.Reflection;

namespace SerializeThis.Serialization.Reflection
{
    public class ClassInfoBuilder : SymbolParser<Type>
    {
        protected override string GetClassName(Type typeSymbol) => typeSymbol.GetNameWithoutGenerics();
        
        // TODO: this will break. Include assembly name with type name?
        protected override string GetCacheName(Type typeSymbol) => typeSymbol.FullName;

        protected override bool IsEnum(Type typeSymbol) => typeSymbol.IsEnum;

        protected override IList<AttributeInfo> GetAttributes(Type typeSymbol) => typeSymbol.GetCustomAttributes().Map();
        
        protected override IList<Serialization.MemberInfo> GetChildProperties(Type type, object value)
        {
            var result = new List<Serialization.MemberInfo>();

            // First go to the root, the first class deriving from System.Object, then walk up to "kind of" order the properties.
            // No guarantees are made on the order in the docs though.
            var baseType = type.BaseType;
            if (baseType != null && baseType != typeof(object))
            {
                result.AddRange(GetChildProperties(baseType, value));
            }

            // Then iterate over this type's public instance properties, not the inherited ones.
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var propertyValue = value == null ? null : p.GetValue(value);
                result.Add(GetMemberInfoRecursive(p.Name, p.PropertyType, p.GetCustomAttributes().Map(), propertyValue));
            }

            return result;
        }

        protected override TypeEnum GetComplexSymbolType(Type typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, out bool isAnonymousType, ref bool isEnum, out IList<Serialization.MemberInfo> typeParameters)
        {
            isNullableValueType = System.Nullable.GetUnderlyingType(typeSymbol) != null;

            isAnonymousType = typeSymbol.IsAnonymousType();

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            var isCollection = typeSymbol != typeof(string)
                            && typeSymbol.IsCollectionType();

            var isArray = typeSymbol.IsArray;
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

            typeParameters = new List<Serialization.MemberInfo>();

            // TODO: generics and nullables.

            return 0;
        }

        private static readonly Dictionary<Type, TypeEnum> KnownValueTypes = new Dictionary<Type, TypeEnum>
        {
            { typeof(Boolean), TypeEnum.Boolean },
            { typeof(String), TypeEnum.String },
            { typeof(Char), TypeEnum.Char },
            { typeof(Byte), TypeEnum.Byte },
            { typeof(Int16), TypeEnum.Int16 },
            { typeof(Int32), TypeEnum.Int32 },
            { typeof(Int64), TypeEnum.Int64 },
            { typeof(Single), TypeEnum.Float32 },
            { typeof(Double), TypeEnum.Float64 },
            { typeof(Decimal), TypeEnum.Decimal },
            { typeof(DateTime), TypeEnum.DateTime },
        };

        protected override TypeEnum? GetKnownValueType(Type typeSymbol)
        {
            return KnownValueTypes.TryGetValue(typeSymbol, out var knownType)
                ? knownType
                : (TypeEnum?)null;
        }

        protected override Serialization.MemberInfo GetArrayTypeParameter(Type typeSymbol)
        {
            if (!typeSymbol.IsArray)
            {
                throw new ArgumentException($"{typeSymbol.FullName} is not an array");
            }

            if (typeSymbol.GetArrayRank() > 1)
            {
                // TODO: proper T[,] support.
                return GetMemberInfoRecursive("ArrayElementType", typeSymbol.MakeArrayType());
            }

            return GetMemberInfoRecursive("ArrayElementType", typeSymbol.GetElementType());
        }

        protected override (Serialization.MemberInfo TKey, Serialization.MemberInfo TValue) GetDictionaryKeyType(Type typeSymbol)
        {
            var dictionaryInterface = typeSymbol.GetIDictionaryTKeyTValueInterface();

            var keyType = dictionaryInterface.GenericTypeArguments[0];
            var valueType = dictionaryInterface.GenericTypeArguments[1];

            var keyInfo = GetMemberInfoRecursive("KeyType", keyType);
            var valueInfo = GetMemberInfoRecursive("ValueType", valueType);

            return (keyInfo, valueInfo);
        }

        protected override Serialization.MemberInfo GetCollectionTypeParameter(Type typeSymbol)
        {
            var collectionInterface = typeSymbol.GetICollectionTInterface();
            var collectionType = collectionInterface.GenericTypeArguments[0];
            return GetMemberInfoRecursive("CollectionType", collectionType);
        }
    }
}
