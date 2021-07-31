using System;
using System.Collections.Generic;

namespace CodeCaster.SerializeThis.Serialization
{
    /// <summary>
    /// One base implementation for parsers, but feel free to roll your own.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SymbolParser<T> : IClassInfoBuilder<T>
    {
        // TODO: this makes it pretty much not thread safe, might we ever need that, store the class/info stuff in a more stateful object?
        private readonly Dictionary<string, Class> _typesSeen = new Dictionary<string, Class>();

        /// <summary>
        /// This is what the caller calls. It does not support recursion and should not be called from within or derived classes.
        /// </summary>
        ClassInfo IClassInfoBuilder<T>.GetMemberInfoRecursive(string objectName, T typeSymbol, object instance)
        {
            _typesSeen.Clear();
            if (typeSymbol == null) throw new ArgumentNullException(nameof(typeSymbol));


            // TODO: something with instance, when debugging.
            var memberInfo = GetMemberInfoRecursive(objectName, typeSymbol);
            return memberInfo;
        }

        protected ClassInfo GetMemberInfoRecursive(string name, T typeSymbol, IList<AttributeInfo> propertyAttributes = null, object instance = null)
        {
            var returnValue = new ClassInfo
            {
                Name = name,
                Attributes = propertyAttributes
            };

            var fullTypeName = GetCacheName(typeSymbol);
            if (_typesSeen.TryGetValue(fullTypeName, out var classInfo))
            {
                returnValue.Class = classInfo;
                return returnValue;
            }

            // Save it _before_ diving into members and type parameters.
            var typeName = GetClassName(typeSymbol);
            classInfo = new Class
            {
                TypeName = typeName
            };
            _typesSeen[fullTypeName] = classInfo;

            ParseClass(classInfo, typeSymbol, instance);

            returnValue.Class = classInfo;
            return returnValue;
        }

        protected TypeEnum GetSymbolType(T typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, out bool isEnum, out IList<ClassInfo> typeParameters)
        {
            var knownValueType = GetKnownValueType(typeSymbol);
            if (knownValueType != null)
            {
                collectionType = null;
                isNullableValueType = false;
                isEnum = false;
                typeParameters = Array.Empty<ClassInfo>();
                return knownValueType.Value;
            }
            
            isEnum = IsEnum(typeSymbol);

            return GetComplexSymbolType(typeSymbol, out collectionType, out isNullableValueType, ref isEnum, out typeParameters);
        }

        private void ParseClass(Class c, T typeSymbol, object value)
        {
            var type = GetSymbolType(typeSymbol, out var collectionType, out var isNullableValueType, out var isEnum, out var genericParameters);

            c.Type = type;
            c.CollectionType = collectionType;
            c.IsNullableValueType = isNullableValueType;
            c.IsEnum = isEnum;
            c.Attributes = GetAttributes(typeSymbol);

            // TODO: handle _all_ generics. There can be a Foo<T1, T2> that (indirectly) inherits List<T2> and adds additional properties...
            // TODO: though for example JSON can't handle that, and do the C# object and collection initializer combine?
            if (c.CollectionType == CollectionType.Dictionary)
            {
                var keyValueType = GetDictionaryKeyType(typeSymbol);
                if (keyValueType.TKey != null && keyValueType.TValue != null)
                {
                    c.GenericParameters.Add(keyValueType.TKey);
                    c.GenericParameters.Add(keyValueType.TValue);
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

                foreach (var child in GetChildProperties(typeSymbol, value))
                {
                    c.Children.Add(child);
                }
            }
        }

        protected abstract bool IsEnum(T typeSymbol);

        protected abstract string GetClassName(T typeSymbol);

        /// <summary>
        /// Return a cachable name for the given type.
        /// </summary>
        protected abstract string GetCacheName(T typeSymbol);
        
        protected abstract IList<AttributeInfo> GetAttributes(T typeSymbol);

        protected abstract IList<ClassInfo> GetChildProperties(T typeSymbol, object value);

        /// <summary>
        /// Should try to parse the given type symbol for all <see cref="TypeEnum"/> members except <see cref="TypeEnum.ComplexType"/>.
        /// </summary>
        protected abstract TypeEnum? GetKnownValueType(T typeSymbol);

        protected abstract TypeEnum GetComplexSymbolType(T typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, ref bool isEnum, out IList<ClassInfo> typeParameters);

        protected abstract ClassInfo GetArrayTypeParameter(T typeSymbol);

        protected abstract ClassInfo GetCollectionTypeParameter(T typeSymbol);

        protected abstract (ClassInfo TKey, ClassInfo TValue) GetDictionaryKeyType(T typeSymbol);
    }
}
