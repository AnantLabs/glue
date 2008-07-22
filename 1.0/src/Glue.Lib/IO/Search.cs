using System;
using System.Collections;
using System.IO;

namespace Glue.Lib.IO
{
	/// <summary>
	/// Summary description for Search.
	/// </summary>
	public class Search
	{
        private Search()
		{
		}

        public static string Where(string path, params string[] directories)
        {
            foreach (string directory in directories)
            {
                if (directory == null || directory.Length == 0)
                    continue;
                string p = Path.Combine(Path.GetFullPath(directory), path);
                if (File.Exists(p))
                    return p;
            }
            return null;
        }

        public static string Where(string path, IEnumerable directories)
        {
            foreach (object directory in directories)
            {
                if (directory == null)
                    continue;
                string p = Path.Combine(Path.GetFullPath(directory.ToString()), path);
                if (File.Exists(p))
                    return p;
            }
            return null;
        }

	}
}
