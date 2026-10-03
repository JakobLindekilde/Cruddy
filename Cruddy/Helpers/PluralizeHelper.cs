namespace Cruddy.Helpers;

/// <summary>
/// Provides helper methods for pluralizing names according to basic English rules.
/// </summary>
static public class PluralizeHelper
{
    /// <summary>
    /// Pluralizes a given name according to basic English rules. If the name ends with 'y' and is preceded
    /// by a consonant, it replaces 'y' with 'ies'. If the name ends with 's', 'x', 'z', 'ch', or 'sh', 
    /// it adds 'es'. Otherwise, it simply adds 's'.
    /// Casing is preserved, so if the last character of the name is uppercase, the pluralized form 
    /// will also be in uppercase.
    /// </summary>
    /// <param name="name">The name to pluralize.</param>
    /// <returns>The pluralized form of the name.</returns>
    static public string Pluralize(string name)
    {
        string pluralized;
        if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !IsVowel(name[^2]))
        {
            pluralized = name[..^1] + "ies";
        }
        else if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            pluralized = name + "es";
        }
        else
        {
            pluralized = name + "s";
        }

        if (char.IsUpper(name[^1]))
        {
            pluralized = pluralized.ToUpper();
        }

        return pluralized;
    }

    /// <summary>
    /// Determines if a character is a vowel (a, e, i, o, u ) in either uppercase or lowercase.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is a vowel; otherwise, false.</returns>
    public static bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c);
    }

}
