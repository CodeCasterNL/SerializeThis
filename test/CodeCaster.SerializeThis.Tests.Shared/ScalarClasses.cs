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
                Name = "Foo",
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

    }
}
