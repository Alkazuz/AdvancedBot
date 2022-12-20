using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot
{
    //https://github.com/flcoder/totro/blob/master/index.js
    public class NickGenerator
    {
        private static string[] vowels = new string[] {
            "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", 
            "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", 
            "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", 
            "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", 
            "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", "a|7", "e|7", "i|7", "o|7", "u|7", 
            "ae|7", "ai|7", "ao|7", "au|7", "aa|7", "ea|7", "eo|7", "eu|7", "ee|7", "ia|7", "io|7", "iu|7", 
            "ii|7", "oa|7", "oe|7", "oi|7", "ou|7", "oo|7", "eau|7", "y|7"
        };

        private static string[] consonants = new string[] {
            "b|7", "c|7", "d|7", "f|7", "g|7", "h|7", "j|7", "k|7", "l|7", "m|7", "n|7", "p|7", 
            "qu|6", "r|7", "s|7", "t|7", "v|7", "w|7", "x|7", "y|7", "z|7", "sc|7", "ch|7", "gh|7", 
            "ph|7", "sh|7", "th|7", "wh|6", "ck|5", "nk|5", "rk|5", "sk|7", "wk|0", "cl|6", "fl|6", "gl|6", 
            "kl|6", "ll|6", "pl|6", "sl|6", "br|6", "cr|6", "dr|6", "fr|6", "gr|6", "kr|6", "pr|6", "sr|6", 
            "tr|6", "ss|5", "st|7", "str|6", "b|7", "c|7", "d|7", "f|7", "g|7", "h|7", "j|7", "k|7", 
            "l|7", "m|7", "n|7", "p|7", "r|7", "s|7", "t|7", "v|7", "w|7", "b|7", "c|7", "d|7", 
            "f|7", "g|7", "h|7", "j|7", "k|7", "l|7", "m|7", "n|7", "p|7", "r|7", "s|7", "t|7", 
            "v|7", "w|7", "br|6", "dr|6", "fr|6", "gr|6", "kr|6"
	    };

        private static Random RNG = new Random();

        private static void select(string[] arr, out int flags, out string data)
        {
            string d = arr[RNG.Next(0, arr.Length)];
            int sep = d.IndexOf('|');

            flags = int.Parse(d.Substring(sep + 1));
            data = d.Substring(0, sep);
        }

        public static string TotroRandomName(int minsyl, int maxsyl)
        {
            int flags;
            string data;

            string genname = ""; // this accumulates the generated name.
            int leng = RNG.Next(minsyl, maxsyl + 1); // Compute number of syllables in the name
            bool isvowel = RNG.Next(2) != 0; // randomly start with vowel or consonant
            for (int i = 1; i <= leng; i++) // syllable #. Start is 1 (not 0)
            {
                do {
                    if (isvowel)
                        select(vowels, out flags, out data); //vowels[rolldie(0, vowels.Count - 1)];
                    else
                        select(consonants, out flags, out data);//consonants[rolldie(0, consonants.Count - 1)];

                    if (i == 1) { // first syllable.
                        if ((flags & 2) != 0)
                            break;
                    } else if (i == leng) { // last syllable.
                        if ((flags & 1) != 0)
                            break;
                    } else { // middle syllable.
                        if ((flags & 4) != 0)
                            break;
                    }
                } while (true);
                genname += data;
                isvowel = !isvowel;
            }
            return genname.Substring(0, 1).ToUpper() + genname.Substring(1);
        }

        public static string PseudoNick()
        {
            string nick;
            do {
                nick = TotroRandomName(4, 8);
                if (RNG.Next(5) != 0) {
                    int mode = RNG.Next(19);
                    switch (mode) {
                        case 0: nick = "_" + nick + "_"; break;
                        case 1: nick += "B" + (RNG.Next(2) == 0 ? "r" : "R"); break;
                        case 2: nick += "_B" + (RNG.Next(2) == 0 ? "r" : "R"); break;
                        case 3: nick += "HD"; break;
                        case 4: nick = "ixyz"[RNG.Next(4)] + nick; break;
                        case 5: nick += RNG.Next(2) == 0 ? "Jr" : "_Jr"; break;
                        case 6: nick = "xX" + nick + "Xx"; break;
                        case 7: nick += RNG.Next(1000); break;
                        case 8: nick += "_"; break;
                        case 9: nick += RNG.Next(2) == 0 ? "XD" : "xD"; break;
                        case 10: nick += RNG.Next(1000).ToString("#000"); break;
                        case 11: nick += NickGenerator.TotroRandomName(2, 4); break;
                        case 12: nick = "The" + nick; break;
                        case 13: nick += "Game" + "sr"[RNG.Next(2)]; break;
                        default: nick = RNG.Next(10) < 3 ? nick.ToUpper() : nick.ToLower(); break;
                    }
                }
            } while (nick.Length > 16);
            return nick;
        }
        public static string RandomNick(int len, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++) {
                for (int j = 0; j < len; j++) {
                    int type = RNG.Next(3);
                    sb.Append((char)((type == 0 ? 'A' : type == 1 ? 'a' : '0') + RNG.Next(type == 2 ? 10 : 26)));
                }
                if (count != 1) sb.AppendLine();
            }
            return sb.ToString();
        }

        private static readonly char[] CharDefaults = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        public static string Sequential(int seq)
        {
            int radix = CharDefaults.Length;

            char[] s = new char[(int)(seq == 0 ? 0 : Math.Log(seq, radix)) + 1];
            for (int i = s.Length - 1; i >= 0; i--, seq /= radix) {
                s[i] = CharDefaults[seq % radix];
            }
            return new string(s);
        }
    }
}
