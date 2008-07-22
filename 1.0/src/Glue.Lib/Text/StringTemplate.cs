using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.CodeDom;
using System.Text;
using System.CodeDom.Compiler;
using Glue.Lib.Text.Template;
using Glue.Lib.Text.Template.AST;

namespace Glue.Lib.Text
{
    /// <summary>
    /// StringTemplate.
    /// </summary>
    public class StringTemplate
    {
        public static StringTemplate Create(string text, params object[] mixins)
        {
            return Create(new StringTemplateReader(text), mixins);
        }

        public static StringTemplate CreateFromFile(string path, params object[] mixins)
        {
            return Create(new StringTemplateLoader(path), mixins);
        }

        public static StringTemplate Create(StringTemplateReader reader, params object[] mixins)
        {
            return Instantiate(Compile(reader, Compiler.GetTypes(mixins)), mixins);
        }

        public static Type Compile(string text, params Type[] mixinTypes)
        {
            return Compile(new StringTemplateReader(text), mixinTypes);
        }

        public static Type CompileFromFile(string path, params Type[] mixinTypes)
        {
            return Compile(new StringTemplateLoader(path), mixinTypes);
        }

        public static Type Compile(StringTemplateReader reader, params Type[] mixinTypes)
        {
            Scanner scanner = new Scanner(reader.Output);
            Log.Debug("StringTemplate::Compile:");
            Log.Debug(reader.Output);
            Parser parser = new Parser(scanner);
            parser.Parse();
            if (parser.Errors.Count != 0)
            {
                throw new StringTemplateException("Parser error: " + parser.Errors);
            }
            Compiler compiler = new Compiler();
            Type compiledType = compiler.Compile(parser.Unit, typeof(StringTemplate), true, false, mixinTypes);
            return compiledType;
        }

        public static StringTemplate Instantiate(Type compiledType, params object[] mixins)
        {
            return (StringTemplate)Activator.CreateInstance(compiledType, Compiler.GetInstances(mixins));
        }

        protected Stack _stack = new Stack();
        protected TextWriter _writer;
        protected Hashtable _variables = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

        protected StringTemplate()
        {
        }
        
        public string Render()
        {
            using (StringWriter writer = new StringWriter())
            {
                Render(writer);
                return writer.ToString();
            }
        }

        public virtual void Render(TextWriter writer)
        {
            _writer = writer;
        }

        public object Get(string identifier)
        {
            return _variables[identifier];
        }

        public void Set(string identifier, object value)
        {
            _variables[identifier] = value;
        }

        public object this[string identifier]
        {
            get { return _variables[identifier]; }
            set { _variables[identifier] = value; }
        }

        protected void Push()
        {
            _stack.Push(_writer);
            _writer = new StringWriter();
        }

        protected void Pop()
        {
            TextWriter popped = (TextWriter)_stack.Pop();
            popped.Write(_writer.ToString());
            _writer = popped;
        }

        protected void Write(string s)
        {
            _writer.Write(s);
        }

        protected void WriteLine(string s)
        {
            _writer.WriteLine(s);
        }
    }

    /// <summary>
    /// Preprocessor for StringTemplateException sources
    /// </summary>
    public class StringTemplateException : Exception
    {
        public StringTemplateException(string message) : base(message) {}
        public StringTemplateException(string message, int line, int col) : base(message + " line: " + line + " col: " + col) {}
    }

    /// <summary>
    /// Preprocessor for StringTemplate sources
    /// </summary>
    public class StringTemplateReader : BaseTokenizer
    {
        protected StringBuilder output;

        public StringTemplateReader(string data): base(data)
        {
        }

        public virtual string Output
        {
            get 
            { 
                if (output == null)
                    Read();
                return output.ToString();
            }
        }

        public override string ToString()
        {
            return Output;
        }

        void Read()
        {
            output = new StringBuilder();
            while (true)
            {
                ReadText();
                char ch = LA(0);
                if (ch == '#')
                {
                    ReadStatement();
                }
                else if (ch == '$')
                {
                    ReadExpression();
                }
                else if (ch == EOF)
                {
                    break;
                }
            }
        }

