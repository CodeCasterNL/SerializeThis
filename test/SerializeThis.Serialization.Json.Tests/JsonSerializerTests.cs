using SerializeThis.Tests.Shared;
using NUnit.Framework;

namespace SerializeThis.Serialization.Json.Tests
{
    // JsonSerializer is not thread safe
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class JsonSerializerTests
    {
        private readonly IClassInfoSerializer _classUnderTest = new JsonSerializer(new ValueGenerator());

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
  ""Bar"": ""BarFooString1""
}";
            Assert.AreEqual(expectedJson, result);
        }

        [Test]
        public void Serialize_Handles_StringArrays()
        {
            // Arrange
            var type = ScalarClasses.ClassWithBarStringArrayProperty();

            // Act
            var result = _classUnderTest.Serialize(type);

            // Assert
            var expectedJson = @"{
  ""Bar"": [
    ""ArrayElementTypeFooString1"",
    ""ArrayElementTypeFooString2"",
    ""ArrayElementTypeFooString3""
  ]
}";
            Assert.AreEqual(expectedJson, result);
        }
    }
}