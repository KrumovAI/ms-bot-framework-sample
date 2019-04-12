namespace BasicBot.Infrastructure.Extensions
{
    using System.Collections.Generic;

    public static class StringExtensions
    {
        public static string ToCyrillic(this string latinPhrase)
        {
            string cyrillicPhrase = latinPhrase.ToLower();

            Dictionary<string, string> letters = new Dictionary<string, string>()
            {
                { "ya", "я" }, { "yu", "ю" }, { "sht", "щ" }, { "sh", "ш" }, { "ch", "ч" }, { "ts", "ц" }, { "zh", "ж" }, { "a", "а" }, { "b", "б" }, { "v", "в" },
                { "g", "г" }, { "d", "д" }, { "e", "е" }, { "z", "з" }, { "i", "и" }, { "y", "й" }, { "k", "к" }, { "l", "л" }, { "m", "м" }, { "n", "н" },
                { "o", "о" }, { "p", "п" }, { "r", "р" }, { "s", "с" }, { "t", "т" }, { "u", "у" }, { "f", "ф" }, { "h", "х" },
            };

            foreach (var letter in letters)
            {
                cyrillicPhrase = cyrillicPhrase.Replace(letter.Key, letter.Value);
            }

            return cyrillicPhrase;
        }
    }
}
