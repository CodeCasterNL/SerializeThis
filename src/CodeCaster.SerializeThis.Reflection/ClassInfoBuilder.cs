using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.Reflection
{
    public class ClassInfoBuilder : IClassInfoBuilder
    {
        private readonly Dictionary<string, Class> _typesSeen = new Dictionary<string, Class>();

        public ClassInfo BuildObjectTree(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _typesSeen.Clear();
            var type = value.GetType();
            var memberInfo = GetMemberInfoRecursive(type.FullName, type, value);
            return memberInfo;
        }

        private ClassInfo GetMemberInfoRecursive(string name, Type type, object value1)
        {
            var returnValue = new ClassInfo
            {
                Name = name,
                //Attributes = propertySymbol?.GetAttributes().Map()
            };

            // TODO: this will break. Include assembly name with type name?
            var fullTypeName = type.FullName;
            if (_typesSeen.TryGetValue(fullTypeName, out var c))
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
            _typesSeen[fullTypeName] = c;

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

                foreach (var child in GetChildProperties(type))
                {
                    c.Children.Add(child);
                }
            }

            returnValue.Class = c;
            return returnValue;
        }

        private IEnumerable<ClassInfo> GetChildProperties(Type type)
        {
            return Array.Empty<ClassInfo>();
        }

        private TypeEnum GetSymbolType(Type typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, out bool isEnum, out List<ClassInfo> typeParameters)
        {
            collectionType = null;
            isNullableValueType = false;
            isEnum = false;
            typeParameters = new List<ClassInfo>();

            return 0;
        }
    }
}
