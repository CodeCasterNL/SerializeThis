namespace SerializeThis.Serialization
{
    public static class StringExtensions
    {
        public static string LowercaseFirst(this string s)
        {
            if (s == null || s.Length < 2)
            {
                return s?.ToLowerInvariant();
            }

            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }
}
