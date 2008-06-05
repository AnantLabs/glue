using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Caching;

namespace Glue.Lib.Compilation
{
	/// <summary>
	/// Summary description for FileCompiler.
	/// </summary>
	public class FileCompiler : BaseCompiler
	{
        private string _path;

        public override void Compile()
        {
            // Get compiler and parameters
            //ICodeCompiler compiler = Settings.Compilers[Language].Provider.CreateCompiler();
            CodeDomProvider provider = Settings.Compilers[Language].Provider;

            foreach (string assembly in Settings.Assemblies)
                Parameters.ReferencedAssemblies.Add(ResolveAssemblyPath(assembly));
            
            CompilerResults results = provider.CompileAssemblyFromFile(Parameters, Path);
            if (results.NativeCompilerReturnValue != 0 || results.Errors.HasErrors)
            {
                throw new CompilationException(Path, results);
            }

            _assembly = results.CompiledAssembly;
        }

        public string Path
        {
            get 
            { 
                return _path; 
            }
            set 
            { 
                _path = value; 
                _language = null;
            }
        }

        public override string Language
        {
            get 
            { 
                if (_language == null || _language.Length == 0)
                {
                    if (_path != null && _path.Length > 0)
                    {
                        CompilerInfo info = Settings.GetCompilerInfoFromFileName(_path);
                        if (info != null)
                            return info.Languages.Split(';')[0];
                    }
                    return Settings.DefaultLanguage;
                }
                return _language;
            }
            set 
            { 
                _language = value; 
            }
        }
	}
}
