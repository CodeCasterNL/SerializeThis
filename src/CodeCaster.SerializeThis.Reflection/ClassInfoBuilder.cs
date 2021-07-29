using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.Reflection
{
    public class ClassInfoBuilder : SymbolParser<Type>
    {
        protected override ClassInfo GetMemberInfoRecursiveImpl(Type typeSymbol, object instance)
        {
            if (typeSymbol == null) throw new ArgumentNullException(nameof(typeSymbol));

            var memberInfo = GetMemberInfoRecursive(typeSymbol.FullName, typeSymbol, propertyAttributes: null, instance);
            return memberInfo;
        }

        private ClassInfo GetMemberInfoRecursive(string name, Type type, ICollection<AttributeInfo> propertyAttributes, object value)
        {
            var returnValue = new ClassInfo
            {
                Name = name,
                Attributes = propertyAttributes
            };

            // TODO: this will break. Include assembly name with type name?
            var fullTypeName = type.FullName;
            if (HasSeenType(fullTypeName, out var c))
            {
                returnValue.Class = c;
                return returnValue;
            }

            // Save it _before_ diving into members and type parameters. 
            var typeName = fullTypeName;
            c = new Class
            {
                TypeName = typeName
            };
            AddSeenType(fullTypeName, c);

            var typeEnum = GetSymbolType(type, out var collectionType, out var isNullableValueType, out var isEnum, out var genericParameters);

            c.Type = typeEnum;
            c.CollectionType = collectionType;
            c.IsNullableValueType = isNullableValueType;
            c.IsEnum = isEnum;
            c.Attributes = type.GetCustomAttributes(true).Cast<Attribute>().Map();

            // TODO: implement the whole thing
            if (c.Type == TypeEnum.ComplexType && !isNullableValueType)
            {
                foreach (var gp in genericParameters)
                {
                    c.GenericParameters.Add(gp);
                }

                foreach (var child in GetChildProperties(type, value))
                {
                    c.Children.Add(child);
                }
            }

            returnValue.Class = c;
            return returnValue;
        }

        private IEnumerable<ClassInfo> GetChildProperties(Type type, object value)
        {
            // First go to the root, the first class deriving from System.Object, then walk up to "kind of" order the properties.
            // No guarantees are made on the order in the docs though.
            var baseType = type.BaseType;
            if (baseType != typeof(object))
            {
                foreach (var baseClassProperty in GetChildProperties(type.BaseType, value))
                {
                    yield return baseClassProperty;
                }
            }

            // Then iterate over this type's public instance properties, not the inherited ones.
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var propertyValue = value == null ? null : p.GetValue(value);
                yield return GetMemberInfoRecursive(p.Name, p.PropertyType, p.GetCustomAttributes().Map(), propertyValue);
            }
        }

        protected override TypeEnum GetComplexSymbolType(Type typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, ref bool isEnum, out List<ClassInfo> typeParameters)
        {
            isNullableValueType = System.Nullable.GetUnderlyingType(typeSymbol) != null;

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
            
            typeParameters = new List<ClassInfo>();

            return 0;
        }

        private static readonly Dictionary<Type, TypeEnum> KnownValueTypes = new Dictionary<Type, TypeEnum>
        {
            { typeof(Boolean), TypeEnum.Boolean },
            { typeof(String), TypeEnum.String },
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

        protected override bool IsEnum(Type typeSymbol) => typeSymbol.IsEnum;
    }
}
