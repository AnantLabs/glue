using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using System.Web.Caching;
using Glue.Lib;
using Glue.Lib.Compilation;

namespace Glue.Web
{
    /// <summary>
	/// Summary description for ViewCompiler
	/// </summary>
	public class ViewCompiler : AspTemplateCompiler
	{
        public static View GetCompiledInstance(Controller controller, string virtualPath)
        {
            Type controllerType = controller == null ? typeof(Controller) : controller.GetType();
            Type viewType = GetCompiledType(controllerType, virtualPath);
            return (View)Activator.CreateInstance(viewType, new object[] {controller});
        }

        /// <summary>
        /// TODO: Check for on-disk dlls
        /// </summary>
        public static Type GetCompiledType(Type controllerType, string virtualPath) 
        {
            string path = App.Current.MapPath(virtualPath);
            string key  = "#Glue.Web#View#" + controllerType.FullName + "#" + path + "#";
            Type   type = (Type)App.Current.Cache[key];
            if (type == null)
            {
                Log.Debug("Begin Compiling: " + virtualPath);
                ViewCompiler compiler = new ViewCompiler();
                compiler.FileName = path;
                compiler.NamespaceName = "Glue_Web_Views_Generated";
                compiler.BaseTypeName = App.Current.BaseViewType.FullName;
                compiler.TypeName = Path.GetFileNameWithoutExtension(StringHelper.StripNonWordChars(virtualPath, '_'));
                compiler.ControllerType = controllerType;
                compiler.Compile();
                type = compiler.CompiledType;
                App.Current.Cache.Insert(key, type, new CacheDependency(path));
                Log.Debug("End Compiling: " + virtualPath);
            }
            return type;
        }

        protected Type ControllerType;

        protected override string MapPath(string path)
        {
            return App.Current.MapPath(path);
        }

        protected override void ParseDirective(string directive, StringDictionary attributes)
        {
            if (directive == "view" || directive == "page")
            {
                if (attributes["language"] != null)
                    Language = attributes["language"];
                if (attributes["inherits"] != null)
                    BaseTypeName = attributes["inherits"];
            }
            else
            {
                base.ParseDirective(directive, attributes);
            }
        }

        protected override void Generate()
        {
            // To be sure add this assembly
            _unit.ReferencedAssemblies.Add(typeof(ViewCompiler).Assembly.Location);
            _unit.ReferencedAssemblies.Add(this.GetType().Assembly.Location);
            _unit.ReferencedAssemblies.Add(ControllerType.Assembly.Location);
            
            // Generate the class
            base.Generate();

            // Generate additional fields
            CodeMemberField fld = new CodeMemberField(ControllerType, "Ctl");
            fld.Attributes = MemberAttributes.Public;
            _type.Members.Add(fld);

            StringCollection excludes = new StringCollection();
            excludes.Add("System.Object");
            excludes.Add("Glue.Web.Controller");
            CodeFieldReferenceExpression thisCtl = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Ctl");
            Proxy.GenerateFields(_type.Members, ControllerType, thisCtl, excludes);
            Proxy.GenerateProperties(_type.Members, ControllerType, thisCtl, excludes);
            Proxy.GenerateMethods(_type.Members, ControllerType, thisCtl, excludes);

            // Generate custom constructor
            _constructor = new CodeConstructor();
            _constructor.Attributes = MemberAttributes.Public;
            _constructor.Parameters.Add(new CodeParameterDeclarationExpression(ControllerType, "acontroller"));
            _constructor.BaseConstructorArgs.Add(
                new CodeArgumentReferenceExpression("acontroller")
                );
            _constructor.Statements.Add(
                new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Ctl"),
                new CodeVariableReferenceExpression("acontroller")
                )
                );
            _type.Members.Add(_constructor);
        }
    }
}
