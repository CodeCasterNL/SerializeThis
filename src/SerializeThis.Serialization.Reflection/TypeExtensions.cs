﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SerializeThis.Serialization.Reflection
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

        public static bool IsAnonymousType(this Type typeSymbol) => 
            typeSymbol.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any()
            && typeSymbol.FullName?.Contains("AnonymousType") == true;

        public static string GetNameWithoutGenerics(this Type typeSymbol)
        {
            var name = typeSymbol.Namespace + "." + typeSymbol.Name;

            return name.RemoveAfter('[', '`');
        }

        public static string RemoveAfter(this string value, params char[] parts)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (parts.Any(p => p == value[i]))
                {
                    return value.Substring(0, i);
                }
            }

            return value;
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
