using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Edf.Lib.Xml
{
	/// <summary>
	/// Summary description for TextualXmlLexer.
	/// </summary>
    public enum TextualXmlToken
    {
        Indent,
        Comment,
        Equal,
        Word,
        String,
        EOF
    }

    public class TextualXmlLexer : BaseLexer
    {
        private string value = null;

        /// <summary>
        /// Construct with given input. 
        /// </summary>
        public TextualXmlLexer(TextReader input) : base(input)
        {
        }
    
        public string Value
        {
            get { return value; }
        }
        
        public TextualXmlToken Token()
        {
            while (next != -1)
            {
                if (next == '\n')
                {
                    return ReadIndent();
                }
                else if (next == '"')
                {
                    return ReadString();
                }
                else if (next == '\'')
                {
                    return ReadString();
                }
                else if (next == '#')
                {
                    return ReadComment();
                }
                else if (next == '=')
                {
                    return TextualXmlToken.Equal;
                }
                else if (!char.IsWhiteSpace((char)next))
                {
                    return ReadValue();
                }
                More();
            }
            return TextualXmlToken.EOF;
        }

        private TextualXmlToken ReadIndent()
        {
            int n = 0;
            while (next != -1 && char.IsWhiteSpace((char)next))
            {
                if (next == '\n')
                    n = 0;
                else if (next == '\t')
                    n += 4;
                else
                    n++;
                More();
            }
            value = new string(' ', n);
            return TextualXmlToken.Indent;
        }
        
        private TextualXmlToken ReadValue()
        {
            StringBuilder s = new StringBuilder();
            while (next != -1 && !char.IsWhiteSpace((char)next) && next != '#' && next != '\'' && next != '"')
            {
                s.Append(next);
                More();
            }
            value = s.ToString();
            return TextualXmlToken.Word;
        }

        private TextualXmlToken ReadString()
        {
            if (next == '\'')
            {
                return ReadSingleTerminatedString('\'');
            }
            int n = 0;
            while (next != -1 && next == '"')
            {
                n++;
                More();
            }
            if (n == 2 || n == 6)
            {
                value = string.Empty;
                return TextualXmlToken.String;
            }
            if (n == 1)
            {
                return ReadSingleTerminatedString('"');
            }
            if (n != 3)
            {
                Error("Mismatching quotes.");
            }
            StringBuilder s = new StringBuilder();
            n = 0;
            while (next != -1)
            {
                if (next != '"')
                    n = 0;
                else if (++n == 3)
                    break;
                s.Append(next);
                More();
            }
            value = s.ToString();
            return TextualXmlToken.String;
        }

        private TextualXmlToken ReadSingleTerminatedString(char terminator)
        {
            StringBuilder s = new StringBuilder();
            More();
            while (next != -1 && next == terminator)
            {
                s.Append(next);
                More();
            }
            value = s.ToString();
            return TextualXmlToken.String;
        }

        private TextualXmlToken ReadComment()
        {
            StringBuilder s = new StringBuilder();
            More();
            while (next != -1 && next == '\n')
            {
                s.Append(next);
                More();
            }
            value = s.ToString();
            return TextualXmlToken.Comment;
        }
    }
}
