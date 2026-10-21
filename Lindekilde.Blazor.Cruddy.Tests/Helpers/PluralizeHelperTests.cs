using Lindekilde.Blazor.Cruddy.Helpers;

namespace Lindekilde.Blazor.Cruddy.Tests.Helpers;

public class PluralizeHelperTests
{
    [Theory]
    [InlineData('a', true)]
    [InlineData('e', true)]
    [InlineData('i', true)]
    [InlineData('o', true)]
    [InlineData('u', true)]
    [InlineData('A', true)]
    [InlineData('E', true)]
    [InlineData('I', true)]
    [InlineData('O', true)]
    [InlineData('U', true)]
    [InlineData('b', false)]
    [InlineData('y', false)]   // 'y' is sometimes a vowel. It's handled in Pluralize()
    [InlineData('z', false)]
    [InlineData('1', false)]
    [InlineData('?', false)]
    [InlineData('_', false)]
    [InlineData(' ', false)]
    [InlineData('-', false)]
    [InlineData('æ', false)]   // Danish/Norwegian vowels
    [InlineData('ø', false)]
    [InlineData('å', false)]
    [InlineData('Æ', false)]
    [InlineData('Ø', false)]
    [InlineData('Å', false)]
    public void IsVowel_Works_ForVariousChars(char c, bool expected)
    {
        var result = PluralizeHelper.IsVowel(c);
        Assert.Equal(expected, result);
    }

    [Theory]
    // y preceded by consonant -> ies
    [InlineData("Category", "Categories")]
    [InlineData("city", "cities")]
    [InlineData("baby", "babies")]
    [InlineData("by", "bies")]

    // y preceded by vowel -> just add s
    [InlineData("key", "keys")]
    [InlineData("day", "days")]

    // endings that add es
    [InlineData("bus", "buses")]
    [InlineData("box", "boxes")]
    [InlineData("buzz", "buzzes")]
    [InlineData("church", "churches")]
    [InlineData("dish", "dishes")]

    // regular plurals
    [InlineData("cat", "cats")]
    [InlineData("dog", "dogs")]

    // case preservation tests
    [InlineData("Bus", "Buses")]
    [InlineData("BUS", "BUSES")]
    [InlineData("CHurCh", "CHurChes")]
    public void Pluralize_Returns_Expected(string singular, string expectedPlural)
    {
        var plural = PluralizeHelper.Pluralize(singular);
        Assert.Equal(expectedPlural, plural);
    }


    [Theory]
    // y preceded by consonant -> ies
    [InlineData("Categories", true)]
    [InlineData("Cities", true)]
    [InlineData("Babies", true)]
    [InlineData("Bies", true)]

    // y preceded by vowel -> just add s
    [InlineData("Keys", true)]
    [InlineData("Days", true)]

    // endings that add es
    [InlineData("Buses", true)]
    [InlineData("Boxes", true)]
    [InlineData("Buzzes", true)]
    [InlineData("Churches", true)]
    [InlineData("Dishes", true)]

    // regular plurals
    [InlineData("Cats", true)]
    [InlineData("Dogs", true)]

    // case preservation tests
    [InlineData("BUSES", true)]
    [InlineData("CHurChes", true)]

    [InlineData("Category", false)]
    [InlineData("city", false)]
    [InlineData("baby", false)]
    [InlineData("by", false)]
    [InlineData("key", false)]
    [InlineData("day", false)]
    [InlineData("bus", false)]
    [InlineData("box", false)]
    [InlineData("buzz", false)]
    [InlineData("church", false)]
    [InlineData("dish", false)]
    [InlineData("cat", false)]
    [InlineData("dog", false)]
    [InlineData("BUS", false)]
    [InlineData("CHurCh", false)]
    [InlineData("", false)]
    public void IsPluralized_Returns_Expected(string plural, bool expected)
    {
        var result = PluralizeHelper.IsPluralized(plural);
        Assert.Equal(expected, result);
    }

}
