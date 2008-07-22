using System;

namespace Glue.Lib
{
	/// <summary>
	/// BinHexEncoding class.
	/// </summary>
    public class BinHexEncoding 
    {
        public static byte[] Decode(char[] chars)
        {
            return Decode(chars, 0, chars.Length);
        }
        
        public static byte[] Decode(char[] chars, int index, int count)
        {
            byte[] bytes = new byte[count / 2];
            Decode(chars, index, count, bytes);
            return bytes;
        }
        
        public static int Decode(char[] chars, int index, int count, byte[] output)
        {
            for (int i = 0; i < count / 2; i++)
            {
                int b = chars[index++] & 0x4F;
                if (b >= 0x40)
                    b = b - 55;
                output[i] = (byte)(b << 4);
                b = chars[index++] & 0x4F;
                if (b >= 0x40)
                    b = b - 55;
                output[i] |= (byte)b;
            }
            return count / 2;
        }
        
        public static char[] Encode(byte[] bytes)
        {
            return Encode(bytes, 0, bytes.Length);
        }
        
        public static char[] Encode(byte[] bytes, int index, int count)
        {
            char[] result = new char[2 * count];
            Encode(bytes, index, count, result);
            return result;
        }
        
        public static int Encode(byte[] bytes, int index, int count, char[] output)
        {
            const string hexchars = "0123456789ABCDEF";
            int j = 0;
            for (int i = index; i < count; i++)
            {
                byte b = bytes[i];
                output[j++] = hexchars[b >> 4];
                output[j++] = hexchars[b & 0xF];
            }
            return j;
        }
    }
}
