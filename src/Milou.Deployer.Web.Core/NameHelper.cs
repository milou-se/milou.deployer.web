using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core
{
    public static class NameHelper
    {
        public static string GetFullProjectName(string organization, string projectName)
        {
            return organization.Replace("-", "_") + "-" + projectName.Replace("-", "_");
        }

        public static bool IsNameValid([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            var allowedCharacters = new List<char>(100)
            {
                'A',
                'B',
                'C',
                'D',
                'E',
                'F',
                'G',
                'H',
                'I',
                'J',
                'K',
                'L',
                'M',
                'N',
                'O',
                'P',
                'Q',
                'R',
                'S',
                'T',
                'U',
                'V',
                'W',
                'X',
                'Y',
                'Z',
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                'g',
                'h',
                'i',
                'j',
                'k',
                'l',
                'm',
                'n',
                'o',
                'p',
                'q',
                'r',
                's',
                't',
                'u',
                'v',
                'w',
                'x',
                'y',
                'z',
                'å',
                'ä',
                'ö',
                'Å',
                'Ä',
                'Ö',
                '_',
                '-'
            };

            bool isNameValid = name.ToCharArray().All(c => allowedCharacters.Contains(c));

            return isNameValid;
        }
    }
}