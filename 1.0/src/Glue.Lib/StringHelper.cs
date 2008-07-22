using System;
using System.Collections;
using System.Text;

namespace Glue.Lib
{
	/// <summary>
	/// String helper functions.
	/// </summary>
    public class StringHelper
    {
        private StringHelper() {}

        public static string Join(string separator, IEnumerable items) 
        {
            return Join(separator, null, null, items, 0, -1);
        }
        public static string Join(string separator, IEnumerable items, int index, int count)
        {
            return Join(separator, null, null, items, index, count);
        }
        public static string Join(string separator, string pre, string post, IEnumerable items) 
        {
            return Join(separator, pre, post, items, 0, -1);
        }
        public static string Join(string separator, string pre, string post, IEnumerable items, int index, int count) 
        {
            StringBuilder s = new StringBuilder();
            foreach (object item in items)
            {
                if (index-- > 0)
                    continue;
                if (count-- == 0)
                    break;
                if (index < -1)
                    s.Append(separator);
                s.Append(pre);
                s.Append(item);
                s.Append(post);
            }
            return s.ToString();
        }

        public static string EscapeCStyle(string s)
        {
            if (s == null)
                return null;
            int next = s.IndexOfAny(CStyleEscapes);
            if (next < 0)
                return s;
            System.Text.StringBuilder r = new System.Text.StringBuilder();
            int prev = 0;
            do 
            {
                r.Append(s, prev, next - prev);
                r.Append('\\');
                if (s[next] == '\t')      r.Append('t');
                else if (s[next] == '\r') r.Append('r');
                else if (s[next] == '\n') r.Append('n');
                else if (s[next] == '"')  r.Append('"');
                else if (s[next] == '\\') r.Append('\\');
                prev = next + 1;
                next = s.IndexOfAny(CStyleEscapes, prev);
            } while (next >= 0);
            r.Append(s, prev, s.Length - prev);
            return r.ToString();
        }

        public static string UnEscapeCStyle(string s)
        {
            if (s == null)
                return null;
            int next = s.IndexOf('\\');
            if (next < 0)
                return s;
            System.Text.StringBuilder r = new System.Text.StringBuilder();
            int prev = 0;
            do 
            {
                r.Append(s, prev, next - prev);
                next++;
                if (s[next] == 't')       r.Append('\t');
                else if (s[next] == 'r')  r.Append('\r');
                else if (s[next] == 'n')  r.Append('\n');
                else if (s[next] == '"')  r.Append('"');
                else if (s[next] == '\\') r.Append('\\');
                next++;
                prev = next;
                next = s.IndexOf('\\', prev);
            } while (next >= 0);
            r.Append(s, prev, s.Length - prev);
            return r.ToString();
        }

        public readonly static char[] EmptyChars = new char[] {};
        public readonly static char[] WhiteSpaceChars = new char[] {' ','\t','\r','\n'};
        public readonly static char[] StringDelimiters = new char[] {'\'','"'};
        public readonly static char[] PairedDelimiters = new char[] {'{','}','[',']','(',')'};
        static char[] CStyleEscapes = new char[] {'\t','\r','\n','\"','\\'};

        /// <summary>
        /// Splits a string in pieces. Handles different separators, string delimiters 
        /// and paired delimiters (such as ( ), { } etc).
        /// TODO special case when separator is whitespace.
        /// </summary>
        public static string[] Split(
            string s, 
            char[] separators, 
            char[] stringDelimiters, 
            char[] pairedDelimiters, 
            char[] trimChars,
            bool compress
            )
        {
            char lastStringDelimiter = (char)0;
            bool inString = false;
            int pairLevel = 0;
            char[] pairStack = new char[100];
            
            if (stringDelimiters == null)
                stringDelimiters = EmptyChars;
            if (pairedDelimiters == null)
                pairedDelimiters = EmptyChars;
            if (trimChars == null)
                trimChars = EmptyChars;

            System.Collections.ArrayList list = new System.Collections.ArrayList();
            System.Text.StringBuilder item = new System.Text.StringBuilder(1000);
            int i = 0;
            while (i < s.Length)
            {
                if (!inString && pairLevel == 0 && CharInside(s[i], separators) >= 0)
                {
                    if (!compress || item.Length > 0)
                        list.Add(item.ToString().Trim(trimChars));
                    item.Length = 0;
                }
                else
                {
                    item.Append(s[i]);
                }
                
                if (inString)
                {
                    if (s[i] == lastStringDelimiter)
                        inString = false;
                }
                else 
                {
                    int j = CharInside(s[i], stringDelimiters);
                    if (j >= 0)
                    {
                        lastStringDelimiter = s[i];
                        inString = true;
                    }
                    else if (pairLevel > 0 && s[i] == pairStack[pairLevel-1])
                    {
                        pairLevel--;
                    }
                    else 
                    {
                        j = CharInside(s[i], pairedDelimiters);
                        if (j >= 0)
                        {
                            pairStack[pairLevel++] = pairedDelimiters[j + 1];
                        }
                    }
                }
                i++;
            }
            if (!compress || item.Length > 0)
                list.Add(item.ToString().Trim(trimChars));
            
            return (string[])list.ToArray(typeof(string));
        }

        static int CharInside(char c, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
                if (chars[i] == c)
                    return i;
            return -1;
        }

        static readonly char[] spaces = new char[] {' ','\t'};

        public static string Slice(string s, int index)
        {
            return Slice(s, index, spaces);
        }

        public static string Slice(string s, int index, char[] separators)
        {
            if (s == null)
                return null;
            int start = 0;
            int next = -1;
            while (index >= 0)
            {
                start = next + 1;
                next = s.IndexOfAny(separators, start);
                index--;
            }
            if (index >= 0)
                return null;
            if (start < next)
                return s.Substring(start, next - start);
            else
                return s.Substring(start);
        }

        public static string Truncate(string s, int maxlen)
        {
            if (s == null || s.Length <= maxlen)
                return s;
            else
                return s.Substring(0, maxlen);
        }

        public static string Ellipse(string s, int maxlen)
        {
            return Ellipse(s, maxlen, "...");
        }

        public static string Ellipse(string s, int maxlen, string ellipseMarker)
        {
            if (s == null || s.Length <= maxlen)
                return s;
            int i = maxlen - ellipseMarker.Length;
            while (i >= 0 && char.IsLetterOrDigit(s[i]))
                i--;
            if (i < 0)
                return s.Substring(0, maxlen - ellipseMarker.Length) + ellipseMarker;
            else
                return s.Substring(0, i) + ellipseMarker;
        }

        public static string Chomp(string s)
        {
            return Chomp(Chomp(s, "\n"), "\r");
        }

        public static string Chomp(string s, string what)
        {
            if (s == null)
                return s;
            if (s.EndsWith(what))
                return s.Substring(0, s.Length - what.Length);
            return s;
        }

        public static string Eat(string s, string what)
        {
            if (s == null)
                return s;
            if (s.StartsWith(what))
                return s.Remove(0, what.Length);
            return s;
        }

        /// <summary>
        /// Replace strings, ignoring case.
        /// </summary>
        public static string ReplaceIgnoreCase(string original, string pattern, string replacement)
        {
            if (original == null || original.Length == 0) 
                return original;
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (pattern == "") throw new ArgumentException("Cannot be empty string", "pattern");
            if (replacement == null) replacement = "";

            int count = 0;
            int position0 = 0;
            int position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int max = Math.Max(original.Length, (original.Length * replacement.Length) / pattern.Length);

            char [] chars = new char[max];
            while ((position1 = upperString.IndexOf(upperPattern, position0)) != -1)
            {
                for (int i = position0; i < position1; i++)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; i++)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) 
                return original;
            for (int i = position0; i < original.Length; i++)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }

        /// <summary>
        /// Returns the number of lines in string s. Only handles '\r\n' and '\n' endings.
        /// "line1" => 1
        /// "line1\r\n" => 2
        /// "line1\r\nline2" => 2
        /// </summary>
        public static int LineCount(string s)
        {
            int n = 0;
            int i = 0;
            while (i >= 0)
            {
                n++;
                i = s.IndexOf('\n', i + 1);
            }
            return n;
        }

        public static string Indent(string s, string indent)
        {
            string[] lines = s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
                lines[i] = indent + lines[i];
            return string.Join("\n", lines);
        }

        public static string Unindent(string s)
        {
            return Unindent(s, 4);
        }
        
