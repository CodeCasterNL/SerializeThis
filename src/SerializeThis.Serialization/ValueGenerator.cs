using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SerializeThis.Serialization
{
    public class ValueGenerator : IPropertyValueProvider
    {
        private int _counter;
        private DateTime _startTime;

        public bool CanHandle(TypeInfo typeInfo, string name)
        {
            _startTime = DateTime.Now;

            return true;
        }

        public MemberInfo Announce(MemberInfo toSerialize, string path)
        {
            return toSerialize;
        }

        public object GetScalarValue(MemberInfo toSerialize, string path)
        {
            return GetValue(toSerialize);
        }

        public IEnumerable<MemberInfo> GetCollectionElements(MemberInfo classInfo, string path, MemberInfo collectionType)
        {
            switch (classInfo.Class.CollectionType)
            {
                case CollectionType.Array:
                    return new MemberInfo[]
                    {
                        //GetValue(...),
                        //GetValue(...),
                        //GetValue(...),
                    };
                    break;
                case CollectionType.Collection:
                    return new Collection<MemberInfo>
                    {
                        //GetValue(...),
                        //GetValue(...),
                        //GetValue(...),
                    };
                    break;
                case CollectionType.Dictionary:
                    return null;//new Dictionary<MemberInfo, MemberInfo>
                    {
                        //GetValue(...),
                        //GetValue(...),
                        //GetValue(...),
                    };
                    break;
            }

            throw new NotImplementedException();
        }

        private void PopulateValue(MemberInfo classInfo)
        {
            if (classInfo.Class.Type == TypeEnum.ComplexType)
            {
                // TODO: this won't work, we can't populate a type's children.
                if (classInfo.Class.IsCollectionType)
                {

                }
                else
                {
                    foreach (var property in classInfo.Class.Children)
                    {
                        PopulateValue(property);
                    }
                }
            }
            else
            {
            }
        }

        private object GetValue(MemberInfo type)
        {
            _counter++;

            if (type.Class.IsEnum)
            {
                // TODO: get enum example member (or 0)?
                var enumMember = type.Class.Children.FirstOrDefault()?.Name ?? "0";
                return $"{type.Class.TypeName}.{enumMember}";
            }

            switch (type.Class.Type)
            {
                case TypeEnum.Boolean:
                    return _counter % 2 == 0;
                case TypeEnum.String:
                    return $"{type.Name}FooString{_counter}";
                case TypeEnum.Char:
                    return 65 + (_counter % 26); // A-Z
                case TypeEnum.DateTime:
                    return _startTime.AddSeconds(_counter);
                case TypeEnum.Int16:
                    return (Int16)_counter;
                case TypeEnum.Int32:
                    return (Int32)_counter;
                case TypeEnum.Int64:
                    return (Int64)_counter;
                case TypeEnum.Float32:
                    return (_counter + .42f);
                case TypeEnum.Float64:
                    return (_counter + .42d);
                case TypeEnum.Decimal:
                    return (_counter + .42m);
                case TypeEnum.Byte:
                    return (Byte)_counter;

                default:
                    return null;
            }
        }
    }
}
