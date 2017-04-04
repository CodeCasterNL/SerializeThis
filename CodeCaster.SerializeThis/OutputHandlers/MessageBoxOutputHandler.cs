using System;
using System.Collections.Generic;
using CodeCaster.SerializeThis.Extension;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.OutputHandlers
{
    public class MessageBoxOutputHandler : IOutputHandler
    {
        private IServiceProvider _serviceProvider;
        public int Priority => 10;

        public void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool Handle(IClassInfoSerializer serializer, ClassInfo classInfo)
        {
            string memberInfoString = PrintMemberInfoRercursive(classInfo, 0);
            string json = serializer.Serialize(classInfo);
            string output = memberInfoString + Environment.NewLine + Environment.NewLine + json;

            return SerializeThisCommand.ShowMessageBox(_serviceProvider, output);
        }

        private string PrintMemberInfoRercursive(ClassInfo memberInfo, int depth, Dictionary<string, string> typesSeen = null)
        {
            if (typesSeen == null)
            {
                typesSeen = new Dictionary<string, string>();
            }

            string representationForType;
            if (typesSeen.TryGetValue(memberInfo.Class.TypeName, out representationForType))
            {
                return representationForType;
            }

            string result = "";

            // First add blank, so it'll be picked up in the case of recursion (A.A or A.B.A).
            typesSeen[memberInfo.Class.TypeName] = result;

            string spaces = new string(' ', depth * 2);

            result += $"{spaces}{memberInfo.Class.TypeName} ({memberInfo.Class.Type}) {memberInfo.Name}{Environment.NewLine}";

            if (memberInfo.Class.Children != null)
            {
                foreach (var child in memberInfo.Class.Children)
                {
                    result += PrintMemberInfoRercursive(child, depth + 1, typesSeen);
                }
            }

            typesSeen[memberInfo.Class.TypeName] = result;

            return result;
        }
    }
}
