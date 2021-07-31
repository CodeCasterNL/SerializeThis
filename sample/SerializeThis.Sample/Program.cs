using JsonTestClasses;
using System.Diagnostics;

namespace SerializeThis.Sample
{
    /// <summary>
    /// You can use this project to test the debugger extension.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {

            var fi = new FooInherited();
            var fd = new FooDictionaries();

            var attributeTest = new FooWithFooBase();

            Debugger.Break();
        }
    }
}
