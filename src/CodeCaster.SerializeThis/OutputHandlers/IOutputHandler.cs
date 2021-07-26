using System;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.OutputHandlers
{
    public interface IOutputHandler
    {
        int Priority { get; }

        void Initialize(IServiceProvider serviceProvider);

        bool Handle(IClassInfoSerializer serializer, ClassInfo classInfo);
    }
}
