using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;

namespace Glue.Lib.IO
{
	/// <summary>
	/// Summary description for FileSet.
	/// </summary>
	public class FileSet
	{
        StringCollection _includes = new StringCollection();
        StringCollection _excludes = new StringCollection();
        StringCollection _files = new StringCollection();
        StringCollection _directories = new StringCollection();
        string _baseDirectory = ".";
        bool _defaultExcludes = true;
        Regex[] _includePatterns;
        Regex[] _excludePatterns;

        public string BaseDirectory
        {
            get { return _baseDirectory; }
            set { _baseDirectory = CleanPath(value); }
        }
        
        public bool DefaultExcludes
        {
            get { return _defaultExcludes; }
            set { _defaultExcludes = value; }
        }

        public StringCollection Includes
        {
            get { return _includes; }
        }

        public StringCollection Excludes
        {
            get { return _excludes; }
        }
        
        public StringCollection Files
        {
            get { return _files; }
        }
        
        public StringCollection Directories
        {
            get { return _directories; }
        }
        
        public FileSet()
        {
        }

        public FileSet(string baseDirectory, params string[] includes)
		{
            BaseDirectory = baseDirectory;
            Includes.AddRange(includes);
		}

        public void Scan()
        {
            _files.Clear();
            _directories.Clear();
            StringCollection includes = new StringCollection();
            foreach (string s in Includes)
                includes.Add(s);
            if (includes.Count == 0)
                includes.Add("**");
            StringCollection excludes = new StringCollection();
            foreach (string s in Excludes)
                excludes.Add(s);
            if (DefaultExcludes)
            {
                excludes.Add("**/*~");
                excludes.Add("**/#*#");
                excludes.Add("**/.#*");
                excludes.Add("**/%*%");
                excludes.Add("**/CVS");
                excludes.Add("**/CVS/**");
                excludes.Add("**/.cvsignore");
                excludes.Add("**/.svn");
                excludes.Add("**/.svn/**");
                excludes.Add("**/_svn");
                excludes.Add("**/_svn/**");
                excludes.Add("**/SCCS");
                excludes.Add("**/SCCS/**");
                excludes.Add("**/vssver.scc");
                excludes.Add("**/_vti_cnf/**");
            }
            _includePatterns = ToRegex(includes);
            _excludePatterns = ToRegex(excludes);

            StringCollection scannedDirectories = new StringCollection();
            SearchDirectory[] searchDirectories = GetSearchDirectories(_baseDirectory, includes);

            foreach (SearchDirectory searchDirectory in searchDirectories)
            {
                Scan(searchDirectory.Directory, searchDirectory.Recursive, scannedDirectories);
            }
        }

		/// <summary>
		/// Helper class SearchDirectory 
		/// </summary>
		private class SearchDirectory : IComparable
		{
			public string Directory;
			public bool Recursive;

			public SearchDirectory(string directory, bool recursive)
			{
				Directory = directory;
				Recursive = recursive;
			}

			public int CompareTo(object obj)
			{
				if (obj is string)
				{
					return string.Compare(this.Directory, (string)obj, true);
				}
				if (obj is SearchDirectory)
				{
					SearchDirectory other = obj as SearchDirectory;
					if (this.Recursive == other.Recursive)
						return CompareTo(other.Directory);
					if (this.Recursive && !other.Recursive)
						return -1;
					else
						return +1;
				}
				throw new ArgumentException("Cannot compare SearchDirectory to " + obj.GetType());
			}
		}

