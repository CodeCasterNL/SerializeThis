using Microsoft.CodeAnalysis;
using System.Linq;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public static class AttributeDataExtensions
    {
        /// <summary>
        /// Returns the Nth constructor argument (if passed non-null argument), or the given property name's value.
        ///
        /// For example `[Foo("Bar")]` will return "Bar" for GetArgOrNamedProperty(0, null), `[Foo(Baz = "Bar"]` will return "Bar" for GetArgOrNamedProperty(null, "Baz").
        /// </summary>
        public static string GetArgOrNamedProperty(this AttributeData attribute, int? constructorArgumentIndex, string propertyName)
        {
            if (constructorArgumentIndex.HasValue && attribute.ConstructorArguments.Length == constructorArgumentIndex + 1)
            {
                return attribute.ConstructorArguments[0].Value?.ToString();
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            return attribute.NamedArguments.Where(namedArg => namedArg.Key == propertyName)
                                           .Select(namedArg => namedArg.Value.Value?.ToString())
                                           .FirstOrDefault();
        }
    }
}
