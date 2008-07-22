using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Glue.Lib;

namespace Glue.Data.Test
{
    [System.Diagnostics.DebuggerNonUserCode]
    public class Tester
    {
        public static void Run(Type type)
        {
            Run(type, false);
        }

        public static void Run(Type type, bool catchExceptions)
        {
            if (type.GetCustomAttributes(typeof(TestFixtureAttribute), true).Length <= 0)
                throw new ApplicationException("Type " + type + " is not a TestFixture.");

            object instance = Activator.CreateInstance(type);
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                if (method.GetCustomAttributes(typeof(SetUpAttribute), true).Length > 0)
                {
                    Log.Info("{0}: Setting up test fixture.", type.Name);
                    Invoke(instance, method, catchExceptions);
                }
            
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                if (method.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                {
                    Log.Info("{0}: Running test: {1}", type.Name, method.Name);
                    Invoke(instance, method, catchExceptions);
                }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                if (method.GetCustomAttributes(typeof(TearDownAttribute), true).Length > 0)
                {
                    Log.Info("{0}: Tearing down test fixture.", type.Name);
                    Invoke(instance, method, catchExceptions);
                }
        }

        static void Invoke(object instance, MethodInfo method, bool catchExceptions)
        {
            if (catchExceptions)
                try
                {
                    method.Invoke(instance, new object[0]);
                }
                catch (TargetInvocationException e)
                {
                    Log.Error("Failed: {0}", method.Name);
                    Log.Error(e.InnerException);
                }
            else
                method.Invoke(instance, new object[0]);
        }

        public static void Run<T>()
        {
            Run(typeof(T));
        }
    }

}
