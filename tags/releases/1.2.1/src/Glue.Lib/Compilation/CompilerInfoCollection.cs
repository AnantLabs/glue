using System;
using System.Collections;

namespace Glue.Lib.Compilation
{
    public class CompilerInfoCollection
    {
        Hashtable compilers;

        public CompilerInfoCollection()
        {
            //compilers = new Hashtable(CaseInsensitiveHashCodeProvider.Default,
            //    CaseInsensitiveComparer.Default);
            compilers = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        }

        public CompilerInfo FindExtension(string extension)
        {
            foreach (CompilerInfo info in compilers.Values)
                if (string.Compare(info.Extension, extension, true) == 0)
                    return info;
            return null;
        }

        public CompilerInfo this[string language] 
        {
            get 
            { 
                return compilers[language] as CompilerInfo; 
            }
            set 
            {
                compilers[language] = value;
                string [] langs = language.Split(';');
                foreach (string s in langs) 
                {
                    string x = s.Trim();
                    if (x != "")
                        compilers[x] = value;
                }
            }
        }
    }
}