        public static string Unindent(string s, int tabsize)
        {
            if (s == null)
                return null;
            string tab = new string(' ', tabsize);
            string[] lines = s.Replace("\t", tab).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int minindent = int.MaxValue;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim(' ');
                if (lines[i].Length == 0)
                    continue;
                for (int j = 0; j < lines[i].Length; j++)
                    if (lines[i][j] != ' ')
                    {
                        if (j < minindent)
                            j = minindent;
                        break;
                    }
            }
            if (minindent < int.MaxValue)
                for (int i = 0; i < lines.Length; i++)
                    lines[i] = lines[i].Remove(0, minindent);
            return string.Join("\n", lines);
        }


        /// <summary>
        /// Returns array of strings present in both arrays a and b.
        /// </summary>
        public static string[] Intersect(string[] a, string[] b, bool ignoreCase)
        {
            ArrayList list = new ArrayList();
            foreach (string s in a)
                if (IndexOfString(b, s, ignoreCase) >= 0)
                    list.Add(s);
            return (string[])list.ToArray(typeof(string));
        }

        /// <summary>
        /// Returns array of strings present in only *one* of given arrays a or b.
        /// </summary>
        public static string[] ExclusiveOr(string[] a, string[] b, bool ignoreCase)
        {
            ArrayList list = new ArrayList();
            foreach (string s in a)
                if (IndexOfString(b, s, ignoreCase) == -1)
                    list.Add(s);
            foreach (string s in b)
                if (IndexOfString(a, s, ignoreCase) == -1)
                    list.Add(s);
            return (string[])list.ToArray(typeof(string));
        }

        static int IndexOfString(string[] list, string s, bool ignoreCase)
        {
            for (int i = 0; i < list.Length; i++)
                if (string.Compare(list[i], s, ignoreCase) == 0)
                    return i;
            return -1;
        }

        /// <summary>
        /// Replaces spans of 2 or more of the same characters with a single character.
        /// RemoveSpans("123---hello-world", '-')  => "123-hello-world"
        /// </summary>
        public static string RemoveSpans(string s, char c)
        {
            StringBuilder d = new StringBuilder(s.Length);
            bool spanning = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != c || !spanning)
                    d.Append(s[i]);
                spanning = s[i] == c;
            }
            return d.ToString();
        }
    
        /// <summary>
        /// Replaces any non-word characters with a ? character.
        /// </summary>
        public static string StripNonWordChars(string s)
        {
            return StripNonWordChars(s, (char)0);
        }

        /// <summary>
        /// Strips any non-word characters from a string. 
        /// Replaces non-words with given replacement character or removes
        /// them when replacer = 0.
        /// </summary>
        public static string StripNonWordChars(string s, char replacer)
        {
            if (s == null || s.Length == 0)
                return s;
            StringBuilder d = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                    d.Append(c);
                else if (replacer != 0)
                    d.Append(replacer);
            }
            return d.ToString();
        }

        /// <summary>
        /// Returns the character without diacritics. The return
        /// value is a string because some special characters
        /// </summary>
        public static string RemoveDiacritic(char c)
        {
            if (c >= 0 && c <= 127)
                return c.ToString();
            if (map == null)
                InitializeDiacritics();
            string s = (string)map[c];
            if (s == null)
                return c.ToString();
            else
                return s;
        }
        
        /// <summary>
        /// Strips diacritics from a Latin-character unicode string. 
        /// Replaces unknown unicode characters with a ? character.
        /// </summary>
        public static string StripDiacritics(string s)
        {
            return StripDiacritics(s, '?');
        }

        /// <summary>
        /// Strips diacritics from a Latin-character unicode string. 
        /// Replaces unknown unicode characters with given replacement
        /// character. If replacer is zero, the unknown characters are
        /// removed.
        /// </summary>
        public static string StripDiacritics(string s, char replacer)
        {
            if (s == null || s.Length == 0)
                return s;

            StringBuilder d = new StringBuilder(s.Length);
            if (map == null)
                InitializeDiacritics();

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= 0 && c <= 127)
                {
                    d.Append(c);
                }
                else
                {
                    string v = (string)map[c];
                    if (v != null)
                        d.Append(v);
                    else if (replacer != 0)
                        d.Append(replacer);
                }
            }
            return d.ToString();
        }

        private static Hashtable map = null;

        /// <summary>
        /// Stores Unicode characters and their "normalized"
        /// values to a hash table. Character codes are referenced
        /// by hex numbers because that's the most common way to
        /// refer to them.
        /// </summary>
        private static void InitializeDiacritics()
        {
            // Upper-case comments are identifiers from the Unicode database. 
            // Lower- and mixed-case comments are the author's
            lock (typeof(StringHelper))
            {
                map = new Hashtable(1000);
                map['\x0041'] = "A";
                map['\x0042'] = "B";
                map['\x0043'] = "C";
                map['\x0044'] = "D";
                map['\x0045'] = "E";
                map['\x0046'] = "F";
                map['\x0047'] = "G";
                map['\x0048'] = "H";
                map['\x0049'] = "I";
                map['\x004A'] = "J";
                map['\x004B'] = "K";
                map['\x004C'] = "L";
                map['\x004D'] = "M";
                map['\x004E'] = "N";
                map['\x004F'] = "O";
                map['\x0050'] = "P";
                map['\x0051'] = "Q";
                map['\x0052'] = "R";
                map['\x0053'] = "S";
                map['\x0054'] = "T";
                map['\x0055'] = "U";
                map['\x0056'] = "V";
                map['\x0057'] = "W";
                map['\x0058'] = "X";
                map['\x0059'] = "Y";
                map['\x005A'] = "Z";
                map['\x0061'] = "a";
                map['\x0062'] = "b";
                map['\x0063'] = "c";
                map['\x0064'] = "d";
                map['\x0065'] = "e";
                map['\x0066'] = "f";
                map['\x0067'] = "g";
                map['\x0068'] = "h";
                map['\x0069'] = "i";
                map['\x006A'] = "j";
                map['\x006B'] = "k";
                map['\x006C'] = "l";
                map['\x006D'] = "m";
                map['\x006E'] = "n";
                map['\x006F'] = "o";
                map['\x0070'] = "p";
                map['\x0071'] = "q";
                map['\x0072'] = "r";
                map['\x0073'] = "s";
                map['\x0074'] = "t";
                map['\x0075'] = "u";
                map['\x0076'] = "v";
                map['\x0077'] = "w";
                map['\x0078'] = "x";
                map['\x0079'] = "y";
                map['\x007A'] = "z";
                map['\x00AA'] = "a";	// FEMININE ORDINAL INDICATOR
                map['\x00BA'] = "o";	// MASCULINE ORDINAL INDICATOR
                map['\x00C0'] = "A";	// LATIN CAPITAL LETTER A WITH GRAVE
                map['\x00C1'] = "A";	// LATIN CAPITAL LETTER A WITH ACUTE
                map['\x00C2'] = "A";	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
                map['\x00C3'] = "A";	// LATIN CAPITAL LETTER A WITH TILDE
                map['\x00C4'] = "A";	// LATIN CAPITAL LETTER A WITH DIAERESIS
                map['\x00C5'] = "A";	// LATIN CAPITAL LETTER A WITH RING ABOVE
                map['\x00C6'] = "AE";	// LATIN CAPITAL LETTER AE -- no decomposition
                map['\x00C7'] = "C";	// LATIN CAPITAL LETTER C WITH CEDILLA
                map['\x00C8'] = "E";	// LATIN CAPITAL LETTER E WITH GRAVE
                map['\x00C9'] = "E";	// LATIN CAPITAL LETTER E WITH ACUTE
                map['\x00CA'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX
                map['\x00CB'] = "E";	// LATIN CAPITAL LETTER E WITH DIAERESIS
                map['\x00CC'] = "I";	// LATIN CAPITAL LETTER I WITH GRAVE
                map['\x00CD'] = "I";	// LATIN CAPITAL LETTER I WITH ACUTE
                map['\x00CE'] = "I";	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
                map['\x00CF'] = "I";	// LATIN CAPITAL LETTER I WITH DIAERESIS
                map['\x00D0'] = "D";	// LATIN CAPITAL LETTER ETH -- no decomposition  	// Eth [D for Vietnamese]
                map['\x00D1'] = "N";	// LATIN CAPITAL LETTER N WITH TILDE
                map['\x00D2'] = "O";	// LATIN CAPITAL LETTER O WITH GRAVE
                map['\x00D3'] = "O";	// LATIN CAPITAL LETTER O WITH ACUTE
                map['\x00D4'] = "O";	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
                map['\x00D5'] = "O";	// LATIN CAPITAL LETTER O WITH TILDE
                map['\x00D6'] = "O";	// LATIN CAPITAL LETTER O WITH DIAERESIS
                map['\x00D8'] = "O";	// LATIN CAPITAL LETTER O WITH STROKE -- no decom
                map['\x00D9'] = "U";	// LATIN CAPITAL LETTER U WITH GRAVE
                map['\x00DA'] = "U";	// LATIN CAPITAL LETTER U WITH ACUTE
                map['\x00DB'] = "U";	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX
                map['\x00DC'] = "U";	// LATIN CAPITAL LETTER U WITH DIAERESIS
                map['\x00DD'] = "Y";	// LATIN CAPITAL LETTER Y WITH ACUTE
                map['\x00DE'] = "Th";	// LATIN CAPITAL LETTER THORN -- no decomposition; // Thorn - Could be nothing other than thorn
                map['\x00DF'] = "s";	// LATIN SMALL LETTER SHARP S -- no decomposition
                map['\x00E0'] = "a";	// LATIN SMALL LETTER A WITH GRAVE
                map['\x00E1'] = "a";	// LATIN SMALL LETTER A WITH ACUTE
                map['\x00E2'] = "a";	// LATIN SMALL LETTER A WITH CIRCUMFLEX
                map['\x00E3'] = "a";	// LATIN SMALL LETTER A WITH TILDE
                map['\x00E4'] = "a";	// LATIN SMALL LETTER A WITH DIAERESIS
                map['\x00E5'] = "a";	// LATIN SMALL LETTER A WITH RING ABOVE
                map['\x00E6'] = "ae";	// LATIN SMALL LETTER AE -- no decomposition     ;
                map['\x00E7'] = "c";	// LATIN SMALL LETTER C WITH CEDILLA
                map['\x00E8'] = "e";	// LATIN SMALL LETTER E WITH GRAVE
                map['\x00E9'] = "e";	// LATIN SMALL LETTER E WITH ACUTE
                map['\x00EA'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX
                map['\x00EB'] = "e";	// LATIN SMALL LETTER E WITH DIAERESIS
                map['\x00EC'] = "i";	// LATIN SMALL LETTER I WITH GRAVE
                map['\x00ED'] = "i";	// LATIN SMALL LETTER I WITH ACUTE
                map['\x00EE'] = "i";	// LATIN SMALL LETTER I WITH CIRCUMFLEX
                map['\x00EF'] = "i";	// LATIN SMALL LETTER I WITH DIAERESIS
                map['\x00F0'] = "d";	// LATIN SMALL LETTER ETH -- no decomposition         // small eth, "d" for benefit of Vietnamese
                map['\x00F1'] = "n";	// LATIN SMALL LETTER N WITH TILDE
                map['\x00F2'] = "o";	// LATIN SMALL LETTER O WITH GRAVE
                map['\x00F3'] = "o";	// LATIN SMALL LETTER O WITH ACUTE
                map['\x00F4'] = "o";	// LATIN SMALL LETTER O WITH CIRCUMFLEX
                map['\x00F5'] = "o";	// LATIN SMALL LETTER O WITH TILDE
                map['\x00F6'] = "o";	// LATIN SMALL LETTER O WITH DIAERESIS
                map['\x00F8'] = "o";	// LATIN SMALL LETTER O WITH STROKE -- no decompo
                map['\x00F9'] = "u";	// LATIN SMALL LETTER U WITH GRAVE
                map['\x00FA'] = "u";	// LATIN SMALL LETTER U WITH ACUTE
                map['\x00FB'] = "u";	// LATIN SMALL LETTER U WITH CIRCUMFLEX
                map['\x00FC'] = "u";	// LATIN SMALL LETTER U WITH DIAERESIS
                map['\x00FD'] = "y";	// LATIN SMALL LETTER Y WITH ACUTE
                map['\x00FE'] = "th";	// LATIN SMALL LETTER THORN -- no decomposition  ;   // Small thorn
                map['\x00FF'] = "y";	// LATIN SMALL LETTER Y WITH DIAERESIS
                map['\x0100'] = "A";	// LATIN CAPITAL LETTER A WITH MACRON
                map['\x0101'] = "a";	// LATIN SMALL LETTER A WITH MACRON
                map['\x0102'] = "A";	// LATIN CAPITAL LETTER A WITH BREVE
                map['\x0103'] = "a";	// LATIN SMALL LETTER A WITH BREVE
                map['\x0104'] = "A";	// LATIN CAPITAL LETTER A WITH OGONEK
                map['\x0105'] = "a";	// LATIN SMALL LETTER A WITH OGONEK
                map['\x0106'] = "C";	// LATIN CAPITAL LETTER C WITH ACUTE
                map['\x0107'] = "c";	// LATIN SMALL LETTER C WITH ACUTE
                map['\x0108'] = "C";	// LATIN CAPITAL LETTER C WITH CIRCUMFLEX
                map['\x0109'] = "c";	// LATIN SMALL LETTER C WITH CIRCUMFLEX
                map['\x010A'] = "C";	// LATIN CAPITAL LETTER C WITH DOT ABOVE
                map['\x010B'] = "c";	// LATIN SMALL LETTER C WITH DOT ABOVE
                map['\x010C'] = "C";	// LATIN CAPITAL LETTER C WITH CARON
                map['\x010D'] = "c";	// LATIN SMALL LETTER C WITH CARON
                map['\x010E'] = "D";	// LATIN CAPITAL LETTER D WITH CARON
                map['\x010F'] = "d";	// LATIN SMALL LETTER D WITH CARON
                map['\x0110'] = "D";	// LATIN CAPITAL LETTER D WITH STROKE -- no decomposition                     // Capital D with stroke
                map['\x0111'] = "d";	// LATIN SMALL LETTER D WITH STROKE -- no decomposition                       // small D with stroke
                map['\x0112'] = "E";	// LATIN CAPITAL LETTER E WITH MACRON
                map['\x0113'] = "e";	// LATIN SMALL LETTER E WITH MACRON
                map['\x0114'] = "E";	// LATIN CAPITAL LETTER E WITH BREVE
                map['\x0115'] = "e";	// LATIN SMALL LETTER E WITH BREVE
                map['\x0116'] = "E";	// LATIN CAPITAL LETTER E WITH DOT ABOVE
                map['\x0117'] = "e";	// LATIN SMALL LETTER E WITH DOT ABOVE
                map['\x0118'] = "E";	// LATIN CAPITAL LETTER E WITH OGONEK
                map['\x0119'] = "e";	// LATIN SMALL LETTER E WITH OGONEK
                map['\x011A'] = "E";	// LATIN CAPITAL LETTER E WITH CARON
                map['\x011B'] = "e";	// LATIN SMALL LETTER E WITH CARON
                map['\x011C'] = "G";	// LATIN CAPITAL LETTER G WITH CIRCUMFLEX
                map['\x011D'] = "g";	// LATIN SMALL LETTER G WITH CIRCUMFLEX
                map['\x011E'] = "G";	// LATIN CAPITAL LETTER G WITH BREVE
                map['\x011F'] = "g";	// LATIN SMALL LETTER G WITH BREVE
                map['\x0120'] = "G";	// LATIN CAPITAL LETTER G WITH DOT ABOVE
                map['\x0121'] = "g";	// LATIN SMALL LETTER G WITH DOT ABOVE
                map['\x0122'] = "G";	// LATIN CAPITAL LETTER G WITH CEDILLA
                map['\x0123'] = "g";	// LATIN SMALL LETTER G WITH CEDILLA
                map['\x0124'] = "H";	// LATIN CAPITAL LETTER H WITH CIRCUMFLEX
                map['\x0125'] = "h";	// LATIN SMALL LETTER H WITH CIRCUMFLEX
                map['\x0126'] = "H";	// LATIN CAPITAL LETTER H WITH STROKE -- no decomposition
                map['\x0127'] = "h";	// LATIN SMALL LETTER H WITH STROKE -- no decomposition
                map['\x0128'] = "I";	// LATIN CAPITAL LETTER I WITH TILDE
                map['\x0129'] = "i";	// LATIN SMALL LETTER I WITH TILDE
                map['\x012A'] = "I";	// LATIN CAPITAL LETTER I WITH MACRON
                map['\x012B'] = "i";	// LATIN SMALL LETTER I WITH MACRON
                map['\x012C'] = "I";	// LATIN CAPITAL LETTER I WITH BREVE
                map['\x012D'] = "i";	// LATIN SMALL LETTER I WITH BREVE
                map['\x012E'] = "I";	// LATIN CAPITAL LETTER I WITH OGONEK
                map['\x012F'] = "i";	// LATIN SMALL LETTER I WITH OGONEK
                map['\x0130'] = "I";	// LATIN CAPITAL LETTER I WITH DOT ABOVE
                map['\x0131'] = "i";	// LATIN SMALL LETTER DOTLESS I -- no decomposition
                map['\x0132'] = "I";	// LATIN CAPITAL LIGATURE IJ    
                map['\x0133'] = "i";	// LATIN SMALL LIGATURE IJ      
                map['\x0134'] = "J";	// LATIN CAPITAL LETTER J WITH CIRCUMFLEX
                map['\x0135'] = "j";	// LATIN SMALL LETTER J WITH CIRCUMFLEX
                map['\x0136'] = "K";	// LATIN CAPITAL LETTER K WITH CEDILLA
                map['\x0137'] = "k";	// LATIN SMALL LETTER K WITH CEDILLA
                map['\x0138'] = "k";	// LATIN SMALL LETTER KRA -- no decomposition
                map['\x0139'] = "L";	// LATIN CAPITAL LETTER L WITH ACUTE
                map['\x013A'] = "l";	// LATIN SMALL LETTER L WITH ACUTE
                map['\x013B'] = "L";	// LATIN CAPITAL LETTER L WITH CEDILLA
                map['\x013C'] = "l";	// LATIN SMALL LETTER L WITH CEDILLA
                map['\x013D'] = "L";	// LATIN CAPITAL LETTER L WITH CARON
                map['\x013E'] = "l";	// LATIN SMALL LETTER L WITH CARON
                map['\x013F'] = "L";	// LATIN CAPITAL LETTER L WITH MIDDLE DOT
                map['\x0140'] = "l";	// LATIN SMALL LETTER L WITH MIDDLE DOT
                map['\x0141'] = "L";	// LATIN CAPITAL LETTER L WITH STROKE -- no decomposition
                map['\x0142'] = "l";	// LATIN SMALL LETTER L WITH STROKE -- no decomposition
                map['\x0143'] = "N";	// LATIN CAPITAL LETTER N WITH ACUTE
                map['\x0144'] = "n";	// LATIN SMALL LETTER N WITH ACUTE
                map['\x0145'] = "N";	// LATIN CAPITAL LETTER N WITH CEDILLA
                map['\x0146'] = "n";	// LATIN SMALL LETTER N WITH CEDILLA
                map['\x0147'] = "N";	// LATIN CAPITAL LETTER N WITH CARON
                map['\x0148'] = "n";	// LATIN SMALL LETTER N WITH CARON
                map['\x0149'] = "'n";	// LATIN SMALL LETTER N PRECEDED BY APOSTROPHE
                map['\x014A'] = "NG";	// LATIN CAPITAL LETTER ENG -- no decomposition
                map['\x014B'] = "ng";	// LATIN SMALL LETTER ENG -- no decomposition
                map['\x014C'] = "O";	// LATIN CAPITAL LETTER O WITH MACRON
                map['\x014D'] = "o";	// LATIN SMALL LETTER O WITH MACRON
                map['\x014E'] = "O";	// LATIN CAPITAL LETTER O WITH BREVE
                map['\x014F'] = "o";	// LATIN SMALL LETTER O WITH BREVE
                map['\x0150'] = "O";	// LATIN CAPITAL LETTER O WITH DOUBLE ACUTE
                map['\x0151'] = "o";	// LATIN SMALL LETTER O WITH DOUBLE ACUTE
                map['\x0152'] = "OE";	// LATIN CAPITAL LIGATURE OE -- no decomposition
                map['\x0153'] = "oe";	// LATIN SMALL LIGATURE OE -- no decomposition
                map['\x0154'] = "R";	// LATIN CAPITAL LETTER R WITH ACUTE
                map['\x0155'] = "r";	// LATIN SMALL LETTER R WITH ACUTE
                map['\x0156'] = "R";	// LATIN CAPITAL LETTER R WITH CEDILLA
                map['\x0157'] = "r";	// LATIN SMALL LETTER R WITH CEDILLA
                map['\x0158'] = "R";	// LATIN CAPITAL LETTER R WITH CARON
                map['\x0159'] = "r";	// LATIN SMALL LETTER R WITH CARON
                map['\x015A'] = "S";	// LATIN CAPITAL LETTER S WITH ACUTE
                map['\x015B'] = "s";	// LATIN SMALL LETTER S WITH ACUTE
                map['\x015C'] = "S";	// LATIN CAPITAL LETTER S WITH CIRCUMFLEX
                map['\x015D'] = "s";	// LATIN SMALL LETTER S WITH CIRCUMFLEX
                map['\x015E'] = "S";	// LATIN CAPITAL LETTER S WITH CEDILLA
                map['\x015F'] = "s";	// LATIN SMALL LETTER S WITH CEDILLA
                map['\x0160'] = "S";	// LATIN CAPITAL LETTER S WITH CARON
                map['\x0161'] = "s";	// LATIN SMALL LETTER S WITH CARON
                map['\x0162'] = "T";	// LATIN CAPITAL LETTER T WITH CEDILLA
                map['\x0163'] = "t";	// LATIN SMALL LETTER T WITH CEDILLA
                map['\x0164'] = "T";	// LATIN CAPITAL LETTER T WITH CARON
                map['\x0165'] = "t";	// LATIN SMALL LETTER T WITH CARON
                map['\x0166'] = "T";	// LATIN CAPITAL LETTER T WITH STROKE -- no decomposition
                map['\x0167'] = "t";	// LATIN SMALL LETTER T WITH STROKE -- no decomposition
                map['\x0168'] = "U";	// LATIN CAPITAL LETTER U WITH TILDE
                map['\x0169'] = "u";	// LATIN SMALL LETTER U WITH TILDE
                map['\x016A'] = "U";	// LATIN CAPITAL LETTER U WITH MACRON
                map['\x016B'] = "u";	// LATIN SMALL LETTER U WITH MACRON
                map['\x016C'] = "U";	// LATIN CAPITAL LETTER U WITH BREVE
                map['\x016D'] = "u";	// LATIN SMALL LETTER U WITH BREVE
                map['\x016E'] = "U";	// LATIN CAPITAL LETTER U WITH RING ABOVE
                map['\x016F'] = "u";	// LATIN SMALL LETTER U WITH RING ABOVE
                map['\x0170'] = "U";	// LATIN CAPITAL LETTER U WITH DOUBLE ACUTE
                map['\x0171'] = "u";	// LATIN SMALL LETTER U WITH DOUBLE ACUTE
                map['\x0172'] = "U";	// LATIN CAPITAL LETTER U WITH OGONEK
                map['\x0173'] = "u";	// LATIN SMALL LETTER U WITH OGONEK
                map['\x0174'] = "W";	// LATIN CAPITAL LETTER W WITH CIRCUMFLEX
                map['\x0175'] = "w";	// LATIN SMALL LETTER W WITH CIRCUMFLEX
                map['\x0176'] = "Y";	// LATIN CAPITAL LETTER Y WITH CIRCUMFLEX
                map['\x0177'] = "y";	// LATIN SMALL LETTER Y WITH CIRCUMFLEX
                map['\x0178'] = "Y";	// LATIN CAPITAL LETTER Y WITH DIAERESIS
                map['\x0179'] = "Z";	// LATIN CAPITAL LETTER Z WITH ACUTE
                map['\x017A'] = "z";	// LATIN SMALL LETTER Z WITH ACUTE
                map['\x017B'] = "Z";	// LATIN CAPITAL LETTER Z WITH DOT ABOVE
                map['\x017C'] = "z";	// LATIN SMALL LETTER Z WITH DOT ABOVE
                map['\x017D'] = "Z";	// LATIN CAPITAL LETTER Z WITH CARON
                map['\x017E'] = "z";	// LATIN SMALL LETTER Z WITH CARON
                map['\x017F'] = "s";	// LATIN SMALL LETTER LONG S    
                map['\x0180'] = "b";	// LATIN SMALL LETTER B WITH STROKE -- no decomposition
                map['\x0181'] = "B";	// LATIN CAPITAL LETTER B WITH HOOK -- no decomposition
                map['\x0182'] = "B";	// LATIN CAPITAL LETTER B WITH TOPBAR -- no decomposition
                map['\x0183'] = "b";	// LATIN SMALL LETTER B WITH TOPBAR -- no decomposition
                map['\x0184'] = "6";	// LATIN CAPITAL LETTER TONE SIX -- no decomposition
                map['\x0185'] = "6";	// LATIN SMALL LETTER TONE SIX -- no decomposition
                map['\x0186'] = "O";	// LATIN CAPITAL LETTER OPEN O -- no decomposition
                map['\x0187'] = "C";	// LATIN CAPITAL LETTER C WITH HOOK -- no decomposition
                map['\x0188'] = "c";	// LATIN SMALL LETTER C WITH HOOK -- no decomposition
                map['\x0189'] = "D";	// LATIN CAPITAL LETTER AFRICAN D -- no decomposition
                map['\x018A'] = "D";	// LATIN CAPITAL LETTER D WITH HOOK -- no decomposition
                map['\x018B'] = "D";	// LATIN CAPITAL LETTER D WITH TOPBAR -- no decomposition
                map['\x018C'] = "d";	// LATIN SMALL LETTER D WITH TOPBAR -- no decomposition
                map['\x018D'] = "d";	// LATIN SMALL LETTER TURNED DELTA -- no decomposition
                map['\x018E'] = "E";	// LATIN CAPITAL LETTER REVERSED E -- no decomposition
                map['\x018F'] = "E";	// LATIN CAPITAL LETTER SCHWA -- no decomposition
                map['\x0190'] = "E";	// LATIN CAPITAL LETTER OPEN E -- no decomposition
                map['\x0191'] = "F";	// LATIN CAPITAL LETTER F WITH HOOK -- no decomposition
                map['\x0192'] = "f";	// LATIN SMALL LETTER F WITH HOOK -- no decomposition
                map['\x0193'] = "G";	// LATIN CAPITAL LETTER G WITH HOOK -- no decomposition
                map['\x0194'] = "G";	// LATIN CAPITAL LETTER GAMMA -- no decomposition
                map['\x0195'] = "hv";	// LATIN SMALL LETTER HV -- no decomposition
                map['\x0196'] = "I";	// LATIN CAPITAL LETTER IOTA -- no decomposition
                map['\x0197'] = "I";	// LATIN CAPITAL LETTER I WITH STROKE -- no decomposition
                map['\x0198'] = "K";	// LATIN CAPITAL LETTER K WITH HOOK -- no decomposition
                map['\x0199'] = "k";	// LATIN SMALL LETTER K WITH HOOK -- no decomposition
                map['\x019A'] = "l";	// LATIN SMALL LETTER L WITH BAR -- no decomposition
                map['\x019B'] = "l";	// LATIN SMALL LETTER LAMBDA WITH STROKE -- no decomposition
                map['\x019C'] = "M";	// LATIN CAPITAL LETTER TURNED M -- no decomposition
                map['\x019D'] = "N";	// LATIN CAPITAL LETTER N WITH LEFT HOOK -- no decomposition
                map['\x019E'] = "n";	// LATIN SMALL LETTER N WITH LONG RIGHT LEG -- no decomposition
                map['\x019F'] = "O";	// LATIN CAPITAL LETTER O WITH MIDDLE TILDE -- no decomposition
                map['\x01A0'] = "O";	// LATIN CAPITAL LETTER O WITH HORN
                map['\x01A1'] = "o";	// LATIN SMALL LETTER O WITH HORN
                map['\x01A2'] = "OI";	// LATIN CAPITAL LETTER OI -- no decomposition
                map['\x01A3'] = "oi";	// LATIN SMALL LETTER OI -- no decomposition
                map['\x01A4'] = "P";	// LATIN CAPITAL LETTER P WITH HOOK -- no decomposition
                map['\x01A5'] = "p";	// LATIN SMALL LETTER P WITH HOOK -- no decomposition
                map['\x01A6'] = "YR";	// LATIN LETTER YR -- no decomposition
                map['\x01A7'] = "2";	// LATIN CAPITAL LETTER TONE TWO -- no decomposition
                map['\x01A8'] = "2";	// LATIN SMALL LETTER TONE TWO -- no decomposition
                map['\x01A9'] = "S";	// LATIN CAPITAL LETTER ESH -- no decomposition
                map['\x01AA'] = "s";	// LATIN LETTER REVERSED ESH LOOP -- no decomposition
                map['\x01AB'] = "t";	// LATIN SMALL LETTER T WITH PALATAL HOOK -- no decomposition
                map['\x01AC'] = "T";	// LATIN CAPITAL LETTER T WITH HOOK -- no decomposition
                map['\x01AD'] = "t";	// LATIN SMALL LETTER T WITH HOOK -- no decomposition
                map['\x01AE'] = "T";	// LATIN CAPITAL LETTER T WITH RETROFLEX HOOK -- no decomposition
                map['\x01AF'] = "U";	// LATIN CAPITAL LETTER U WITH HORN
                map['\x01B0'] = "u";	// LATIN SMALL LETTER U WITH HORN
                map['\x01B1'] = "u";	// LATIN CAPITAL LETTER UPSILON -- no decomposition
                map['\x01B2'] = "V";	// LATIN CAPITAL LETTER V WITH HOOK -- no decomposition
                map['\x01B3'] = "Y";	// LATIN CAPITAL LETTER Y WITH HOOK -- no decomposition
                map['\x01B4'] = "y";	// LATIN SMALL LETTER Y WITH HOOK -- no decomposition
                map['\x01B5'] = "Z";	// LATIN CAPITAL LETTER Z WITH STROKE -- no decomposition
                map['\x01B6'] = "z";	// LATIN SMALL LETTER Z WITH STROKE -- no decomposition
                map['\x01B7'] = "Z";	// LATIN CAPITAL LETTER EZH -- no decomposition
                map['\x01B8'] = "Z";	// LATIN CAPITAL LETTER EZH REVERSED -- no decomposition
                map['\x01B9'] = "Z";	// LATIN SMALL LETTER EZH REVERSED -- no decomposition
                map['\x01BA'] = "z";	// LATIN SMALL LETTER EZH WITH TAIL -- no decomposition
                map['\x01BB'] = "2";	// LATIN LETTER TWO WITH STROKE -- no decomposition
                map['\x01BC'] = "5";	// LATIN CAPITAL LETTER TONE FIVE -- no decomposition
                map['\x01BD'] = "5";	// LATIN SMALL LETTER TONE FIVE -- no decomposition
                map['\x01BE'] = "´";	// LATIN LETTER INVERTED GLOTTAL STOP WITH STROKE -- no decomposition
                map['\x01BF'] = "w";	// LATIN LETTER WYNN -- no decomposition
                map['\x01C0'] = "!";	// LTIN LETTER DENTAL CLICK -- no decomposition
                map['\x01C1'] = "!";	// LTIN LETTER LATERAL CLICK -- no decomposition
                map['\x01C2'] = "!";	// LTIN LETTER ALVEOLAR CLICK -- no decomposition
                map['\x01C3'] = "!";	// LTIN LETTER RETROFLEX CLICK -- no decomposition
                map['\x01C4'] = "DZ";	// LATIN CAPITAL LETTER DZ WITH CARON
                map['\x01C5'] = "DZ";	// LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON
                map['\x01C6'] = "d";	// LATIN SMALL LETTER DZ WITH CARON
                map['\x01C7'] = "Lj";	// LATIN CAPITAL LETTER LJ
                map['\x01C8'] = "Lj";	// LATIN CAPITAL LETTER L WITH SMALL LETTER J
                map['\x01C9'] = "lj";	// LATIN SMALL LETTER LJ
                map['\x01CA'] = "NJ";	// LATIN CAPITAL LETTER NJ
                map['\x01CB'] = "NJ";	// LATIN CAPITAL LETTER N WITH SMALL LETTER J
                map['\x01CC'] = "nj";	// LATIN SMALL LETTER NJ
                map['\x01CD'] = "A";	// LATIN CAPITAL LETTER A WITH CARON
                map['\x01CE'] = "a";	// LATIN SMALL LETTER A WITH CARON
                map['\x01CF'] = "I";	// LATIN CAPITAL LETTER I WITH CARON
                map['\x01D0'] = "i";	// LATIN SMALL LETTER I WITH CARON
                map['\x01D1'] = "O";	// LATIN CAPITAL LETTER O WITH CARON
                map['\x01D2'] = "o";	// LATIN SMALL LETTER O WITH CARON
                map['\x01D3'] = "U";	// LATIN CAPITAL LETTER U WITH CARON
                map['\x01D4'] = "u";	// LATIN SMALL LETTER U WITH CARON
                map['\x01D5'] = "U";	// LATIN CAPITAL LETTER U WITH DIAERESIS AND MACRON
                map['\x01D6'] = "u";	// LATIN SMALL LETTER U WITH DIAERESIS AND MACRON
                map['\x01D7'] = "U";	// LATIN CAPITAL LETTER U WITH DIAERESIS AND ACUTE
                map['\x01D8'] = "u";	// LATIN SMALL LETTER U WITH DIAERESIS AND ACUTE
                map['\x01D9'] = "U";	// LATIN CAPITAL LETTER U WITH DIAERESIS AND CARON
                map['\x01DA'] = "u";	// LATIN SMALL LETTER U WITH DIAERESIS AND CARON
                map['\x01DB'] = "U";	// LATIN CAPITAL LETTER U WITH DIAERESIS AND GRAVE
                map['\x01DC'] = "u";	// LATIN SMALL LETTER U WITH DIAERESIS AND GRAVE
                map['\x01DD'] = "e";	// LATIN SMALL LETTER TURNED E -- no decomposition
                map['\x01DE'] = "A";	// LATIN CAPITAL LETTER A WITH DIAERESIS AND MACRON
                map['\x01DF'] = "a";	// LATIN SMALL LETTER A WITH DIAERESIS AND MACRON
                map['\x01E0'] = "A";	// LATIN CAPITAL LETTER A WITH DOT ABOVE AND MACRON
                map['\x01E1'] = "a";	// LATIN SMALL LETTER A WITH DOT ABOVE AND MACRON
                map['\x01E2'] = "AE";	// LATIN CAPITAL LETTER AE WITH MACRON
                map['\x01E3'] = "ae";	// LATIN SMALL LETTER AE WITH MACRON
                map['\x01E4'] = "G";	// LATIN CAPITAL LETTER G WITH STROKE -- no decomposition
                map['\x01E5'] = "g";	// LATIN SMALL LETTER G WITH STROKE -- no decomposition
                map['\x01E6'] = "G";	// LATIN CAPITAL LETTER G WITH CARON
                map['\x01E7'] = "g";	// LATIN SMALL LETTER G WITH CARON
                map['\x01E8'] = "K";	// LATIN CAPITAL LETTER K WITH CARON
                map['\x01E9'] = "k";	// LATIN SMALL LETTER K WITH CARON
                map['\x01EA'] = "O";	// LATIN CAPITAL LETTER O WITH OGONEK
                map['\x01EB'] = "o";	// LATIN SMALL LETTER O WITH OGONEK
                map['\x01EC'] = "O";	// LATIN CAPITAL LETTER O WITH OGONEK AND MACRON
                map['\x01ED'] = "o";	// LATIN SMALL LETTER O WITH OGONEK AND MACRON
                map['\x01EE'] = "Z";	// LATIN CAPITAL LETTER EZH WITH CARON
                map['\x01EF'] = "Z";	// LATIN SMALL LETTER EZH WITH CARON
                map['\x01F0'] = "j";	// LATIN SMALL LETTER J WITH CARON
                map['\x01F1'] = "DZ";	// LATIN CAPITAL LETTER DZ
                map['\x01F2'] = "DZ";	// LATIN CAPITAL LETTER D WITH SMALL LETTER Z
                map['\x01F3'] = "dz";	// LATIN SMALL LETTER DZ
                map['\x01F4'] = "G";	// LATIN CAPITAL LETTER G WITH ACUTE
                map['\x01F5'] = "g";	// LATIN SMALL LETTER G WITH ACUTE
                map['\x01F6'] = "hv";	// LATIN CAPITAL LETTER HWAIR -- no decomposition
                map['\x01F7'] = "w";	// LATIN CAPITAL LETTER WYNN -- no decomposition
                map['\x01F8'] = "N";	// LATIN CAPITAL LETTER N WITH GRAVE
                map['\x01F9'] = "n";	// LATIN SMALL LETTER N WITH GRAVE
                map['\x01FA'] = "A";	// LATIN CAPITAL LETTER A WITH RING ABOVE AND ACUTE
                map['\x01FB'] = "a";	// LATIN SMALL LETTER A WITH RING ABOVE AND ACUTE
                map['\x01FC'] = "AE";	// LATIN CAPITAL LETTER AE WITH ACUTE
                map['\x01FD'] = "ae";	// LATIN SMALL LETTER AE WITH ACUTE
                map['\x01FE'] = "O";	// LATIN CAPITAL LETTER O WITH STROKE AND ACUTE
                map['\x01FF'] = "o";	// LATIN SMALL LETTER O WITH STROKE AND ACUTE
                map['\x0200'] = "A";	// LATIN CAPITAL LETTER A WITH DOUBLE GRAVE
                map['\x0201'] = "a";	// LATIN SMALL LETTER A WITH DOUBLE GRAVE
                map['\x0202'] = "A";	// LATIN CAPITAL LETTER A WITH INVERTED BREVE
                map['\x0203'] = "a";	// LATIN SMALL LETTER A WITH INVERTED BREVE
                map['\x0204'] = "E";	// LATIN CAPITAL LETTER E WITH DOUBLE GRAVE
                map['\x0205'] = "e";	// LATIN SMALL LETTER E WITH DOUBLE GRAVE
                map['\x0206'] = "E";	// LATIN CAPITAL LETTER E WITH INVERTED BREVE
                map['\x0207'] = "e";	// LATIN SMALL LETTER E WITH INVERTED BREVE
                map['\x0208'] = "I";	// LATIN CAPITAL LETTER I WITH DOUBLE GRAVE
                map['\x0209'] = "i";	// LATIN SMALL LETTER I WITH DOUBLE GRAVE
                map['\x020A'] = "I";	// LATIN CAPITAL LETTER I WITH INVERTED BREVE
                map['\x020B'] = "i";	// LATIN SMALL LETTER I WITH INVERTED BREVE
                map['\x020C'] = "O";	// LATIN CAPITAL LETTER O WITH DOUBLE GRAVE
                map['\x020D'] = "o";	// LATIN SMALL LETTER O WITH DOUBLE GRAVE
                map['\x020E'] = "O";	// LATIN CAPITAL LETTER O WITH INVERTED BREVE
                map['\x020F'] = "o";	// LATIN SMALL LETTER O WITH INVERTED BREVE
                map['\x0210'] = "R";	// LATIN CAPITAL LETTER R WITH DOUBLE GRAVE
                map['\x0211'] = "r";	// LATIN SMALL LETTER R WITH DOUBLE GRAVE
                map['\x0212'] = "R";	// LATIN CAPITAL LETTER R WITH INVERTED BREVE
                map['\x0213'] = "r";	// LATIN SMALL LETTER R WITH INVERTED BREVE
                map['\x0214'] = "U";	// LATIN CAPITAL LETTER U WITH DOUBLE GRAVE
                map['\x0215'] = "u";	// LATIN SMALL LETTER U WITH DOUBLE GRAVE
                map['\x0216'] = "U";	// LATIN CAPITAL LETTER U WITH INVERTED BREVE
                map['\x0217'] = "u";	// LATIN SMALL LETTER U WITH INVERTED BREVE
                map['\x0218'] = "S";	// LATIN CAPITAL LETTER S WITH COMMA BELOW
                map['\x0219'] = "s";	// LATIN SMALL LETTER S WITH COMMA BELOW
                map['\x021A'] = "T";	// LATIN CAPITAL LETTER T WITH COMMA BELOW
                map['\x021B'] = "t";	// LATIN SMALL LETTER T WITH COMMA BELOW
                map['\x021C'] = "Z";	// LATIN CAPITAL LETTER YOGH -- no decomposition
                map['\x021D'] = "z";	// LATIN SMALL LETTER YOGH -- no decomposition
                map['\x021E'] = "H";	// LATIN CAPITAL LETTER H WITH CARON
                map['\x021F'] = "h";	// LATIN SMALL LETTER H WITH CARON
                map['\x0220'] = "N";	// LATIN CAPITAL LETTER N WITH LONG RIGHT LEG -- no decomposition
                map['\x0221'] = "d";	// LATIN SMALL LETTER D WITH CURL -- no decomposition
                map['\x0222'] = "OU";	// LATIN CAPITAL LETTER OU -- no decomposition
                map['\x0223'] = "ou";	// LATIN SMALL LETTER OU -- no decomposition
                map['\x0224'] = "Z";	// LATIN CAPITAL LETTER Z WITH HOOK -- no decomposition
                map['\x0225'] = "z";	// LATIN SMALL LETTER Z WITH HOOK -- no decomposition
                map['\x0226'] = "A";	// LATIN CAPITAL LETTER A WITH DOT ABOVE
                map['\x0227'] = "a";	// LATIN SMALL LETTER A WITH DOT ABOVE
                map['\x0228'] = "E";	// LATIN CAPITAL LETTER E WITH CEDILLA
                map['\x0229'] = "e";	// LATIN SMALL LETTER E WITH CEDILLA
                map['\x022A'] = "O";	// LATIN CAPITAL LETTER O WITH DIAERESIS AND MACRON
                map['\x022B'] = "o";	// LATIN SMALL LETTER O WITH DIAERESIS AND MACRON
                map['\x022C'] = "O";	// LATIN CAPITAL LETTER O WITH TILDE AND MACRON
                map['\x022D'] = "o";	// LATIN SMALL LETTER O WITH TILDE AND MACRON
                map['\x022E'] = "O";	// LATIN CAPITAL LETTER O WITH DOT ABOVE
                map['\x022F'] = "o";	// LATIN SMALL LETTER O WITH DOT ABOVE
                map['\x0230'] = "O";	// LATIN CAPITAL LETTER O WITH DOT ABOVE AND MACRON
                map['\x0231'] = "o";	// LATIN SMALL LETTER O WITH DOT ABOVE AND MACRON
                map['\x0232'] = "Y";	// LATIN CAPITAL LETTER Y WITH MACRON
                map['\x0233'] = "y";	// LATIN SMALL LETTER Y WITH MACRON
                map['\x0234'] = "l";	// LATIN SMALL LETTER L WITH CURL -- no decomposition
                map['\x0235'] = "n";	// LATIN SMALL LETTER N WITH CURL -- no decomposition
                map['\x0236'] = "t";	// LATIN SMALL LETTER T WITH CURL -- no decomposition
                map['\x0250'] = "a";	// LATIN SMALL LETTER TURNED A -- no decomposition
                map['\x0251'] = "a";	// LATIN SMALL LETTER ALPHA -- no decomposition
                map['\x0252'] = "a";	// LATIN SMALL LETTER TURNED ALPHA -- no decomposition
                map['\x0253'] = "b";	// LATIN SMALL LETTER B WITH HOOK -- no decomposition
                map['\x0254'] = "o";	// LATIN SMALL LETTER OPEN O -- no decomposition
                map['\x0255'] = "c";	// LATIN SMALL LETTER C WITH CURL -- no decomposition
                map['\x0256'] = "d";	// LATIN SMALL LETTER D WITH TAIL -- no decomposition
                map['\x0257'] = "d";	// LATIN SMALL LETTER D WITH HOOK -- no decomposition
                map['\x0258'] = "e";	// LATIN SMALL LETTER REVERSED E -- no decomposition
                map['\x0259'] = "e";	// LATIN SMALL LETTER SCHWA -- no decomposition
                map['\x025A'] = "e";	// LATIN SMALL LETTER SCHWA WITH HOOK -- no decomposition
                map['\x025B'] = "e";	// LATIN SMALL LETTER OPEN E -- no decomposition
                map['\x025C'] = "e";	// LATIN SMALL LETTER REVERSED OPEN E -- no decomposition
                map['\x025D'] = "e";	// LATIN SMALL LETTER REVERSED OPEN E WITH HOOK -- no decomposition
                map['\x025E'] = "e";	// LATIN SMALL LETTER CLOSED REVERSED OPEN E -- no decomposition
                map['\x025F'] = "j";	// LATIN SMALL LETTER DOTLESS J WITH STROKE -- no decomposition
                map['\x0260'] = "g";	// LATIN SMALL LETTER G WITH HOOK -- no decomposition
                map['\x0261'] = "g";	// LATIN SMALL LETTER SCRIPT G -- no decomposition
                map['\x0262'] = "G";	// LATIN LETTER SMALL CAPITAL G -- no decomposition
                map['\x0263'] = "g";	// LATIN SMALL LETTER GAMMA -- no decomposition
                map['\x0264'] = "y";	// LATIN SMALL LETTER RAMS HORN -- no decomposition
                map['\x0265'] = "h";	// LATIN SMALL LETTER TURNED H -- no decomposition
                map['\x0266'] = "h";	// LATIN SMALL LETTER H WITH HOOK -- no decomposition
                map['\x0267'] = "h";	// LATIN SMALL LETTER HENG WITH HOOK -- no decomposition
                map['\x0268'] = "i";	// LATIN SMALL LETTER I WITH STROKE -- no decomposition
                map['\x0269'] = "i";	// LATIN SMALL LETTER IOTA -- no decomposition
                map['\x026A'] = "I";	// LATIN LETTER SMALL CAPITAL I -- no decomposition
                map['\x026B'] = "l";	// LATIN SMALL LETTER L WITH MIDDLE TILDE -- no decomposition
                map['\x026C'] = "l";	// LATIN SMALL LETTER L WITH BELT -- no decomposition
                map['\x026D'] = "l";	// LATIN SMALL LETTER L WITH RETROFLEX HOOK -- no decomposition
                map['\x026E'] = "lz";	// LATIN SMALL LETTER LEZH -- no decomposition
                map['\x026F'] = "m";	// LATIN SMALL LETTER TURNED M -- no decomposition
                map['\x0270'] = "m";	// LATIN SMALL LETTER TURNED M WITH LONG LEG -- no decomposition
                map['\x0271'] = "m";	// LATIN SMALL LETTER M WITH HOOK -- no decomposition
                map['\x0272'] = "n";	// LATIN SMALL LETTER N WITH LEFT HOOK -- no decomposition
                map['\x0273'] = "n";	// LATIN SMALL LETTER N WITH RETROFLEX HOOK -- no decomposition
                map['\x0274'] = "N";	// LATIN LETTER SMALL CAPITAL N -- no decomposition
                map['\x0275'] = "o";	// LATIN SMALL LETTER BARRED O -- no decomposition
                map['\x0276'] = "OE";	// LATIN LETTER SMALL CAPITAL OE -- no decomposition
                map['\x0277'] = "o";	// LATIN SMALL LETTER CLOSED OMEGA -- no decomposition
                map['\x0278'] = "ph";	// LATIN SMALL LETTER PHI -- no decomposition
                map['\x0279'] = "r";	// LATIN SMALL LETTER TURNED R -- no decomposition
                map['\x027A'] = "r";	// LATIN SMALL LETTER TURNED R WITH LONG LEG -- no decomposition
                map['\x027B'] = "r";	// LATIN SMALL LETTER TURNED R WITH HOOK -- no decomposition
                map['\x027C'] = "r";	// LATIN SMALL LETTER R WITH LONG LEG -- no decomposition
                map['\x027D'] = "r";	// LATIN SMALL LETTER R WITH TAIL -- no decomposition
                map['\x027E'] = "r";	// LATIN SMALL LETTER R WITH FISHHOOK -- no decomposition
                map['\x027F'] = "r";	// LATIN SMALL LETTER REVERSED R WITH FISHHOOK -- no decomposition
                map['\x0280'] = "R";	// LATIN LETTER SMALL CAPITAL R -- no decomposition
                map['\x0281'] = "r";	// LATIN LETTER SMALL CAPITAL INVERTED R -- no decomposition
                map['\x0282'] = "s";	// LATIN SMALL LETTER S WITH HOOK -- no decomposition
                map['\x0283'] = "s";	// LATIN SMALL LETTER ESH -- no decomposition
                map['\x0284'] = "j";	// LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK -- no decomposition
                map['\x0285'] = "s";	// LATIN SMALL LETTER SQUAT REVERSED ESH -- no decomposition
                map['\x0286'] = "s";	// LATIN SMALL LETTER ESH WITH CURL -- no decomposition
                map['\x0287'] = "y";	// LATIN SMALL LETTER TURNED T -- no decomposition
                map['\x0288'] = "t";	// LATIN SMALL LETTER T WITH RETROFLEX HOOK -- no decomposition
                map['\x0289'] = "u";	// LATIN SMALL LETTER U BAR -- no decomposition
                map['\x028A'] = "u";	// LATIN SMALL LETTER UPSILON -- no decomposition
                map['\x028B'] = "u";	// LATIN SMALL LETTER V WITH HOOK -- no decomposition
                map['\x028C'] = "v";	// LATIN SMALL LETTER TURNED V -- no decomposition
                map['\x028D'] = "w";	// LATIN SMALL LETTER TURNED W -- no decomposition
                map['\x028E'] = "y";	// LATIN SMALL LETTER TURNED Y -- no decomposition
                map['\x028F'] = "Y";	// LATIN LETTER SMALL CAPITAL Y -- no decomposition
                map['\x0290'] = "z";	// LATIN SMALL LETTER Z WITH RETROFLEX HOOK -- no decomposition
                map['\x0291'] = "z";	// LATIN SMALL LETTER Z WITH CURL -- no decomposition
                map['\x0292'] = "z";	// LATIN SMALL LETTER EZH -- no decomposition
                map['\x0293'] = "z";	// LATIN SMALL LETTER EZH WITH CURL -- no decomposition
                map['\x0294'] = "'";	// LTIN LETTER GLOTTAL STOP -- no decomposition
                map['\x0295'] = "'";	// LTIN LETTER PHARYNGEAL VOICED FRICATIVE -- no decomposition
                map['\x0296'] = "'";	// LTIN LETTER INVERTED GLOTTAL STOP -- no decomposition
                map['\x0297'] = "C";	// LATIN LETTER STRETCHED C -- no decomposition
                map['\x0298'] = "O˜";	// LATIN LETTER BILABIAL CLICK -- no decomposition
                map['\x0299'] = "B";	// LATIN LETTER SMALL CAPITAL B -- no decomposition
                map['\x029A'] = "e";	// LATIN SMALL LETTER CLOSED OPEN E -- no decomposition
                map['\x029B'] = "G";	// LATIN LETTER SMALL CAPITAL G WITH HOOK -- no decomposition
                map['\x029C'] = "H";	// LATIN LETTER SMALL CAPITAL H -- no decomposition
                map['\x029D'] = "j";	// LATIN SMALL LETTER J WITH CROSSED-TAIL -- no decomposition
                map['\x029E'] = "k";	// LATIN SMALL LETTER TURNED K -- no decomposition
                map['\x029F'] = "L";	// LATIN LETTER SMALL CAPITAL L -- no decomposition
                map['\x02A0'] = "q";	// LATIN SMALL LETTER Q WITH HOOK -- no decomposition
                map['\x02A1'] = "'";	// LTIN LETTER GLOTTAL STOP WITH STROKE -- no decomposition
                map['\x02A2'] = "'";	// LTIN LETTER REVERSED GLOTTAL STOP WITH STROKE -- no decomposition
                map['\x02A3'] = "dz";	// LATIN SMALL LETTER DZ DIGRAPH -- no decomposition
                map['\x02A4'] = "dz";	// LATIN SMALL LETTER DEZH DIGRAPH -- no decomposition
                map['\x02A5'] = "dz";	// LATIN SMALL LETTER DZ DIGRAPH WITH CURL -- no decomposition
                map['\x02A6'] = "ts";	// LATIN SMALL LETTER TS DIGRAPH -- no decomposition
                map['\x02A7'] = "ts";	// LATIN SMALL LETTER TESH DIGRAPH -- no decomposition
                map['\x02A8'] = "";   // LTIN SMALL LETTER TC DIGRAPH WITH CURL -- no decomposition
                map['\x02A9'] = "fn";	// LATIN SMALL LETTER FENG DIGRAPH -- no decomposition
                map['\x02AA'] = "ls";	// LATIN SMALL LETTER LS DIGRAPH -- no decomposition
                map['\x02AB'] = "lz";	// LATIN SMALL LETTER LZ DIGRAPH -- no decomposition
                map['\x02AC'] = "w";	// LATIN LETTER BILABIAL PERCUSSIVE -- no decomposition
                map['\x02AD'] = "t";	// LATIN LETTER BIDENTAL PERCUSSIVE -- no decomposition
                map['\x02AE'] = "h";	// LATIN SMALL LETTER TURNED H WITH FISHHOOK -- no decomposition
                map['\x02AF'] = "h";	// LATIN SMALL LETTER TURNED H WITH FISHHOOK AND TAIL -- no decomposition
                map['\x02B0'] = "h";	// MODIFIER LETTER SMALL H
                map['\x02B1'] = "h";	// MODIFIER LETTER SMALL H WITH HOOK
                map['\x02B2'] = "j";	// MODIFIER LETTER SMALL J
                map['\x02B3'] = "r";	// MODIFIER LETTER SMALL R
                map['\x02B4'] = "r";	// MODIFIER LETTER SMALL TURNED R
                map['\x02B5'] = "r";	// MODIFIER LETTER SMALL TURNED R WITH HOOK
                map['\x02B6'] = "R";	// MODIFIER LETTER SMALL CAPITAL INVERTED R
                map['\x02B7'] = "w";	// MODIFIER LETTER SMALL W
                map['\x02B8'] = "y";	// MODIFIER LETTER SMALL Y
                map['\x02E1'] = "l";	// MODIFIER LETTER SMALL L
                map['\x02E2'] = "s";	// MODIFIER LETTER SMALL S
                map['\x02E3'] = "x";	// MODIFIER LETTER SMALL X
                map['\x02E4'] = "'";	// MDIFIER LETTER SMALL REVERSED GLOTTAL STOP
                map['\x1D00'] = "A";	// LATIN LETTER SMALL CAPITAL A -- no decomposition
                map['\x1D01'] = "AE";	// LATIN LETTER SMALL CAPITAL AE -- no decomposition
                map['\x1D02'] = "ae";	// LATIN SMALL LETTER TURNED AE -- no decomposition
                map['\x1D03'] = "B";	// LATIN LETTER SMALL CAPITAL BARRED B -- no decomposition
                map['\x1D04'] = "C";	// LATIN LETTER SMALL CAPITAL C -- no decomposition
                map['\x1D05'] = "D";	// LATIN LETTER SMALL CAPITAL D -- no decomposition
                map['\x1D06'] = "TH";	// LATIN LETTER SMALL CAPITAL ETH -- no decomposition
                map['\x1D07'] = "E";	// LATIN LETTER SMALL CAPITAL E -- no decomposition
                map['\x1D08'] = "e";	// LATIN SMALL LETTER TURNED OPEN E -- no decomposition
                map['\x1D09'] = "i";	// LATIN SMALL LETTER TURNED I -- no decomposition
                map['\x1D0A'] = "J";	// LATIN LETTER SMALL CAPITAL J -- no decomposition
                map['\x1D0B'] = "K";	// LATIN LETTER SMALL CAPITAL K -- no decomposition
                map['\x1D0C'] = "L";	// LATIN LETTER SMALL CAPITAL L WITH STROKE -- no decomposition
                map['\x1D0D'] = "M";	// LATIN LETTER SMALL CAPITAL M -- no decomposition
                map['\x1D0E'] = "N";	// LATIN LETTER SMALL CAPITAL REVERSED N -- no decomposition
                map['\x1D0F'] = "O";	// LATIN LETTER SMALL CAPITAL O -- no decomposition
                map['\x1D10'] = "O";	// LATIN LETTER SMALL CAPITAL OPEN O -- no decomposition
                map['\x1D11'] = "o";	// LATIN SMALL LETTER SIDEWAYS O -- no decomposition
                map['\x1D12'] = "o";	// LATIN SMALL LETTER SIDEWAYS OPEN O -- no decomposition
                map['\x1D13'] = "o";	// LATIN SMALL LETTER SIDEWAYS O WITH STROKE -- no decomposition
                map['\x1D14'] = "oe";	// LATIN SMALL LETTER TURNED OE -- no decomposition
                map['\x1D15'] = "ou";	// LATIN LETTER SMALL CAPITAL OU -- no decomposition
                map['\x1D16'] = "o";	// LATIN SMALL LETTER TOP HALF O -- no decomposition
                map['\x1D17'] = "o";	// LATIN SMALL LETTER BOTTOM HALF O -- no decomposition
                map['\x1D18'] = "P";	// LATIN LETTER SMALL CAPITAL P -- no decomposition
                map['\x1D19'] = "R";	// LATIN LETTER SMALL CAPITAL REVERSED R -- no decomposition
                map['\x1D1A'] = "R";	// LATIN LETTER SMALL CAPITAL TURNED R -- no decomposition
                map['\x1D1B'] = "T";	// LATIN LETTER SMALL CAPITAL T -- no decomposition
                map['\x1D1C'] = "U";	// LATIN LETTER SMALL CAPITAL U -- no decomposition
                map['\x1D1D'] = "u";	// LATIN SMALL LETTER SIDEWAYS U -- no decomposition
                map['\x1D1E'] = "u";	// LATIN SMALL LETTER SIDEWAYS DIAERESIZED U -- no decomposition
                map['\x1D1F'] = "m";	// LATIN SMALL LETTER SIDEWAYS TURNED M -- no decomposition
                map['\x1D20'] = "V";	// LATIN LETTER SMALL CAPITAL V -- no decomposition
                map['\x1D21'] = "W";	// LATIN LETTER SMALL CAPITAL W -- no decomposition
                map['\x1D22'] = "Z";	// LATIN LETTER SMALL CAPITAL Z -- no decomposition
                map['\x1D23'] = "EZH";	// LATIN LETTER SMALL CAPITAL EZH -- no decomposition
                map['\x1D24'] = "'";	// LTIN LETTER VOICED LARYNGEAL SPIRANT -- no decomposition
                map['\x1D25'] = "L";	// LATIN LETTER AIN -- no decomposition
                map['\x1D2C'] = "A";	// MODIFIER LETTER CAPITAL A
                map['\x1D2D'] = "AE";	// MODIFIER LETTER CAPITAL AE
                map['\x1D2E'] = "B";	// MODIFIER LETTER CAPITAL B
                map['\x1D2F'] = "B";	// MODIFIER LETTER CAPITAL BARRED B -- no decomposition
                map['\x1D30'] = "D";	// MODIFIER LETTER CAPITAL D
                map['\x1D31'] = "E";	// MODIFIER LETTER CAPITAL E
                map['\x1D32'] = "E";	// MODIFIER LETTER CAPITAL REVERSED E
                map['\x1D33'] = "G";	// MODIFIER LETTER CAPITAL G
                map['\x1D34'] = "H";	// MODIFIER LETTER CAPITAL H
                map['\x1D35'] = "I";	// MODIFIER LETTER CAPITAL I
                map['\x1D36'] = "J";	// MODIFIER LETTER CAPITAL J
                map['\x1D37'] = "K";	// MODIFIER LETTER CAPITAL K
                map['\x1D38'] = "L";	// MODIFIER LETTER CAPITAL L
                map['\x1D39'] = "M";	// MODIFIER LETTER CAPITAL M
                map['\x1D3A'] = "N";	// MODIFIER LETTER CAPITAL N
                map['\x1D3B'] = "N";	// MODIFIER LETTER CAPITAL REVERSED N -- no decomposition
                map['\x1D3C'] = "O";	// MODIFIER LETTER CAPITAL O
                map['\x1D3D'] = "OU";	// MODIFIER LETTER CAPITAL OU
                map['\x1D3E'] = "P";	// MODIFIER LETTER CAPITAL P
                map['\x1D3F'] = "R";	// MODIFIER LETTER CAPITAL R
                map['\x1D40'] = "T";	// MODIFIER LETTER CAPITAL T
                map['\x1D41'] = "U";	// MODIFIER LETTER CAPITAL U
                map['\x1D42'] = "W";	// MODIFIER LETTER CAPITAL W
                map['\x1D43'] = "a";	// MODIFIER LETTER SMALL A
                map['\x1D44'] = "a";	// MODIFIER LETTER SMALL TURNED A
                map['\x1D46'] = "ae";	// MODIFIER LETTER SMALL TURNED AE
                map['\x1D47'] = "b";    // MODIFIER LETTER SMALL B
                map['\x1D48'] = "d";    // MODIFIER LETTER SMALL D
                map['\x1D49'] = "e";    // MODIFIER LETTER SMALL E
                map['\x1D4A'] = "e";    // MODIFIER LETTER SMALL SCHWA
                map['\x1D4B'] = "e";    // MODIFIER LETTER SMALL OPEN E
                map['\x1D4C'] = "e";    // MODIFIER LETTER SMALL TURNED OPEN E
                map['\x1D4D'] = "g";    // MODIFIER LETTER SMALL G
                map['\x1D4E'] = "i";    // MODIFIER LETTER SMALL TURNED I -- no decomposition
                map['\x1D4F'] = "k";    // MODIFIER LETTER SMALL K
                map['\x1D50'] = "m";	// MODIFIER LETTER SMALL M
                map['\x1D51'] = "g";	// MODIFIER LETTER SMALL ENG
                map['\x1D52'] = "o";	// MODIFIER LETTER SMALL O
                map['\x1D53'] = "o";	// MODIFIER LETTER SMALL OPEN O
                map['\x1D54'] = "o";	// MODIFIER LETTER SMALL TOP HALF O
                map['\x1D55'] = "o";	// MODIFIER LETTER SMALL BOTTOM HALF O
                map['\x1D56'] = "p";	// MODIFIER LETTER SMALL P
                map['\x1D57'] = "t";	// MODIFIER LETTER SMALL T
                map['\x1D58'] = "u";	// MODIFIER LETTER SMALL U
                map['\x1D59'] = "u";	// MODIFIER LETTER SMALL SIDEWAYS U
                map['\x1D5A'] = "m";	// MODIFIER LETTER SMALL TURNED M
                map['\x1D5B'] = "v";	// MODIFIER LETTER SMALL V
                map['\x1D62'] = "i";	// LATIN SUBSCRIPT SMALL LETTER I
                map['\x1D63'] = "r";	// LATIN SUBSCRIPT SMALL LETTER R
                map['\x1D64'] = "u";	// LATIN SUBSCRIPT SMALL LETTER U
                map['\x1D65'] = "v";	// LATIN SUBSCRIPT SMALL LETTER V
                map['\x1D6B'] = "ue";	// LATIN SMALL LETTER UE -- no decomposition
                map['\x1E00'] = "A";	// LATIN CAPITAL LETTER A WITH RING BELOW
                map['\x1E01'] = "a";	// LATIN SMALL LETTER A WITH RING BELOW
                map['\x1E02'] = "B";	// LATIN CAPITAL LETTER B WITH DOT ABOVE
                map['\x1E03'] = "b";	// LATIN SMALL LETTER B WITH DOT ABOVE
                map['\x1E04'] = "B";	// LATIN CAPITAL LETTER B WITH DOT BELOW
                map['\x1E05'] = "b";	// LATIN SMALL LETTER B WITH DOT BELOW
                map['\x1E06'] = "B";	// LATIN CAPITAL LETTER B WITH LINE BELOW
                map['\x1E07'] = "b";	// LATIN SMALL LETTER B WITH LINE BELOW
                map['\x1E08'] = "C";	// LATIN CAPITAL LETTER C WITH CEDILLA AND ACUTE
                map['\x1E09'] = "c";	// LATIN SMALL LETTER C WITH CEDILLA AND ACUTE
                map['\x1E0A'] = "D";	// LATIN CAPITAL LETTER D WITH DOT ABOVE
                map['\x1E0B'] = "d";	// LATIN SMALL LETTER D WITH DOT ABOVE
                map['\x1E0C'] = "D";	// LATIN CAPITAL LETTER D WITH DOT BELOW
                map['\x1E0D'] = "d";	// LATIN SMALL LETTER D WITH DOT BELOW
                map['\x1E0E'] = "D";	// LATIN CAPITAL LETTER D WITH LINE BELOW
                map['\x1E0F'] = "d";	// LATIN SMALL LETTER D WITH LINE BELOW
                map['\x1E10'] = "D";	// LATIN CAPITAL LETTER D WITH CEDILLA
                map['\x1E11'] = "d";	// LATIN SMALL LETTER D WITH CEDILLA
                map['\x1E12'] = "D";	// LATIN CAPITAL LETTER D WITH CIRCUMFLEX BELOW
                map['\x1E13'] = "d";	// LATIN SMALL LETTER D WITH CIRCUMFLEX BELOW
                map['\x1E14'] = "E";	// LATIN CAPITAL LETTER E WITH MACRON AND GRAVE
                map['\x1E15'] = "e";	// LATIN SMALL LETTER E WITH MACRON AND GRAVE
                map['\x1E16'] = "E";	// LATIN CAPITAL LETTER E WITH MACRON AND ACUTE
                map['\x1E17'] = "e";	// LATIN SMALL LETTER E WITH MACRON AND ACUTE
                map['\x1E18'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX BELOW
                map['\x1E19'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX BELOW
                map['\x1E1A'] = "E";	// LATIN CAPITAL LETTER E WITH TILDE BELOW
                map['\x1E1B'] = "e";	// LATIN SMALL LETTER E WITH TILDE BELOW
                map['\x1E1C'] = "E";	// LATIN CAPITAL LETTER E WITH CEDILLA AND BREVE
                map['\x1E1D'] = "e";	// LATIN SMALL LETTER E WITH CEDILLA AND BREVE
                map['\x1E1E'] = "F";	// LATIN CAPITAL LETTER F WITH DOT ABOVE
                map['\x1E1F'] = "f";	// LATIN SMALL LETTER F WITH DOT ABOVE
                map['\x1E20'] = "G";	// LATIN CAPITAL LETTER G WITH MACRON
                map['\x1E21'] = "g";	// LATIN SMALL LETTER G WITH MACRON
                map['\x1E22'] = "H";	// LATIN CAPITAL LETTER H WITH DOT ABOVE
                map['\x1E23'] = "h";	// LATIN SMALL LETTER H WITH DOT ABOVE
                map['\x1E24'] = "H";	// LATIN CAPITAL LETTER H WITH DOT BELOW
                map['\x1E25'] = "h";	// LATIN SMALL LETTER H WITH DOT BELOW
                map['\x1E26'] = "H";	// LATIN CAPITAL LETTER H WITH DIAERESIS
                map['\x1E27'] = "h";	// LATIN SMALL LETTER H WITH DIAERESIS
                map['\x1E28'] = "H";	// LATIN CAPITAL LETTER H WITH CEDILLA
                map['\x1E29'] = "h";	// LATIN SMALL LETTER H WITH CEDILLA
                map['\x1E2A'] = "H";	// LATIN CAPITAL LETTER H WITH BREVE BELOW
                map['\x1E2B'] = "h";	// LATIN SMALL LETTER H WITH BREVE BELOW
                map['\x1E2C'] = "I";	// LATIN CAPITAL LETTER I WITH TILDE BELOW
                map['\x1E2D'] = "i";	// LATIN SMALL LETTER I WITH TILDE BELOW
                map['\x1E2E'] = "I";	// LATIN CAPITAL LETTER I WITH DIAERESIS AND ACUTE
                map['\x1E2F'] = "i";	// LATIN SMALL LETTER I WITH DIAERESIS AND ACUTE
                map['\x1E30'] = "K";	// LATIN CAPITAL LETTER K WITH ACUTE
                map['\x1E31'] = "k";	// LATIN SMALL LETTER K WITH ACUTE
                map['\x1E32'] = "K";	// LATIN CAPITAL LETTER K WITH DOT BELOW
                map['\x1E33'] = "k";	// LATIN SMALL LETTER K WITH DOT BELOW
                map['\x1E34'] = "K";	// LATIN CAPITAL LETTER K WITH LINE BELOW
                map['\x1E35'] = "k";	// LATIN SMALL LETTER K WITH LINE BELOW
                map['\x1E36'] = "L";	// LATIN CAPITAL LETTER L WITH DOT BELOW
                map['\x1E37'] = "l";	// LATIN SMALL LETTER L WITH DOT BELOW
                map['\x1E38'] = "L";	// LATIN CAPITAL LETTER L WITH DOT BELOW AND MACRON
                map['\x1E39'] = "l";	// LATIN SMALL LETTER L WITH DOT BELOW AND MACRON
                map['\x1E3A'] = "L";	// LATIN CAPITAL LETTER L WITH LINE BELOW
                map['\x1E3B'] = "l";	// LATIN SMALL LETTER L WITH LINE BELOW
                map['\x1E3C'] = "L";	// LATIN CAPITAL LETTER L WITH CIRCUMFLEX BELOW
                map['\x1E3D'] = "l";	// LATIN SMALL LETTER L WITH CIRCUMFLEX BELOW
                map['\x1E3E'] = "M";	// LATIN CAPITAL LETTER M WITH ACUTE
                map['\x1E3F'] = "m";	// LATIN SMALL LETTER M WITH ACUTE
                map['\x1E40'] = "M";	// LATIN CAPITAL LETTER M WITH DOT ABOVE
                map['\x1E41'] = "m";	// LATIN SMALL LETTER M WITH DOT ABOVE
                map['\x1E42'] = "M";	// LATIN CAPITAL LETTER M WITH DOT BELOW
                map['\x1E43'] = "m";	// LATIN SMALL LETTER M WITH DOT BELOW
                map['\x1E44'] = "N";	// LATIN CAPITAL LETTER N WITH DOT ABOVE
                map['\x1E45'] = "n";	// LATIN SMALL LETTER N WITH DOT ABOVE
                map['\x1E46'] = "N";	// LATIN CAPITAL LETTER N WITH DOT BELOW
                map['\x1E47'] = "n";	// LATIN SMALL LETTER N WITH DOT BELOW
                map['\x1E48'] = "N";	// LATIN CAPITAL LETTER N WITH LINE BELOW
                map['\x1E49'] = "n";	// LATIN SMALL LETTER N WITH LINE BELOW
                map['\x1E4A'] = "N";	// LATIN CAPITAL LETTER N WITH CIRCUMFLEX BELOW
                map['\x1E4B'] = "n";	// LATIN SMALL LETTER N WITH CIRCUMFLEX BELOW
                map['\x1E4C'] = "O";	// LATIN CAPITAL LETTER O WITH TILDE AND ACUTE
                map['\x1E4D'] = "o";	// LATIN SMALL LETTER O WITH TILDE AND ACUTE
                map['\x1E4E'] = "O";	// LATIN CAPITAL LETTER O WITH TILDE AND DIAERESIS
                map['\x1E4F'] = "o";	// LATIN SMALL LETTER O WITH TILDE AND DIAERESIS
                map['\x1E50'] = "O";	// LATIN CAPITAL LETTER O WITH MACRON AND GRAVE
                map['\x1E51'] = "o";	// LATIN SMALL LETTER O WITH MACRON AND GRAVE
                map['\x1E52'] = "O";	// LATIN CAPITAL LETTER O WITH MACRON AND ACUTE
                map['\x1E53'] = "o";	// LATIN SMALL LETTER O WITH MACRON AND ACUTE
                map['\x1E54'] = "P";	// LATIN CAPITAL LETTER P WITH ACUTE
                map['\x1E55'] = "p";	// LATIN SMALL LETTER P WITH ACUTE
                map['\x1E56'] = "P";	// LATIN CAPITAL LETTER P WITH DOT ABOVE
                map['\x1E57'] = "p";	// LATIN SMALL LETTER P WITH DOT ABOVE
                map['\x1E58'] = "R";	// LATIN CAPITAL LETTER R WITH DOT ABOVE
                map['\x1E59'] = "r";	// LATIN SMALL LETTER R WITH DOT ABOVE
                map['\x1E5A'] = "R";	// LATIN CAPITAL LETTER R WITH DOT BELOW
                map['\x1E5B'] = "r";	// LATIN SMALL LETTER R WITH DOT BELOW
                map['\x1E5C'] = "R";	// LATIN CAPITAL LETTER R WITH DOT BELOW AND MACRON
                map['\x1E5D'] = "r";	// LATIN SMALL LETTER R WITH DOT BELOW AND MACRON
                map['\x1E5E'] = "R";	// LATIN CAPITAL LETTER R WITH LINE BELOW
                map['\x1E5F'] = "r";	// LATIN SMALL LETTER R WITH LINE BELOW
                map['\x1E60'] = "S";	// LATIN CAPITAL LETTER S WITH DOT ABOVE
                map['\x1E61'] = "s";	// LATIN SMALL LETTER S WITH DOT ABOVE
                map['\x1E62'] = "S";	// LATIN CAPITAL LETTER S WITH DOT BELOW
                map['\x1E63'] = "s";	// LATIN SMALL LETTER S WITH DOT BELOW
                map['\x1E64'] = "S";	// LATIN CAPITAL LETTER S WITH ACUTE AND DOT ABOVE
                map['\x1E65'] = "s";	// LATIN SMALL LETTER S WITH ACUTE AND DOT ABOVE
                map['\x1E66'] = "S";	// LATIN CAPITAL LETTER S WITH CARON AND DOT ABOVE
                map['\x1E67'] = "s";	// LATIN SMALL LETTER S WITH CARON AND DOT ABOVE
                map['\x1E68'] = "S";	// LATIN CAPITAL LETTER S WITH DOT BELOW AND DOT ABOVE
                map['\x1E69'] = "s";	// LATIN SMALL LETTER S WITH DOT BELOW AND DOT ABOVE
                map['\x1E6A'] = "T";	// LATIN CAPITAL LETTER T WITH DOT ABOVE
                map['\x1E6B'] = "t";	// LATIN SMALL LETTER T WITH DOT ABOVE
                map['\x1E6C'] = "T";	// LATIN CAPITAL LETTER T WITH DOT BELOW
                map['\x1E6D'] = "t";	// LATIN SMALL LETTER T WITH DOT BELOW
                map['\x1E6E'] = "T";	// LATIN CAPITAL LETTER T WITH LINE BELOW
                map['\x1E6F'] = "t";	// LATIN SMALL LETTER T WITH LINE BELOW
                map['\x1E70'] = "T";	// LATIN CAPITAL LETTER T WITH CIRCUMFLEX BELOW
                map['\x1E71'] = "t";	// LATIN SMALL LETTER T WITH CIRCUMFLEX BELOW
                map['\x1E72'] = "U";	// LATIN CAPITAL LETTER U WITH DIAERESIS BELOW
                map['\x1E73'] = "u";	// LATIN SMALL LETTER U WITH DIAERESIS BELOW
                map['\x1E74'] = "U";	// LATIN CAPITAL LETTER U WITH TILDE BELOW
                map['\x1E75'] = "u";	// LATIN SMALL LETTER U WITH TILDE BELOW
                map['\x1E76'] = "U";	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX BELOW
                map['\x1E77'] = "u";	// LATIN SMALL LETTER U WITH CIRCUMFLEX BELOW
                map['\x1E78'] = "U";	// LATIN CAPITAL LETTER U WITH TILDE AND ACUTE
                map['\x1E79'] = "u";	// LATIN SMALL LETTER U WITH TILDE AND ACUTE
                map['\x1E7A'] = "U";	// LATIN CAPITAL LETTER U WITH MACRON AND DIAERESIS
                map['\x1E7B'] = "u";	// LATIN SMALL LETTER U WITH MACRON AND DIAERESIS
                map['\x1E7C'] = "V";	// LATIN CAPITAL LETTER V WITH TILDE
                map['\x1E7D'] = "v";	// LATIN SMALL LETTER V WITH TILDE
                map['\x1E7E'] = "V";	// LATIN CAPITAL LETTER V WITH DOT BELOW
                map['\x1E7F'] = "v";	// LATIN SMALL LETTER V WITH DOT BELOW
                map['\x1E80'] = "W";	// LATIN CAPITAL LETTER W WITH GRAVE
                map['\x1E81'] = "w";	// LATIN SMALL LETTER W WITH GRAVE
                map['\x1E82'] = "W";	// LATIN CAPITAL LETTER W WITH ACUTE
                map['\x1E83'] = "w";	// LATIN SMALL LETTER W WITH ACUTE
                map['\x1E84'] = "W";	// LATIN CAPITAL LETTER W WITH DIAERESIS
                map['\x1E85'] = "w";	// LATIN SMALL LETTER W WITH DIAERESIS
                map['\x1E86'] = "W";	// LATIN CAPITAL LETTER W WITH DOT ABOVE
                map['\x1E87'] = "w";	// LATIN SMALL LETTER W WITH DOT ABOVE
                map['\x1E88'] = "W";	// LATIN CAPITAL LETTER W WITH DOT BELOW
                map['\x1E89'] = "w";	// LATIN SMALL LETTER W WITH DOT BELOW
                map['\x1E8A'] = "X";	// LATIN CAPITAL LETTER X WITH DOT ABOVE
                map['\x1E8B'] = "x";	// LATIN SMALL LETTER X WITH DOT ABOVE
                map['\x1E8C'] = "X";	// LATIN CAPITAL LETTER X WITH DIAERESIS
                map['\x1E8D'] = "x";	// LATIN SMALL LETTER X WITH DIAERESIS
                map['\x1E8E'] = "Y";	// LATIN CAPITAL LETTER Y WITH DOT ABOVE
                map['\x1E8F'] = "y";	// LATIN SMALL LETTER Y WITH DOT ABOVE
                map['\x1E90'] = "Z";	// LATIN CAPITAL LETTER Z WITH CIRCUMFLEX
                map['\x1E91'] = "z";	// LATIN SMALL LETTER Z WITH CIRCUMFLEX
                map['\x1E92'] = "Z";	// LATIN CAPITAL LETTER Z WITH DOT BELOW
                map['\x1E93'] = "z";	// LATIN SMALL LETTER Z WITH DOT BELOW
                map['\x1E94'] = "Z";	// LATIN CAPITAL LETTER Z WITH LINE BELOW
                map['\x1E95'] = "z";	// LATIN SMALL LETTER Z WITH LINE BELOW
                map['\x1E96'] = "h";	// LATIN SMALL LETTER H WITH LINE BELOW
                map['\x1E97'] = "t";	// LATIN SMALL LETTER T WITH DIAERESIS
                map['\x1E98'] = "w";	// LATIN SMALL LETTER W WITH RING ABOVE
                map['\x1E99'] = "y";	// LATIN SMALL LETTER Y WITH RING ABOVE
                map['\x1E9A'] = "a";	// LATIN SMALL LETTER A WITH RIGHT HALF RING
                map['\x1E9B'] = "s";	// LATIN SMALL LETTER LONG S WITH DOT ABOVE
                map['\x1EA0'] = "A";	// LATIN CAPITAL LETTER A WITH DOT BELOW
                map['\x1EA1'] = "a";	// LATIN SMALL LETTER A WITH DOT BELOW
                map['\x1EA2'] = "A";	// LATIN CAPITAL LETTER A WITH HOOK ABOVE
                map['\x1EA3'] = "a";	// LATIN SMALL LETTER A WITH HOOK ABOVE
                map['\x1EA4'] = "A";	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND ACUTE
                map['\x1EA5'] = "a";	// LATIN SMALL LETTER A WITH CIRCUMFLEX AND ACUTE
                map['\x1EA6'] = "A";	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND GRAVE
                map['\x1EA7'] = "a";	// LATIN SMALL LETTER A WITH CIRCUMFLEX AND GRAVE
                map['\x1EA8'] = "A";	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND HOOK ABOVE
                map['\x1EA9'] = "a";	// LATIN SMALL LETTER A WITH CIRCUMFLEX AND HOOK ABOVE
                map['\x1EAA'] = "A";	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND TILDE
                map['\x1EAB'] = "a";	// LATIN SMALL LETTER A WITH CIRCUMFLEX AND TILDE
                map['\x1EAC'] = "A";	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND DOT BELOW
                map['\x1EAD'] = "a";	// LATIN SMALL LETTER A WITH CIRCUMFLEX AND DOT BELOW
                map['\x1EAE'] = "A";	// LATIN CAPITAL LETTER A WITH BREVE AND ACUTE
                map['\x1EAF'] = "a";	// LATIN SMALL LETTER A WITH BREVE AND ACUTE
                map['\x1EB0'] = "A";	// LATIN CAPITAL LETTER A WITH BREVE AND GRAVE
                map['\x1EB1'] = "a";	// LATIN SMALL LETTER A WITH BREVE AND GRAVE
                map['\x1EB2'] = "A";	// LATIN CAPITAL LETTER A WITH BREVE AND HOOK ABOVE
                map['\x1EB3'] = "a";	// LATIN SMALL LETTER A WITH BREVE AND HOOK ABOVE
                map['\x1EB4'] = "A";	// LATIN CAPITAL LETTER A WITH BREVE AND TILDE
                map['\x1EB5'] = "a";	// LATIN SMALL LETTER A WITH BREVE AND TILDE
                map['\x1EB6'] = "A";	// LATIN CAPITAL LETTER A WITH BREVE AND DOT BELOW
                map['\x1EB7'] = "a";	// LATIN SMALL LETTER A WITH BREVE AND DOT BELOW
                map['\x1EB8'] = "E";	// LATIN CAPITAL LETTER E WITH DOT BELOW
                map['\x1EB9'] = "e";	// LATIN SMALL LETTER E WITH DOT BELOW
                map['\x1EBA'] = "E";	// LATIN CAPITAL LETTER E WITH HOOK ABOVE
                map['\x1EBB'] = "e";	// LATIN SMALL LETTER E WITH HOOK ABOVE
                map['\x1EBC'] = "E";	// LATIN CAPITAL LETTER E WITH TILDE
                map['\x1EBD'] = "e";	// LATIN SMALL LETTER E WITH TILDE
                map['\x1EBE'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND ACUTE
                map['\x1EBF'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX AND ACUTE
                map['\x1EC0'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND GRAVE
                map['\x1EC1'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX AND GRAVE
                map['\x1EC2'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND HOOK ABOVE
                map['\x1EC3'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX AND HOOK ABOVE
                map['\x1EC4'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND TILDE
                map['\x1EC5'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX AND TILDE
                map['\x1EC6'] = "E";	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND DOT BELOW
                map['\x1EC7'] = "e";	// LATIN SMALL LETTER E WITH CIRCUMFLEX AND DOT BELOW
                map['\x1EC8'] = "I";	// LATIN CAPITAL LETTER I WITH HOOK ABOVE
                map['\x1EC9'] = "i";	// LATIN SMALL LETTER I WITH HOOK ABOVE
                map['\x1ECA'] = "I";	// LATIN CAPITAL LETTER I WITH DOT BELOW
                map['\x1ECB'] = "i";	// LATIN SMALL LETTER I WITH DOT BELOW
                map['\x1ECC'] = "O";	// LATIN CAPITAL LETTER O WITH DOT BELOW
                map['\x1ECD'] = "o";	// LATIN SMALL LETTER O WITH DOT BELOW
                map['\x1ECE'] = "O";	// LATIN CAPITAL LETTER O WITH HOOK ABOVE
                map['\x1ECF'] = "o";	// LATIN SMALL LETTER O WITH HOOK ABOVE
                map['\x1ED0'] = "O";	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND ACUTE
                map['\x1ED1'] = "o";	// LATIN SMALL LETTER O WITH CIRCUMFLEX AND ACUTE
                map['\x1ED2'] = "O";	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND GRAVE
                map['\x1ED3'] = "o";	// LATIN SMALL LETTER O WITH CIRCUMFLEX AND GRAVE
                map['\x1ED4'] = "O";	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND HOOK ABOVE
                map['\x1ED5'] = "o";	// LATIN SMALL LETTER O WITH CIRCUMFLEX AND HOOK ABOVE
                map['\x1ED6'] = "O";	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND TILDE
                map['\x1ED7'] = "o";	// LATIN SMALL LETTER O WITH CIRCUMFLEX AND TILDE
                map['\x1ED8'] = "O";	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND DOT BELOW
                map['\x1ED9'] = "o";	// LATIN SMALL LETTER O WITH CIRCUMFLEX AND DOT BELOW
                map['\x1EDA'] = "O";	// LATIN CAPITAL LETTER O WITH HORN AND ACUTE
                map['\x1EDB'] = "o";	// LATIN SMALL LETTER O WITH HORN AND ACUTE
                map['\x1EDC'] = "O";	// LATIN CAPITAL LETTER O WITH HORN AND GRAVE
                map['\x1EDD'] = "o";	// LATIN SMALL LETTER O WITH HORN AND GRAVE
                map['\x1EDE'] = "O";	// LATIN CAPITAL LETTER O WITH HORN AND HOOK ABOVE
                map['\x1EDF'] = "o";	// LATIN SMALL LETTER O WITH HORN AND HOOK ABOVE
                map['\x1EE0'] = "O";	// LATIN CAPITAL LETTER O WITH HORN AND TILDE
                map['\x1EE1'] = "o";	// LATIN SMALL LETTER O WITH HORN AND TILDE
                map['\x1EE2'] = "O";	// LATIN CAPITAL LETTER O WITH HORN AND DOT BELOW
                map['\x1EE3'] = "o";	// LATIN SMALL LETTER O WITH HORN AND DOT BELOW
                map['\x1EE4'] = "U";	// LATIN CAPITAL LETTER U WITH DOT BELOW
                map['\x1EE5'] = "u";	// LATIN SMALL LETTER U WITH DOT BELOW
                map['\x1EE6'] = "U";	// LATIN CAPITAL LETTER U WITH HOOK ABOVE
                map['\x1EE7'] = "u";	// LATIN SMALL LETTER U WITH HOOK ABOVE
                map['\x1EE8'] = "U";	// LATIN CAPITAL LETTER U WITH HORN AND ACUTE
                map['\x1EE9'] = "u";	// LATIN SMALL LETTER U WITH HORN AND ACUTE
                map['\x1EEA'] = "U";	// LATIN CAPITAL LETTER U WITH HORN AND GRAVE
                map['\x1EEB'] = "u";	// LATIN SMALL LETTER U WITH HORN AND GRAVE
                map['\x1EEC'] = "U";	// LATIN CAPITAL LETTER U WITH HORN AND HOOK ABOVE
                map['\x1EED'] = "u";	// LATIN SMALL LETTER U WITH HORN AND HOOK ABOVE
                map['\x1EEE'] = "U";	// LATIN CAPITAL LETTER U WITH HORN AND TILDE
                map['\x1EEF'] = "u";	// LATIN SMALL LETTER U WITH HORN AND TILDE
                map['\x1EF0'] = "U";	// LATIN CAPITAL LETTER U WITH HORN AND DOT BELOW
                map['\x1EF1'] = "u";	// LATIN SMALL LETTER U WITH HORN AND DOT BELOW
                map['\x1EF2'] = "Y";	// LATIN CAPITAL LETTER Y WITH GRAVE
                map['\x1EF3'] = "y";	// LATIN SMALL LETTER Y WITH GRAVE
                map['\x1EF4'] = "Y";	// LATIN CAPITAL LETTER Y WITH DOT BELOW
                map['\x1EF5'] = "y";	// LATIN SMALL LETTER Y WITH DOT BELOW
                map['\x1EF6'] = "Y";	// LATIN CAPITAL LETTER Y WITH HOOK ABOVE
                map['\x1EF7'] = "y";	// LATIN SMALL LETTER Y WITH HOOK ABOVE
                map['\x1EF8'] = "Y";	// LATIN CAPITAL LETTER Y WITH TILDE
                map['\x1EF9'] = "y";	// LATIN SMALL LETTER Y WITH TILDE
                map['\x2071'] = "i";	// SUPERSCRIPT LATIN SMALL LETTER I
                map['\x207F'] = "n";	// SUPERSCRIPT LATIN SMALL LETTER N
                map['\x212A'] = "K";	// KELVIN SIGN
                map['\x212B'] = "A";	// ANGSTROM SIGN
                map['\x212C'] = "B";	// SCRIPT CAPITAL B
                map['\x212D'] = "C";	// BLACK-LETTER CAPITAL C
                map['\x212F'] = "e";	// SCRIPT SMALL E
                map['\x2130'] = "E";	// SCRIPT CAPITAL E
                map['\x2131'] = "F";	// SCRIPT CAPITAL F
                map['\x2132'] = "F";	// TURNED CAPITAL F -- no decomposition
                map['\x2133'] = "M";	// SCRIPT CAPITAL M
                map['\x2134'] = "0";	// SCRIPT SMALL O
                map['\x213A'] = "0";	// ROTATED CAPITAL Q -- no decomposition
                map['\x2141'] = "G";	// TURNED SANS-SERIF CAPITAL G -- no decomposition
                map['\x2142'] = "L";	// TURNED SANS-SERIF CAPITAL L -- no decomposition
                map['\x2143'] = "L";	// REVERSED SANS-SERIF CAPITAL L -- no decomposition
                map['\x2144'] = "Y";	// TURNED SANS-SERIF CAPITAL Y -- no decomposition
                map['\x2145'] = "D";	// DOUBLE-STRUCK ITALIC CAPITAL D
                map['\x2146'] = "d";	// DOUBLE-STRUCK ITALIC SMALL D
                map['\x2147'] = "e";	// DOUBLE-STRUCK ITALIC SMALL E
                map['\x2148'] = "i";	// DOUBLE-STRUCK ITALIC SMALL I
                map['\x2149'] = "j";	// DOUBLE-STRUCK ITALIC SMALL J
                map['\xFB00'] = "ff";	// LATIN SMALL LIGATURE FF
                map['\xFB01'] = "fi";	// LATIN SMALL LIGATURE FI
                map['\xFB02'] = "fl";	// LATIN SMALL LIGATURE FL
                map['\xFB03'] = "ffi";	// LATIN SMALL LIGATURE FFI
                map['\xFB04'] = "ffl";	// LATIN SMALL LIGATURE FFL
                map['\xFB05'] = "st";	// LATIN SMALL LIGATURE LONG S T
                map['\xFB06'] = "st";	// LATIN SMALL LIGATURE ST
                map['\xFF21'] = "A";	// FULLWIDTH LATIN CAPITAL LETTER B
                map['\xFF22'] = "B";	// FULLWIDTH LATIN CAPITAL LETTER B
                map['\xFF23'] = "C";	// FULLWIDTH LATIN CAPITAL LETTER C
                map['\xFF24'] = "D";	// FULLWIDTH LATIN CAPITAL LETTER D
                map['\xFF25'] = "E";	// FULLWIDTH LATIN CAPITAL LETTER E
                map['\xFF26'] = "F";	// FULLWIDTH LATIN CAPITAL LETTER F
                map['\xFF27'] = "G";	// FULLWIDTH LATIN CAPITAL LETTER G
                map['\xFF28'] = "H";	// FULLWIDTH LATIN CAPITAL LETTER H
                map['\xFF29'] = "I";	// FULLWIDTH LATIN CAPITAL LETTER I
                map['\xFF2A'] = "J";	// FULLWIDTH LATIN CAPITAL LETTER J
                map['\xFF2B'] = "K";	// FULLWIDTH LATIN CAPITAL LETTER K
                map['\xFF2C'] = "L";	// FULLWIDTH LATIN CAPITAL LETTER L
                map['\xFF2D'] = "M";	// FULLWIDTH LATIN CAPITAL LETTER M
                map['\xFF2E'] = "N";	// FULLWIDTH LATIN CAPITAL LETTER N
                map['\xFF2F'] = "O";	// FULLWIDTH LATIN CAPITAL LETTER O
                map['\xFF30'] = "P";	// FULLWIDTH LATIN CAPITAL LETTER P
                map['\xFF31'] = "Q";	// FULLWIDTH LATIN CAPITAL LETTER Q
                map['\xFF32'] = "R";	// FULLWIDTH LATIN CAPITAL LETTER R
                map['\xFF33'] = "S";	// FULLWIDTH LATIN CAPITAL LETTER S
                map['\xFF34'] = "T";	// FULLWIDTH LATIN CAPITAL LETTER T
                map['\xFF35'] = "U";	// FULLWIDTH LATIN CAPITAL LETTER U
                map['\xFF36'] = "V";	// FULLWIDTH LATIN CAPITAL LETTER V
                map['\xFF37'] = "W";	// FULLWIDTH LATIN CAPITAL LETTER W
                map['\xFF38'] = "X";	// FULLWIDTH LATIN CAPITAL LETTER X
                map['\xFF39'] = "Y";	// FULLWIDTH LATIN CAPITAL LETTER Y
                map['\xFF3A'] = "Z";	// FULLWIDTH LATIN CAPITAL LETTER Z
                map['\xFF41'] = "a";	// FULLWIDTH LATIN SMALL LETTER A
                map['\xFF42'] = "b";	// FULLWIDTH LATIN SMALL LETTER B
                map['\xFF43'] = "c";	// FULLWIDTH LATIN SMALL LETTER C
                map['\xFF44'] = "d";	// FULLWIDTH LATIN SMALL LETTER D
                map['\xFF45'] = "e";	// FULLWIDTH LATIN SMALL LETTER E
                map['\xFF46'] = "f";	// FULLWIDTH LATIN SMALL LETTER F
                map['\xFF47'] = "g";	// FULLWIDTH LATIN SMALL LETTER G
                map['\xFF48'] = "h";	// FULLWIDTH LATIN SMALL LETTER H
                map['\xFF49'] = "i";	// FULLWIDTH LATIN SMALL LETTER I
                map['\xFF4A'] = "j";	// FULLWIDTH LATIN SMALL LETTER J
                map['\xFF4B'] = "k";	// FULLWIDTH LATIN SMALL LETTER K
                map['\xFF4C'] = "l";	// FULLWIDTH LATIN SMALL LETTER L
                map['\xFF4D'] = "m";	// FULLWIDTH LATIN SMALL LETTER M
                map['\xFF4E'] = "n";	// FULLWIDTH LATIN SMALL LETTER N
                map['\xFF4F'] = "o";	// FULLWIDTH LATIN SMALL LETTER O
                map['\xFF50'] = "p";	// FULLWIDTH LATIN SMALL LETTER P
                map['\xFF51'] = "q";	// FULLWIDTH LATIN SMALL LETTER Q
                map['\xFF52'] = "r";	// FULLWIDTH LATIN SMALL LETTER R
                map['\xFF53'] = "s";	// FULLWIDTH LATIN SMALL LETTER S
                map['\xFF54'] = "t";	// FULLWIDTH LATIN SMALL LETTER T
                map['\xFF55'] = "u";	// FULLWIDTH LATIN SMALL LETTER U
                map['\xFF56'] = "v";	// FULLWIDTH LATIN SMALL LETTER V
                map['\xFF57'] = "w";	// FULLWIDTH LATIN SMALL LETTER W
                map['\xFF58'] = "x";	// FULLWIDTH LATIN SMALL LETTER X
                map['\xFF59'] = "y";	// FULLWIDTH LATIN SMALL LETTER Y
                map['\xFF5A'] = "z";	// FULLWIDTH LATIN SMALL LETTER Z
            }
        } 
    }
}
