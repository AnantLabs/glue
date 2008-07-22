using System;
using System.IO;

namespace Glue.Lib.Text
{
    /// <summary>
    /// StringTokenizer tokenized string (or stream) into tokens.
    /// </summary>
    public class BaseTokenizer
    {
        public const char EOF = (char)0;

        protected int line;
        protected int column;
        protected int pos;	// position within data

        protected string data;

        protected int saveLine;
        protected int saveCol;
        protected int savePos;

        public BaseTokenizer(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.data = data;

            line = 1;
            column = 1;
            pos = 0;
        }

        /// <summary>
        /// Return char in lookahead buffer
        /// </summary>
        protected char LA(int count)
        {
            if (pos + count >= data.Length)
                return EOF;
            else
                return data[pos+count];
        }

        /// <summary>
        /// Consume character and update line and column positions.
        /// </summary>
        protected char Consume()
        {
            char ret = data[pos];
            if (ret == '\r' || ret == '\n')
            {
                column = 1;
                line++;
            }
            else
            {
                column++;
            }
            pos++;
            if (ret == '\r' && pos < data.Length && data[pos] == '\n')
                pos++;
            return ret;
        }

        /// <summary>
        /// save read point positions so that CreateToken can use those
        /// </summary>
        protected void StartRead()
        {
            saveLine = line;
            saveCol = column;
            savePos = pos;
        }

        /// <summary>
        /// Returns the token as read from the last call to StartRead
        /// </summary>
        protected string GetToken()
        {
            return data.Substring(savePos, pos - savePos);
        }

        /// <summary>
        /// Utility function to eat paired stuff
        /// </summary>
        protected void ReadPaired(string pairs, bool multiLine, bool stringEscapeSlash, bool stringEscapeDouble, bool stringMultiLine)
        {
            char ch = LA(0);
            int i = pairs.IndexOf(ch);
            if (i % 2 != 0)
                throw new ArgumentException("Cannot find corresponding stop character for '" + ch + "' at line: " + line);

            char stop = pairs[i+1];
            Consume();
            
            while (true)
            {
                ch = LA(0);
                if (ch == EOF || ((ch == '\r' || ch == '\n') && !multiLine))
                {
                    throw new InvalidOperationException("Unexpected end of data looking for '" + stop + "' at line: " + line);
                }
                else if (ch == stop)
                {
                    Consume();
                    break;
                }
                else if (ch == '"' || ch == '\'')
                {
                    ReadString(stringEscapeSlash, stringEscapeDouble, stringMultiLine);
                }
                else
                {
                    i = pairs.IndexOf(ch);
                    if (i % 2 == 0)
                    {
                        ReadPaired(pairs, multiLine, stringEscapeSlash, stringEscapeDouble, stringMultiLine);
                    }
                    else
                    {
                        Consume();
                    }
                }
            }
        }

        /// <summary>
        /// Utility function to eat string
        /// </summary>
        protected void ReadString(bool escapeSlash, bool escapeDouble, bool multiLine)
        {
            char quote = Consume();
            while (true)
            {
                char ch = LA(0);
                if (ch == EOF || ((ch == '\r' || ch == '\n') && !multiLine))
                {
                    throw new InvalidOperationException("Unexpected end of string at line: " + line);
                }
                else if (ch == '\\' && escapeSlash)
                {
                    Consume();
                    if (LA(0) == quote)
                        Consume();
                }
                else if (ch == quote)
                {
                    Consume();
                    if (LA(0) == quote && escapeDouble)
                        Consume();
                    else
                        break;
                }
                else
                {   
                    Consume();
                }
            }
        }
    }
}
