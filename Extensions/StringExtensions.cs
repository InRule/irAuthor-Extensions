using System;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Allows case insensitive checks
        /// </summary>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return Contains(source,toCheck, StringComparison.OrdinalIgnoreCase);
        }
    }
}
