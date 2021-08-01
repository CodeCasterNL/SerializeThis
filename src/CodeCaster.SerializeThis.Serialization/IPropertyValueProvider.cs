using System.Collections.Generic;

namespace CodeCaster.SerializeThis.Serialization
{
    public interface IPropertyValueProvider
    {
        object GetScalarValue(MemberInfo toSerialize, string path);

        IEnumerable<object> GetCollectionElements(MemberInfo child, string path, MemberInfo collectionType);
        
        void Initialize(TypeInfo typeInfo, string name);
    }
}
