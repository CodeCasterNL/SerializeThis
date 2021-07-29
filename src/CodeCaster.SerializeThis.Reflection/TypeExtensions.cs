using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCaster.SerializeThis.Reflection
{
    public static class TypeExtensions
    {
        public static Type CollectionInterfaceType { get; } = typeof(ICollection<>);
        public static Type DictionaryInterfaceType { get; } = typeof(IDictionary<,>);

        public static Type GetICollectionTInterface(this Type typeSymbol)
        {
            return GetInterface(typeSymbol, x => CollectionInterfaceType.IsAssignableFrom(x));
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
            if (testFunction(typeSymbol))
            {
                return typeSymbol;
            }

            return typeSymbol.GetInterfaces().FirstOrDefault(testFunction);
        }
    }
}
