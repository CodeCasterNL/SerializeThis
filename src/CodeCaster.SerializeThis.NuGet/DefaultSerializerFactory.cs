using System;
using CodeCaster.SerializeThis.Serialization;
using CodeCaster.SerializeThis.Serialization.CSharp;
using CodeCaster.SerializeThis.Serialization.Json;

namespace CodeCaster.SerializeThis.NuGet
{
    public class DefaultSerializerFactory : ISerializerFactory
    {
        public IClassInfoSerializer GetSerializer(object serializer)
        {
            var serializerEnum = serializer as SerializerEnum?;

            switch (serializerEnum)
            {
                case SerializerEnum.Json:
                    return new JsonSerializer();
                case SerializerEnum.Xml:
                    return new CSharpObjectInitializer();

                default:
                    // TODO: DI.
                    throw new ArgumentException(string.Format(Resources.Serializer_not_supported, serializer), nameof(serializer));
            }
        }
    }
}
