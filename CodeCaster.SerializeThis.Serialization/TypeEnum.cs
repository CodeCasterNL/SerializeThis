using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCaster.SerializeThis.Serialization
{
    /// <summary>
    /// Types that we support
    /// </summary>
    public enum TypeEnum
    {
        ComplexType = 0,

        Boolean = 1,

        String = 2,

        Int8 = 3,
        Int16 = 4,
        Int32 = 5,
        Int64 = 6,

        Float8 = 7,
        Float16 = 8,
        Float32 = 9,

        Decimal = 10,

        DateTime = 11,

        Byte = 12,
    }
}
