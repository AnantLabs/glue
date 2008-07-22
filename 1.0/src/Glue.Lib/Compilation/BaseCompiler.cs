using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

namespace Glue.Lib.Compilation
{
	/// <summary>
	/// Summary description for BaseCompiler
	/// </summary>
    public abstract class BaseCompiler
    {
        protected Assembly _assembly = null;
        protected string _language = "";
        protected Settings _settings = null;
        protected CompilerParameters _parameters = null;

        /// <summary>
        /// Resultant compiled assembly.
        /// </summary>
        public Assembly CompiledAssembly
        {
            get { return this._assembly; }
        }

        /// <summary>
        /// Programming language
        /// </summary>
        public virtual string Language
        {
            get { return _language == null || _language.Length == 0 ? Settings.DefaultLanguage : _language; }
            set { _language = value; }
        }

        /// <summary>
        /// Configured compiler settings.
        /// </summary>
        public Settings Settings
        {
            get 
            {
                if (_settings == null)
                    _settings = Settings.Current;
                return _settings;
            }
        }

        /// <summary>
        ///  Constructs parameters based on Settings. 
        /// </summary>
        public CompilerParameters Parameters
        {
            get 
            {
                if (_parameters == null)
                {
                    _parameters = new CompilerParameters();
                    _parameters.IncludeDebugInformation = Settings.Debug;
                    _parameters.GenerateInMemory = Settings.GenerateInMemory;
                    _parameters.GenerateExecutable = false;
                    _parameters.TempFiles.KeepFiles = Settings.KeepTempFiles;
                    // _parameters.CompilerOptions = Settings.Compilers[Language].CompilerOptions;
                    if (Settings.AssembliesInBin)
                    {
                        foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "*.dll"))
                        {
                            // HACK: Should determine if this DLL is a .NET assembly
                            if (Path.GetFileName(file).ToLower() != "sqlite.dll" &&
                                Path.GetFileName(file).ToLower() != "sqlite3.dll")
                            _parameters.ReferencedAssemblies.Add(file);
                        }
                        if (Assembly.GetEntryAssembly() != null)
                            _parameters.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);
                    }
                }
                return _parameters;
            }
        }

        /// <summary>
        /// Protected default constructor.
        /// </summary>
        protected BaseCompiler()
        {
        }

        /// <summary>
        /// Override Compile in derived class.
        /// </summary>
        public abstract void Compile();

        /// <summary>
        /// Loads text from given file.
        /// </summary>
        protected virtual string GetFileContents(string path)
        {
            using (StreamReader reader = new StreamReader(path)) 
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Returns number of lines in given text. Handles different CR/LF, LF, CR 
        /// combinations.
        /// </summary>
        protected int GetLineCount(string text)
        {
            return GetLineCount(text, 0, text.Length);
        }

        /// <summary>
        /// Returns number of lines in given section of text. 
        /// Handles different CR/LF, LF, CR  combinations.
        /// </summary>
        protected int GetLineCount(string text, int start, int end)
        {
            int n = 0;
            int i = start;
            while (i < end)
            {
                if (text[i] == '\r')
                {
                    n++;
                    i++;
                    if (i < end && text[i] == '\n')
                        i++;
                } 
                else if (text[i] == '\n')
                {
                    n++;
                    i++;
                }
                else
                {
                    i++;
                }
            }
            return n;
        }

        /// <summary>
        /// Map a path as specified in a source directive to a physical path.
        /// Override this to achieve path virtualisation.
        /// </summary>
        protected virtual string MapPath(string path)
        {
            return path;
        }

        /// <summary>
        /// Resolves the physical path to the given assembly.
        /// </summary>
        public virtual string ResolveAssemblyPath(string assemblyName)
        {
            if (string.Compare(Path.GetExtension(assemblyName), ".dll", true) == 0)
                assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

            // Check current directory
            string test = Path.GetFullPath(assemblyName + ".dll");
            if (File.Exists(test))
                return test;

            // Check mapped directory
            test = MapPath(assemblyName + ".dll");
            if (File.Exists(test))
                return test;

            // Check path in which Glue.Lib resides
            Assembly a = typeof(BaseCompiler).Assembly;
            if (a != null)
            {
                test = Path.Combine(Path.GetDirectoryName(a.Location), assemblyName + ".dll");
                if (File.Exists(test))
                    return test;
            }

            // Check path of calling executable
            a = Assembly.GetCallingAssembly();
            if (a != null)
            {
                test = Path.Combine(Path.GetDirectoryName(a.Location), assemblyName + ".dll");
                if (File.Exists(test))
                    return test;
            }

            // Check path of entry executable
            a = Assembly.GetEntryAssembly();
            if (a != null)
            {
                test = Path.Combine(Path.GetDirectoryName(a.Location), assemblyName + ".dll");
                if (File.Exists(test))
                    return test;
            }
            
            // Check .net path
            a = typeof(int).Assembly;
            if (a != null)
            {
                test = Path.Combine(Path.GetDirectoryName(a.Location), assemblyName + ".dll");
                if (File.Exists(test))
                    return test;
            }

            //a = Assembly.LoadWithPartialName(assemblyName);
            a = Assembly.Load(assemblyName);
            if (a != null)
                return a.Location; 

            throw new CompilationException(string.Format("Unable to resolve assembly \"{0}\".", assemblyName));
        } 
    }
}
