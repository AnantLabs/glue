//----------------------------------------------------------------------------
// Copyright (C) 2004-2005 Electric Dream Factory. All rights reserved.
// http://www.edf.nl
//
// You must not remove this notice, or any other, from this software.
//----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Edf.Lib.Text.Template.AST;

namespace Edf.Lib.Text.Template
{
	/// <summary>
	/// DirectCompiler.
	/// </summary>
    public class GeneratorCompiler
    {
        IDictionary _locals = null;

        public GeneratorCompiler()
        {
        }

        public Type Compile(Unit unit, Type baseClass, bool memory, bool debug)
        {
            return null; // compiledType;
        }

        private void EmitBlock(ElementList elements, TextWriter output)
        {
            foreach (Element element in elements)
            {
                if (element is MethodDefinition)
                {
                    EmitMethod(element as MethodDefinition, output);
                }
                /*
                else if (element is TemplateDefinition)
                {
                    EmitTemplate(element as TemplateDefinition, definitions);
                }
                */
                else if (element is PutStatement)
                {
                    EmitPrint((element as PutStatement).Expression, output);
                }
                else if (element is HaltStatement)
                {
                    // TODO: Debugger.Break;
                }
                else if (element is Expression)
                {
                    EmitPrint(element as Expression, output);
                }
                else if (element is ForStatement)
                {
                    EmitForStatement(element as ForStatement, output);
                }
                else if (element is PutStatement)
                {
                    EmitPrint((element as PutStatement).Expression, output);
                }
                else if (element is IfStatement)
                {
                    EmitIfStatement(element as IfStatement, output);
                }
                else if (element is AssignStatement)
                {
                    EmitAssignStatement(element as AssignStatement, output);
                }
                /*
                else if (element is ApplyStatement)
                {
                    EmitApplyStatement(element as ApplyStatement, statements);
                }
                */
                else
                {
                    throw new ArgumentException("Unknown element type: " + element);
                }
            }
        }

        private void EmitMethod(MethodDefinition definition, TextWriter output)
        {
            Type[] parameters = new Type[definition.Parameters.Count];
            for (int i = 0; i < definition.Parameters.Count; i++)
                parameters[i] = typeof(object);
            
            // TODO: _scope.Push(_locals);
            _locals = CollectionsUtil.CreateCaseInsensitiveHashtable();

            for (int i = 0; i < definition.Parameters.Count; i++)
            {
            }

            // TODO: _locals = (Hashtable)_scope.Pop();
        }

        private void EmitIfStatement(IfStatement statement, TextWriter output)
        {
            if (statement.False.Count == 0)
            {
                EmitBlock(statement.True, output);
            }
            else
            {
                EmitBlock(statement.True, output);
                EmitBlock(statement.False, output);
            }
        }

        private void EmitWhileStatement(IfStatement statement, TextWriter output)
        {
            if (statement.False.Count == 0)
            {
                EmitExpression(statement.Test, output);
                EmitBlock(statement.True, output);
            }
            else
            {
                EmitExpression(statement.Test, output);
                EmitBlock(statement.True, output);
                EmitExpression(statement.Test, output);
                EmitBlock(statement.False, output);
            }
        }

        private void EmitAssignStatement(AssignStatement statement, TextWriter output)
        {
            EmitExpression(statement.Expression, output);
        }

        private void EmitForStatement(ForStatement statement, TextWriter output)
        {
            // enumerator = GetEnumerator(Container)
            EmitExpression(statement.Container, output);

            // index = -1;

            // goto test
            // loop:
            
            //     object iter = enumerator.Current;
            
            // Separator statements
            if (statement.Sep.Count > 0)
            {
                EmitBlock(statement.Sep, output);
            }

            if (statement.Alt.Count > 0)
            {
                // if (index & 2 != 0)
                //     goto alt
            }
            // inner:
            //   inner statements
            EmitBlock(statement.Inner, output);
            if (statement.Alt.Count > 0)
            {
                // goto test
                // alt:
                //   Alt statements
                EmitBlock(statement.Alt, output);
            }
            
            // test:
            //      index++;
            //     if (enumerator.MoveNext())
            //         goto loop;
        }

        private void EmitPrint(Expression expression, TextWriter output)
        {
            // if inside_render_method: output.Emit(OpCodes.Ldarg_1);
            EmitExpression(expression, output);
        }

