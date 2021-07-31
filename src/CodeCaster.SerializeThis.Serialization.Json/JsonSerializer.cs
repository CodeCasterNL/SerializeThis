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
        private int _counter;
        private readonly Dictionary<string, JObject> _typesSeen = new Dictionary<string, JObject>();

        public string FileExtension => "json";

        public string DisplayName => "JSON";

        public bool CanSerialize(ClassInfo type) => type.Class.Type == TypeEnum.ComplexType;

        public string Serialize(ClassInfo type)
        {
            _counter = 0;

            if (!CanSerialize(type))
            {
                // Sure, maybe later we'll support collection or scalar types.
                throw new NotSupportedException("Root type must be complex type");
            }

            var rootObject = GetComplexType(type);
            return rootObject.ToString();
        }

        private JObject GetComplexType(ClassInfo toSerialize)
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
        
        private JToken SerializeChild(ClassInfo child)
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

        private JToken GetDictionary(ClassInfo child)
        {
            // A dictionary's key type is the first child, the value type the second.
            var keyType = child.Class.GenericParameters.FirstOrDefault();
            var valueType = child.Class.GenericParameters.Skip(1).FirstOrDefault();

            var jObject = new JObject();

            // TODO: 3 is hardcoded here.
            for (int i = 0; i < 3; i++)
            {
                var exampleKey = SerializeChild(keyType);
                var exampleValue = SerializeChild(valueType);

                var property = new JProperty(exampleKey.ToString(), exampleValue);
                jObject.Add(property);
            }

            return jObject;
        }

        private JToken GetCollection(ClassInfo child)
        {
            // We store the collection's type in its first generic parameter.
            var collectionType = child.Class.GenericParameters.FirstOrDefault();
            if (collectionType == null)
            {
                return new JArray();
            }

            // TODO: 3 is hardcoded here.
            object[] arrayMembers = {
                SerializeChild(collectionType),
                SerializeChild(collectionType),
                SerializeChild(collectionType),
            };

            return new JArray(arrayMembers);
        }

        // ReSharper disable BuiltInTypeReferenceStyle - for consistent naming
        private object GetContents(ClassInfo toSerialize)
        {
            if (toSerialize.Value != null)
            {
                return toSerialize.Value;
            }

            _counter++;

            if (toSerialize.Class.IsEnum)
            {
                return $"{toSerialize.Name}-FooEnum{_counter}";
            }

            switch (toSerialize.Class.Type)
            {
                case TypeEnum.Boolean:
                    return _counter % 2 == 0;
                case TypeEnum.String:
                    return $"{toSerialize.Name}FooString{_counter}";
                case TypeEnum.DateTime:
                    return DateTime.Now.ToUniversalTime().AddSeconds(_counter);
                case TypeEnum.Int16:
                    return (Int16)_counter;
                case TypeEnum.Int32:
                    return (Int32)_counter;
                case TypeEnum.Int64:
                    return (Int64)_counter;
                case TypeEnum.Float32:
                    return ((Single)_counter) + .000042;
                case TypeEnum.Float64:
                    return ((Double)_counter) + .000000000000042;
                case TypeEnum.Decimal:
                    return ((Decimal)_counter) + .00000000000042m;
                case TypeEnum.Byte:
                    return (Byte)_counter;

                default:
                    return null;
            }
        }
        // ReSharper enable BuiltInTypeReferenceStyle - for consistent naming
    }
}
