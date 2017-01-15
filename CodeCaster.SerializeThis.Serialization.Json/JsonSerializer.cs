using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (child.IsComplexType)
            {
                return GetComplexType(child);
            }

            // TODO: Collections.
            
            return new JValue(GetContents(child));
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
