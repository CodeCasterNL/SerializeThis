using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SerializeThis.Serialization.Json
{
    /// <summary>
    /// This class is _not_ thread safe.
    /// </summary>
    public class JsonSerializer : IClassInfoSerializer
    {
        private readonly IPropertyValueProvider _valueProvider;
        
        private readonly Dictionary<string, JObject> _typesSeen = new Dictionary<string, JObject>();

        public string FileExtension => "json";

        public string DisplayName => "JSON";

        public bool CanSerialize(MemberInfo type) => true;

        public JsonSerializer(IPropertyValueProvider valueProvider)
        {
            _valueProvider = valueProvider;
        }

        public string Serialize(MemberInfo type)
        {
            _valueProvider.Initialize();

            var rootObject = SerializeChild(type, type.Name);
            return JsonConvert.SerializeObject(rootObject, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
        }

        private JObject GetComplexType(MemberInfo toSerialize, string path)
        {
            //if (_typesSeen.TryGetValue(toSerialize.Class.TypeName, out var existing))
            //{
            //    // TODO: this is broken if we have a property of our own type. This just happens to prevent infinite recursion, but also doesn't properly show what the object can contain.
            //    return existing;
            //}

            var existing = new JObject();
            _typesSeen[toSerialize.Class.TypeName] = existing;

            foreach (var child in toSerialize.Class.Children)
            {
                var propertyName = child.GetPropertyName(toSerialize);
                var childProperty = SerializeChild(child, AppendPath(path, child.Name));
                existing[propertyName] = childProperty;
            }

            return existing;
        }

        private string AppendPath(string path, string subpath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return subpath;
            }

            return path + "." + subpath;
        }

        private JToken SerializeChild(MemberInfo child, string path)
        {
            child = _valueProvider.Announce(child, path);
            
            if (child.Class.CollectionType == CollectionType.Dictionary)
            {
                return GetDictionary(child, path);
            }

            if (child.Class.CollectionType == CollectionType.Collection || child.Class.CollectionType == CollectionType.Array)
            {
                return GetCollection(child, path);
            }

            if (child.Class.IsComplexType)
            {
                return GetComplexType(child, path);
            }

            return new JValue(GetContents(child, path));
        }

        private JToken GetDictionary(MemberInfo child, string path)
        {
            // A dictionary's key type is the first child, the value type the second.
            var keyType = child.Class.GenericParameters.FirstOrDefault();
            var valueType = child.Class.GenericParameters.Skip(1).FirstOrDefault();
            var keyValueType = _valueProvider.Announce(keyType, valueType, path);

            var jObject = new JObject();

            foreach (var elementInfo in _valueProvider.GetCollectionElements(child, path, keyValueType))
            {
                var key = SerializeChild(elementInfo.Class.GenericParameters[0], path);
                var value = SerializeChild(elementInfo.Class.GenericParameters[1], path);

                var property = new JProperty(key.ToString(), value);
                jObject.Add(property);
            }

            return jObject;
        }

        private JToken GetCollection(MemberInfo child, string path)
        {
            // We store the collection's type in its first generic parameter.
            var collectionType = child.Class.GenericParameters.FirstOrDefault();
            if (collectionType == null)
            {
                throw new ArgumentException($"Cannot find element type of '{child.Name}' ('{path}')", nameof(collectionType));
            }

            var arrayMembers = new List<object>();

            foreach (var elementInfo in _valueProvider.GetCollectionElements(child, path, collectionType))
            {
                arrayMembers.Add(SerializeChild(elementInfo, path));
            }

            return new JArray(arrayMembers);
        }

        private object GetContents(MemberInfo toSerialize, string path)
        {
            return _valueProvider.GetScalarValue(toSerialize, path);
        }
    }
}
