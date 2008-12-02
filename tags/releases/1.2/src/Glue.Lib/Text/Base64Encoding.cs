using System;

namespace Glue.Lib
{
	/// <summary>
	/// BinHexEncoding class.
	/// </summary>
    public class Base64Encoding 
    {
        public static byte[] Decode(char[] chars)
        {
            return Convert.FromBase64CharArray(chars, 0, chars.Length);
        }
        
        public static byte[] Decode(char[] chars, int index, int count)
        {
            return Convert.FromBase64CharArray(chars, index, count);
        }
        
        public static int Decode(char[] chars, int index, int count, byte[] output)
        {
            byte[] data = Convert.FromBase64CharArray(chars, index, count);
            Buffer.BlockCopy(data, 0, output, 0, data.Length);
            return data.Length;
        }
        
        public static char[] Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).ToCharArray();
        }
        
        public static char[] Encode(byte[] bytes, int index, int count)
        {
            return Convert.ToBase64String(bytes, index, count).ToCharArray();
        }
        
        public static int Encode(byte[] bytes, int index, int count, char[] output)
        {
            return Convert.ToBase64CharArray(bytes, index, count, output, 0);
        }
    }
}
