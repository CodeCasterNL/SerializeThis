using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CodeCaster.SerializeThis.Serialization.Json
{
    /// <summary>
    /// This class is _not_ thread safe.
    /// </summary>
    public class JsonSerializer : IClassInfoSerializer
    {
        private readonly Dictionary<string, JObject> _typesSeen = new Dictionary<string, JObject>();

        public string FileExtension => "json";

        public string DisplayName => "JSON";

        public bool CanSerialize(MemberInfo type) => type.Class.Type == TypeEnum.ComplexType;

        public string Serialize(MemberInfo type)
        {
            if (!CanSerialize(type))
            {
                // Sure, maybe later we'll support collection or scalar types.
                throw new NotSupportedException("Root type must be complex type");
            }

            var rootObject = GetComplexType(type);
            return rootObject.ToString();
        }

        private JObject GetComplexType(MemberInfo toSerialize)
        {
            if (_typesSeen.TryGetValue(toSerialize.Class.TypeName, out var existing))
            {
                // TODO: this is broken if we have a property of our own type. This just happens to prevent infinite recursion, but also doesn't properly show what the object can contain.
                return existing;
            }

            existing = new JObject();
            _typesSeen[toSerialize.Class.TypeName] = existing;

            foreach (var child in toSerialize.Class.Children)
            {
                var propertyName = child.GetPropertyName(toSerialize);
                var childProperty = SerializeChild(child);
                existing[propertyName] = childProperty;
            }

            return existing;
        }
        
        private JToken SerializeChild(MemberInfo child)
        {
            if (child.Class.CollectionType == CollectionType.Dictionary)
            {
                return GetDictionary(child);
            }

            if (child.Class.CollectionType == CollectionType.Collection || child.Class.CollectionType == CollectionType.Array)
            {
                return GetCollection(child);
            }

            if (child.Class.IsComplexType)
            {
                return GetComplexType(child);
            }

            return new JValue(GetContents(child));
        }

        private JToken GetDictionary(MemberInfo child)
        {
            // A dictionary's key type is the first child, the value type the second.
            var keyType = child.Class.GenericParameters.FirstOrDefault();
            var valueType = child.Class.GenericParameters.Skip(1).FirstOrDefault();

            var jObject = new JObject();

            if (child.Value is IEnumerable<(object, object)> dictionary)
            {
                foreach (var item in dictionary)
                {
                    // TODO: use item.Key and item.Value...
                    var exampleKey = SerializeChild(keyType);
                    var exampleValue = SerializeChild(valueType);

                    var property = new JProperty(exampleKey.ToString(), exampleValue);
                    jObject.Add(property);

                    //EmitDictionaryEntry(indent + 1, value: item, generateValue: false);
                }
            }


            return jObject;
        }

        private JToken GetCollection(MemberInfo child)
        {
            // We store the collection's type in its first generic parameter.
            var collectionType = child.Class.GenericParameters.FirstOrDefault();
            if (collectionType == null)
            {
                return new JArray();
            }

            var arrayMembers = new List<object>();
            if (child.Value is IEnumerable<object> collectionItems)
            {
                foreach (var item in collectionItems)
                {
                    arrayMembers.Add(item);
                }
            }

            return new JArray(arrayMembers);
        }

        // ReSharper disable BuiltInTypeReferenceStyle - for consistent naming
        private object GetContents(MemberInfo toSerialize)
        {
            return toSerialize.Class.IsEnum 
                ? toSerialize.Value?.ToString() 
                : toSerialize.Value;
        }
        // ReSharper enable BuiltInTypeReferenceStyle - for consistent naming
    }
}
