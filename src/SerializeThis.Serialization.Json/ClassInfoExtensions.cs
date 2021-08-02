using System.Linq;

namespace SerializeThis.Serialization.Json
{
    public static class ClassInfoExtensions
    {
        /// <summary>
        /// Returns the declared property name, or its name if overridden through any known attributes such as [JsonProperty], [DataMember].
        /// </summary>
        public static string GetPropertyName(this MemberInfo typeInfo, MemberInfo containingTypeInfo)
        {
            return typeInfo.GetPropertyNameFromAttributes(containingTypeInfo)
                   ?? typeInfo.Name;
        }

        private static string GetPropertyNameFromAttributes(this MemberInfo typeInfo, MemberInfo containingTypeInfo)
        {
            if (!typeInfo.Attributes.Any())
            {
                return null;
            }

            foreach (var attribute in typeInfo.Attributes)
            {
                switch (attribute.Name)
                {
                    case TypeNameConstants.NewtonsoftJsonPropertyAttribute:
                        return attribute.GetArgOrNamedProperty(0, TypeNameConstants.PropertyName);

                    case TypeNameConstants.SystemTextJsonPropertyAttribute:
                        return attribute.GetArgOrNamedProperty(0, TypeNameConstants.Name);

                    // [DataMember] only applies inside a class marked with [DataContract].
                    case TypeNameConstants.DataMemberAttribute:
                        return containingTypeInfo?.Class.Attributes.Any(a => a.Name == TypeNameConstants.DataContractAttribute) == true
                            ? attribute.GetArgOrNamedProperty(null, TypeNameConstants.Name)
                            : null;

                    default:
                        continue;
                }
            }

            return null;
        }
    }
}
