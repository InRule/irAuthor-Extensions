using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Utility
{
    public static class ElementNameGenerationExtensions
    {
        public static string EnsureUniquePeerName<T>(this IEnumerable<T> peers, Func<T, string> getName, string baseName)
        {
            var peerNames = peers.Select(peerDef => getName(peerDef ));
            int? suffix;
            return GetUniqueName(baseName, peerNames, out suffix);
        }

        private static string GetUniqueName(string name, IEnumerable<string> existingNames, out int? suffix)
        {
            suffix = null;

            if (existingNames.Contains(name))
            {
                name = GetBaseName(name);

                string newName;
                var count = 1;

                do
                {
                    suffix = count;
                    newName = name + count++;
                } while (existingNames.Contains(newName));


                name = newName;
            }

            return name;
        }
        private static string GetBaseName(string name)
        {
            // Mostly irAuthor v.Old
            // If name exists, attempt to strip off trailing numbers (if they exist)
            var match = new Regex(@"(.*)[0-9]*$", RegexOptions.RightToLeft).Match(name);

            if (match.Success)
            {
                name = match.Result("$1");
            }

            return name;
        }
    }
}
