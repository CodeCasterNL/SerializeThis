using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        private int _counter;
        public string FileExtension => "cs";

        public string DisplayName => "C# object initializer";

        public bool CanSerialize(ClassInfo type) => true;

        public string Serialize(ClassInfo type)
        {
            _counter = 0;
            
            var builder = new StringBuilder();
            builder.Append("var foo = ");

            EmitInitializer(builder, type, 0, StatementEndOptions.Semicolon | StatementEndOptions.Newline);
            
            return builder.ToString();
        }

        private void EmitInitializer(StringBuilder builder, ClassInfo type, int indent, StatementEndOptions statementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
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

            EmitValueTypeConstant(builder, type, statementEnd);
        }

        private static string GetSpaces(int indent) => new string(' ', indent * 4);

        private void EmitComplexType(StringBuilder builder, ClassInfo type, int indent, StatementEndOptions statementEnd)
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

        private string GetGenericTypeName(Class type)
        {
            if (!type.GenericParameters.Any())
            {
                return type.TypeName;
            }
            
            // TODO: cache?
            return type.TypeName + "<" + string.Join(", ", type.GenericParameters.Select(p => GetGenericTypeName(p.Class))) + ">";
        }

        private void AppendChild(StringBuilder builder, ClassInfo type, ClassInfo child, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);

            builder.Append(spaces).Append(child.Name).Append(" = ");

            EmitInitializer(builder, child, indent, statementEnd);
        }
        
        private void EmitArray(StringBuilder builder, ClassInfo type, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var elementType = type.Class.GenericParameters[0];

            // TypeName includes square brackets for arrays (System.String[])
            builder.AppendFormat("new {0}{1}", type.Class.TypeName, Environment.NewLine);
            EmitCollectionInitializer(builder, elementType, indent, spaces, statementEnd);
        }

        private void EmitCollection(StringBuilder builder, ClassInfo type, int indent, StatementEndOptions statementEnd)
        {
            var spaces = GetSpaces(indent);
            var elementType = type.Class.GenericParameters[0];

            var typeName = $"{type.Class.TypeName}<{elementType.Class.TypeName}>";

            builder.AppendFormat("new {0}{1}", typeName, Environment.NewLine);
            EmitCollectionInitializer(builder, elementType, indent, spaces, statementEnd);
        }

        private void EmitCollectionInitializer(StringBuilder builder, ClassInfo elementType, int indent, string spaces, StatementEndOptions statementEnd)
        {
            void EmitCollectionEntry(int elementIndent, StatementEndOptions elementStatementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
            {
                builder.Append(GetSpaces(elementIndent));
                EmitInitializer(builder, elementType, elementIndent, elementStatementEnd);
            }

            builder.Append(spaces).AppendLine("{");

            // TODO: 3 is hardcoded here
            EmitCollectionEntry(indent + 1);
            EmitCollectionEntry(indent + 1);
            EmitCollectionEntry(indent + 1, StatementEndOptions.Newline);

            builder.Append(spaces).Append("}");
            EndStatement(builder, statementEnd);
        }

        private void EmitDictionary(StringBuilder builder, ClassInfo type, int indent, StatementEndOptions statementEnd)
        {
            var keyType = type.Class.GenericParameters[0];
            var valueType = type.Class.GenericParameters[1];

            void EmitDictionaryEntry(int elementIndent, StatementEndOptions elementStatementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
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
            
            // TODO: 3 is hardcoded here
            EmitDictionaryEntry(indent + 1);
            EmitDictionaryEntry(indent + 1);
            EmitDictionaryEntry(indent + 1, StatementEndOptions.Newline);
            
            builder.Append(spaces).Append("}");
            EndStatement(builder, statementEnd);
        }

        private void EmitValueTypeConstant(StringBuilder builder, ClassInfo type, StatementEndOptions statementEnd = StatementEndOptions.Comma | StatementEndOptions.Newline)
        {
            _counter++;

            var value = GetValue(type);

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

            EndStatement(builder, statementEnd);
        }

        private object GetValue(ClassInfo type)
        {
            if (type.Class.IsEnum)
            {
                // TODO: get enum example member (or 0)?
                var enumMember = type.Class.Children.FirstOrDefault()?.Name ?? "0";
                return $"{type.Class.TypeName}.{enumMember}";
            }

            switch (type.Class.Type)
            {
                case TypeEnum.Boolean:
                    return _counter % 2 == 0;
                case TypeEnum.String:
                    return $"\"{type.Name}FooString{_counter}\"";
                case TypeEnum.DateTime:
                    return DateTime.Now.ToUniversalTime().AddSeconds(_counter);
                case TypeEnum.Int16:
                    return (Int16)_counter;
                case TypeEnum.Int32:
                    return (Int32)_counter;
                case TypeEnum.Int64:
                    return (Int64)_counter;
                case TypeEnum.Float32:
                    return (_counter + .42f).ToString(CultureInfo.InvariantCulture) + "f";
                case TypeEnum.Float64:
                    return (_counter + .42d).ToString(CultureInfo.InvariantCulture) + "d";
                case TypeEnum.Decimal:
                    return (_counter + .42m).ToString(CultureInfo.InvariantCulture) + "m";
                case TypeEnum.Byte:
                    return (Byte)_counter;

                default:
                    return null;
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
