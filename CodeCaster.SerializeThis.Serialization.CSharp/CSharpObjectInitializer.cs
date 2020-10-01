using System;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        public string FileExtension => "cs";

        public string Serialize(ClassInfo type)
        {
            return "var foo = new " + type.Name + Environment.NewLine + "{" + Environment.NewLine + "};";
        }
    }
}
