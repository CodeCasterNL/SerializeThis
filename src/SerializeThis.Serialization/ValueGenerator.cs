using System;
using System.Collections.Generic;
using System.Linq;

namespace SerializeThis.Serialization
{
    public class ValueGenerator : IPropertyValueProvider
    {
        private int _counter;
        private DateTime _startTime;

        public bool CanHandle(TypeInfo typeInfo, string name)
        {
            return true;
        }

        public void Initialize()
        {
            _startTime = DateTime.Now;
        }

        public MemberInfo Announce(MemberInfo toSerialize, string path)
        {
            return toSerialize;
        }

        public object GetScalarValue(MemberInfo toSerialize, string path) => GetValue(toSerialize);

        // TODO: ValueInfo
        public IEnumerable<MemberInfo> GetCollectionElements(MemberInfo classInfo, string path, MemberInfo collectionType)
        {
            switch (classInfo.Class.CollectionType)
            {
                case CollectionType.Array:
                case CollectionType.Collection:
                    return new[]
                    {
                        new MemberInfo { Class = collectionType.Class, Name = path + "[0]" },
                        new MemberInfo { Class = collectionType.Class, Name = path + "[1]" },
                        new MemberInfo { Class = collectionType.Class, Name = path + "[2]" },
                    };
                case CollectionType.Dictionary:
                    return new []
                    {
                        //new MemberInfo { Class = collectionType.Class, Name = path + $"[{collectionType.Class.GenericParameters[0].}]" },
                        new MemberInfo { Class = collectionType.Class, Name = path + $"[]" },
                        new MemberInfo { Class = collectionType.Class, Name = path + $"[]" },
                        new MemberInfo { Class = collectionType.Class, Name = path + $"[]" },
                    };
            }

            throw new ArgumentException(nameof(classInfo.Class.CollectionType));
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

        public MemberInfo Announce(MemberInfo keyType, MemberInfo valueType, string path)
        {
            var kvtName = $"KeyValueType<{keyType.Class.TypeName}, {valueType.Class.TypeName}>";
            var keyValueType = new MemberInfo
            {
                Name = kvtName,
                Class = new TypeInfo
                {
                    TypeName = kvtName,
                    GenericParameters = new List<MemberInfo>
                    {
                        keyType,
                        valueType
                    }
                }
            };

            return keyValueType;
        }
    }
}
