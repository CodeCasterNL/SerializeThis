using System;
using SerializeThis.Serialization;

namespace SerializeThis.Extension.VS2019.OutputHandlers
{
    public interface IOutputHandler
    {
        int Priority { get; }

        void Initialize(IServiceProvider serviceProvider);

        bool Handle(IClassInfoSerializer serializer, MemberInfo classInfo);
    }
}
