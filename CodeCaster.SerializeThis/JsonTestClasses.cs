using System;
using System.Collections.Generic;

namespace JsonTestClasses
{
    /// <summary>
    /// Properties of built-in types.
    /// </summary>
    public class FooBase
    {
        public string Name { get; set; }
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
    /// Collections.
    /// </summary>
    public class FooCollections : FooBase
    {
        public List<FooInherited> Children { get; set; }
        public IList<FooInherited> ChildrenI { get; set; }
        public bool?[] NullableBoolArray { get; set; }
        public Dictionary<int, string> DictionaryProperty { get; set; }
    }

    /// <summary>
    /// Recursion.
    /// </summary>
    public class FooRecursion : FooBase
    {
        public FooRecursion Parent { get; set; }
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
}
