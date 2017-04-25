using System;
using CodeCaster.SerializeThis.Package;
using CodeCaster.SerializeThis.Serialization;
using CodeCaster.SerializeThis.Serialization.Json;

namespace CodeCaster.SerializeThis
{
    public class DefaultSerializerFactory : ISerializerFactory
    {
        public IClassInfoSerializer GetSerializer(string contentType)
        {
            switch (contentType.ToLowerInvariant())
            {
                case "json":
                    return new JsonSerializer();

                default:
                    // TODO: DI.
                    throw new ArgumentException(string.Format(VSPackage.SerializerNotSupported, contentType), nameof(contentType));
            }
        }
    }
}
