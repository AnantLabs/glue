using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Text;
using Glue.Lib;

namespace Glue.Data.Mapping
{
	public abstract class AccessorHelper
	{
        public static Type GenerateAccessor(Type type, string references, string dbprefix, string parmchar)
        {
            Entity info = Entity.Obtain(type);
            string typeName = dbprefix + type.FullName.Replace('.', '_');
            string namespaceName = "Glue_Data_Mapping_Generated";
            StringWriter code = new StringWriter();
            code.WriteLine("using System;");
            code.WriteLine("using System.Collections;");
            code.WriteLine("using System.Data;");
            code.WriteLine("using {0};", references);
            code.WriteLine("using Glue.Lib;");
            code.WriteLine("using Glue.Data;");
            code.WriteLine("using Glue.Data.Mapping;");
            code.WriteLine("namespace " + namespaceName);
            code.WriteLine("{");
            code.WriteLine("  public class " + typeName + " : " + typeof(Accessor).FullName);
            code.WriteLine("  {");
            code.WriteLine("    public " + typeName + "(IDataProvider provider, Type type) : base(provider, type) {} ");
            code.WriteLine("    public override void InitFromReaderFixed(object obj, IDataReader reader, int index)");
            code.WriteLine("    {");
            code.WriteLine("      " + type.FullName + " instance = obj as " + type.FullName + ";");
            foreach (EntityMember m in info.AllMembers)
                GenerateInitFromReaderFixed(code, m, "");
            code.WriteLine("    }");
            code.WriteLine("    public override void InitFromReaderDynamic(object obj, IDataReader reader, IDictionary ordinals)");
            code.WriteLine("    {");
            code.WriteLine("      " + type.FullName + " instance = obj as " + type.FullName + ";");
            code.WriteLine("      object ordinal;");
            foreach (EntityMember m in info.AllMembers)
                GenerateInitFromReaderDynamic(code, m, "");
            code.WriteLine("    }");
            code.WriteLine("    public override void AddParametersToCommandFixed(object obj, IDbCommand command)");
            code.WriteLine("    {");
            code.WriteLine("      " + type.FullName + " instance = obj as " + type.FullName + ";");
            code.WriteLine("      " + dbprefix + "ParameterCollection parameters = (" + dbprefix + "ParameterCollection)command.Parameters;");
            foreach (EntityMember m in info.KeyMembers)
                GenerateAddParameter(code, m, "", dbprefix, parmchar);
            foreach (EntityMember m in EntityMemberList.Subtract(info.AllMembers, info.KeyMembers, info.AutoMembers))
                GenerateAddParameter(code, m, "", dbprefix, parmchar);
            code.WriteLine("    }");
            code.WriteLine("  }");
            code.WriteLine("}");
            Log.Debug(code.ToString());
            Glue.Lib.Compilation.SourceCompiler compiler = new Glue.Lib.Compilation.SourceCompiler();
            compiler.Language = "C#";
            compiler.Source = code.ToString();
            try
            {
                compiler.Compile();
            }
            catch (Glue.Lib.Compilation.CompilationException e)
            {
                Log.Error(e.ErrorMessage);
                throw;
            }
            return compiler.CompiledAssembly.GetType(namespaceName + "." + typeName);
        }

        static void GenerateInitFromReaderFixed(TextWriter code, EntityMember member, string prefix)
        {
            if (member.Aggregated)
            {
                bool readOnly = (member.Field != null && member.Field.IsInitOnly || member.Property != null && member.Property.CanWrite);
                if (!readOnly)
                    code.WriteLine("      instance." + prefix + member.Name + " = new " + member.Type.FullName + "(); // aggregated");
                foreach (EntityMember child in member.Children)
                    GenerateInitFromReaderFixed(code, child, member.Name + ".");
            }
            else if (member.Foreign)
            {
                Entity foreign = Entity.Obtain(member.Type);
                code.WriteLine("      instance." + prefix + member.Name + " = (" + member.Type.FullName + ")Provider.Find(");
                code.Write("        typeof(" + foreign.Type.FullName + ")");
                foreach (EntityMember fk in foreign.KeyMembers)
                {
                    code.WriteLine(",");
                    code.Write("        " + GetFromDataExpression(fk, "reader[index++]"));
                }
                code.WriteLine();
                code.WriteLine("      );");
            }
            else
            {
                code.WriteLine("      instance." + prefix + member.Name + " = " + GetFromDataExpression(member, "reader[index++]") + ";");
            }
        }

