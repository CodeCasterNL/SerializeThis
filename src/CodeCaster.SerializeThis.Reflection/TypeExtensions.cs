using System;
using System.Collections.Generic;
using System.Text;

namespace CodeCaster.SerializeThis.Reflection
{
    public static class TypeExtensions
    {
        public static Type CollectionInterfaceType { get; } = typeof(ICollection<>);
        public static Type DictionaryInterfaceType { get; } = typeof(IDictionary<,>);

        public static Type GetICollectionTInterface(this Type typeSymbol)
        {
            return GetInterface(typeSymbol, x => x.IsAssignableFrom(CollectionInterfaceType));
        }

        public static Type GetIDictionaryTKeyTValueInterface(this Type typeSymbol)
        {
            return GetInterface(typeSymbol, x => x.IsAssignableFrom(DictionaryInterfaceType));
        }

        public static bool IsDictionaryType(this Type typeSymbol)
        {
            return typeSymbol.GetIDictionaryTKeyTValueInterface() != null;
        }

        public static bool IsCollectionType(this Type typeSymbol)
        {
            return typeSymbol.GetICollectionTInterface() != null;
        }

        private static Type GetInterface(Type typeSymbol, Func<Type, bool> testFunction)
        {
            return testFunction(typeSymbol) 
                ? typeSymbol 
                : null;
        }
    }
}
