using CodeCaster.SerializeThis.Reflection;
using CodeCaster.SerializeThis.Serialization;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Relection.Tests
{
    public class ClassInfoBuilderTests
    {
        private readonly IClassInfoBuilder _classUnderTest = new ClassInfoBuilder();
        
        [Test]
        public void BuildObjectTree_Works_Recursively()
        {
            // Arrange
            var toSerialize = new JsonTestClasses.FooInherited();

            // Act
            var result = _classUnderTest.BuildObjectTree(toSerialize);

            // Assert
            Assert.AreEqual(nameof(JsonTestClasses.FooInherited.Age), result.Class.Children[4].Name);
        }
    }
}