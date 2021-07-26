using System;
using CodeCaster.SerializeThis.Reflection;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.NuGet
{
    public static class TypeSerializer
    {
        public static ISerializerFactory SerializerFactory { get; set; } = new DefaultSerializerFactory();

        public static IClassInfoBuilder ClassInfoBuilder { get; set; } = new ClassInfoBuilder();

        public static string InitializeObject(object value, SerializerEnum serializer)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (SerializerFactory == null) throw new InvalidOperationException();
            if (ClassInfoBuilder == null) throw new InvalidOperationException();

            var classInfo = ClassInfoBuilder.BuildObjectTree(value);

            var s = SerializerFactory.GetSerializer(serializer);
            return s.Serialize(classInfo);
        }
    }
}
