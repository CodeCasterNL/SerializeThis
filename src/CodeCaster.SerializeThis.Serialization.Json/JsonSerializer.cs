using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CodeCaster.SerializeThis.Serialization.Json
{
    public class JsonSerializer : IClassInfoSerializer
    {
        private int _counter;
        private readonly Dictionary<string, JObject> _typesSeen = new Dictionary<string, JObject>();

        public string FileExtension => "json";

        public string Serialize(ClassInfo type)
        {
            _counter = 0;

            if (type.Class.Type != TypeEnum.ComplexType)
            {
                // Sure, maybe later we'll support collection or scalar types.
                throw new NotSupportedException("root type must be complex type");
            }

            var rootObject = GetComplexType(type);
            return rootObject.ToString();
        }

        private JObject GetComplexType(ClassInfo toSerialize)
        {
            if (_typesSeen.TryGetValue(toSerialize.Class.TypeName, out var existing))
            {
                return existing;
            }

            existing = new JObject();
            _typesSeen[toSerialize.Class.TypeName] = existing;

            foreach (var child in toSerialize.Class.Children)
            {
                var childProperty = SerializeChild(child);
                existing[child.Name] = childProperty;
            }

            return existing;
        }

        private JToken SerializeChild(ClassInfo child)
        {
            if (child.Class.IsDictionary)
            {
                return GetDictionary(child);
            }

            if (child.Class.IsCollection)
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
            var keyType = child.Class.Children.FirstOrDefault();
            var valueType = child.Class.Children.Skip(1).FirstOrDefault();

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
            // We store the collection's type in its first child.
            var collectionType = child.Class.Children.FirstOrDefault();
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
                    return $"{toSerialize.Name}-FooString{_counter}";
                case TypeEnum.DateTime:
                    return DateTime.Now.ToUniversalTime().AddSeconds(_counter);
                case TypeEnum.Int16:
                    return (Int16)_counter;
                case TypeEnum.Int32:
                    return (Int32)_counter;
                case TypeEnum.Int64:
                    return (Int64)_counter;
                case TypeEnum.Float16:
                    return ((Single)_counter) + .42;
                case TypeEnum.Float32:
                    return ((Double)_counter) +.42;
                case TypeEnum.Decimal:
                    return ((Decimal)_counter) + .42m;
                case TypeEnum.Byte:
                    return (Byte)_counter;

                default:
                    return null;
            }
        }
        // ReSharper enable BuiltInTypeReferenceStyle - for consistent naming
    }
}