        /// <summary>
        /// </summary>
		private void Scan(string relativeDirectory, bool recursive, StringCollection scannedDirectories)
        {
            // Scan a directory only once
            if (scannedDirectories.Contains(relativeDirectory))
                return;
            Log.Debug("Scanning: " + relativeDirectory);

            string absoluteDirectory = Path.Combine(_baseDirectory, relativeDirectory);
            DirectoryInfo directoryInfo = new DirectoryInfo(absoluteDirectory);
            if (!directoryInfo.Exists)
                return;

            foreach (FileInfo info in directoryInfo.GetFiles())
            {
                string relativePath = Path.Combine(relativeDirectory, info.Name);
                bool match = false;
                foreach (Regex include in _includePatterns)
                    if (include.IsMatch(relativePath))
                    {
                        match = true;
                        break;
                    }
                if (!match)
                    continue;
                match = false;
                foreach (Regex exclude in _excludePatterns)
                    if (exclude.IsMatch(relativePath))
                    {
                        match = true;
                    }
                if (match)
                    continue;
                
                _files.Add(string.Concat(absoluteDirectory, "\\", relativePath));
            }
            scannedDirectories.Add(relativeDirectory);

            foreach (DirectoryInfo info in directoryInfo.GetDirectories())
            {
                string relativePath = Path.Combine(relativeDirectory, info.Name);
                bool match = false;
                foreach (Regex include in _includePatterns)
                    if (include.IsMatch(relativePath))
                    {
                        match = true;
                        break;
                    }
                if (match)
                {
                    match = false;
                    foreach (Regex exclude in _excludePatterns)
                        if (exclude.IsMatch(relativePath))
                        {
                            match = true;
                        }
                    if (!match)
                        _directories.Add(string.Concat(absoluteDirectory, "\\", relativePath));
                }

                if (recursive)
                {
                    Scan(Path.Combine(relativeDirectory, info.Name), true, scannedDirectories);
                }
            }
        }

        private static SearchDirectory[] GetSearchDirectories(string baseDirectory, StringCollection patterns)
        {
            ArrayList list = new ArrayList();
            foreach (string pattern in patterns)
            {
                string s = pattern;
                s = s.Replace('\\', Path.DirectorySeparatorChar);
                s = s.Replace('/', Path.DirectorySeparatorChar);

                // Get indices of pieces used for recursive check only
                int indexOfFirstDirectoryWildcard = s.IndexOf("**");
                int indexOfLastOriginalDirectorySeparator = s.LastIndexOf(Path.DirectorySeparatorChar);

                // search for the first wildcard character (if any) and exclude the rest of the string beginnning from the character
                char[] wildcards = {'?', '*'};
                int indexOfFirstWildcard = s.IndexOfAny(wildcards);
                if (indexOfFirstWildcard != -1) 
                { 
                    // if found any wildcard characters
                    s = s.Substring(0, indexOfFirstWildcard);
                }

                // find the last DirectorySeparatorChar (if any) and exclude the rest of the string
                int indexOfLastDirectorySeparator = s.LastIndexOf(Path.DirectorySeparatorChar);

                // The pattern is potentially recursive if and only if more than one base directory could be matched.
                // ie: 
                //    **
                //    **/*.txt
                //    foo*/xxx
                //    x/y/z?/www
                // This condition is true if and only if:
                //  - The first wildcard is before the last directory separator, or
                //  - The pattern contains a directory wildcard ("**")
                bool recursive = (indexOfFirstWildcard != -1 && (indexOfFirstWildcard < indexOfLastOriginalDirectorySeparator )) || indexOfFirstDirectoryWildcard != -1;

                // substring preceding the separator represents our search directory 
                // and the part following it represents nant search pattern relative 
                // to it
                if (indexOfLastDirectorySeparator != -1) 
                {
                    s = pattern.Substring(0, indexOfLastDirectorySeparator);
                    if (s.Length == 2 && s[1] == Path.VolumeSeparatorChar) 
                    {
                        s += Path.DirectorySeparatorChar;
                    }
                } 
                else 
                {
                    s = "";
                }
            
                //We only prepend BaseDirectory when s represents a relative path.
                if (Path.IsPathRooted(s)) 
                {
                    // throw new ArgumentException("Path must be relative: '" + s + "'");
                    s = new DirectoryInfo(s).FullName;
                } 
                string searchDirectory = s;
                Log.Debug("SearchDirectory " + searchDirectory);

				int index = list.IndexOf(searchDirectory);
                if (index < 0)
                    list.Add(new SearchDirectory(searchDirectory, recursive));
                else
                    if (recursive)
                        ((SearchDirectory)list[index]).Recursive = true;
            }
            // Sort the search directories. Put recursive ones on top, then sort
            // on directory name. This is necessary for efficient scanning later on.
            list.Sort();

            return (SearchDirectory[])list.ToArray(typeof(SearchDirectory));
        }

