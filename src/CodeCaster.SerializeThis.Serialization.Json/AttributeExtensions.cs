using System.Linq;

namespace CodeCaster.SerializeThis.Serialization.Json
{
    public static class AttributeExtensions
    {
        /// <summary>
        /// Returns the Nth constructor argument (if passed non-null argument), or the given property name's value, or null if not found.
        ///
        /// For example `[Foo("Bar")]` will return "Bar" for GetArgOrNamedProperty(0, null), `[Foo(Baz = "Bar"]` will return "Bar" for GetArgOrNamedProperty(null, "Baz").
        /// </summary>
        public static string GetArgOrNamedProperty(this AttributeInfo attribute, int? constructorArgumentIndex, string propertyName)
        {

            if (constructorArgumentIndex.HasValue && attribute.ConstructorArguments.Length == constructorArgumentIndex + 1)
            {
                return attribute.ConstructorArguments[0]?.ToString();
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            return attribute.Properties.Where(namedArg => namedArg.Key == propertyName)
                .Select(namedArg => namedArg.Value?.ToString())
                .FirstOrDefault();
        }
    }
}
