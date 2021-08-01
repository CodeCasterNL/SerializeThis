using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        public string FileExtension => "cs";

        public string DisplayName => "C# object initializer";

        public bool CanSerialize(MemberInfo type) => true;

        public string Serialize(MemberInfo type)
        {
            var builder = new StringBuilder();

            builder.Append($"var {type.Name} = ");

            EmitInitializer(builder, type, indent: 0, StatementEndOptions.Semicolon | StatementEndOptions.Newline);

            return builder.ToString();
        }

        private void EmitInitializer(StringBuilder builder, MemberInfo type, int indent, StatementEndOptions statementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
        {
            switch (type.Class.CollectionType)
            {
                case CollectionType.Array:
                    EmitArray(builder, type, indent, statementEnd);
                    return;
                case CollectionType.Collection:
                    EmitCollection(builder, type, indent, statementEnd);
                    return;
                case CollectionType.Dictionary:
                    EmitDictionary(builder, type, indent, statementEnd);
                    return;
            }

            if (!type.Class.IsEnum && type.Class.IsComplexType)
            {
                // TODO: prevent infinite recursion here. We need to save "membername-typeInfo" tuples, maybe?
                // TODO: members can occur multiple times within the same or multiple types with different or equal names.
                // TODO: in other words, The property Foo.Bar.Foo doesn't have to point back to the first Foo. 
                // TODO: or Foo.Bar isn't the same as Foo.Baz.Bar. Define a max recursion depth, and also distance, i.e.
                // TODO: how often to repeat the pattern A.B.C.D[.A.B.C.D[...]] if D has a property of type A?)
                // TODO: maybe print `// A.B.C.D.A = max recursion depth reached` or something like that.

                EmitComplexType(builder, type, indent, statementEnd);
                return;
            }

            // Leaf nodes.
            EmitValueTypeConstant(builder, type);
            EndStatement(builder, statementEnd);
        }

        private static string GetSpaces(int indent) => new string(' ', indent * 4);

        private void EmitComplexType(StringBuilder builder, MemberInfo type, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var typeName = GetGenericTypeName(type.Class);

            builder.AppendFormat("new {0}{1}{2}{{{1}", typeName, Environment.NewLine, spaces);

            for (var index = 0; index < type.Class.Children.Count; index++)
            {
                var child = type.Class.Children[index];
                var childEnd = index == type.Class.Children.Count - 1
                    ? StatementEndOptions.Newline
                    : StatementEndOptions.Comma | StatementEndOptions.Newline;

                AppendChild(builder, type, child, indent + 1, childEnd);
            }

            builder.Append(spaces).Append("}");

            EndStatement(builder, statementEnd);
        }

        private string GetGenericTypeName(TypeInfo type)
        {
            if (!type.GenericParameters.Any())
            {
                return type.TypeName;
            }

            // TODO: cache?
            return type.TypeName + "<" + string.Join(", ", type.GenericParameters.Select(p => GetGenericTypeName(p.Class))) + ">";
        }

        private void AppendChild(StringBuilder builder, MemberInfo type, MemberInfo child, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);

            builder.Append(spaces).Append(child.Name).Append(" = ");

            EmitInitializer(builder, child, indent, statementEnd);
        }

        private void EmitArray(StringBuilder builder, MemberInfo type, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var elementType = type.Class.GenericParameters[0];

            // TypeName includes square brackets for arrays (System.String[])
            builder.AppendFormat("new {0}{1}", type.Class.TypeName, Environment.NewLine);
            EmitCollectionInitializer(builder, elementType, indent, spaces, statementEnd);
        }

        private void EmitCollection(StringBuilder builder, MemberInfo type, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var elementType = type.Class.GenericParameters[0];

            var typeName = $"{type.Class.TypeName}<{elementType.Class.TypeName}>";

            builder.AppendFormat("new {0}{1}", typeName, Environment.NewLine);
            EmitCollectionInitializer(builder, elementType, indent, spaces, statementEnd);
        }

        private void EmitCollectionInitializer(StringBuilder builder, MemberInfo elementType, int indent, string spaces, StatementEndOptions statementEnd)
        {
            void EmitCollectionEntry(int elementIndent, object value = null, StatementEndOptions elementStatementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
            {
                builder.Append(GetSpaces(elementIndent));
                EmitInitializer(builder, elementType, elementIndent, elementStatementEnd);
            }

            builder.Append(spaces).AppendLine("{");

            if (elementType.Value is IEnumerable<object> collectionItems)
            {
                foreach (var item in collectionItems)
                {
                    EmitCollectionEntry(indent + 1, value: item);
                }
            }

            builder.Append(spaces).Append("}");
            EndStatement(builder, statementEnd);
        }

        private void EmitDictionary(StringBuilder builder, MemberInfo type, int indent, StatementEndOptions statementEnd)
        {
            var keyType = type.Class.GenericParameters[0];
            var valueType = type.Class.GenericParameters[1];

            void EmitDictionaryEntry(int elementIndent, (object, object) value = default, StatementEndOptions elementStatementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
            {
                builder.Append(GetSpaces(elementIndent)).Append("{ ");
                EmitInitializer(builder, keyType, indent + 1, StatementEndOptions.None);
                builder.Append(", ");
                EmitInitializer(builder, valueType, indent + 1, StatementEndOptions.None);
                builder.Append(" }");
                EndStatement(builder, elementStatementEnd);
            }

            var spaces = GetSpaces(indent);

            var typeName = $"{type.Class.TypeName}<{keyType.Class.TypeName}, {valueType.Class.TypeName}>";

            builder.AppendFormat("new {0}{1}", typeName, Environment.NewLine);
            builder.Append(spaces).AppendLine("{");

            if (type.Value is IEnumerable<(object, object)> dictionary)
            {
                foreach (var item in dictionary)
                {
                    EmitDictionaryEntry(indent + 1, value: item);
                }
            }

            builder.Append(spaces).Append("}");
            EndStatement(builder, statementEnd);
        }

        private void EmitValueTypeConstant(StringBuilder builder, MemberInfo type)
        {
            switch (type.Value)
            {
                case Single s:
                    builder.Append(s.ToString(CultureInfo.InvariantCulture) + "f");
                    break;
                case Double d:
                    builder.Append(d.ToString(CultureInfo.InvariantCulture) + "d");
                    break;
                case Decimal d:
                    builder.Append(d.ToString(CultureInfo.InvariantCulture) + "m");
                    break;
                case String s:
                    builder.Append($"\"{s}\"");
                    break;
                case Boolean b:
                    builder.Append(b ? "true" : "false");
                    break;
                case DateTime dt:
                    builder.AppendFormat("new DateTime({0}, {1}, {2}, {3}, {4}, {5})", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    break;
                default:
                    builder.Append(type.Value ?? "null");
                    break;
            }
        }

        private void EndStatement(StringBuilder builder, StatementEndOptions statementEnd)
        {
            if (statementEnd.HasFlag(StatementEndOptions.Semicolon))
                builder.Append(";");
            if (statementEnd.HasFlag(StatementEndOptions.Comma))
                builder.Append(",");
            if (statementEnd.HasFlag(StatementEndOptions.Newline))
                builder.AppendLine();
        }
    }
}
