using System.Collections.Generic;

namespace SerializeThis.Serialization
{
    public interface IPropertyValueProvider
    {
        object GetScalarValue(MemberInfo toSerialize, string path);

        IEnumerable<MemberInfo> GetCollectionElements(MemberInfo child, string path, MemberInfo collectionType);
        
        /// <summary>
        /// Return true when you can provide values for the requested type and name.
        /// </summary>
        bool CanHandle(TypeInfo typeInfo, string path);
        
        /// <summary>
        /// Called by the serializer when it encounters another property.
        /// </summary>
        MemberInfo Announce(MemberInfo type, string path);
        
        MemberInfo Announce(MemberInfo keyType, MemberInfo valueType, string path);
        void Initialize();
    }
}