        void ReadText()
        {
            StartRead();
            bool start = column == 1;
            while (true)
            {
                char ch = LA(0);
                if (ch == '#' && Char.IsLetter(LA(1)))
                {
                    EmitText(start);
                    break;
                }
                if (ch == '$' && (LA(1) == '{' || Char.IsLetter(LA(1))))
                {
                    EmitText(start);
                    break;
                }
                if (ch == EOF)
                {
                    EmitText(start);
                    break;
                }
                if (ch != ' ' && ch != '\t')
                {
                    start = false;
                }
                Consume();
                if (ch == '\r' || ch == '\n')
                {
                    EmitText();
                    EmitCrLf();
                    StartRead();
                    start = true;
                }
            }
        }

        void ReadStatement()
        {
            Consume();
            StartRead();
            while (true)
            {
                char ch = LA(0);
                if (ch == '#')
                {
                    EmitStatement();
                    Consume();
                    break;
                }
                if (ch == '\r' || ch == '\n' || ch == EOF)
                {
                    EmitStatement();
                    EmitCrLf();
                    if (ch != EOF)
                        Consume();
                    break;
                }
                Consume();
            }
        }

        void ReadExpression()
        {
            Consume();
            if (LA(0) == '{')
            {
                StartRead();
                ReadPaired("{}()[]", false, true, false, false);
                EmitExpression();
            }
            else
            {
                StartRead();
                while (true)
                {
                    char ch = LA(0);
                    if (ch == '(' || ch == '[')
                    {
                        ReadPaired("()[]", false, true, false, false);
                    }
                    else if (ch != '.' && !Char.IsLetterOrDigit(ch))
                    {
                        EmitExpression();
                        break;
                    }
                    else
                    {
                        Consume();
                    }
                }
            }
        }

        void EmitCrLf()
        {
            output.Append("\r\n");
        }

        void EmitText()
        {
            EmitText(false);
        }
        
        void EmitText(bool ignoreWhiteSpace)
        {
            if (savePos < pos)
            {
                if (ignoreWhiteSpace && IsWhiteSpace(data, savePos, pos - savePos))
                    return;
                output.Append("put \"");
                EmitSafeText(output, data, savePos, pos - savePos);
                output.Append("\" ");
            }
        }

        bool IsWhiteSpace(string data, int index, int count)
        {
            while (count > 0)
            {
                if (data[index] != ' ' && data[index] != '\t')
                    return false;
                index++;
                count--;
            }
            return true;
        }

        protected virtual void EmitSafeText(StringBuilder output, string data, int index, int count)
        {
            while (count > 0)
            {
                switch (data[index])
                {
                    case '\\':
                        output.Append("\\\\");
                        break;
                    case '\r':
                        output.Append("\\r");
                        break;
                    case '\n':
                        output.Append("\\n");
                        break;
                    case '"':
                        output.Append("\\\"");
                        break;
                    default:
                        output.Append(data[index]);
                        break;
                }
                index++;
                count--;
            }
        }

        protected virtual void EmitStatement()
        {
            output.Append(data, savePos, pos - savePos);
            output.Append(" ");
        }
        
        protected virtual void EmitExpression()
        {
            output.Append("put ");
            if (data[savePos] == '{')
                output.Append(data, savePos + 1, pos - savePos - 2);
            else
                output.Append(data, savePos, pos - savePos);
            output.Append(" ");
        }
    }

    /// <summary>
    /// Preprocessor for StringTemplate sources from file.
    /// </summary>
    public class StringTemplateLoader : StringTemplateReader
    {
        string path;

        public StringTemplateLoader(string path) : base(Load(path))
        {
            this.path = Path.GetFullPath(path);
        }
        
        protected override void EmitStatement()
        {
            if (string.Compare(data, savePos, "include ", 0, 8) == 0)
            {
                int i = data.IndexOfAny(new char[]{'"','\''}, savePos, pos - savePos);
                int j = data.IndexOf(data[i], i+1, pos - i - 1);
                string include = Path.Combine(Path.GetDirectoryName(path), data.Substring(i + 1, j - i - 1));
                StringTemplateLoader loader = new StringTemplateLoader(include);
                output.Append(loader.Output);
            } 
            else
            {
                base.EmitStatement();
            }
        }

        static string Load(string path)
        {
            Log.Debug("StringTemplateLoader::Load " + path);
            using (TextReader reader = File.OpenText(path))
                return reader.ReadToEnd();
        }
    }
}
