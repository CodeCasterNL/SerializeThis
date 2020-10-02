﻿using System;
using System.Globalization;
using System.Linq;
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
            
            if (type.Class.Type != TypeEnum.ComplexType)
            {
                // Sure, maybe later we'll support collection or scalar types.
                throw new NotSupportedException("root type must be complex type");
            }

            var builder = new StringBuilder();
            builder.Append("var foo = ");

            EmitComplexType(builder, type, 0);
            
            return builder.ToString();
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

            if (!child.Class.IsEnum && child.Class.IsComplexType)
            {
                EmitComplexType(builder, child, indent);
                return;
            }

            EmitValueTypeConstant(builder, child);
        }

        private void EmitComplexType(StringBuilder builder, ClassInfo type, int indent)
        {
            var spaces = new string(' ', indent * 4);
            builder.AppendFormat("new {0}{1}{2}{{{1}", type.Class.TypeName, Environment.NewLine, spaces);

            foreach (var children in type.Class.Children)
            {
                AppendChild(builder, type, children, indent + 1);
            }

            if (indent == 0)
            {
                builder.AppendLine("};");
            }
            else
            {
                builder.Append(spaces).AppendLine("},");
            }
        }

        private void EmitCollection(StringBuilder builder, ClassInfo child, int indent)
        {
            builder.AppendFormat("new {0}[0],", child.Class.TypeName);
            builder.AppendLine();
        }

        private void EmitDictionary(StringBuilder builder, ClassInfo child, int indent)
        {
            var keyType = child.Class.GenericParameters[0].Class.TypeName;
            var valueType = child.Class.GenericParameters[1].Class.TypeName;
            builder.AppendFormat("new {0}(),", child.Class.TypeName);
            builder.AppendLine();
        }

        private void AppendChild(StringBuilder builder, ClassInfo type, ClassInfo child, int indent)
        {
            var spaces = new string(' ', indent * 4);

            builder.Append(spaces).Append(child.Name).Append(" = ");

            EmitInitializer(builder, child, indent);
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
                // TODO: get enum example member (or 0)?
                var enumMember = child.Class.Children.FirstOrDefault()?.Name ?? "0";
                return $"{child.Class.TypeName}.{enumMember}";
            }

            switch (child.Class.Type)
            {
                case TypeEnum.Boolean:
                    return _counter % 2 == 0;
                case TypeEnum.String:
                    return $"\"{child.Name}-FooString{_counter}\"";
                case TypeEnum.DateTime:
                    return DateTime.Now.ToUniversalTime().AddSeconds(_counter);
                case TypeEnum.Int16:
                    return (Int16)_counter;
                case TypeEnum.Int32:
                    return (Int32)_counter;
                case TypeEnum.Int64:
                    return (Int64)_counter;
                case TypeEnum.Float16:
                    return (_counter + .42f).ToString(CultureInfo.InvariantCulture) + "f";
                case TypeEnum.Float32:
                    return (_counter + .42d).ToString(CultureInfo.InvariantCulture) + "d";
                case TypeEnum.Decimal:
                    return (_counter + .42m).ToString(CultureInfo.InvariantCulture) + "m";
                case TypeEnum.Byte:
                    return (Byte)_counter;

                default:
                    return null;
            }
        }
    }
}
