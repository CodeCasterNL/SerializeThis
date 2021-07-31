using JsonTestClasses;
using System;
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
            var fi = new FooInherited
            {
                Firstname = "@Runtime1",
                Lastname = null, // PlzGenerate
                Age = 42,
                DateOfBirth = new DateTime(2021, 07, 31),
            };
            
            var fd = new FooDictionaries();

            var attributeTest = new FooWithFooBase();

            var fooSimpleCollections = new FooSimpleCollections
            {
                StringArray = new[]
                {
                    "1",
                    null,
                    null,
                    "4",
                    "Foo5",
                }
            };

            Debugger.Break();
        }
    }
}
