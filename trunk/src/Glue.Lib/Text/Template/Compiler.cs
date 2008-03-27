using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Glue.Lib.Text.Template.AST;

namespace Glue.Lib.Text.Template
{
	/// <summary>
	/// DirectCompiler.
	/// </summary>
    public class Compiler
    {
        Unit _unit;
        TypeBuilder _typeBuilder;
        Type _baseClass;
        Type[] _mixinTypes;
        FieldBuilder[] _mixinFields;    // array of FieldBuilder objects
        ArrayList _fields;              // array of FieldBuilder / FieldInfo objects
        ArrayList _methods;             // array of MethodBuilder / MethodInfo objects
        Stack _scope;                   // local scopes
        Hashtable _locals;              // local variables in current scope
        ConstructorBuilder _constructor;
        MethodBuilder _renderMethod;
        ISymbolDocumentWriter _symbols;

        FieldInfo  _writerField;
        MethodInfo _textWriterWriteString;
        MethodInfo _textWriterWriteObject;
        MethodInfo _typeFromHandle;
        ConstructorInfo _debuggableAttribute;

        MethodInfo _runtimeEQ;
        MethodInfo _runtimeNE;
        MethodInfo _runtimeLT;
        MethodInfo _runtimeGT;
        MethodInfo _runtimeLE;
        MethodInfo _runtimeGE;
        MethodInfo _runtimeAnd;
        MethodInfo _runtimeOr;
        MethodInfo _runtimeAdd;
        MethodInfo _runtimeSub;
        MethodInfo _runtimeDiv;
        MethodInfo _runtimeMul;
        MethodInfo _runtimeMod;
        MethodInfo _runtimeTest;
        MethodInfo _runtimeGet;
        MethodInfo _runtimeGetWithBag;
        MethodInfo _runtimeSet;
        MethodInfo _runtimeInvoke;
        MethodInfo _runtimeGetEnumerator;
        MethodInfo _runtimeRange;
        MethodInfo _runtimeBag;

        internal static Type[] GetTypes(object[] mixins)
        {
            Type[] types = new Type[mixins.Length];
            for (int i = 0; i < mixins.Length; i++)
                if (mixins[i] is Type)
                    types[i] = (Type)mixins[i];
                else if (mixins[i] != null)
                    types[i] = mixins[i].GetType();
            return types;
        }

        internal static object[] GetInstances(object[] mixins)
        {
            object[] instances = new object[mixins.Length];
            for (int i = 0; i < mixins.Length; i++)
                if (mixins[i] is Type)
                    instances[i] = null;
                else
                    instances[i] = mixins[i];
            return instances;
        }

        public Compiler()
        {
        }

        public Type Compile(Unit unit, Type baseClass, bool memory, bool debug, Type[] mixinTypes)
        {
            InitDefinitions();

            AssemblyName name = new AssemblyName();
            name.Name = "edf_lib_text_template";
            System.Security.PermissionSet permissions = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);
            
