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

        public ClassInfo GetMemberInfoRecursive(T typeSymbol, object instance)
        {
            _typesSeen.Clear();
            return GetMemberInfoRecursiveImpl(typeSymbol, instance);
        }

        protected TypeEnum GetSymbolType(T typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, out bool isEnum, out List<ClassInfo> typeParameters)
        {
            var knownValueType = GetKnownValueType(typeSymbol);
            if (knownValueType != null)
            {
                collectionType = null;
                isNullableValueType = false;
                isEnum = false;
                typeParameters = null;
                return knownValueType.Value;
            }
            
            isEnum = IsEnum(typeSymbol);

            return GetComplexSymbolType(typeSymbol, out collectionType, out isNullableValueType, ref isEnum, out typeParameters);
        }

        protected abstract bool IsEnum(T typeSymbol);

        protected abstract TypeEnum GetComplexSymbolType(T typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, ref bool isEnum, out List<ClassInfo> typeParameters);

        protected bool HasSeenType(string fullTypeName, out Class classInfo) => _typesSeen.TryGetValue(fullTypeName, out classInfo);

        protected void AddSeenType(string fullTypeName, Class classInfo) => _typesSeen[fullTypeName] = classInfo;

        /// <summary>
        /// This takes the deep dive.
        /// </summary>
        protected abstract ClassInfo GetMemberInfoRecursiveImpl(T typeSymbol, object instance);

        /// <summary>
        /// Should try to parse the given type symbol for all <see cref="TypeEnum"/> members except <see cref="TypeEnum.ComplexType"/>.
        /// </summary>
        protected abstract TypeEnum? GetKnownValueType(T typeSymbol);
    }
}
