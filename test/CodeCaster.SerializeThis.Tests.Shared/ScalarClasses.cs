using System.Collections.Generic;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.Tests.Shared
{
    public static class ScalarClasses
    {
        public static ClassInfo ClassWithBarStringProperty()
        {
            return new ClassInfo
            {
                Name = "FooWithBarString",
                Class = new Class
                {
                    Type = TypeEnum.ComplexType,
                    TypeName = "Foo",
                    Children = new List<ClassInfo>
                    {
                        new ClassInfo
                        {
                            Name = "Bar",
                            Class = new Class
                            {
                                TypeName = "System.String",
                                Type = TypeEnum.String
                            }
                        }
                    }
                }
            };
        }
        public static ClassInfo ClassWithBarStringArrayProperty()
        {
            return new ClassInfo
            {
                Name = "FooWithBarStringArray",
                Class = new Class
                {
                    Type = TypeEnum.ComplexType,
                    TypeName = "Foo",
                    Children = new List<ClassInfo>
                    {
                        new ClassInfo
                        {
                            Name = "Bar",
                            Class = new Class
                            {
                                TypeName = "System.String[]",
                                Type = TypeEnum.ComplexType
                            }
                        }
                    }
                }
            };
        }
    }
}