            AssemblyBuilder assembly;
            ModuleBuilder module;
            if (memory)
                assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, permissions, permissions, new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None));
            else
                assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, permissions, permissions, new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None));
            
            if (debug)
            {
                CustomAttributeBuilder attr = new CustomAttributeBuilder(_debuggableAttribute, new object[] {true, true});
                assembly.SetCustomAttribute(attr);
            }
           
            if (memory)
                module = assembly.DefineDynamicModule("edf_lib_text_template.dll", debug);
            else
                module = assembly.DefineDynamicModule("edf_lib_text_template.dll", "edf_lib_text_template.dll", debug);
            
            if (debug)
            {
                // HACK:
                _symbols = module.DefineDocument(
                    @"d:\svn\spirit.intranet\web\spirit.intranet.web\views\page\view2.html", 
                    Guid.Empty, 
                    Guid.Empty, 
                    SymDocumentType.Text
                    );
            }
            
            _unit = unit;
            _baseClass = baseClass;
            _writerField = _baseClass.GetField("_writer", BindingFlags.NonPublic | BindingFlags.Instance);

            // Emit class:
            // public class MyClass: BaseClass { 
            //   [fill below]
            // }
            _typeBuilder = module.DefineType(unit.Name, TypeAttributes.Class | TypeAttributes.Public, _baseClass);

            // Mixin fields:
            //   private MixinType1 __M1;
            //   private MixinType2 __M2;
            //   ...
            _mixinTypes = mixinTypes;
            _mixinFields = new FieldBuilder[_mixinTypes .Length];
            for (int i = 0; i < _mixinTypes.Length; i++)
                _mixinFields[i] = _typeBuilder.DefineField("__M" + i, _mixinTypes[i], FieldAttributes.Private);
            
            _fields = new ArrayList();
            _methods = new ArrayList();
            _scope = new Stack();
            _locals = CollectionsUtil.CreateCaseInsensitiveHashtable();

            // Constructor:
            //   public MyClass(MixinType1 m1, MixinType2 m2) {
            //     this.__M1 = m1;
            //     this.__M1 = m2;
            //     ...
            //   }
            _constructor = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, _mixinTypes);
            ILGenerator il = _constructor.GetILGenerator();
            
            for (int i = 0; i < _mixinFields.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_S, i + 1);
                il.Emit(OpCodes.Stfld, _mixinFields[i]);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _baseClass.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]{}, null));

            il.Emit(OpCodes.Ret);

            // method Render:
            //   public Render(TextWriter writer) {
            //   }
            _renderMethod = _typeBuilder.DefineMethod("Render", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(void), new Type[] {typeof(System.IO.TextWriter)});
            
            il = _renderMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, _writerField);

             EmitBlock(unit.Inner, il);
             il.Emit(OpCodes.Ret);

            Type compiledType = _typeBuilder.CreateType();
            if (!memory)
            {
                assembly.Save("edf_lib_text_template.dll");
            }
            return compiledType;
        }

        private void InitDefinitions()
        {
            _debuggableAttribute = typeof(System.Diagnostics.DebuggableAttribute).GetConstructor(new Type[] {typeof(bool),typeof(bool)});
            
            _textWriterWriteString = typeof(System.IO.TextWriter).GetMethod("Write", new Type[] {typeof(string)});
            _textWriterWriteObject = typeof(System.IO.TextWriter).GetMethod("Write", new Type[] {typeof(object)});
            _typeFromHandle        = typeof(System.Type).GetMethod("GetTypeFromHandle", new Type[] {typeof(RuntimeTypeHandle)});

            _runtimeAdd            = typeof(Runtime).GetMethod("Add", new Type[] {typeof(object), typeof(object)});
            _runtimeSub            = typeof(Runtime).GetMethod("Sub", new Type[] {typeof(object), typeof(object)});
            _runtimeDiv            = typeof(Runtime).GetMethod("Div", new Type[] {typeof(object), typeof(object)});
            _runtimeMul            = typeof(Runtime).GetMethod("Mul", new Type[] {typeof(object), typeof(object)});
            _runtimeMod            = typeof(Runtime).GetMethod("Mod", new Type[] {typeof(object), typeof(object)});
            _runtimeTest           = typeof(Runtime).GetMethod("Test", new Type[] {typeof(object)});
            _runtimeGet            = typeof(Runtime).GetMethod("Get", new Type[] {typeof(object), typeof(string), typeof(object[])});
            _runtimeSet            = typeof(Runtime).GetMethod("Set", new Type[] {typeof(object), typeof(string), typeof(object), typeof(object[])});
            _runtimeBag            = typeof(Runtime).GetMethod("Bag");
            _runtimeGetWithBag     = typeof(Runtime).GetMethod("GetWithBag");
            _runtimeInvoke         = typeof(Runtime).GetMethod("Invoke", new Type[] {typeof(object), typeof(string), typeof(object[])});
            _runtimeRange          = typeof(Runtime).GetMethod("Range", new Type[] {typeof(object),typeof(object)});
            _runtimeGetEnumerator  = typeof(Runtime).GetMethod("GetEnumerator", new Type[] {typeof(object)});
            _runtimeLT             = typeof(Runtime).GetMethod("LT", new Type[] {typeof(object),typeof(object)});
            _runtimeLE             = typeof(Runtime).GetMethod("LE", new Type[] {typeof(object),typeof(object)});
            _runtimeGT             = typeof(Runtime).GetMethod("GT", new Type[] {typeof(object),typeof(object)});
            _runtimeGE             = typeof(Runtime).GetMethod("GE", new Type[] {typeof(object),typeof(object)});
            _runtimeEQ             = typeof(Runtime).GetMethod("EQ", new Type[] {typeof(object),typeof(object)});
            _runtimeNE             = typeof(Runtime).GetMethod("NE", new Type[] {typeof(object),typeof(object)});
            _runtimeAnd            = typeof(Runtime).GetMethod("And", new Type[] {typeof(object),typeof(object)});
            _runtimeOr             = typeof(Runtime).GetMethod("Or", new Type[] {typeof(object),typeof(object)});
        }

        private void EmitBlock(ElementList elements, ILGenerator il)
        {
            foreach (Element element in elements)
            {
                if (_symbols != null)
                {
                    il.MarkSequencePoint(_symbols, element.Line, 0, element.Line+1, 0);
                    il.Emit(OpCodes.Nop);
                }

                if (element is MethodDefinition)
                {
                    EmitMethod(element as MethodDefinition, il);
                }
                /*
                else if (element is TemplateDefinition)
                {
                    EmitTemplate(element as TemplateDefinition, definitions);
                }
                */
                else if (element is PutStatement)
                {
                    EmitPrint((element as PutStatement).Expression, il);
                }
                else if (element is HaltStatement)
                {
                    il.EmitCall(OpCodes.Call, typeof(System.Diagnostics.Debugger).GetMethod("Break"), null);
                }
                else if (element is Expression)
                {
                    EmitPrint(element as Expression, il);
                }
                else if (element is ForStatement)
                {
                    EmitForStatement(element as ForStatement, il);
                }
                else if (element is WhileStatement)
                {
                    EmitWhileStatement(element as WhileStatement, il);
                }
                else if (element is IfStatement)
                {
                    EmitIfStatement(element as IfStatement, il);
                }
                else if (element is AssignStatement)
                {
                    EmitAssignStatement(element as AssignStatement, il);
                }
                /*
                else if (element is ApplyStatement)
                {
                    EmitApplyStatement(element as ApplyStatement, statements);
                }
                else
                {
                    throw new ArgumentException("Unknown element type: " + element);
                }
                */
            }
        }

        private void EmitMethod(MethodDefinition definition, ILGenerator il)
        {
            Type[] parameters = new Type[definition.Parameters.Count];
            for (int i = 0; i < definition.Parameters.Count; i++)
                parameters[i] = typeof(object);
            
            MethodBuilder method = _typeBuilder.DefineMethod(definition.Name, MethodAttributes.Public, typeof(void), parameters);

            _scope.Push(_locals);
            _locals = CollectionsUtil.CreateCaseInsensitiveHashtable();

            il = method.GetILGenerator();
            for (int i = 0; i < definition.Parameters.Count; i++)
            {
                ParameterDefinition par = (ParameterDefinition)definition.Parameters[i];
                LocalBuilder local = il.DeclareLocal(typeof(object));
                il.Emit(OpCodes.Ldarg_S, i+1);
                il.Emit(OpCodes.Stloc_S, i);
                _locals[par.Name] = local;
            }
            EmitBlock(definition.Inner, il);
            il.Emit(OpCodes.Ret);

            _locals = (Hashtable)_scope.Pop();
        }

        private void EmitIfStatement(IfStatement statement, ILGenerator il)
        {
            EmitExpression(statement.Test, il);
            il.Emit(OpCodes.Call, _runtimeTest);
            if (statement.False.Count == 0)
            {
                Label endlabel = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, endlabel);
                EmitBlock(statement.True, il);
                il.MarkLabel(endlabel);
            }
            else
            {
                Label notlabel = il.DefineLabel();
                Label endlabel = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, notlabel);
                EmitBlock(statement.True, il);
                il.Emit(OpCodes.Br, endlabel);
                il.MarkLabel(notlabel);
                EmitBlock(statement.False, il);
                il.MarkLabel(endlabel);
            }
        }

        private void EmitWhileStatement(IfStatement statement, ILGenerator il)
        {
            if (statement.False.Count == 0)
            {
                Label beginlabel = il.DefineLabel();
                Label endlabel = il.DefineLabel();

                il.MarkLabel(beginlabel);
                EmitExpression(statement.Test, il);
                il.Emit(OpCodes.Call, _runtimeTest);
                il.Emit(OpCodes.Brfalse, endlabel);
                
                EmitBlock(statement.True, il);
                il.Emit(OpCodes.Br, beginlabel);
                il.MarkLabel(endlabel);
            }
            else
            {
                Label looplabel = il.DefineLabel();
                Label notlabel = il.DefineLabel();
                Label endlabel = il.DefineLabel();

                EmitExpression(statement.Test, il);
                il.Emit(OpCodes.Call, _runtimeTest);
                il.Emit(OpCodes.Brfalse, notlabel);

                il.MarkLabel(looplabel);
                EmitBlock(statement.True, il);

                EmitExpression(statement.Test, il);
                il.Emit(OpCodes.Call, _runtimeTest);
                il.Emit(OpCodes.Brfalse, endlabel);
                il.Emit(OpCodes.Br, looplabel);

                il.MarkLabel(notlabel);
                EmitBlock(statement.False, il);
                il.MarkLabel(endlabel);
            }
        }

        private void EmitAssignStatement(AssignStatement statement, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, statement.Name);
            EmitExpression(statement.Expression, il);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Call, _runtimeSet);
        }

        private void EmitForStatement(ForStatement statement, ILGenerator il)
        {
            // enumerator = GetEnumerator(Container)
            EmitExpression(statement.Container, il);
            LocalBuilder enumerator = il.DeclareLocal(typeof(IEnumerator));
            //il.Emit(OpCodes.Castclass, typeof(System.Collections.IEnumerable));
            //il.Emit(OpCodes.Callvirt, typeof(IEnumerable).GetMethod("GetEnumerator", new Type[] {}));
            il.Emit(OpCodes.Call, _runtimeGetEnumerator);
            il.Emit(OpCodes.Stloc, enumerator);

            // index = -1;
            LocalBuilder index = il.DeclareLocal(typeof(Int32));
            il.Emit(OpCodes.Ldc_I4, -1);
            il.Emit(OpCodes.Stloc, index);

            // goto test
            // loop:
            Label loop = il.DefineLabel();
            Label test = il.DefineLabel();
            il.Emit(OpCodes.Br, test);
            il.MarkLabel(loop);

            //     object iter = enumerator.Current;
            LocalBuilder iter = il.DeclareLocal(typeof(object));
            _locals[statement.Iterator] = iter;
            il.Emit(OpCodes.Ldloc, enumerator);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetProperty("Current").GetGetMethod());
            il.Emit(OpCodes.Stloc, iter);

            // Separator statements
            if (statement.Sep.Count > 0)
            {
                Label endsep = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, index);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Brfalse, endsep);
                EmitBlock(statement.Sep, il);
                il.MarkLabel(endsep);
            }

            Label alt = il.DefineLabel();
            if (statement.Alt.Count > 0)
            {
                // if (index & 2 != 0)
                //     goto alt
                il.Emit(OpCodes.Ldloc, index);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Brfalse, alt);
            }
            // inner:
            //   inner statements
            EmitBlock(statement.Inner, il);
            if (statement.Alt.Count > 0)
            {
                // goto test
                il.Emit(OpCodes.Br, test);
                // alt:
                //   Alt statements
                il.MarkLabel(alt);
                EmitBlock(statement.Alt, il);
            }
            
            // test:
            //      index++;
            il.MarkLabel(test);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldc_I4, 1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, index);
            
            //     if (enumerator.MoveNext())
            //         goto loop;
            il.Emit(OpCodes.Ldloc, enumerator);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext", new Type[] {}));
            il.Emit(OpCodes.Brtrue, loop);
        }

        private void EmitPrint(Expression expression, ILGenerator il)
        {
            // if inside_render_method: il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _writerField);
            EmitExpression(expression, il);
            il.Emit(OpCodes.Callvirt, _textWriterWriteObject);
        }

        private void EmitExpression(Expression expression, ILGenerator il)
        {
            if (expression is ReferenceExpression)
            {
                ReferenceExpression expr = expression as ReferenceExpression;
                ArrayList steps = Resolve(expr);
                
                for (int i = 0; i < steps.Count; i++)
                {
                    if (steps[i] is LocalBuilder)
                    {
                        il.Emit(OpCodes.Ldloc, steps[i] as LocalBuilder);
                    }
                    else if (steps[i] is FieldInfo)
                    {
                        FieldInfo field = steps[i] as FieldInfo;
                        if (i == 0 && !field.IsStatic)
                            il.Emit(OpCodes.Ldarg_0);
                        if (!field.IsStatic)
                            il.Emit(OpCodes.Ldfld, field);
                        else
                            il.Emit(OpCodes.Ldsfld, field);
                        if (field.FieldType.IsValueType)
                            il.Emit(OpCodes.Box, field.FieldType);
                    }
                    else if (steps[i] is PropertyInfo)
                    {
                        PropertyInfo property = steps[i] as PropertyInfo;
                        MethodInfo getter = property.GetGetMethod();
                        if (i == 0 && !getter.IsStatic)
                            il.Emit(OpCodes.Ldarg_0);
                        if (getter.IsVirtual)
                            il.Emit(OpCodes.Callvirt, getter);
                        else
                            il.Emit(OpCodes.Call, getter);
                        if (property.PropertyType.IsValueType)
                            il.Emit(OpCodes.Box, property.PropertyType);
                    }
                    else if (steps[i] is MethodInfo)
                    {
                        MethodInfo method = steps[i] as MethodInfo;
                        if (method.GetParameters().Length == 0)
                        {
                            if (i == 0 && !method.IsStatic)
                                il.Emit(OpCodes.Ldarg_0);
                            if (method.IsVirtual)
                                il.Emit(OpCodes.Callvirt, method);
                            else
                                il.Emit(OpCodes.Call, method);
                            if (method.ReturnType.IsValueType)
                                il.Emit(OpCodes.Box, method.ReturnType);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                if (method.IsStatic)
                                {
                                    il.Emit(OpCodes.Ldtoken, method.DeclaringType);
                                    il.Emit(OpCodes.Call, _typeFromHandle);
                                }
                                else
                                    il.Emit(OpCodes.Ldarg_0);
                            }
                            
                            if (expr.Arguments.Count > 0 && expr.Arguments[0] is NamedArgumentExpression)
                            {
                                il.Emit(OpCodes.Ldstr, method.Name);
                                il.Emit(OpCodes.Ldc_I4, expr.Arguments.Count * 2);
                                il.Emit(OpCodes.Newarr, typeof(object));
                                for (int j = 0; j < expr.Arguments.Count; j++)
                                {
                                    il.Emit(OpCodes.Dup);
                                    il.Emit(OpCodes.Ldc_I4, j*2);
                                    il.Emit(OpCodes.Ldstr, (expr.Arguments[j] as NamedArgumentExpression).Name);
                                    il.Emit(OpCodes.Stelem_Ref);

                                    il.Emit(OpCodes.Dup);
                                    il.Emit(OpCodes.Ldc_I4, j*2+1);
                                    EmitExpression((expr.Arguments[j] as NamedArgumentExpression).Expression, il);
                                    il.Emit(OpCodes.Stelem_Ref);
                                }
                                il.Emit(OpCodes.Call, _runtimeBag);
                                il.Emit(OpCodes.Call, _runtimeGetWithBag);
                            }
                            else
                            {
                                il.Emit(OpCodes.Ldstr, method.Name);
                                il.Emit(OpCodes.Ldc_I4, expr.Arguments.Count);
                                il.Emit(OpCodes.Newarr, typeof(object));
                                for (int j = 0; j < expr.Arguments.Count; j++)
                                {
                                    il.Emit(OpCodes.Dup);
                                    il.Emit(OpCodes.Ldc_I4, j);
                                    EmitExpression(expr.Arguments[j] as Expression, il);
                                    il.Emit(OpCodes.Stelem_Ref);
                                }
                                il.Emit(OpCodes.Call, _runtimeGet);
                            }
                        }
                    }
                    else if (steps[i] is string)
                    {
                        string member = (string)steps[i];
                        if (i == 0)
                            il.Emit(OpCodes.Ldarg_0);
                        
                        if (i < steps.Count - 1 || expr.Arguments.Count == 0)
                        {
                            il.Emit(OpCodes.Ldstr, member);
                            il.Emit(OpCodes.Ldnull);
                            il.Emit(OpCodes.Call, _runtimeGet);
                        }
                        else if (expr.Arguments[0] is NamedArgumentExpression)
                        {
                            il.Emit(OpCodes.Ldstr, member);
                            il.Emit(OpCodes.Ldc_I4, expr.Arguments.Count * 2);
                            il.Emit(OpCodes.Newarr, typeof(object));
                            for (int j = 0; j < expr.Arguments.Count; j++)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Ldc_I4, j*2);
                                il.Emit(OpCodes.Ldstr, (expr.Arguments[j] as NamedArgumentExpression).Name);
                                il.Emit(OpCodes.Stelem_Ref);

                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Ldc_I4, j*2+1);
                                EmitExpression((expr.Arguments[j] as NamedArgumentExpression).Expression, il);
                                il.Emit(OpCodes.Stelem_Ref);
                            }
                            il.Emit(OpCodes.Call, _runtimeBag);
                            il.Emit(OpCodes.Call, _runtimeGetWithBag);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldstr, member);
                            il.Emit(OpCodes.Ldc_I4, expr.Arguments.Count);
                            il.Emit(OpCodes.Newarr, typeof(object));
                            for (int j = 0; j < expr.Arguments.Count; j++)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Ldc_I4, j);
                                EmitExpression(expr.Arguments[j] as Expression, il);
                                il.Emit(OpCodes.Stelem_Ref);
                            }
                            il.Emit(OpCodes.Call, _runtimeGet);
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
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeAdd);
                        break;
                    case "-": 
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeSub);
                        break;
                    case "*": 
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeMul);
                        break;
                    case "/": 
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeDiv);
                        break;
                    case "%": 
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeMod);
                        break;
                    case "<":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeLT);
                        break;
                    case ">":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeGT);
                        break;
                    case "<=":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeLE);
                        break;
                    case ">=":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeGE);
                        break;
                    case "==":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeEQ);
                        break;
                    case "!=":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeNE);
                        break;
                    case "&&":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeAnd);
                        break;
                    case "||":
                        EmitExpression(bin.Left, il);
                        EmitExpression(bin.Right, il);
                        il.Emit(OpCodes.Call, _runtimeOr);
                        break;
                    default:
                        il.Emit(OpCodes.Ldstr, "????:" + expression);
                        break;
                }
            }
            else if (expression is PrimitiveExpression)
            {
                PrimitiveExpression primitive = expression as PrimitiveExpression;
                if (primitive.Value is String)
                {
                    il.Emit(OpCodes.Ldstr, (string)primitive.Value);
                }
                else if (primitive.Value is Int32)
                {
                    il.Emit(OpCodes.Ldc_I4, (Int32)primitive.Value);
                    il.Emit(OpCodes.Box, typeof(Int32));
                }
                else if (primitive.Value is Boolean)
                {
                    if ((Boolean)primitive.Value)
                        il.Emit(OpCodes.Ldc_I4_1);
                    else
                        il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Box, typeof(Boolean));
                }
                else if (primitive.Value is Double)
                {
                    il.Emit(OpCodes.Ldc_R8, (Double)primitive.Value);
                    il.Emit(OpCodes.Box, typeof(Double));
                }
                else
                {
                    il.Emit(OpCodes.Ldstr, "????:" + expression);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldstr, "????:" + expression);
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