        static void GenerateInitFromReaderDynamic(TextWriter code, EntityMember member, string prefix)
        {
            if (member.Aggregated)
            {
                bool readOnly = (member.Field != null && member.Field.IsInitOnly || member.Property != null && member.Property.CanWrite);
                if (!readOnly)
                    code.WriteLine("      instance." + prefix + member.Name + " = new " + member.Type.FullName + "();");
                foreach (EntityMember child in member.Children)
                    GenerateInitFromReaderDynamic(code, child, member.Name + ".");
            }
            else if (member.Foreign)
            {
                Entity foreign = Entity.Obtain(member.Type);
                code.WriteLine("      ordinal = ordinals[\"" + member.Column.Name + "\"];");
                code.WriteLine("      if (ordinal != null)");
                code.WriteLine("        instance." + prefix + member.Name + " = (" + member.Type.FullName + ")Provider.Find(");
                code.WriteLine("          typeof(" + foreign.Type.FullName + "), ");
                code.WriteLine("          " + GetFromDataExpression(foreign.KeyMembers[0], "reader[(int)ordinal]"));
                code.WriteLine("        );");
            }
            else
            {
                code.WriteLine("      ordinal = ordinals[\"" + member.Column.Name + "\"];");
                code.WriteLine("      if (ordinal != null)");
                code.WriteLine("        instance." + prefix + member.Name + " = " + GetFromDataExpression(member, "reader[(int)ordinal]") + ";");
            }
        }

        static void GenerateAddParameter(TextWriter code, EntityMember member, string prefix, string dbprefix, string parmchar)
        {
            if (member.Aggregated)
            {
                foreach (EntityMember child in member.Children)
                    GenerateAddParameter(code, child, member.Name + ".", dbprefix, parmchar);
            }
            else if (member.Foreign)
            {
                Entity foreign = Entity.Obtain(member.Type);
                code.WriteLine("      if (instance." + prefix + member.Name + " == null)");
                code.WriteLine("        parameters.Add(new " + dbprefix + "Parameter(\"" + parmchar + member.Column.Name + "\", DBNull.Value));");
                code.WriteLine("      else ");
                code.Write("        parameters.Add( new " + dbprefix + "Parameter(\"" + parmchar + member.Column.Name + "\", ");
                code.WriteLine("instance." + prefix + member.Name + "." + foreign.KeyMembers[0].Name + "));");
            }
            else
            {
                code.Write("      parameters.Add(new " + dbprefix + "Parameter(\"" + parmchar + member.Column.Name + "\", ");
                code.Write(GetIntoDataExpression(member, "instance." + prefix + member.Name));
                code.WriteLine("));");
            }
        }

        static string GetFromDataExpression(EntityMember member, string sourceExpression)
        {
            if (member.Column.GenericNullable)
                return "NullConvert.To<" + member.Column.Type.Name + "?>(" + sourceExpression + ")";
            else if (member.Column.Nullable)
                if (member.Column.ConventionalNullValue != null)
                    return "NullConvert.To" + member.Type.Name + "(" + sourceExpression + ", " + member.Column.ConventionalNullValue + ")";
                else
                    return "NullConvert.To" + member.Type.Name + "(" + sourceExpression + ")";

            if (member.Type == typeof(Guid))
                return "(Guid)(" + sourceExpression + ")";
            else if (member.Type.IsEnum)
                return "(" + member.Type.FullName + ")" + "Convert.ToInt32(" + sourceExpression + ")";
            else
                return "Convert.To" + member.Type.Name + "(" + sourceExpression + ")";
        }

        static string GetIntoDataExpression(EntityMember member, string sourceExpression)
        {
            if (member.Column.MaxLength > 0)
                sourceExpression = "NullConvert.Truncate(" + sourceExpression + ", " + member.Column.MaxLength + ")";

            if (member.Column.GenericNullable)
                return "((" + sourceExpression + ") == null ? DBNull.Value : (object)(" + sourceExpression + "))";
            else if (member.Column.Nullable)
                if (member.Column.ConventionalNullValue != null)
                    return "NullConvert.From(" + sourceExpression + ", " + member.Column.ConventionalNullValue + ")";
                else
                    return "NullConvert.From(" + sourceExpression + ")";
            else
                return sourceExpression;
        }
    }
}
