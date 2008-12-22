using System;
using System.Collections;

namespace Glue.Lib.IO
{
	/// <summary>
	/// Summary description for WildCard.
	/// </summary>
	public class WildCard
	{
        private WildCard()
        {
        }

        /// <summary>
        ///   Matches tests whether the given input string matches the wildcard. WildCard
        ///   can contain * and ? special characters.
        /// </summary>
        public static bool Matches(string input, string wild)
        {
            return Matches(input, 0, wild, 0);
        }

        /// <summary>
        ///   Matches tests whether the given input string matches the list of wildcards. 
        ///   WildCards can contain * and ? special characters.
        /// </summary>
        public static bool Matches(string input, string[] wildcards)
        {
            foreach (string wild in wildcards)
                if (Matches(input, wild))
                    return true;
            return false;
        }

        /// <summary>
        ///   Matches tests whether the given input string matches the list of wildcards. 
        ///   WildCards can contain * and ? special characters.
        /// </summary>
        public static bool Matches(string input, IEnumerable wildcards)
        {
            foreach (object wild in wildcards)
                if (Matches(input, wild.ToString()))
                    return true;
            return false;
        }

        /// <summary>
        ///   Matches tests whether the given input string matches the wildcard. WildCard
        ///   can contain * and ? special characters.
        /// </summary>
        private static bool Matches(string input, int i, string wild, int j)
        {
            // compare until we hit a wild card
            while (i < input.Length && j < wild.Length)
            {
                if (wild[j] == '?' || char.ToUpper(input[i]) == char.ToUpper(wild[j]))
                {
                    i++;
                    j++;
                }
                else if (wild[j] == '*')
                {
                    j++;
                    // end of pattern?
                    if (j >= wild.Length)
                    {
                        return true;
                    }
                    // do recursion on hitting a *
                    while (i < input.Length)
                    {
                        // see if this remainder matches
                        if (Matches(input, i, wild, j))
                        {
                            return true;
                        }
                        // next remainder
                        i++;
                    } 
                    return false;
                } 
                else
                {
                    // return mismatch
                    return false;
                }
            }
            // skip remaining '*'
            while (j < wild.Length && wild[j] == '*')
            {
                j++;
            }
            // it's a match if we are at the end of both wildcard and input
            return (i >= input.Length && j >= wild.Length);
        }
    }
}
