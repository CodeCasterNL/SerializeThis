using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2VSIXProject
{
    public class Foo
    {
        public string Name { get; set; }
    }

    public class Foo2 : Foo
    {
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    /// <summary>
    /// Collections
    /// </summary>
    public class Foo3 : Foo
    {
        public Foo3 Parent { get; set; }

        public List<Foo3> Children { get; set; }

        public IList<Foo3> ChildrenI { get; set; }
    }

    public class Foo4 : Foo
    {
        private Foo4()
        {
            // Cannot be constructed?
        }
    }

    /// <summary>
    /// Nullables
    /// </summary>
    public class Foo5 : Foo
    {
        public int? NullableIntProperty { get; set; }
        public DateTime? NullableDateTimeProperty { get; set; }
    }

    /// <summary>
    /// Numerics
    /// </summary>
    public class Foo6 : Foo
    {
        public byte Byte { get; set; }
        public short Int16 { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }        
    }
}
