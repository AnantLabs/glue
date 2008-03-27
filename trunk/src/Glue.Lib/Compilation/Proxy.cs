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
	/// Summary description for Proxy.
	/// </summary>
	public class Proxy
	{
        public static void GenerateFields(CodeTypeMemberCollection members, Type type, CodeExpression target, StringCollection excludes)
        {
            foreach (FieldInfo f in type.GetFields())
            {
                if (f.IsPublic && !excludes.Contains(f.DeclaringType.FullName) &&  !excludes.Contains(f.Name))
                {
                    CodeMemberProperty cp = new CodeMemberProperty();
                    cp.Name = f.Name;
                    cp.Type = new CodeTypeReference(f.FieldType);
                    cp.GetStatements.Add(
                        new CodeMethodReturnStatement(
                            new CodeFieldReferenceExpression(target, f.Name)));
                    cp.SetStatements.Add(
                        new CodeAssignStatement(
                            new CodeFieldReferenceExpression(target, f.Name), 
                            new CodeArgumentReferenceExpression("value")));
                    members.Add(cp);
                    excludes.Add(f.Name);
                }
            }
        }
        
        public static void GenerateProperties(CodeTypeMemberCollection members, Type type, CodeExpression target, StringCollection excludes)
        {
            foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!excludes.Contains(p.DeclaringType.FullName) && !excludes.Contains(p.Name))
                {
                    CodeMemberProperty cp = new CodeMemberProperty();
                    cp.Name = p.Name;
                    cp.Type = new CodeTypeReference(p.PropertyType);
                    foreach (ParameterInfo parm in p.GetIndexParameters())
                    {
                        CodeParameterDeclarationExpression cparm = new CodeParameterDeclarationExpression(parm.ParameterType, parm.Name);
                        cp.Parameters.Add(cparm);
                    }
                    if (p.CanRead)
                    {
                        if (cp.Parameters.Count > 0)
                        {
                            CodeArgumentReferenceExpression[] indices = new CodeArgumentReferenceExpression[cp.Parameters.Count];
                            for (int i = 0; i < cp.Parameters.Count; i++)
                                indices[i] = new CodeArgumentReferenceExpression(cp.Parameters[i].Name);
                            cp.GetStatements.Add(
                                new CodeMethodReturnStatement(
                                new CodeIndexerExpression(
                                new CodePropertyReferenceExpression(target, p.Name),
                                indices)));
                        }
                        else
                        {
                            CodeArgumentReferenceExpression[] indices = new CodeArgumentReferenceExpression[cp.Parameters.Count];
                            for (int i = 0; i < cp.Parameters.Count; i++)
                                indices[i] = new CodeArgumentReferenceExpression(cp.Parameters[i].Name);
                            cp.GetStatements.Add(
                                new CodeMethodReturnStatement(
                                new CodePropertyReferenceExpression(target, p.Name)));
                        }
                    }
                    if (p.CanWrite)
                    {
                    }
                    members.Add(cp);
                    excludes.Add(p.Name);
                }
            }
        }

        public static void GenerateMethods(CodeTypeMemberCollection members, Type type, CodeExpression target, StringCollection excludes)
        {
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (m.IsSpecialName)
                    continue;
                if (excludes.Contains(m.DeclaringType.FullName))
                    continue;
                string signature = m.Name;
                ParameterInfo[] parms = m.GetParameters();
                foreach (ParameterInfo parm in parms)
                    signature += "#" + parm.ParameterType.FullName;
                if (excludes.Contains(signature))
                    continue;
                
                CodeMemberMethod cm = new CodeMemberMethod();
                cm.Name = m.Name;
                if ((m.Attributes & MethodAttributes.Static) != 0)
                    cm.Attributes |= MemberAttributes.Static;
                cm.ReturnType = new CodeTypeReference(m.ReturnType);
                foreach (ParameterInfo parm in parms)
                    cm.Parameters.Add(new CodeParameterDeclarationExpression(parm.ParameterType, parm.Name));

                CodeMethodInvokeExpression invoke;
                if ((m.Attributes & MethodAttributes.Static) != 0)
                    invoke = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(type), cm.Name);
                else
                    invoke = new CodeMethodInvokeExpression(target, cm.Name);
                foreach (ParameterInfo parm in parms)
                    invoke.Parameters.Add(new CodeArgumentReferenceExpression(parm.Name));

                if (m.ReturnType == typeof(void))
                    cm.Statements.Add(invoke);
                else
                    cm.Statements.Add(new CodeMethodReturnStatement(invoke));
                members.Add(cm);
                excludes.Add(signature);
            }
        }
    }
}
