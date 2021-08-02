using System;
using System.Collections.Generic;
using System.Linq;
using CodeCaster.SerializeThis.Reflection;
using CodeCaster.SerializeThis.Serialization;
using JsonTestClasses;
using NUnit.Framework;

namespace CodeCaster.SerializeThis.Relection.Tests
{
    public class ClassInfoBuilderTests
    {
        private readonly IClassInfoBuilder<Type> _classUnderTest = new ClassInfoBuilder();
        private readonly string _rootObjectName = "test";

        [Test]
        public void ParentProperties()
        {
            // Arrange
            var toSerialize = new JsonTestClasses.FooInherited();

            // Act
            var result = _classUnderTest.GetMemberInfoRecursive(_rootObjectName, toSerialize.GetType(), toSerialize);

            // Assert 
            Assert.AreEqual(_rootObjectName, result.Name);
            Assert.AreEqual(TypeEnum.ComplexType, result.Class.Type);

            // Base property
            AssertProperty(result.Class, nameof(toSerialize.Age), typeof(int), TypeEnum.Int32);
            AssertProperty(result.Class, nameof(toSerialize.Firstname), typeof(string), TypeEnum.String);
        }

        [Test]
        public void Collections()
        {
            // Arrange
            var toSerialize = new JsonTestClasses.FooSimpleCollections();

            // Act
            var result = _classUnderTest.GetMemberInfoRecursive(_rootObjectName, toSerialize.GetType(), toSerialize);

            // Assert 
            Assert.AreEqual(_rootObjectName, result.Name);
            Assert.AreEqual(TypeEnum.ComplexType, result.Class.Type);

            // Base property
            AssertProperty(result.Class, nameof(toSerialize.FooBaseList), typeof(List<FooBase>), TypeEnum.ComplexType);
            AssertProperty(result.Class, nameof(toSerialize.StringArray), typeof(string[]), TypeEnum.ComplexType, collectionType: CollectionType.Array);
        }

        private void AssertProperty(TypeInfo classInfo, string propertyName, Type propertyType, TypeEnum typeEnum, CollectionType? collectionType = null)
        {
            var prop = classInfo.Children.FirstOrDefault(c => c.Name == propertyName);
            if (prop == null)
            {
                Assert.Fail($"Property '{propertyName}' not found on Class '{classInfo.TypeName}'.");
            }

            if (collectionType.HasValue && prop.Class.CollectionType != collectionType)
            {
                Assert.Fail($"Property '{propertyName}' doesn't have collection type '{collectionType}'.");
            }

            AssertProperty(prop, propertyName, propertyType, typeEnum);
        }

        private static void AssertProperty(MemberInfo c, string propertyName, Type propertyType, TypeEnum typeEnum)
        {
            // Just to be sure
            Assert.AreEqual(propertyName, c.Name);

            Assert.AreEqual(propertyType.GetNameWithoutGenerics(), c.Class.TypeName);
            Assert.AreEqual(typeEnum, c.Class.Type);
        }
    }
}