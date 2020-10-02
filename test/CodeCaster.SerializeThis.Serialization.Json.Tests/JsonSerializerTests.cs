using CodeCaster.SerializeThis.Tests.Shared;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Serialization.Json.Tests
{
    public class JsonSerializerTests
    {
        private readonly IClassInfoSerializer _classUnderTest = new JsonSerializer();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Serialize_Handles_Strings()
        {
            // Arrange
            var type = ScalarClasses.ClassWithBarStringProperty();
            
            // Act
            var result = _classUnderTest.Serialize(type);

            // Assert
            var expectedJson = @"{
  ""Bar"": ""Bar-FooString1""
}";
            Assert.AreEqual(expectedJson, result);
        }
    }
}