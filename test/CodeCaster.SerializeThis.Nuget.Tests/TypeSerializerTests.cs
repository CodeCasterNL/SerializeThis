using CodeCaster.SerializeThis.NuGet;
using CodeCaster.SerializeThis.Serialization;
using Moq;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Nuget.Tests
{
    public class TypeSerializerTests
    {

        [Test]
        public void InitializeObject_Uses_TypeSerializerProperties()
        {
            // Arrange
            var value = new { };
            var serialized = "{}";
            var classInfo = new ClassInfo();

            var classInfoBuilderMock = new Mock<IClassInfoBuilder>(MockBehavior.Strict);
            classInfoBuilderMock.Setup(c => c.BuildObjectTree(value)).Returns(classInfo);

            var serializerMock = new Mock<IClassInfoSerializer>(MockBehavior.Strict);
            serializerMock.Setup(s => s.Serialize(classInfo)).Returns(serialized);

            var serializerFactoryMock = new Mock<ISerializerFactory>(MockBehavior.Strict);
            serializerFactoryMock.Setup(f => f.GetSerializer(SerializerEnum.Csharp)).Returns(serializerMock.Object);
            
            // Act
            TypeSerializer.SerializerFactory = serializerFactoryMock.Object;
            TypeSerializer.ClassInfoBuilder = classInfoBuilderMock.Object;

            var result = TypeSerializer.InitializeObject(value, SerializerEnum.Csharp);

            // Assert
            Assert.AreEqual(serialized, result);
            serializerMock.Verify();
            serializerFactoryMock.Verify();
            classInfoBuilderMock.Verify();
        }
    }
}