using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// For FooAttributes.
using System.Runtime.Serialization;

namespace JsonTestClasses
{
    /// <summary>
    /// You can use this project to test the debugger extension.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        { 
            object fi = new FooInherited
            {
                Firstname = "@Runtime1",
                Lastname = null,
                Age = 42,
                DateOfBirth = new DateTime(2021, 07, 31),
            };

            var twoSame = new FooComplexType
            {
                BoolProperty = true,
                ChildProperty1 = new FooBase
                {
                    Firstname = "F001",
                    Lastname = "",
                },
                ChildProperty2 = new FooInherited
                {
                    Firstname = "class2",
                    Lastname = null,
                    Age = 42,
                    DateOfBirth = new DateTime(2021, 08, 01),
                }
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
                },
                IntStringDict = new Dictionary<int, string>
                {
                    { 42, "Forty-Two" },
                    { 1, null },
                    { Int32.MaxValue, "Over 9000" },
                }
            };

            var fooAnon = new
            {
                StringProp = "Foo",
                AnInt = 42,
                Q = GetQueryable().AsQueryable()
            };

            Debugger.Break();
        }

        private static IEnumerable<string> GetQueryable()
        {
            yield return "";
        }
    }

    /// <summary>
    /// Properties of built-in types.
    /// </summary>
    public class FooBase
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool BoolProperty { get; set; }
    }

    /// <summary>
    /// Inheritance.
    /// </summary>
    public class FooInherited : FooBase
    {
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    /// <summary>
    /// Some simple collections.
    /// </summary>
    public class FooSimpleCollections
    {
        public string[] StringArray { get; set; }
        public List<FooBase> FooBaseList { get; set; }
        public Dictionary<int, string> IntStringDict { get; set; }
    }

    /// <summary>
    /// Nullables.
    /// </summary>
    public class FooNullable : FooBase
    {
        public int? NullableIntProperty { get; set; }
        public DateTime? NullableDateTimeProperty { get; set; }
    }

    /// <summary>
    /// Numerics.
    /// </summary>
    public class FooNumerics : FooBase
    {
        public byte Char { get; set; }
        public byte? NullableChar { get; set; }
        public byte Byte { get; set; }
        public byte? NullableByte { get; set; }
        public short Int16 { get; set; }
        public short? NullableInt16 { get; set; }
        public int Int32 { get; set; }
        public int? NullableInt32 { get; set; }
        public long Int64 { get; set; }
        public long? NullableInt64 { get; set; }
        public float Float { get; set; }
        public float? NullableFloat { get; set; }
        public double Double { get; set; }
        public double? NullableDouble { get; set; }
        public decimal Decimal { get; set; }
        public decimal? NullableDecimal { get; set; }
    }

    /// <summary>
    /// Enums.
    /// </summary>
    public class FooWithEnum : FooBase
    {
        public enum FooEnum
        {
            Foo = 0,
            Bar = 1,
        }

        public FooEnum EnumMember { get; set; }
    }

    /// <summary>
    /// ComplexType properties.
    /// </summary>
    public class FooComplexType : FooBase
    {
        public FooBase ChildProperty1 { get; set; }
        public FooBase ChildProperty2 { get; set; }
    }

    /// <summary>
    /// Collections.
    /// </summary>
    public class FooCollections
    {
        public int[,] MultiDimInt32s { get; set; }
        public int[][] JaggedInt32s { get; set; }
        public ICollection<FooInherited> ChildrenICollection { get; set; }
        public IMyCollection<FooInherited> InterfaceInheritance { get; set; }
        public IMyCollection2<FooInherited> InterfaceDeeperInheritance { get; set; }
        public string[] StringArray { get; set; }
        public List<FooBase> ChildrenList { get; set; }
        public IList<FooInherited> ChildrenIList { get; set; }
        public bool?[] NullableBoolArray { get; set; }
        public Dictionary<int, string> DictionaryIntString { get; set; }
    }

    /// <summary>
    /// Dictionaries.
    /// </summary>
    public class FooDictionaries
    {
        public Dictionary<int, string> IntStringDict { get; set; }
        public IDictionary<string, FooBase> StringFooBaseInterface { get; set; }
        public IMyDictionary<int, Dictionary<int, FooComplexType>> IntFooComplexDerivedInterface { get; set; }
    }

    public interface IMyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
    }

    public interface IMyCollection2<T> : IMyCollection<T>
    {
    }

    public interface IMyCollection<T> : ICollection<T>
    {
    }


    /// <summary>
    /// Recursion.
    /// </summary>
    public class FooRecursion : FooBase
    {
        public FooRecursion Parent { get; set; }
    }

    /// <summary>
    /// Recursion, but more distant.
    /// </summary>
    public class FooRecursionDeeper : FooBase
    {
        public class FooRecursionD1 : FooBase
        {
            public class FooRecursionD2 : FooBase
            {
                public FooRecursionDeeper RecursionDeeper { get; set; }
            }

            public FooRecursionD2 Recursion2 { get; set; }
        }

        public FooRecursionD1 Recursion1 { get; set; }
    }

    /// <summary>
    /// Reflection.
    /// </summary>
    public class FooPrivateConstructor : FooBase
    {
        private FooPrivateConstructor()
        {
            // Cannot be constructed? Should we care?
        }
    }

    /// <summary>
    /// Generics.
    /// </summary>
    public class FooGenerics : FooBase
    {
        public class Foo7Child<T>
        {
            public T TProp { get; set; }
        }

        public Foo7Child<string> FooString { get; set; }
        public Foo7Child<string>[] FooStrings { get; set; }
    }

    public class FooWithFooBase : FooWithT<FooAttributes> { }

    public class FooWithT<T>
    {
        public T PropertyOfT { get; set; }
    }

    /// <summary>
    /// Attributes to rename stuff.
    /// </summary>
    [DataContract]
    public class FooAttributes
    {
        [Newtonsoft.Json.JsonProperty("first_name")]
        public string Firstname { get; set; }

        [System.Text.Json.JsonPropertyName(Name = "Last_Name")]
        public string Lastname { get; set; }

        [DataMember(Name = "YesNo")]
        public bool BoolProperty { get; set; }
    }
}

// These are here so the attribute test works. This is a standalone file without references, so Roslyn won't know the attributes otherwise.
namespace Newtonsoft.Json
{
    public class JsonPropertyAttribute : System.Attribute
    {
        public string PropertyName { get; set; }

        public JsonPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}

namespace System.Text.Json
{
    public class JsonPropertyNameAttribute : System.Attribute
    {
        public string Name { get; set; }
    }
}

namespace System.Runtime.Serialization
{
    public class DataContractAttribute : System.Attribute { }

    public class DataMemberAttribute : System.Attribute
    {
        public string Name { get; set; }
    }
}
// End attributes.