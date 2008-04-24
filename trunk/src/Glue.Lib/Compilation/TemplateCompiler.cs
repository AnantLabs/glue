using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Glue.Lib;

namespace Glue.Lib.Compilation
{
    /// <summary>
    /// Summary description for TemplateCompiler 
    /// </summary>
    public abstract class TemplateCompiler : BaseCompiler
    {
        // These are available during the parsing.
        protected CodeCompileUnit           _unit;
        protected CodeNamespace             _namespace;
        protected CodeStatementCollection   _statements;
        protected CodeTypeMemberCollection  _members;

        // These will be initialized during Generate
        protected CodeTypeDeclaration       _type;
        protected CodeConstructor           _constructor;
        protected CodeMemberMethod          _renderMethod;

        protected string                    _namespaceName = "";
        protected string                    _typeName = "";
        protected string                    _baseTypeName = "";
        protected string                    _fileName = "";

        public Type CompiledType
        {
            get { return this._assembly.GetType(NamespaceName + "." + TypeName, true, false); }
        }

        public string NamespaceName
        {
            get { return _namespaceName; }
            set { _namespaceName = value.Replace(" ", "").Replace(".", "_"); }
        }

        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value.Replace(" ", "").Replace(".", "_"); }
        }

        public string BaseTypeName
        {
            get { return _baseTypeName; }
            set { _baseTypeName = value; }
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public override void Compile()
        {
            // Create code unit and namespace
            _unit = new CodeCompileUnit();
            _unit.UserData["RequireVariableDeclaration"] = Settings.Explicit;
            _unit.UserData["AllowLateBound"] = !Settings.Strict;
            _namespace = new CodeNamespace(NamespaceName);
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

            // Obtain compiler and parameters
            ICodeCompiler compiler = Settings.Compilers[Language].Provider.CreateCompiler();
                        
            // And shoot
            CompilerResults results = compiler.CompileAssemblyFromDom(Parameters, _unit);
            if (results.NativeCompilerReturnValue != 0)
                throw new CompilationException(FileName, results);
            
            // ADDED 2005/02/22
            if (results.Errors.Count > 0)
                throw new CompilationException(FileName, results);

            _assembly = results.CompiledAssembly;
        }

        protected abstract void Parse(string path);

        protected virtual void Generate()
        {
            // Generate class declaration
            _type = new CodeTypeDeclaration(TypeName);
            if (BaseTypeName != null && BaseTypeName != "")
                _type.BaseTypes.Add(BaseTypeName);
            _type.TypeAttributes = TypeAttributes.Public;
            _namespace.Types.Add(_type);

            // Generate render method
            _renderMethod = new CodeMemberMethod();
            _renderMethod.Name = "Render";
            _renderMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Overloaded;
            _renderMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(TextWriter), "writer"));
            
            // Add statements
            _renderMethod.Statements.AddRange(_statements);

            // Add render method and other members
            _type.Members.Add(_renderMethod);
            _type.Members.AddRange(_members);
        }
    }
}
