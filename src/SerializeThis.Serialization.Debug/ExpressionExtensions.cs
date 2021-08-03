using EnvDTE;
using System.Collections.Generic;

namespace SerializeThis.Serialization.Debug
{
    public static class ExpressionExtensions
    {
        private static readonly Dictionary<string, string> CSharpBuiltins = new Dictionary<string, string>
        {
            { "bool", "System.Boolean" },
            { "string", "System.String" },
            { "byte", "System.Byte" },
            { "char", "System.Char" },
            { "decimal", "System.Decimal" },
            { "double", "System.Double" },
            { "float", "System.Single" },
            { "int", "System.Int32" },
            { "long", "System.Int64" },
        };

        public static string GetTypeName(this Expression expresion)
        {
            if (CSharpBuiltins.TryGetValue(expresion.Type, out var typeName))
            {
                return typeName;
            }

            return expresion.Type;
        }
    }
}
