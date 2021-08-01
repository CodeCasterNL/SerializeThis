using System;
using CodeCaster.SerializeThis.Package;
using CodeCaster.SerializeThis.Serialization;
using CodeCaster.SerializeThis.Serialization.CSharp;
using CodeCaster.SerializeThis.Serialization.Json;

namespace CodeCaster.SerializeThis
{
    public class DefaultSerializerFactory : ISerializerFactory
    {
        public IClassInfoSerializer GetSerializer(object serializer, IPropertyValueProvider valueProvider)
        {
            var serializerString = serializer as string;

            switch (serializerString?.ToLowerInvariant())
            {
                case "json":
                    return new JsonSerializer(valueProvider);
                case "c#":
                    return new CSharpObjectInitializer(valueProvider);

                default:
                    // TODO: DI.
                    throw new ArgumentException(string.Format(VSPackage.SerializerNotSupported, serializer), nameof(serializer));
            }
        }
    }
}
