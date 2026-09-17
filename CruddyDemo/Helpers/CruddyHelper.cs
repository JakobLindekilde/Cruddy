using System.Reflection;
using static Dapper.SqlMapper;

namespace CruddyDemo.Helpers
{

    /// <summary>
    /// Provides helper methods for Cruddy.
    /// </summary>
    static public class CruddyHelper
    {
        /// <summary>
        /// Pluralizes a given name according to basic English rules. If the name ends with 'y' and is preceded by a
        /// consonant, it replaces 'y' with 'ies'. If the name ends with 's', 'x', 'z', 'ch', or 'sh', it adds 'es'. 
        /// Otherwise, it simply adds 's'.
        /// </summary>
        /// <param name="name">The name to pluralize.</param>
        /// <returns>The pluralized form of the name.</returns>
        static public string Pluralize(string name)
        {
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !IsVowel(name[name.Length - 2]))
            {
                return name.Substring(0, name.Length - 1) + "ies";
            }
            else if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                return name + "es";
            }
            else
            {
                return name + "s";
            }
        }

        /// <summary>
        /// Determines if a character is a vowel (a, e, i, o, u) in either uppercase or lowercase.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is a vowel; otherwise, false.</returns>
        public static bool IsVowel(char c)
        {
            return "aeiouAEIOU".IndexOf(c) >= 0;
        }

    }
}
