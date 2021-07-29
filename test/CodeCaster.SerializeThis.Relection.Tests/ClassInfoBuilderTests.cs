using System;
using System.Linq;
using CodeCaster.SerializeThis.Reflection;
using CodeCaster.SerializeThis.Serialization;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Relection.Tests
{
    public class ClassInfoBuilderTests
    {
        private readonly IClassInfoBuilder<Type> _classUnderTest = new ClassInfoBuilder();

        [Test]
        public void BuildObjectTree_Works_Recursively()
        {
            // Arrange
            var toSerialize = new JsonTestClasses.FooInherited();

            // Act
            var result = _classUnderTest.GetMemberInfoRecursive(toSerialize.GetType(), toSerialize);

            // Assert 
            Assert.AreEqual(TypeEnum.ComplexType, result.Class.Type);

            // Base property
            AssertProperty(result.Class, nameof(JsonTestClasses.FooInherited.Age), typeof(int), TypeEnum.Int32);
            AssertProperty(result.Class, nameof(JsonTestClasses.FooBase.Firstname), typeof(string), TypeEnum.String);
        }




        private void AssertProperty(Class classInfo, string propertyName, Type propertyType, TypeEnum typeEnum)
        {
            var prop = classInfo.Children.FirstOrDefault(c => c.Name == propertyName);
            if (prop == null)
            {
                Assert.Fail($"Property '{propertyName}' not found on Class '{classInfo.TypeName}'.");
            }

            AssertProperty(prop, propertyName, propertyType, typeEnum);
        }

        private static void AssertProperty(ClassInfo c, string propertyName, Type propertyType, TypeEnum typeEnum)
        {
            // Just to be sure
            Assert.AreEqual(propertyName, c.Name);

            Assert.AreEqual(propertyType.FullName, c.Class.TypeName);
            Assert.AreEqual(typeEnum, c.Class.Type);
        }
    }
}