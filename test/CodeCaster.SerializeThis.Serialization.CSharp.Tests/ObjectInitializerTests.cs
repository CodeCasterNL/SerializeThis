using System.Collections.Generic;
using CodeCaster.SerializeThis.Tests.Shared;
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
            var type = ScalarClasses.ClassWithBarStringProperty();

            // Act
            var result = _classUnderTest.Serialize(type);

            // Assert
            Assert.IsTrue(result.Contains("Bar = \"BarFooString1\""));
        }


        [Test]
        public void Serialize_Handles_Arrays()
        {
            // Arrange
            var type = ScalarClasses.ClassWithBarStringArrayProperty();

            // Act
            var result = _classUnderTest.Serialize(type);

            // Assert
            Assert.IsTrue(result.Contains("Bar = new System.String[]"));
        }
    }
}