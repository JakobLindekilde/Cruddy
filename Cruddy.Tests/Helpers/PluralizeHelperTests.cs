using Cruddy.Components;
using Cruddy.Helpers;

namespace Cruddy.Tests.Helpers
{
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
    }
}
