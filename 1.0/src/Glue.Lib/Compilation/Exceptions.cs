using System;
using System.Collections;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;

namespace Glue.Lib.Compilation
{
    /// <summary>
    /// CompilationException 
    /// </summary>
    public class CompilationException : Exception
    {
        string sourceFile;
        string errorMessage;
        CompilerResults results;

        public CompilationException(string message) : base(message) {}

        public CompilationException(CompilerResults results) : this(null, results)
        {
        }

        public CompilationException(string sourceFile, CompilerResults results) : base("Compiler Error.")
        {
            this.sourceFile = sourceFile;
            this.results = results;
        }

        public string SourceFile 
        {
            get 
            {
                if (results == null || results.Errors == null || results.Errors.Count == 0)
                    return sourceFile;
                return results.Errors[0].FileName;
            }
        }

        public string ErrorMessage 
        {
            get 
            {
                if (errorMessage == null && results != null && results.Errors != null) 
                {
                    StringBuilder sb = new StringBuilder ();
                    foreach (CompilerError err in results.Errors) 
                    {
                        // sb.AppendFormat("{0} ({1},{2}): {3} {4}", err.FileName, err.Line, err.Column, err.ErrorNumber,  err.ErrorText);
                        sb.Append(err);
                        sb.Append ("\n");
                    }
                    errorMessage = sb.ToString ();
                }
                return errorMessage;
            }
        }

        public CompilerResults Results
        {
            get { return results; }
        }
    }

    public class RuntimeException : Exception
    {
        public string SourceFile;
        public int LineNumber;

        public RuntimeException(string message, string sourceFile, int lineNumber)
            : base(message)
        {
            SourceFile = sourceFile;
            LineNumber = lineNumber;
        }
    }
}