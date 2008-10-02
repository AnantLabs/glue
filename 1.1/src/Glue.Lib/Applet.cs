using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Glue.Lib;
using System.Xml;

namespace Glue.Lib
{
    /// <summary>
	/// Applet.
	/// </summary>
	public class Applet
	{
		public Applet()
		{
		}

        public static int Exec(string program, params string[] arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo(program, JoinArguments(arguments));
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            using (Process process = Process.Start(info))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        class RedirectionReaderWriter
        {
            TextReader reader;
            TextWriter writer;
            public RedirectionReaderWriter(TextReader reader, TextWriter writer) 
            { 
                this.reader = reader; 
                this.writer = writer; 
            }
            public void Start()
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(this.Run)).Start();
            }
            private void Run()
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    writer.WriteLine(line);
            }
        }

        public static int Exec(
            string program, 
            TextReader input, 
            TextWriter output, 
            TextWriter error,
            params string[] arguments
            )
        {
            ProcessStartInfo info = new ProcessStartInfo(program, JoinArguments(arguments));
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardInput = input != null;
            info.RedirectStandardOutput = output != null;
            info.RedirectStandardError = error != null;
            using (Process process = Process.Start(info))
            {
                if (input != null) new RedirectionReaderWriter(input, process.StandardInput).Start();
                if (output != null) new RedirectionReaderWriter(process.StandardOutput, output).Start();
                if (error != null) new RedirectionReaderWriter(process.StandardError, error).Start();
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        /*
        public static void Merge(string path)
        {
            if (!File.Exists(path + ".base"))
            {
                File.Copy(path + ".new", path + ".base", true);
                File.Delete(path + ".new");
                Reconcile(path, path + ".base");
                return;
            }
            if ((Merge(path, path + ".base", path + ".new", path + ".merged") != 0) && 
                (Reconcile(path, path + ".base", path + ".new", path + ".merged") != 0))
            {
                return;
            }
            File.Copy(path + ".new", path + ".base", true);
            File.Delete(path + ".new");
            File.Copy(path, path + ".bak", true);
            File.Copy(path + ".merged", path, true);
            File.Delete(path + ".merged");
            return;
        }
        
        public static int Merge(string mine, string base_, string theirs, string reconciled)
        {
            using (Process process = new Process())
            {
                string cmd = CommandMerge3Program;
                cmd = cmd.Replace("$mine", Quote(mine));
                cmd = cmd.Replace("$base", Quote(base_));
                cmd = cmd.Replace("$theirs", Quote(theirs));
                cmd = Environment.ExpandEnvironmentVariables(cmd);
                string program = EatQuotedArg(ref cmd);
                process.StartInfo.FileName = program;
                process.StartInfo.Arguments = cmd;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                using (StreamWriter output = File.CreateText(reconciled))
                {
                    output.Write(process.StandardOutput.ReadToEnd());
                }
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        public static int Reconcile(string mine, string theirs)
        {
            string cmd = CommandDiff2Editor;
            cmd= cmd.Replace("$mine", Quote(mine));
            cmd = cmd.Replace("$base", Quote(theirs));
            cmd = Environment.ExpandEnvironmentVariables(cmd);
            string program = EatQuotedArg(ref cmd);
            return Exec(program, SplitQuotedArgs(cmd));
        }
 
        public static int Reconcile(string mine, string base_, string theirs, string reconciled)
        {
            string cmd = CommandDiff3Editor;
            cmd = cmd.Replace("$mine", Quote(mine));
            cmd = cmd.Replace("$base", Quote(base_));
            cmd = cmd.Replace("$theirs", Quote(theirs));
            cmd = cmd.Replace("$reconciled", Quote(reconciled));
            cmd = Environment.ExpandEnvironmentVariables(cmd);
            string program = EatQuotedArg(ref cmd);
            return Exec(program, SplitQuotedArgs(cmd));
        }

        private static string CommandMerge3Program = @"diff3.exe --merge $mine $base $theirs";
        private static string CommandDiff2Editor   = @"""%programfiles%\araxis\araxis~1\merge.exe"" /nosplash $base $mine";
        private static string CommandDiff3Editor   = @"""%programfiles%\tortoisesvn\bin\tortoisemerge.exe"" /yours:$mine /base:$base /theirs:$theirs /merged:$reconciled";
        */
        

        public static TextReader OpenText(string path)
        {
            if (path == null || path.Length == 0)
                return Inp;
            else
                return File.OpenText(path);
        }

        public static string LoadText(string path)
        {
            using (TextReader reader = OpenText(path))
                return reader.ReadToEnd();
        }

        public static void SaveText(string path, string text)
        {
            using (TextWriter writer = CreateText(path))
                writer.Write(text);
        }

        public static XmlTextReader OpenXml(string path)
        {
            if (path == null || path.Length == 0)
                return new XmlTextReader(Console.In);
            else
                return new XmlTextReader(path);
        }
        
        public static XmlDocument LoadXml(string path)
        {
            XmlDocument document = new XmlDocument();
            if (path == null || path.Length == 0)
                document.Load(Console.In);
            else
                document.Load(path);
            return document;
        }
        
        public static TextWriter CreateText(string path)
        {
            return CreateText(path, System.Text.Encoding.UTF8);
        }

        public static TextWriter CreateText(string path, System.Text.Encoding encoding)
        {
            if (path == null || path.Length == 0)
                return Out;
            else
                return new StreamWriter(path, false, encoding);
        }

        public static XmlTextWriter CreateXml(string path)
        {
            return CreateXml(path, System.Text.Encoding.UTF8);
        }

        public static XmlTextWriter CreateXml(string path, System.Text.Encoding encoding)
        {
            if (path == null || path.Length == 0)
                return new XmlTextWriter(Console.Out);
            else
                return new XmlTextWriter(path, encoding);
        }

        static Random _random = new Random();
        static object _uniqueRandomFileNameLock = 0;
        static object _uniqueSequentialFileNameLock = 0;
        
        public static string GetUniqueRandomFileName(string path)
        {
            path = Path.GetFullPath(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            string dir = Path.GetDirectoryName(path);
            path = Path.Combine(dir, name + _random.Next() + ext);
            lock (_uniqueRandomFileNameLock)
            {
                while (File.Exists(path))
                    path = Path.Combine(dir, name + _random.Next() + ext);
                File.Create(path).Close(); 
            }
            return path;
        }

        public static string GetUniqueSequentialFileName(string path)
        {
            path = Path.GetFullPath(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            string dir = Path.GetDirectoryName(path);
            int i = 2;
            path = Path.Combine(dir, name + ext);
            // perform the first scan through subsequent names without locking
            // the function. 
            while (File.Exists(path))
                path = Path.Combine(dir, name + "." + (i++) + ext);
            // our name is probable good or almost good, now we can safely lock.
            lock (_uniqueSequentialFileNameLock)
            {
                while (File.Exists(path))
                    path = Path.Combine(dir, name + "." + (i++) + ext);
                File.Create(path).Close(); 
            }
            return path;
        }

        public static bool IsUpToDate(string target, string source)
        {
            if (!File.Exists(source))
                throw new ArgumentException("Source file does not exists. " + source);
            if (!File.Exists(target))
                return false;
            return File.GetLastWriteTimeUtc(target) >= File.GetLastWriteTimeUtc(source);
        }

        public static void GetOption(string argument, out string option, out string value)
        {
            option = null;
            value  = null;
            
            if (argument == null || argument.Length == 0)
                return;
            if (argument[0] != '-')
            {
                value = argument;
                return;
            }

            int i = argument.IndexOfAny(new char[] {':','='});
            if (i > 0)
            {
                option = argument.Substring(0, i);
                value = argument.Substring(i + 1);
            }
            else
            {
                option = argument;
            }
        }

        /*
        public static string[] LoadOptions(string path)
        {
            if (File.Exists(path))
                return OptionConvert.ToStringArray(OptionConvert.Load(path));
            else
                return new string[0];
        }
        */

        public static string Quote(string s)
        {
            return Quote(s, false);
        }

        public static string Quote(string s, bool always)
        {
            if (s == null)
                return s;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length-1] == '"')
                return s;
            if (always || s.IndexOfAny(StringHelper.WhiteSpaceChars) >= 0)
                return "\"" + s + "\"";
            return s;
        }

        public static string UnQuote(string s)
        {
            if (s == null)
                return null;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length-1] == '"')
                return s.Substring(1, s.Length-2);
            return s;
        }

        public static string[] SplitArguments(string arguments)
        {
            return StringHelper.Split(arguments, StringHelper.WhiteSpaceChars, new char[] {'"'}, null, StringHelper.WhiteSpaceChars, true); 
        }

        public static string JoinArguments(string[] arguments)
        {
            if (arguments == null)
                return null;
            for (int i = 0; i < arguments.Length; i++)
                arguments[i] = Quote(arguments[i], false);
            return String.Join(" ", arguments);
        }

        /// <summary>
        /// Returns the path of the executing assembly.
        /// </summary>
        public static string ExePath
        {
            get { return System.Reflection.Assembly.GetEntryAssembly().Location; }
        }

        /// <summary>
        /// Returns the directory of the executing assembly.
        /// </summary>
        public static string ExeDirectory
        {
            get { return Path.GetDirectoryName(ExePath); }
        }

        /// <summary>
        /// Returns the current working directory.
        /// </summary>
        public static string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
        }

        /// <summary>
        /// Returns the .NET framework directory for this assembly.
        /// </summary>
        public static string FrameworkDirectory
        {
            get { return Path.GetDirectoryName(typeof(System.Int32).Assembly.Location); }
        }

        public static TextReader Inp
        {
            get { return Console.In; }
            set { Console.SetIn(value); }
        }

        public static TextWriter Out
        {
            get { return Console.Out; }
            set { Console.SetOut(value); }
        }

        public static TextWriter Err
        {
            get { return Console.Error; }
            set { Console.SetError(value); }
        }

        public static int Read()
        {
            return Inp.Read();
        }

        public static string ReadLine()
        {
            return Inp.ReadLine();
        }

        public static string ReadToEnd()
        {
            return Inp.ReadToEnd();
        }

        public static TextWriter Write(object o)
        {
            Out.Write(o);
            return Out;
        }

        public static TextWriter Write(string s)
        {
            Out.Write(s);
            return Out;
        }

        public static TextWriter Write(string fmt, params object[] args)
        {
            Out.Write(fmt, args);
            return Out;
        }

        public static TextWriter WriteLine()
        {
            Out.WriteLine();
            return Out;
        }
        
        public static TextWriter WriteLine(object o)
        {
            Out.WriteLine(o);
            return Out;
        }

        public static TextWriter WriteLine(string s)
        {
            Out.WriteLine(s);
            return Out;
        }

        public static TextWriter WriteLine(string fmt, params object[] args)
        {
            Out.WriteLine(fmt, args);
            return Out;
        }

        public static TextWriter Error(object o)
        {
            Err.Write(o);
            return Err;
        }

        public static TextWriter Error(string s)
        {
            Err.Write(s);
            return Err;
        }

        public static TextWriter Error(string fmt, params object[] args)
        {
            Err.Write(fmt, args);
            return Err;
        }

        public static TextWriter ErrorLine(object o)
        {
            Err.WriteLine(o);
            return Err;
        }

        public static TextWriter ErrorLine(string s)
        {
            Err.WriteLine(s);
            return Err;
        }

        public static TextWriter ErrorLine(string fmt, params object[] args)
        {
            Err.WriteLine(fmt, args);
            return Err;
        }
    }
}
