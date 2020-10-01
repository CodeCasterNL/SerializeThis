using System;
using System.Text;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        public string FileExtension => "cs";

        public string Serialize(ClassInfo type)
        {
            var builder = new StringBuilder();
            builder.AppendLine("var foo = new " + type.Class.TypeName + Environment.NewLine + "{");

            foreach (var children in type.Class.Children)
            {
                AppendChild(builder, type, children, indent: 1);
            }

            builder.AppendLine("};");
            return builder.ToString();
        }

        private void AppendChild(StringBuilder builder, ClassInfo type, ClassInfo child, int indent)
        {
            var spaces = new string(' ', indent * 4);

            builder.Append(spaces).Append(child.Name).Append(" = ");

            EmitInitializer(builder, child, indent);
        }

        private void EmitInitializer(StringBuilder builder, ClassInfo child, int indent)
        {
            builder.AppendLine("new " + child.Class.TypeName + "(),");
        }
    }
}
