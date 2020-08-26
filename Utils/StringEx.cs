namespace GitStatFilter.Utils
{
    internal class StringEx
    {
        public static string OrDefault(string? s, string s2)
        {
            return s == null || string.IsNullOrEmpty(s) ? s2 : s;
        }
    }
}
