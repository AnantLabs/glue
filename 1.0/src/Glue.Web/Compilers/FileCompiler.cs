using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Caching;
using Glue.Lib.Compilation;

namespace Glue.Web
{
	/// <summary>
	/// Summary description for FileCompiler.
    /// TODO: Remove this class?
	/// </summary>
	public class FileCompiler : Glue.Lib.Compilation.FileCompiler
	{
        public static object GetCompiledInstance(string virtualPath)
        {
            return Activator.CreateInstance(GetCompiledType(virtualPath));
        }

        public static Type GetCompiledType(string virtualPath) 
        {
            string path = App.Current.MapPath(virtualPath);
            string key  = "#Glue.Web#Source#" + path + "#";
            Type  type = (Type)App.Current.Cache[key];
            if (type == null)
            {
                FileCompiler compiler = new FileCompiler();
                compiler.Path = path;
                compiler.Compile();
                type = compiler.CompiledAssembly.GetType("Glue.Web.Sample1.Views.Class1");
                App.Current.Cache.Insert(key, type, new CacheDependency(App.Current.MapPath(virtualPath)));
            }
            return type;
        }

        protected override string MapPath(string path)
        {
            return App.Current.MapPath(path);
        }
    }
}
