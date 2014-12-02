using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpEntropy;

namespace InvalidEmailDetector.src
{
    public class InvalidEmailDetectionContextGenerator : IContextGenerator<string>
    {
        private readonly Regex _vowelsRegex = new Regex("[aeiouy]+", RegexOptions.Compiled);
        private readonly Regex _digitsRegex = new Regex("\\d+", RegexOptions.Compiled);

        // top level domains have been retrieved on http://en.wikipedia.org/wiki/List_of_Internet_top-level_domains
        private readonly List<string> _originalTlds = new List<string>(){"com", "org", "net", "int", "edu", "gov", "mil"};
        private readonly List<string> _countryTlds = new List<string>()
        {
            "ac", "uk", "ad", "ae", "af", "ag", "ai", "al", "am", "an", "ao", "aq", "ar", "as", "at", "au", "aw", "ax", "az", 
            "ba", "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bm", "bn", "bo", "bq", "br", "bs", "bt", "bv", "no", "bw", 
            "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", "ci", "ck", "cl", "cm", "cn", "co", "cr", "cs", "cu", "cv", "cw",
            "cx", "cy", "cz", "dd", "de", "dj", "dk", "dm", "do", "dz", "ec", "ee", "eg", "eh", "er", "es", "et", "eu", "fi",
            "fj", "fk", "fm", "fo", "fr", "ga", "gb", "uk", "gd", "ge", "gf", "gg", "gh", "gi", "gl", "gm", "gn", "gp", "gq", 
            "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr", "ht", "hu", "id", "ie", "il", "im", "in", "io", "iq", 
            "ir", "is", "it", "je", "jm", "jo", "jp", "ke", "kg", "kh", "ki", "km", "kn", "kp", "kr", "kw", "ky", "kz", "la", 
            "lb", "lc", "li", "lk", "lr", "ls", "lt", "lu", "lv", "ly", "ma", "mc", "md", "me", "mg", "mh", "mk", "ml", "mm", 
            "mn", "mn", "mo", "mp", "mq", "mr", "ms", "mt", "mu", "mv", "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng", 
            "ni", "nl", "no", "np", "nr", "nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl", "pm", "pn", "pr", "ps", 
            "pt", "pw", "py", "qa", "re", "ro", "rs", "ru", "su", "rw", "sa", "sb", "sc", "sd", "se", "sg", "sh", "si", "sj", 
            "no", "sk", "sl", "sm", "sn", "so", "sr", "ss", "st", "su", "sv", "sx", "sy", "sz", "tc", "td", "tf", "tg", "th", 
            "tj", "tk", "tl", "tp", "tm", "tn", "to", "tp", "tl", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "uk", "us", "uy", 
            "uz", "va", "vc", "ve", "vg", "vi", "vn", "vu", "wf", "ws", "ye", "yt", "yu", "za", "zm", "zr", "zw"
        };


        public string[] GetContext(string input)
        {
            var results = new List<string>();

            var parts = input.Split('@');
            var localPart = parts.First();
            var domain = parts.Last();
            var domainParts = domain.Split('.');

            // root part

            // nb of characters
            var nbOfCharacters = localPart.Length;
            results.Add("nb=" + nbOfCharacters);
            // nb of different characters
            var nbOfDifferentCharacters = localPart.ToCharArray().Distinct().Count();
            results.Add("nbdiff=" + nbOfDifferentCharacters);
            // percent of distinct characters (0, 5... 95, 100)
            var percentOfDistinctCharacters = ((nbOfDifferentCharacters * 100) / (5 * nbOfCharacters)) * 5;//only 5 in 5
            results.Add("perDistChar=" + percentOfDistinctCharacters);

            // has vowels
            var hasVowels = _vowelsRegex.IsMatch(localPart);
            results.Add("hVow=" + hasVowels);
            // has digits
            var hasDigits = _digitsRegex.IsMatch(localPart);
            results.Add("hDig=" + hasDigits);
            // contains '.'
            var hasDot = localPart.Contains(".");
            results.Add("hDot=" + hasDot);
            // contains '_'
            var hasUnderscore = localPart.Contains("_");
            results.Add("hUnders=" + hasUnderscore);
            // contains '-'
            var hasDash = localPart.Contains("-");
            results.Add("hDash=" + hasDash);

            // repeated n-grams
            for (var i = 2; i <= 3; i++)
            {
                var nGramsToOccurrences = new Dictionary<string, int>();
                for (var j = 0; j < localPart.Length - i + 1; j++)
                {
                    var ngram = localPart.Substring(j, i);
                    if (!nGramsToOccurrences.ContainsKey(ngram))
                    {
                        var nbOfOccurences = localPart.Split(new[] { ngram }, StringSplitOptions.None).Count() - 1;
                        if (nbOfOccurences > 1)
                        {
                            nGramsToOccurrences.Add(ngram, nbOfOccurences);
                        }
                    }
                }

                var nbOfRepetitionOfRepeatedNgrams = nGramsToOccurrences.Sum(ent => ent.Value);
                if (nbOfRepetitionOfRepeatedNgrams > 0)
                {
                    results.Add("ngram" + i + "=" + nbOfRepetitionOfRepeatedNgrams);
                }
            }

            // domain
            var hostName = string.Join(".", domainParts.Take(domainParts.Count() - 1));
            var hasHnVowel = _vowelsRegex.IsMatch(hostName);
            results.Add("hnHVow=" + hasHnVowel);

            // Top level domain
            var tld = domainParts.Last();
            if (_originalTlds.Contains(tld))
            {
                results.Add("oTld");
            } else if (_countryTlds.Contains(tld))
            {
                results.Add("cTld");
            }
            else
            {
                // unreferenced tld
                results.Add("uTld");
            }

            return results.ToArray();
        }
    }
}
