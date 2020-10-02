using System.Collections.Generic;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Serialization.CSharp.Tests
{
    public class ObjectInitializerTests
    {
        private readonly CSharpObjectInitializer _classUnderTest = new CSharpObjectInitializer();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Serialize_Handles_ValueTypes()
        {
            // Arrange
            var type = new ClassInfo
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

            // Act
            var result = _classUnderTest.Serialize(type);

            // Assert
            Assert.IsTrue(result.Contains("Bar = \"Bar-FooString1\""));
        }
    }
}