        private static Regex[] ToRegex(StringCollection nantPatterns)
        {
            Regex[] list = new Regex[nantPatterns.Count];
            for (int i = 0; i < nantPatterns.Count; i++)
            {
                list[i] = ToRegex(nantPatterns[i]);
                Log.Debug("Pattern: " + nantPatterns[i] + " => " + list[i]);
            }
            return list;
        }

        /// <summary>
        /// Converts search pattern to a regular expression.
        /// </summary>
        /// <param name="nantPattern">Search pattern relative to the search directory.</param>
        /// <returns>Regular expresssion</returns>
        private static Regex ToRegex(string nantPattern) 
        {
            StringBuilder pattern = new StringBuilder(nantPattern);
            pattern.Replace('/', Path.DirectorySeparatorChar);
            pattern.Replace('\\', Path.DirectorySeparatorChar);

            // The '\' character is a special character in regular expressions
            // and must be escaped before doing anything else.
            pattern.Replace(@"\", @"\\");

            // Escape the rest of the regular expression special characters.
            // NOTE: Characters other than . $ ^ { [ ( | ) * + ? \ match themselves.
            // TODO: Decide if ] and } are missing from this list, the above
            // list of characters was taking from the .NET SDK docs.
            pattern.Replace(".", @"\.");
            pattern.Replace("$", @"\$");
            pattern.Replace("^", @"\^");
            pattern.Replace("{", @"\{");
            pattern.Replace("[", @"\[");
            pattern.Replace("(", @"\(");
            pattern.Replace(")", @"\)");
            pattern.Replace("+", @"\+");

            // Special case directory separator string under Windows.
            string separator = Path.DirectorySeparatorChar.ToString();
            if (separator == @"\") 
            {
                separator = @"\\";
            }

            // Convert NAnt pattern characters to regular expression patterns.

            // Start with ? - it's used below
            pattern.Replace("?", "[^" + separator + "]?");

            // SPECIAL CASE: any *'s directory between slashes or at the end of the
            // path are replaced with a 1..n pattern instead of 0..n: (?<=\\)\*(?=($|\\))
            // This ensures that C:\*foo* matches C:\foo and C:\* won't match C:.
            pattern = new StringBuilder(Regex.Replace(pattern.ToString(), "(?<=" + separator + ")\\*(?=($|" + separator + "))", "[^" + separator + "]+"));
            
            // SPECIAL CASE: to match subdirectory OR current directory, If
            // we do this then we can write something like 'src/**/*.cs'
            // to match all the files ending in .cs in the src directory OR
            // subdirectories of src.
            pattern.Replace(separator + "**" + separator, separator + "(.|?" + separator + ")?" );
            pattern.Replace("**" + separator, ".|(?<=^|" + separator + ")" );
            pattern.Replace(separator + "**", "(?=$|" + separator + ").|" );

            // .| is a place holder for .* to prevent it from being replaced in next line
            pattern.Replace("**", ".|");
            pattern.Replace("*", "[^" + separator + "]*");
            pattern.Replace(".|", ".*"); // replace place holder string

            // Help speed up the search
            if (pattern.Length > 0) 
            {
                pattern.Insert(0, '^'); // start of line
                pattern.Append('$'); // end of line
            }

            string patternText = pattern.ToString();

            if (patternText.StartsWith("^.*"))
                patternText = patternText.Substring(3);
            if (patternText.EndsWith(".*$"))
                patternText = patternText.Substring(0, pattern.Length-3);

            return new Regex(patternText, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Ensure path has valid directory separator chars:
        /// '\' on Win32, '/' on *nix.
        /// </summary>
        private static string CleanPath(string path) 
        {
            // NAnt patterns can use either / \ as a directory separator.
            // We must replace both of these characters with Path.DirectoryseparatorChar
            return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}
