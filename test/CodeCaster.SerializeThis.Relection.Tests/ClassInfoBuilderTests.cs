using CodeCaster.SerializeThis.Reflection;
using CodeCaster.SerializeThis.Serialization;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Relection.Tests
{
    public class ClassInfoBuilderTests
    {
        private readonly IClassInfoBuilder _classUnderTest = new ClassInfoBuilder();

        private class Foo
        {
            public string Bar { get; set; }
        }

        [Test]
        public void BuildObjectTree_Works_Recursively()
        {
            // Arrange
            var toSerialize = new Foo
            {
                Bar = "Bar"
            };

            // Act
            var result = _classUnderTest.BuildObjectTree(toSerialize);

            // Assert
            Assert.AreEqual(nameof(Foo.Bar), result.Class.Children[0].Name);
        }
    }
}