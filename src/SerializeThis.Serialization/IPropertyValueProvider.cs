using System.Collections.Generic;

namespace CodeCaster.SerializeThis.Serialization
{
    public interface IPropertyValueProvider
    {
        object GetScalarValue(MemberInfo toSerialize, string path);

        IEnumerable<object> GetCollectionElements(MemberInfo child, string path, MemberInfo collectionType);
        
        /// <summary>
        /// Return true when you can provide values for the requested type and name.
        /// </summary>
        bool CanHandle(TypeInfo typeInfo, string name);
        
        /// <summary>
        /// Called by the serializer when it encounters another property.
        /// </summary>
        MemberInfo Announce(MemberInfo type, string path);
    }
}
