using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CodeCaster.SerializeThis.Serialization.Json
{
    public class JsonSerializer : IClassInfoSerializer
    {
        private readonly Dictionary<string, JObject> _typesSeen = new Dictionary<string, JObject>();

        public string Extension => "json";

        public string Serialize(ClassInfo type)
        {
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

        private JToken GetCollection(ClassInfo child)
        {
            // For now, we store the collection's type in its first child.
            var collectionType = child.Class.Children.FirstOrDefault();
            if (collectionType == null)
            {
                return new JArray();
            }

            object[] arrayMembers = {
                SerializeChild(collectionType),
                SerializeChild(collectionType),
                SerializeChild(collectionType),
            };

            return new JArray(arrayMembers);
        }

        private object GetContents(ClassInfo toSerialize)
        {
            if (toSerialize.Class.IsEnum)
            {
                return $"{toSerialize.Name}-FooEnum";
            }

            switch (toSerialize.Class.Type)
            {
                case TypeEnum.Boolean:
                    return true;
                case TypeEnum.String:
                    return $"{toSerialize.Name}-FooString";
                case TypeEnum.DateTime:
                    return DateTime.Now.ToUniversalTime();
                case TypeEnum.Int16:
                    return Int16.MaxValue;
                case TypeEnum.Int32:
                    return Int32.MaxValue;
                case TypeEnum.Int64:
                    return Int64.MaxValue;
                case TypeEnum.Float16:
                    return Single.MaxValue;
                case TypeEnum.Float32:
                    return Double.MaxValue;
                case TypeEnum.Decimal:
                    return Decimal.MaxValue;
                case TypeEnum.Byte:
                    return Byte.MaxValue;

                default:
                    return null;
            }
        }
    }
}
