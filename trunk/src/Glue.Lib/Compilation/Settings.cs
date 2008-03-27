using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Glue.Lib.Compilation
{
    /// <summary>
    /// Compiler settings.
    /// </summary>
    public class Settings
	{
        // Static members

        static public Settings Current
        {
            get { return (Settings)Configuration.Get("compilation", typeof(Settings)); }
        }
		
        // Instance members

        bool _debug = false;
        string _defaultLanguage = "c#";
        bool _explicit = true;
        bool _strict = false;
        string _tempDirectory;
        bool _assembliesInBin = false;
        bool _keepTempFiles = false;
        bool _generateInMemory = false;
        CompilerInfoCollection _compilers;
        StringCollection _imports;
        StringCollection _assemblies;

        /// <summary>
        /// Include debug information in compiled assemblies.
        /// </summary>
        public bool Debug 
        {
            get { return _debug; }
        }

        /// <summary>
        /// Default language if none is specified.
        /// </summary>
        public string DefaultLanguage 
        {
            get { return _defaultLanguage; }
        }

        /// <summary>
        /// Use explicit typing, when applicable (VB.NET for example).
        /// </summary>
        public bool Explicit 
        {
            get { return _explicit; }
        }

        /// <summary>
        /// Keep temporary files used during compilation on disk. Can be
        /// useful for debugging purposes.
        /// </summary>
        public bool KeepTempFiles
        {
            get { return _keepTempFiles; }
        }

        /// <summary>
        /// Strict compilation.
        /// </summary>
        public bool Strict 
        {
            get { return _strict; }
        }

        /// <summary>
        /// Generate in-memory.
        /// </summary>
        public bool GenerateInMemory
        {
            get { return _generateInMemory; }
        }

        /// <summary>
        /// Individual compiler information.
        /// </summary>
        public CompilerInfoCollection Compilers 
        {
            get { return _compilers; }
        }

        /// <summary>
        /// Assemblies to reference.
        /// </summary>
        public StringCollection Assemblies 
        {
            get { return _assemblies; }
        }

        /// <summary>
        /// Reference all assemblies found in the /bin folder of the current
        /// application.
        /// </summary>
        public bool AssembliesInBin 
        {
            get { return _assembliesInBin; }
        }

        /// <summary>
        /// Default namespaces to import.
        /// </summary>
        public StringCollection Imports
        {
            get { return _imports; }
        }

        /// <summary>
        /// Obtains compiler information based on given filename.
        /// </summary>
        public CompilerInfo GetCompilerInfoFromFileName(string filename)
        {
            return Compilers.FindExtension(Path.GetExtension(filename));
        }
        
        /// <summary>
        /// Default constructor, reads settings from machine.config in the
        /// .NET Framework directory.
        /// </summary>
        protected Settings()
        {
            _compilers = new CompilerInfoCollection();
            _assemblies = new StringCollection();
            _imports = new StringCollection();
            _defaultLanguage = "c#";
            _tempDirectory = System.IO.Path.GetTempPath();

            // Load machine settings
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
<compilation debug='false' explicit='true' defaultLanguage='c#'>
    <compilers>
        <compiler language='c#;cs;csharp' extension='.cs' type='Microsoft.CSharp.CSharpCodeProvider, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' warningLevel='1'/>
        <compiler language='vb;vbs;visualbasic;vbscript' extension='.vb' type='Microsoft.VisualBasic.VBCodeProvider, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'/>
        <compiler language='js;jscript;javascript' extension='.js' type='Microsoft.JScript.JScriptCodeProvider, Microsoft.JScript, Version=7.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        <!--
        <compiler language='VJ#;VJS;VJSharp' extension='.jsl' type='Microsoft.VJSharp.VJSharpCodeProvider, VJSharpCodeProvider, Version=7.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        -->
    </compilers>
    <assemblies>
        <add assembly='mscorlib'/>
        <add assembly='System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'/>
        <add assembly='System.Web, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        <add assembly='System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'/>
        <add assembly='System.Web.Services, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        <add assembly='System.Xml, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'/>
        <add assembly='System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        <add assembly='System.EnterpriseServices, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        <add assembly='System.Web.Mobile, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'/>
        <add assembly='*'/>
    </assemblies>
</compilation>"
);
            Initialize(doc.SelectSingleNode("/compilation"));
            // doc.Load(Path.Combine(Path.GetDirectoryName(typeof(int).Assembly.Location), "config\\machine.config"));
            // 1.0 and 1.1
            // Initialize(doc.SelectSingleNode("/configuration/system.web/compilation")); 
            // 2.0
            // Initialize(doc.SelectSingleNode("/configuration/system.codedom")); 
        }

        /// <summary>
        /// Constructor to be called from Config. Reads settings from machine.config 
        /// in the .NET Framework directory and overrides with custom settings defined
        /// in a config key.
        /// </summary>
        protected Settings(XmlNode node) : this()
        {
            // Override default settings with our own
            Initialize(node);
        }

        void Initialize(XmlNode node)
        {
            if (node == null)
                return;

            _debug = Configuration.GetAttrBool(node, "debug", _debug);
            _explicit = Configuration.GetAttrBool(node, "explicit", _explicit);
            _strict = Configuration.GetAttrBool(node, "strict", _strict);
            _tempDirectory = Configuration.GetAttr(node, "tempDirectory", _tempDirectory);
            _defaultLanguage = Configuration.GetAttr(node, "defaultLanguage", _defaultLanguage);
            _keepTempFiles = Configuration.GetAttrBool(node, "keepTempFiles", _keepTempFiles);
            _assembliesInBin = Configuration.GetAttrBool(node, "assembliesInBin", _assembliesInBin);
            _generateInMemory = !_debug; // Configuration.GetAttrBool(node, "generateInMemory", _generateInMemory);

            ReadCompilers(node.SelectSingleNode("compilers"));
            ReadAssemblies(node.SelectSingleNode("assemblies"));
            ReadImports(node.SelectSingleNode("imports"));
        }

        void ReadCompilers(XmlNode node)
        {
            if (node == null)
                return;

            foreach (XmlNode child in node.SelectNodes("compiler")) 
            {
                CompilerInfo info = new CompilerInfo(child);
                _compilers[info.Languages] = info;
            }
        }

        void ReadAssemblies(XmlNode node)
        {
            if (node == null)
                return;
            
            foreach (XmlNode child in node.SelectNodes("*")) 
            {
                if (child.Name == "clear") 
                {
                    _assemblies.Clear();
                    _assembliesInBin = false;
                    continue;
                }

                string aname = Configuration.GetAttr(child, "assembly");
                if (child.Name == "add") 
                {
                    if (aname == "*") 
                    {
                        _assembliesInBin = true;
                        continue;
                    }

                    aname = ShortAsmName(aname);
                    if (!_assemblies.Contains(aname))
                        _assemblies.Add(aname);

                    continue;
                }

                if (child.Name == "remove") 
                {
                    if (aname == "*") 
                    {
                        _assembliesInBin = false;
                        continue;
                    }
                    aname = ShortAsmName(aname);
                    _assemblies.Remove(aname);
                    continue;
                }

                throw new ConfigurationException("Unexpected element " + child.Name, child);
            }
        }

        void ReadImports(XmlNode node)
        {
            if (node == null)
                return;

            foreach (XmlNode child in node.SelectNodes("*")) 
            {
                if (child.Name == "clear") 
                {
                    _imports.Clear();
                    continue;
                }

                string aname = Configuration.GetAttr(child, "namespace");
                if (child.Name == "add") 
                {
                    if (!_imports.Contains(aname))
                        _imports.Add(aname);

                    continue;
                }

                if (child.Name == "remove") 
                {
                    _imports.Remove(aname);
                    continue;
                }

                throw new ConfigurationException("Unexpected element " + child.Name, child);
            }
        }

        string ShortAsmName(string longname)
        {
            int i = longname.IndexOf(',');
            string s;
            if (i < 0)
                s = longname;
            else
                s = longname.Substring(0, i); 
            if (!s.EndsWith(".exe") && !s.EndsWith(".dll"))
                s += ".dll";
            return s;
        }
    }
}
