namespace SerializeThis.Serialization
{
    /// <summary>
    /// Types that we support
    /// </summary>
    public enum TypeEnum
    {
        ComplexType = 0,
        //ValueType = 1,
        Interface = 13,
        AbstractClass = 14,
        Struct = 15,

        // TODO: add ValueType = 1, move below to ValueTypeEnum

        Boolean = 1,

        String = 2,
        Char = 3,

        Byte = 4,
        Int16 = 5,
        Int32 = 6,
        Int64 = 7,

        Float32 = 8,
        Float64 = 9,

        Decimal = 10,

        DateTime = 11,
        DateTimeOffset = 12,
        

        //sbyte   System.SByte

        //uint    System.UInt32
        //nint    System.IntPtr
        //nuint   System.UIntPtr

        //ulong   System.UInt64
        //short   System.Int16
        //ushort  System.UInt16
    }
}
