using System;
using System.IO;

namespace Glue.Lib.Text.JSON
{
	/// <summary>
	/// Static helper function for JSON notation.
	/// </summary>
	public class Helper
	{
        public static object Parse(string text)
        {
            Parser parser = new Parser(new Scanner(text));
            parser.Parse();
            if (parser.Errors.Count > 0)
                throw new Exception("Error parsing JSON.: " + parser.Errors);
            return parser.Result;
        }
        
        public static object Parse(TextReader reader)
        {
            Parser parser = new Parser(new Scanner(reader.ReadToEnd()));
            parser.Parse();
            if (parser.Errors.Count > 0)
                throw new Exception("Error parsing JSON.: " + parser.Errors);
            return parser.Result;
        }

        private Helper() {}
    }
}
