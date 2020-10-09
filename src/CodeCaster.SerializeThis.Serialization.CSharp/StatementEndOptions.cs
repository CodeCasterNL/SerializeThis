using System;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    [Flags]
    public enum StatementEndOptions
    {
        None = 0,
        Newline = 1,
        Comma = 2,
        Semicolon = 4,
    }
}
