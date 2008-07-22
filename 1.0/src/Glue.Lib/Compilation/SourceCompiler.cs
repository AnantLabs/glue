using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Caching;

namespace Glue.Lib.Compilation
{
	/// <summary>
	/// Summary description for SourceCompiler.
	/// </summary>
	public class SourceCompiler : BaseCompiler
	{
        private string _source;

        public override void Compile()
        {
            // Get compiler and parameters
            //ICodeCompiler compiler = Settings.Compilers[Language].Provider.CreateCompiler();
            CodeDomProvider provider = Settings.Compilers[Language].Provider;

            foreach (string assembly in Settings.Assemblies)
                Parameters.ReferencedAssemblies.Add(ResolveAssemblyPath(assembly));
            
            CompilerResults results = provider.CompileAssemblyFromSource(Parameters, Source);
            if (results.NativeCompilerReturnValue != 0 || results.Errors.HasErrors)
            {
                throw new CompilationException(results);
            }

            _assembly = results.CompiledAssembly;
        }

        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public override string Language
        {
            get { return _language == null ? Settings.DefaultLanguage : _language; }
            set { _language = value; }
        }
	}
}
