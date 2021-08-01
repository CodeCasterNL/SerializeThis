using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeCaster.SerializeThis.Serialization
{
    public class ValueGenerator
    {
        private int _counter;
        private DateTime _startTime;

        public void Populate(MemberInfo classInfo)
        {
            _counter = 0;
            _startTime = DateTime.Now.ToUniversalTime();

            PopulateValue(classInfo);
        }

        private void PopulateValue(MemberInfo classInfo)
        {
            if (classInfo.Class.Type == TypeEnum.ComplexType)
            {
                // TODO: this won't work, we can't populate a type's children.
                if (classInfo.Class.CollectionType != null)
                {
                    switch (classInfo.Class.CollectionType)
                    {
                        case CollectionType.Array:
                            classInfo.Value = new object[]
                            {
                                //GetValue(...),
                                //GetValue(...),
                                //GetValue(...),
                            };
                            break;
                        case CollectionType.Collection:
                            classInfo.Value = new Collection<object>[]
                            {
                                //GetValue(...),
                                //GetValue(...),
                                //GetValue(...),
                            };
                            break;
                        case CollectionType.Dictionary:
                            classInfo.Value = new Dictionary<object, object>[]
                            {
                                //GetValue(...),
                                //GetValue(...),
                                //GetValue(...),
                            };
                            break;
                    }
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
                classInfo.Value = GetValue(classInfo);
            }
        }

        public object GetValue(MemberInfo type)
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
