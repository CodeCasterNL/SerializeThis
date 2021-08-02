using System.Collections.Generic;
using SerializeThis.Serialization;

namespace SerializeThis.Tests.Shared
{
    public static class ScalarClasses
    {
        public static MemberInfo ClassWithBarStringProperty()
        {
            return new MemberInfo
            {
                Name = "FooWithBarString",
                Class = new TypeInfo
                {
                    Type = TypeEnum.ComplexType,
                    TypeName = "Foo",
                    Children = new List<MemberInfo>
                    {
                        new MemberInfo
                        {
                            Name = "Bar",
                            Class = StringClass()
                        }
                    }
                }
            };
        }

        public static MemberInfo ClassWithBarStringArrayProperty()
        {
            return new MemberInfo
            {
                Name = "FooWithBarStringArray",
                Class = new TypeInfo
                {
                    Type = TypeEnum.ComplexType,
                    TypeName = "Foo",
                    Children = new List<MemberInfo>
                    {
                        new MemberInfo
                        {
                            Name = "Bar",
                            Class = new TypeInfo
                            {
                                Type = TypeEnum.ComplexType,
                                CollectionType = CollectionType.Array,
                                GenericParameters = new List<MemberInfo>
                                {
                                    new MemberInfo
                                    {
                                        Name = "ArrayElementType",
                                        Class = StringClass()
                                    }
                                },
                                TypeName = "System.String[]",
                            }
                        }
                    }
                }
            };
        }

        public static TypeInfo StringClass() => new TypeInfo
        {
            TypeName = "System.String",
            Type = TypeEnum.String
        };
    }
}
