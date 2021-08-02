using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SerializeThis.Serialization.Roslyn
{
    public class TypeSymbolParser : SymbolParser<ITypeSymbol>
    {
        protected override string GetClassName(ITypeSymbol typeSymbol) => typeSymbol.GetTypeName();

        // TODO: this will break. Include assembly name with type name?
        protected override string GetCacheName(ITypeSymbol typeSymbol) => typeSymbol.GetTypeName(withGenericParameterNames: true);

        protected override bool IsEnum(ITypeSymbol typeSymbol)
        {
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
            return namedTypeSymbol.IsEnum();
        }

        protected override IList<AttributeInfo> GetAttributes(ITypeSymbol typeSymbol) => typeSymbol.GetAttributes().Map();

        /// <summary>
        /// For a T[] array, return the <see cref="MemberInfo"/> of T.
        /// </summary>
        protected override MemberInfo GetArrayTypeParameter(ITypeSymbol typeSymbol)
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
        /// For an ICollection{T}-implementing type, return the <see cref="MemberInfo"/> of T.
        /// </summary>
        protected override MemberInfo GetCollectionTypeParameter(ITypeSymbol typeSymbol)
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
        /// For an IDictionary{TKey, TValue}, return the <see cref="MemberInfo"/> of TKey and TValue.
        /// </summary>
        protected override (MemberInfo, MemberInfo) GetDictionaryKeyType(ITypeSymbol typeSymbol)
        {
            INamedTypeSymbol iDictionarynInterface = typeSymbol.GetIDictionaryTKeyTValueInterface();

            var keyType = iDictionarynInterface.TypeArguments.FirstOrDefault();
            var valueType = iDictionarynInterface.TypeArguments.Skip(1).FirstOrDefault();

            if (keyType != null && valueType != null)
            {
                var keyInfo = GetMemberInfoRecursive("KeyType", keyType);
                var valueInfo = GetMemberInfoRecursive("ValueType", valueType);

                return (keyInfo, valueInfo);
            }

            return (null, null);
        }

        protected override IList<MemberInfo> GetChildProperties(ITypeSymbol typeSymbol, object value)
        {
            var result = new List<MemberInfo>();

            // Walking up the inheritance tree. Root is System.Object without any more BaseTypes.
            if (typeSymbol.BaseType != null)
            {
                result.AddRange(GetChildProperties(typeSymbol.BaseType, value));
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member.Kind == SymbolKind.Property && member.DeclaredAccessibility == Accessibility.Public)
                {
                    if (member is IPropertySymbol memberTypeSymbol)
                    {
                        result.Add(GetMemberInfoRecursive(memberTypeSymbol.Name, memberTypeSymbol.Type, memberTypeSymbol.GetAttributes().Map()));
                    }
                }
            }

            return result;
        }

        protected override TypeEnum GetComplexSymbolType(ITypeSymbol typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, ref bool isEnum, out IList<MemberInfo> typeParameters)
        {
            collectionType = null;
            

            //isAnonymousType = typeSymbol.IsAnonymousType;

            var isArray = typeSymbol.IsArray();

            // Don't count strings as collections, even though they implement IEnumerable<string>.
            var isCollection = typeSymbol.SpecialType != SpecialType.System_String 
                            && typeSymbol.IsCollectionType();

            var isDictionary = isCollection && typeSymbol.IsDictionaryType();
            if (isDictionary)
            {
                // And don't count dictionaries as collections.
                isCollection = false;
            }

            // TODO: we want to support most collection types, as quick starting point ICollection<T> was chosen to detect them.
            // TODO: the proper way would be to look for an Add() method or indexer property, but what would be the appropriate type in the first case?.
            // TODO: also, arrays can be jagged or multidimensional... what code to generate?
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

            typeParameters = new List<MemberInfo>();
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
            if (namedTypeSymbol?.TypeArguments.Any() == true)
            {
                foreach (var typeArg in namedTypeSymbol.TypeArguments)
                {
                    typeParameters.Add(GetMemberInfoRecursive(typeArg.Name, typeArg));
                }
            }
            
            isNullableValueType = typeSymbol.IsNullableType();

            if (isNullableValueType)
            {
                var nullableType = namedTypeSymbol?.TypeArguments.FirstOrDefault();
                if (nullableType != null)
                {
                    var nullableTypeEnum = GetSymbolType(nullableType, out _, out _, out _, out _);
                    return nullableTypeEnum;
                }
            }
            
            return TypeEnum.ComplexType;
        }

        protected override TypeEnum? GetKnownValueType(ITypeSymbol typeSymbol)
        {
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
                    return TypeEnum.Float32;
                case SpecialType.System_Double:
                    return TypeEnum.Float64;
                case SpecialType.System_Decimal:
                    return TypeEnum.Decimal;
            }

            return null;
        }
    }
}
