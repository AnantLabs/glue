using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Specialized;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text.RegularExpressions;

using Glue.Lib.Compilation;

using IronPython.Hosting;

namespace Glue.Web
{
    class MyStream : Stream
    {
        TextWriter _writer;
        public MyStream(TextWriter writer)
        {
            _writer = writer;
        }
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override long Position
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writer.Write(System.Text.Encoding.UTF8.GetString(buffer, offset, count));
        }
    }


    //public class MyPythonView
    //{
    //    public MyPythonView(PythonEngine engine)
    //    {
    //        this.engine = engine;
    //    }

    //    public IDictionary Variables
    //    {
    //        get { }
    //        set { }
    //    }

    //    public virtual void Render(TextWriter writer)
    //    {
    //        // foreach item in Variables do
    //        //   engine.Globals[key] = val
    //        // engine.Globals["writer"] = writer;
    //        // engine.Execute("PyTemplateClass().Render(writer)");
    //    }
    //}

    //class MyPythonCompilerHelper
    //{
    //    // generate source code
    //    // create python engine
    //    public void Compile()
    //    {
    //    }
    //    public Assembly CompiledAssembly
    //    {
    //        get { return typeof(MyPythonView).Assembly; }
    //    }
    //    public Type CompiledType
    //    {
    //        get { return typeof(MyPythonView); }
    //    }
    //}

    /// <summary>
    /// Class to process and render a template with asp-like tags and python source.
    /// </summary>
    public class PyTemplate : Glue.Lib.Compilation.AspTemplateCompiler
    {
        private string content; // unparsed template string
        private string source; // generated python source code for template

        public Dictionary<string, object> Variables = new Dictionary<string, object>();
        //public Dictionary<string, Delegate> methods = new Dictionary<string, Delegate>();

        public PyTemplate()
            : this(null)
        {
        }

        public PyTemplate(string path)
        {
            this.Language = "python";
            this.TypeName = "PyTemplateClass";
            this.FileName = path;
        }

        public void Render(TextWriter writer)
        {
            PythonEngine engine = new PythonEngine();
            EngineModule module = engine.CreateModule("__main__", true);
            engine.SetStandardOutput(new MyStream(writer));

            if (source == null)
                Compile();

            // load assemblies.
            foreach (string assembly in this._unit.ReferencedAssemblies)
            {
                engine.LoadAssembly(System.Reflection.Assembly.LoadFrom(assembly));
            }

            engine.DefaultModule = module;
            // add variables
            foreach (string key in Variables.Keys)
                engine.Globals[key] = Variables[key];
            engine.Globals["writer"] = writer;

            engine.Execute("import System");
            engine.Execute("from clr import *");
            try
            {
                engine.Execute(source);
            }
            // In case of an error, find the find the line causing the error in the original template to
            // include it in the error message.
            catch (IronPython.Runtime.Exceptions.PythonSyntaxErrorException pe)
            {
                string sourceFile = FileName;
                int lineNumber = pe.Line;

                // To find the line number, look for pattern:
                // # ExternalSource ("c:/path...", 32)
                string[] lines = source.Split('\n');
                Regex reg = new Regex(@"#ExternalSource\(""(?<path>.*)"",(?<line>\d+)", RegexOptions.Compiled);
                
                for (int i = pe.Line - 1; i >= 0; i--)
                {
                    Match match = reg.Match(lines[i]);
                    if (match.Success)
                    {
                        sourceFile = match.Groups["path"].Value;
                        lineNumber = int.Parse(match.Groups["line"].Value);
                        break;
                    }
                }
                throw new RuntimeException(pe.Message, sourceFile, lineNumber);
            }
            engine.Execute("PyTemplateNamespace.PyTemplateClass().Render(writer)");

            //CompiledCode code = engine.Compile(source);
            //code.Execute(
        }

        public override void Compile()
        {
            // Create code unit and namespace
            _unit = new CodeCompileUnit();
            _unit.UserData["RequireVariableDeclaration"] = Settings.Explicit;
            _unit.UserData["AllowLateBound"] = !Settings.Strict;
            _namespace = new CodeNamespace("PyTemplateNamespace"); //NamespaceName);
            _unit.Namespaces.Add(_namespace);

            // Import namespaces
            foreach (string s in Settings.Imports)
                _namespace.Imports.Add(new CodeNamespaceImport(s));

            // Add assembly references
            foreach (string s in Settings.Assemblies)
                _unit.ReferencedAssemblies.Add(ResolveAssemblyPath(s));

            // Initialize statement and member collection for parsing
            _members = new CodeTypeMemberCollection();
            _statements = new CodeStatementCollection();

            // Parse directives, statements and members
            Parse(FileName);

            // Generate type
            Generate();

            //
            // Obtain compiler and parameters
            //ICodeCompiler compiler = Settings.Compilers[Language].Provider.CreateCompiler();

            StringWriter sw = new StringWriter();
            Glue.Lib.Compilation.CompilerInfo compiler = Settings.Compilers[Language];
            //IronPython.CodeDom.PythonProvider.
            Settings.Compilers[Language].Provider.GenerateCodeFromCompileUnit(_unit, sw, null);
            source = sw.ToString();

            //// And shoot
            //CompilerResults results = compiler.CompileAssemblyFromDom(Parameters, _unit);
            //if (results.NativeCompilerReturnValue != 0)
            //    throw new CompilationException(FileName, results);

            //// ADDED 2005/02/22
            //if (results.Errors.Count > 0)
            //    throw new CompilationException(FileName, results);

            //_assembly = results.CompiledAssembly;
        }

        public void Compile(string Content)
        {
            this.content = Content;
            Compile();
        }

        public static void Render(string path, Dictionary<string, object> variables, TextWriter writer)
        {
            PyTemplate template = new PyTemplate(path);
            template.Compile();
            template.Variables = variables;
            template.Render(writer);
        }

        //public static void CompileTest()
        //{
        //    // try to generate an assembly
        //    List<string> sourceFiles = new List<string>();
        //    sourceFiles.Add(Path.Combine(App.Current.BinDirectory, "source.py"));

        //    MySink sink = new MySink();

        //    IronPython.Hosting.PythonCompiler pc = new PythonCompiler(sourceFiles, "c:/Temp/python_out.dll", sink);

        //    pc.Compile();

        //    foreach (string Error in sink.Errors)
        //    {
        //        Console.Out.WriteLine(Error);
        //    }

            

        //    //System.Reflection.Emit.AssemblyBuilder ab = new System.Reflection.Emit.AssemblyBuilder();
        //    //IronPython.Compiler.gen
        //}
    }

    public class MySink : IronPython.Hosting.CompilerSink
    {
        public List<string> Errors = new List<string>();

        public override void AddError(string path, string message, string lineText, CodeSpan location, int errorCode, Severity severity)
        {
            Errors.Add(message);
        }
    }
}
