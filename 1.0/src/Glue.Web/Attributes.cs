using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Glue.Web
{
	/// <summary>
	/// Summary description for Attributes.
	/// </summary>
	public class UrlAttribute : Attribute
	{
        static Regex patternToRegex = new Regex(@"\$([a-zA-Z_\.]+)", RegexOptions.Compiled);
        
        public Regex Pattern;
        private string[] groupNames;
        private int[] groupNumbers;
        
        public UrlAttribute(string pattern)
        {
            pattern = pattern.Replace("*", "[^/?]+");
            pattern = patternToRegex.Replace(pattern, "(?<$1>[^/?]+)");
            Pattern = new Regex(pattern, RegexOptions.Compiled);
            groupNames = Pattern.GetGroupNames();
            groupNumbers = Pattern.GetGroupNumbers();
        }

        public UrlAttribute(Regex pattern)
        {
            Pattern = pattern;
            groupNames = Pattern.GetGroupNames();
            groupNumbers = Pattern.GetGroupNumbers();
        }

        public void Process(string path, NameValueCollection destination)
        {
            Match match = Pattern.Match(path);
            if (!match.Success)
                return;
            for (int i = 1; i < groupNumbers.Length; i++)
                destination[groupNames[i]] = match.Groups[groupNumbers[i]].Value;
        }
	}
}
