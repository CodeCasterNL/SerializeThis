using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CodeCaster.SerializeThis.Serialization.Json
{
    public class JsonSerializer
    {
        public string Serialize(Class type)
        {
            if (type.Type != TypeEnum.ComplexType)
            {
                // Sure, maybe later we'll support collection or scalar types.
                throw new NotSupportedException("root type must be complex type");
            }

            var rootObject = GetComplexType(type);
            return rootObject.ToString();
        }
        
        private JObject GetComplexType(Class toSerialize)
        {
            var result = new JObject();

            foreach (var child in toSerialize.Children)
            {
                var childProperty = SerializeChild(child);
                result.Add(child.Name, childProperty);
            }

            return result;
        }

        private JToken SerializeChild(Class child)
        {
            if (child.IsCollection)
            {
                return GetCollection(child);
            }

            if (child.IsComplexType)
            {
                return GetComplexType(child);
            }

            return new JValue(GetContents(child));
        }

        private JToken GetCollection(Class child)
        {
            // For now, we store the collection's type in its first child.
            var collectionType = child.Children.FirstOrDefault();
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

        private object GetContents(Class toSerialize)
        {
            if (toSerialize.IsEnum)
            {
                return $"{toSerialize.Name}-FooEnum";
            }

            switch (toSerialize.Type)
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
