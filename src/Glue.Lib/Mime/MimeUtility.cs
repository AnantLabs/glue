using System;
using System.IO;
using System.Text;

namespace Glue.Lib.Mime
{
    /// <summary>
    /// Utility functions
    /// </summary>
    public class MimeUtility
    {
        /// <summary>
        /// The iso-8859-1 encoding (Western European (ISO), codepage 28591)
        /// is isomorph between .NET native string and a bytearray. That is,
        /// for each byte value 0..255 the corresponding char value in a
        /// string will be the same (between 0 and 255).
        /// 
        /// In short:
        ///   bytes == ISO.GetBytes(ISO.GetString(bytes));
        /// </summary>
        public static readonly Encoding ISO = Encoding.GetEncoding("iso-8859-1");
        
        /// <summary>
        /// Determines if a string contains only simple 7bit US ASCII 
        /// characters. Useful to determine if text needs to be encoded
        /// before stuffing it inside a MIME part.
        /// </summary>
        public static bool IsASCII(string s)
        {
            int n = s.Length;
            for (int i = 0; i < n; i++)
            {
                int c = s[i];
                if (!(c == 9 || c == 10 || c == 13 || c >= 32 && c < 127))
                    return false;
            }
            return true;
        }

        static Random random = new Random();

        /// <summary>
        /// Generates a boundary suitable for multipart MIME messages.
        /// </summary>
        public static string GenerateBoundary()
        {
            StringBuilder boundary = new StringBuilder("__Part__");
	    
            DateTime now = DateTime.Now;
            boundary.Append(now.Year);
            boundary.Append(now.Month);
            boundary.Append(now.Day);
            boundary.Append(now.Hour);
            boundary.Append(now.Minute);
            boundary.Append(now.Second);
            boundary.Append(now.Millisecond);
	    	    
            boundary.Append("__");
            boundary.Append(random.Next());
            boundary.Append("__");
	    
            return boundary.ToString();
        }

        /// <summary>
        /// Reads lines until a MIME boundary is hit, returns both the 
        /// text and the last line read.
        /// </summary>
        public static string ReadUntil(TextReader reader, string boundary, out string last)
        {
            StringBuilder text = new StringBuilder();
            string line = reader.ReadLine();
            while (line != null)
            {
                if (boundary != null && 
                    (line == "--" + boundary || line == "--" + boundary + "--"))
                    break;
                text.Append(line).Append("\r\n");
                line = reader.ReadLine();
            }
            last = line;
            return text.ToString();
        }

        /// <summary>
        /// Reads lines until a MIME boundary is hit, returns both the 
        /// text (in raw 8-bit ASCII bytes) and the last line read.
        /// </summary>
        public static byte[] ReadBytesUntil(TextReader reader, string boundary, out string last)
        {
            MemoryStream bytes = new MemoryStream();
            string line = reader.ReadLine();
            while (line != null)
            {
                if (boundary != null && 
                    (line == "--" + boundary || line == "--" + boundary + "--"))
                    break;
                byte[] b = StringToBytes(line + "\r\n");
                bytes.Write(b, 0, b.Length);
                line = reader.ReadLine();
            }
            last = line;
            return bytes.ToArray();
        }

        /// <summary>
        /// Writes a raw 8-bit ASCII string to output and appends a CRLF newline.
        /// </summary>
        public static void WriteLine(Stream output, string line)
        {
            byte[] b = StringToBytes(line + "\r\n");
            output.Write(b, 0, b.Length);
        }

        /// <summary>
        /// Converts bytes to a raw 8-bit ASCII character array.
        /// </summary>
        public static int BytesToChars(byte[] input, int offsetIn, int length, char[] output, int offsetOut)
        {
            for (int i = offsetIn, j = offsetOut, n = length; n > 0; i++, j++, n--)
                output[j] = (char)input[i];
            return length;
        }

        /// <summary>
        /// Converts bytes to a raw ASCII character array.
        /// </summary>
        public static char[] BytesToChars(byte[] b, int offset, int length)
        {
            char[] c = new char[length];
            for (int i = 0, j = offset; i < length; i++, j++)
                c[i] = (char)b[j];
            return c;
        }

        /// <summary>
        /// Converts bytes to a raw 8-bit ASCII character array.
        /// </summary>
        public static char[] BytesToChars(byte[] b)
        {
            return BytesToChars(b, 0, b.Length);
        }

        /// <summary>
        /// Converts 8-bit ASCII characters to a byte array.
        /// </summary>
        public static int CharsToBytes(char[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            for (int i = offsetIn, j = offsetOut, n = length; n > 0; i++, j++, n--)
                output[j] = (byte)input[i];
            return length;
        }

        /// <summary>
        /// Converts raw 8-bit ASCII characters to a byte array.
        /// </summary>
        public static byte[] CharsToBytes(char[] c, int offset, int length)
        {
            byte[] b = new byte[length];
            for (int i = 0, j = offset; i < length; i++, j++)
                b[i] = (byte)c[j];
            return b;
        }

        /// <summary>
        /// Converts raw ASCII characters to a byte array.
        /// </summary>
        public static byte[] CharsToBytes(char[] c)
        {
            return CharsToBytes(c, 0, c.Length);
        }

        /// <summary>
        /// Converts raw 8-bit ASCII string to a byte array.
        /// </summary>
        public static byte[] StringToBytes(string s, int offset, int length)
        {
            byte[] b = new byte[length];
            for (int i = 0, j = offset; i < length; i++, j++)
                b[i] = (byte)s[j];
            return b;
        }
        
        /// <summary>
        /// Converts raw 8-bit ASCII string to a byte array.
        /// </summary>
        public static byte[] StringToBytes(string s)
        {
            return StringToBytes(s, 0, s.Length);
        }
    }
}
