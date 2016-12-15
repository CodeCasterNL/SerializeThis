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

            var rootObject = new JObject();

            foreach (var child in type.Children)
            {
                rootObject.Add(SerializeChild(child));
            }

            return rootObject.ToString();
        }

        private JToken SerializeChild(Class child)
        {
            var thisObject = new JProperty(child.Name, GetContents(child));
            return thisObject;
        }

        private object GetContents(Class child)
        {
            switch (child.Type)
            {
                case TypeEnum.String:
                    return $"{child.Name}-FooString";
                case TypeEnum.Int32:
                    return int.MaxValue;
                default:
                    return null;
            }
        }
    }
}
