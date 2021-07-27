using Microsoft.CodeAnalysis;
using System.Linq;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public static class PropertySymbolExtensions
    {
        /// <summary>
        /// Returns the declared property name, or its name if overridden through any known attributes such as [JsonProperty], [DataMember].
        /// </summary>
        public static string GetPropertyName(this IPropertySymbol property)
        {
            var name = property.Name;

            foreach (var attr in property.GetAttributes())
            {
                var attributeName = attr.AttributeClass.GetTypeName(withGenericParameterNames: true);

                switch (attributeName)
                {
                    case TypeNameConstants.NewtonsoftJsonProperty:
                        return attr.GetArgOrNamedProperty(0, "PropertyName") ?? name;

                    case TypeNameConstants.SystemTextJsonProperty:
                        return attr.GetArgOrNamedProperty(0, "Name") ?? name;

                    // [DataMember] only applies inside a class marked with [DataContract].
                    case TypeNameConstants.DataMember:
                        if (property.ContainingType.GetAttributes().Any(a => a.AttributeClass.GetTypeName() == TypeNameConstants.DataContract))
                        {
                            break;
                        }
                        return attr.GetArgOrNamedProperty(null, "Name") ?? name;
                    default:
                        continue;
                }
            }

            return name;
        }
    }
}
