using System;
using System.Text;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        private int _counter;
        public string FileExtension => "cs";

        public string Serialize(ClassInfo type)
        {
            _counter = 0;

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
            if (child.Class.IsDictionary)
            {
                EmitDictionary(builder, child, indent);
                return;
            }

            if (child.Class.IsCollection)
            {
                EmitCollection(builder, child, indent);
                return;
            }

            if (child.Class.IsComplexType)
            {
                EmitComplexType(builder, child, indent);
                return;
            }

            EmitValueTypeConstant(builder, child);
        }

        private void EmitComplexType(StringBuilder builder, ClassInfo child, int indent)
        {
            throw new NotImplementedException();
        }

        private void EmitCollection(StringBuilder builder, ClassInfo child, int indent)
        {
            throw new NotImplementedException();
        }

        private void EmitDictionary(StringBuilder builder, ClassInfo child, int indent)
        {
            throw new NotImplementedException();
        }

        private void EmitValueTypeConstant(StringBuilder builder, ClassInfo child)
        {
            _counter++;

            var value = GetValue(child);

            switch (value)
            {
                case bool b:
                    builder.Append(b ? "true" : "false");
                    break;
                case string s:
                    builder.AppendFormat("\"{0}\"", s);
                    break;
                case DateTime dt:
                    builder.AppendFormat("new DateTime({0}, {1}, {2}, {3}, {4}, {5})", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    break;
                default:
                    builder.Append(value ?? "null");
                    break;
            }

            builder.AppendLine(",");
        }

        private object GetValue(ClassInfo child)
        {
            if (child.Class.IsEnum)
            {
                return $"{child.Name}-FooEnum{_counter}";
            }

            switch (child.Class.Type)
            {
                case TypeEnum.Boolean:
                    return _counter % 2 == 0;
                case TypeEnum.String:
                    return $"{child.Name}-FooString{_counter}";
                case TypeEnum.DateTime:
                    return DateTime.Now.ToUniversalTime().AddSeconds(_counter);
                case TypeEnum.Int16:
                    return (Int16)_counter;
                case TypeEnum.Int32:
                    return (Int32)_counter;
                case TypeEnum.Int64:
                    return (Int64)_counter;
                case TypeEnum.Float16:
                    return (_counter + .42f) + "f";
                case TypeEnum.Float32:
                    return (_counter + .42f) + "d";
                case TypeEnum.Decimal:
                    return (_counter + .42m) + "m";
                case TypeEnum.Byte:
                    return (Byte)_counter;

                default:
                    return null;
            }
        }
    }
}
