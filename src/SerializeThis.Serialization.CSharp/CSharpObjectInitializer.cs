using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        private readonly IPropertyValueProvider _valueProvider;

        public CSharpObjectInitializer(IPropertyValueProvider valueProvider)
        {
            _valueProvider = valueProvider;
        }

        public string FileExtension => "cs";

        public string DisplayName => "C# object initializer";

        public bool CanSerialize(MemberInfo type) => true;

        public string Serialize(MemberInfo type)
        {
            var builder = new StringBuilder();

            _valueProvider.Initialize();

            builder.Append($"var {type.Name} = ");

            EmitInitializer(builder, type, indent: 0, path: type.Name, StatementEndOptions.Semicolon | StatementEndOptions.Newline);

            return builder.ToString();
        }

        private void EmitInitializer(StringBuilder builder, MemberInfo type, int indent, string path = null, StatementEndOptions statementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
        {
            type = _valueProvider.Announce(type, path);

            switch (type.Class.CollectionType)
            {
                case CollectionType.Array:
                    EmitArray(builder, type, indent, path, statementEnd);
                    return;
                case CollectionType.Collection:
                    if (type.Class.Type == TypeEnum.Interface)
                    {
                        builder.Append($"/* TODO: an interface ({GetGenericTypeName(type.Class)})! */ null");
                        EndStatement(builder, statementEnd);
                    }
                    else
                    {
                        EmitCollection(builder, type, indent, path, statementEnd);
                    }
                    return;
                case CollectionType.Dictionary:
                    EmitDictionary(builder, type, indent, path, statementEnd);
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

                if (type.Class.Type == TypeEnum.Interface)
                {
                    builder.Append($"/* TODO: an interface ({GetGenericTypeName(type.Class)})! */ null");
                    EndStatement(builder, statementEnd);
                }
                else
                {
                    EmitComplexType(builder, type, indent, path, statementEnd);
                }

                return;
            }

            // Leaf nodes.
            EmitValueTypeConstant(builder, type, path);
            EndStatement(builder, statementEnd);
        }

        private static string GetSpaces(int indent) => new string(' ', indent * 4);

        private void EmitComplexType(StringBuilder builder, MemberInfo type, int indent, string path, StatementEndOptions statementEnd)
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

                AppendChild(builder, type, child, indent + 1, AppendPath(path, child.Name), childEnd);
            }

            builder.Append(spaces).Append("}");

            EndStatement(builder, statementEnd);
        }

        private string AppendPath(string path, string name)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return name;
            }

            return path + "." + name;
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

        private void AppendChild(StringBuilder builder, MemberInfo type, MemberInfo child, int indent, string path, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);

            builder.Append(spaces).Append(child.Name).Append(" = ");

            EmitInitializer(builder, child, indent, path, statementEnd);
        }

        private void EmitArray(StringBuilder builder, MemberInfo collectionType, int indent, string path, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var elementType = collectionType.Class.GenericParameters[0];

            // TypeName includes square brackets for arrays (System.String[])
            builder.AppendFormat("new {0}{1}", collectionType.Class.TypeName, Environment.NewLine);
            EmitCollectionInitializer(builder, collectionType, elementType, indent, path, spaces, statementEnd);
        }

        private void EmitCollection(StringBuilder builder, MemberInfo collectionType, int indent, string path, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var elementType = collectionType.Class.GenericParameters[0];

            var typeName = $"{collectionType.Class.TypeName}<{elementType.Class.TypeName}>";

            builder.AppendFormat("new {0}{1}", typeName, Environment.NewLine);
            EmitCollectionInitializer(builder, collectionType, elementType, indent, path, spaces, statementEnd);
        }

        private void EmitCollectionInitializer(StringBuilder builder, MemberInfo collectionType, MemberInfo elementType, int indent, string path, string spaces, StatementEndOptions statementEnd)
        {
            builder.Append(spaces).AppendLine("{");

            var elementSpaces = GetSpaces(indent + 1);

            int i = 0;

            foreach (var elementInfo in _valueProvider.GetCollectionElements(collectionType, path, elementType))
            {
                builder.Append(elementSpaces);

                EmitInitializer(builder, elementInfo, indent + 1, $"{path}[{i++}]");
            }

            builder.Append(spaces).Append("}");

            EndStatement(builder, statementEnd);
        }

        private void EmitDictionary(StringBuilder builder, MemberInfo collectionType, int indent, string path, StatementEndOptions statementEnd)
        {
            var keyType = collectionType.Class.GenericParameters[0];
            var valueType = collectionType.Class.GenericParameters[1];

            var keyValueType = _valueProvider.Announce(keyType, valueType, path);

            var spaces = GetSpaces(indent);

            var typeName = $"{collectionType.Class.TypeName}<{keyType.Class.TypeName}, {valueType.Class.TypeName}>";

            builder.AppendFormat("new {0}{1}", typeName, Environment.NewLine);
            builder.Append(spaces).AppendLine("{");

            foreach (var elementInfo in _valueProvider.GetCollectionElements(collectionType, path, keyValueType))
            {
                builder.Append(GetSpaces(indent + 1)).Append("{ ");
                EmitInitializer(builder, elementInfo.Class.GenericParameters[0], indent + 1, path, StatementEndOptions.None);
                builder.Append(", ");
                EmitInitializer(builder, elementInfo.Class.GenericParameters[1], indent + 1, path, StatementEndOptions.None);
                builder.Append(" }");
                EndStatement(builder, StatementEndOptions.Comma | StatementEndOptions.Newline);
            }

            builder.Append(spaces).Append("}");
            EndStatement(builder, statementEnd);
        }

        private void EmitValueTypeConstant(StringBuilder builder, MemberInfo type, string path)
        {
            var value = _valueProvider.GetScalarValue(type, path);

            switch (value)
            {
                case Single s:
                    builder.Append(s.ToString(CultureInfo.InvariantCulture) + "f");
                    break;
                case Double d:
                    builder.Append(d.ToString(CultureInfo.InvariantCulture) + "d");
                    break;
                case Decimal m:
                    builder.Append(m.ToString(CultureInfo.InvariantCulture) + "m");
                    break;
                case Int64 l:
                    builder.Append(l.ToString(CultureInfo.InvariantCulture) + "l");
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
                case DateTimeOffset dto:
                    builder.AppendFormat("new DateTime({0}, {1}, {2}, {3}, {4}, {5}, TimeSpan.FromHours(2))", dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second);
                    break;
                default:
                    builder.Append(value ?? "null");
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
