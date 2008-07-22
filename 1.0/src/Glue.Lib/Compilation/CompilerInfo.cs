using System;
using System.Collections;
using System.CodeDom.Compiler;
using System.Xml;
using Glue.Lib;

namespace Glue.Lib.Compilation
{
    /// <summary>
    /// Stores compiler information. Used by Glue.Lib.Compilation.Settings.
    /// </summary>
    public class CompilerInfo
    {
        public readonly Type ProviderType;
        public readonly string Languages;
        public readonly string Extension;
        public readonly int WarningLevel;
        public readonly string CompilerOptions;
        private CodeDomProvider provider;
        
        internal CompilerInfo(XmlNode node)
        {
            Languages       = Configuration.GetAttr(node, "language");
            Extension       = Configuration.GetAttr(node, "extension");
            ProviderType    = Configuration.GetAttrType(node, "type");
            CompilerOptions = Configuration.GetAttr(node, "compilerOptions", "", true);
            WarningLevel    = Configuration.GetAttrUInt(node, "warningLevel", 0);
        }

        public CodeDomProvider Provider
        {
            get 
            {
                if (provider == null)
                    provider = (CodeDomProvider)Activator.CreateInstance(ProviderType);
                return provider;
            }
        }

        public override string ToString ()
        {
            return "Languages: " + Languages + "\n" +
                "Extension: " + Extension + "\n" +
                "Type: " + ProviderType + "\n" +
                "WarningLevel: " + WarningLevel + "\n" +
                "CompilerOptions: " + CompilerOptions + "\n";
        }
    }
}

