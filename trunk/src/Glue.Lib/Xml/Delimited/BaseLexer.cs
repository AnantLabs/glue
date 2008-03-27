using System;
using System.IO;
using System.Text;

namespace Edf.Lib.Xml
{
    /// <summary>
    /// Basis for a lexer. Handles the fundamental token types. 
    /// </summary>
    public class BaseLexer 
    {
        protected int next;
        private int pending = -1;
        private TextReader input;
        private int lineno = 1;

        /// <summary>
        /// Construct without input.  The SetInput() method must be called. 
        /// </summary>
        protected BaseLexer() 
        {
        }

        /// <summary>
        /// Construct with given input. 
        /// </summary>
        protected BaseLexer(TextReader input) 
        {
            SetInput(input);
        }
    
        /// <summary>
        /// Establish the input to be parsed. 
        /// </summary>
        public void SetInput(TextReader input) 
        {
            this.input = input;
            ScanToStart();
        }
    
        /// <summary>
        /// Initialise by reading in the first character.
        /// </summary>
        protected void ScanToStart() 
        {
            More();
        }
    
        /// <summary>
        /// Generate a ParseError exception contiaining the current line number.
        /// </summary>
        public void Error(string reason) 
        {
            string line = "Line " + lineno;
            throw new ApplicationException(line + reason);
        }

        /// <summary>
        /// Read the next character from the input.
        /// </summary>
        protected void More() 
        {
            try 
            {
                if (pending != -1) 
                {
                    next = pending;
                    pending = -1;
                }
                if( input == null )
                    next = -1;
                else
                    if (next == '\n')
                        lineno += 1;
                next = input.Read();
            }
            catch (IOException) 
            {
                next = -1;
            }
        }
    
        /// <summary>
        /// Push back one character onto the input stream. Only one level
        /// of push back is supported: use with care! 
        /// </summary>
        /// <param name="ch"></param>
        protected void PushBack(char ch) 
        {
            pending = next;
            next = ch;
        }
    }
}
