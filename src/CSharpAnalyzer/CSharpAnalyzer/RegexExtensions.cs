using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpAnalyzer
{
    internal static class RegexExtensions
    {
        public static bool TryGetMatch(this Regex regex, string input, out Match match)
        {
            if (input == null)
            {
                match = null;
                return false;
            }

            match = regex.Match(input);

            return match.Success;
        }
    }
}