        private void EmitExpression(Expression expression, TextWriter output)
        {
            if (expression is ReferenceExpression)
            {
                ReferenceExpression expr = expression as ReferenceExpression;
                ArrayList steps = Resolve(expr);
                
                for (int i = 0; i < steps.Count; i++)
                {
                    if (steps[i] is LocalBuilder)
                    {
                        output.Emit(OpCodes.Ldloc, steps[i] as LocalBuilder);
                    }
                    else if (steps[i] is FieldInfo)
                    {
                        FieldInfo field = steps[i] as FieldInfo;
                        if (i == 0 && !field.IsStatic)
                            output.Emit(OpCodes.Ldarg_0);
                        if (!field.IsStatic)
                            output.Emit(OpCodes.Ldfld, field);
                        else
                            output.Emit(OpCodes.Ldsfld, field);
                        if (field.FieldType.IsValueType)
                            output.Emit(OpCodes.Box, field.FieldType);
                    }
                    else if (steps[i] is PropertyInfo)
                    {
                        PropertyInfo property = steps[i] as PropertyInfo;
                        MethodInfo getter = property.GetGetMethod();
                        if (i == 0 && !getter.IsStatic)
                            output.Emit(OpCodes.Ldarg_0);
                        if (getter.IsVirtual)
                            output.Emit(OpCodes.Callvirt, getter);
                        else
                            output.Emit(OpCodes.Call, getter);
                        if (property.PropertyType.IsValueType)
                            output.Emit(OpCodes.Box, property.PropertyType);
                    }
                    else if (steps[i] is MethodInfo)
                    {
                        MethodInfo method = steps[i] as MethodInfo;
                        if (method.GetParameters().Length == 0)
                        {
                            if (i == 0 && !method.IsStatic)
                                output.Emit(OpCodes.Ldarg_0);
                            if (method.IsVirtual)
                                output.Emit(OpCodes.Callvirt, method);
                            else
                                output.Emit(OpCodes.Call, method);
                            if (method.ReturnType.IsValueType)
                                output.Emit(OpCodes.Box, method.ReturnType);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                if (method.IsStatic)
                                {
                                    output.Emit(OpCodes.Ldtoken, method.DeclaringType);
                                    output.Emit(OpCodes.Call, _typeFromHandle);
                                }
                                else
                                    output.Emit(OpCodes.Ldarg_0);
                            }
                            
                            if (expr.Arguments.Count > 0 && expr.Arguments[0] is NamedArgumentExpression)
                            {
                                output.Emit(OpCodes.Ldstr, method.Name);
                                output.Emit(OpCodes.Ldc_I4, expr.Arguments.Count * 2);
                                output.Emit(OpCodes.Newarr, typeof(object));
                                for (int j = 0; j < expr.Arguments.Count; j++)
                                {
                                    output.Emit(OpCodes.Dup);
                                    output.Emit(OpCodes.Ldc_I4, j*2);
                                    output.Emit(OpCodes.Ldstr, (expr.Arguments[j] as NamedArgumentExpression).Name);
                                    output.Emit(OpCodes.Stelem_Ref);

                                    output.Emit(OpCodes.Dup);
                                    output.Emit(OpCodes.Ldc_I4, j*2+1);
                                    EmitExpression((expr.Arguments[j] as NamedArgumentExpression).Expression, output);
                                    output.Emit(OpCodes.Stelem_Ref);
                                }
                                output.Emit(OpCodes.Call, _runtimeBag);
                                output.Emit(OpCodes.Call, _runtimeGetWithBag);
                            }
                            else
                            {
                                output.Emit(OpCodes.Ldstr, method.Name);
                                output.Emit(OpCodes.Ldc_I4, expr.Arguments.Count);
                                output.Emit(OpCodes.Newarr, typeof(object));
                                for (int j = 0; j < expr.Arguments.Count; j++)
                                {
                                    output.Emit(OpCodes.Dup);
                                    output.Emit(OpCodes.Ldc_I4, j);
                                    EmitExpression(expr.Arguments[j] as Expression, output);
                                    output.Emit(OpCodes.Stelem_Ref);
                                }
                                output.Emit(OpCodes.Call, _runtimeGet);
                            }
                        }
                    }
                    else if (steps[i] is string)
                    {
                        string member = (string)steps[i];
                        if (i == 0)
                            output.Emit(OpCodes.Ldarg_0);
                        
                        if (i < steps.Count - 1 || expr.Arguments.Count == 0)
                        {
                            output.Emit(OpCodes.Ldstr, member);
                            output.Emit(OpCodes.Ldnull);
                            output.Emit(OpCodes.Call, _runtimeGet);
                        }
                        else if (expr.Arguments[0] is NamedArgumentExpression)
                        {
                            output.Emit(OpCodes.Ldstr, member);
                            output.Emit(OpCodes.Ldc_I4, expr.Arguments.Count * 2);
                            output.Emit(OpCodes.Newarr, typeof(object));
                            for (int j = 0; j < expr.Arguments.Count; j++)
                            {
                                output.Emit(OpCodes.Dup);
                                output.Emit(OpCodes.Ldc_I4, j*2);
                                output.Emit(OpCodes.Ldstr, (expr.Arguments[j] as NamedArgumentExpression).Name);
                                output.Emit(OpCodes.Stelem_Ref);

                                output.Emit(OpCodes.Dup);
                                output.Emit(OpCodes.Ldc_I4, j*2+1);
                                EmitExpression((expr.Arguments[j] as NamedArgumentExpression).Expression, output);
                                output.Emit(OpCodes.Stelem_Ref);
                            }
                            output.Emit(OpCodes.Call, _runtimeBag);
                            output.Emit(OpCodes.Call, _runtimeGetWithBag);
                        }
                        else
                        {
                            output.Emit(OpCodes.Ldstr, member);
                            output.Emit(OpCodes.Ldc_I4, expr.Arguments.Count);
                            output.Emit(OpCodes.Newarr, typeof(object));
                            for (int j = 0; j < expr.Arguments.Count; j++)
                            {
                                output.Emit(OpCodes.Dup);
                                output.Emit(OpCodes.Ldc_I4, j);
                                EmitExpression(expr.Arguments[j] as Expression, output);
                                output.Emit(OpCodes.Stelem_Ref);
                            }
                            output.Emit(OpCodes.Call, _runtimeGet);
                        }
                    }
                }
            }
            else if (expression is BinaryExpression)
            {
                BinaryExpression bin = expression as BinaryExpression;

                switch (bin.Operator)
                {
                    case "+": 
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeAdd);
                        break;
                    case "-": 
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeSub);
                        break;
                    case "*": 
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeMul);
                        break;
                    case "/": 
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeDiv);
                        break;
                    case "%": 
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeMod);
                        break;
                    case "<":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeLT);
                        break;
                    case ">":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeGT);
                        break;
                    case "<=":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeLE);
                        break;
                    case ">=":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeGE);
                        break;
                    case "==":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeEQ);
                        break;
                    case "!=":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeNE);
                        break;
                    case "&&":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeAnd);
                        break;
                    case "||":
                        EmitExpression(bin.Left, output);
                        EmitExpression(bin.Right, output);
                        output.Emit(OpCodes.Call, _runtimeOr);
                        break;
                    default:
                        output.Emit(OpCodes.Ldstr, "????:" + expression);
                        break;
                }
            }
            else if (expression is PrimitiveExpression)
            {
                PrimitiveExpression primitive = expression as PrimitiveExpression;
                if (primitive.Value is String)
                {
                    output.Emit(OpCodes.Ldstr, (string)primitive.Value);
                }
                else if (primitive.Value is Int32)
                {
                    output.Emit(OpCodes.Ldc_I4, (Int32)primitive.Value);
                    output.Emit(OpCodes.Box, typeof(Int32));
                }
                else if (primitive.Value is Boolean)
                {
                    if ((Boolean)primitive.Value)
                        output.Emit(OpCodes.Ldc_I4_1);
                    else
                        output.Emit(OpCodes.Ldc_I4_0);
                    output.Emit(OpCodes.Box, typeof(Boolean));
                }
                else if (primitive.Value is Double)
                {
                    output.Emit(OpCodes.Ldc_R8, (Double)primitive.Value);
                    output.Emit(OpCodes.Box, typeof(Double));
                }
                else
                {
                    output.Emit(OpCodes.Ldstr, "????:" + expression);
                }
            }
            else
            {
                output.Emit(OpCodes.Ldstr, "????:" + expression);
            }
        }

        private ArrayList Resolve(ReferenceExpression expression)
        {
            ArrayList list = new ArrayList();
            
            string tail = expression.Name;
            string head = EatIdentifier(ref tail);

            LocalBuilder local = (LocalBuilder)_locals[head];
            if (local != null)
            {
                list.Add(local);
                head = EatIdentifier(ref tail);
                goto latebound;
            }

            MemberInfo info = null;
            foreach (FieldInfo field in _fields)
                if (string.Compare(field.Name, head, true) == 0)
                {
                    info = field;
                    break;
                }
            
            if (info == null)
                info = FindMember(_baseClass, head, tail.Length > 0, expression.Arguments, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (info == null)
                info = FindMember(typeof(Runtime), head, tail.Length > 0, expression.Arguments, BindingFlags.Public | BindingFlags.Static);

            if (info == null)
                for (int i = 0; i < _mixinTypes.Length; i++)
                {
                    info = FindMember(_mixinTypes[i], head, tail.Length > 0, expression.Arguments, BindingFlags.Public | BindingFlags.Instance);
                    if (info != null)
                    {
                        list.Add(_mixinFields[i]);
                        break;
                    }
                    info = FindMember(_mixinTypes[i], head, tail.Length > 0, expression.Arguments, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (info != null)
                        break;
                }

            if (info == null)
            {
                Type type = FindType("", expression.Name, out tail);

                if (type == null)
                    // TODO: walk namespaces
                    foreach (string prefix in new string[] {"System.","System.Data.","System.Xml."})
                    {
                        type = FindType(prefix, expression.Name, out tail);
                        if (type != null)
                            break;
                    }

                head = EatIdentifier(ref tail);
                if (type != null)
                    info = FindMember(type, head, tail.Length > 0, expression.Arguments, BindingFlags.Public | BindingFlags.Static);
            }
            
            while (info != null)
            {
                list.Add(info);
                head = EatIdentifier(ref tail);
                if (head.Length == 0)
                    break;
                
                Type type = null;
                if (info is FieldInfo)
                    type = ((FieldInfo)info).FieldType;
                else if (info is PropertyInfo)
                    type = ((PropertyInfo)info).PropertyType;
                else if (info is MethodInfo)
                    type = ((MethodInfo)info).ReturnType;

                info = FindMember(type, head, tail.Length > 0, expression.Arguments, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            }
            
        latebound:
            while (head.Length > 0)
            {
                list.Add(head);
                head = EatIdentifier(ref tail);
            }

            return list;
        }

        private MemberInfo FindMember(Type type, string name, bool simpleMethods, ElementList arguments, BindingFlags bindingFlags)
        {
            MemberFilter filter;
            if (simpleMethods)
                filter = new MemberFilter(FindMemberByNameSimple);
            else
                filter = new MemberFilter(FindMemberByName);
            MemberInfo[] members = type.FindMembers(
                MemberTypes.Method | MemberTypes.Property | MemberTypes.Field, 
                BindingFlags.IgnoreCase | bindingFlags,
                filter, 
                name);
            if (members == null || members.Length == 0)
                return null;
            if (members.Length == 1)
                return members[0];
            
            if (members[0] is FieldInfo)
                return members[0];

            if (simpleMethods)
                return null;

            bool named = (arguments.Count > 0) && (arguments[0] is NamedArgumentExpression);
            foreach (MethodInfo method in members)
            {
                // parameter | argument matching
                ParameterInfo[] parms = method.GetParameters();
                int count = (parms == null ? 0 : parms.Length);
                if (named && count == 1 && parms[0].ParameterType == typeof(IDictionary))
                    return method;
                if (count == arguments.Count)
                    return method;
            }
            return null;
        }

        static bool FindMemberByName(MemberInfo info, Object data)
        {
            return (string.Compare(info.Name, (string)data, true) == 0);
        }

        static bool FindMemberByNameSimple(MemberInfo info, Object data)
        {
            if (string.Compare(info.Name, (string)data, true) == 0)
            {
                if (info is MethodInfo)
                {
                    ParameterInfo[] parms = ((MethodInfo)info).GetParameters();
                    if (parms == null || parms.Length == 0)
                        return true;
                    else
                        return false;
                }
                return true;
            }
            return false;
        }

        private Type FindType(string prefix, string identifier, out string tail)
        {
            int index = -1;
            Type type = null;
            while (true)
            {
                index = identifier.IndexOf('.', index + 1);
                string probe = string.Concat(prefix, (index < 0 ? identifier : identifier.Substring(0, index)));
                type = Configuration.FindType(probe);
                if (type != null)
                {
                    if (index < 0)
                        tail = "";
                    else
                        tail = identifier.Substring(index + 1);
                    return type;
                }
                if (index < 0)
                    break;
            }
            tail = identifier;
            return null;
        }

        private string EatIdentifier(ref string identifier)
        {
            int index = identifier.IndexOf('.');
            string head;
            if (index < 0)
            {
                head = identifier;
                identifier = "";
            }
            else
            {
                head = identifier.Substring(0, index);
                identifier = identifier.Substring(index + 1);
            }
            return head;
        }
    }
}
