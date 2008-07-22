using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Glue.Lib.Mime
{
    /// <summary> 
    /// TransferEncoding abstract base class. MIME Content-Transfer-Encoding
    /// helper classes are derived from this one. For more information see RFC 1521.
    /// </summary>
    public abstract class TransferEncoding
    {
        public static readonly Bit7TransferEncoding Bit7 = new Bit7TransferEncoding();
        public static readonly Bit8TransferEncoding Bit8 = new Bit8TransferEncoding();
        public static readonly QuotedPrintableTransferEncoding QuotedPrintable = new QuotedPrintableTransferEncoding();
        public static readonly Base64TransferEncoding Base64 = new Base64TransferEncoding();
        public static readonly BinaryTransferEncoding Binary = new BinaryTransferEncoding();

        public static TransferEncoding Get(string name)
        {
            switch (name.ToLower())
            {
                case "7bit":
                    return Bit7;
                case "quoted-printable":
                    return QuotedPrintable;
                case "base64":
                    return Base64;
                case "8bit":
                    return Bit8;
                case "binary":
                    return Binary;
                default:
                    return null;
            }
        }

        public abstract string Name { get; }
        public abstract int Encode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut);
        public abstract int EncodeLength(byte[] input, int offsetIn, int length);
        public abstract int Decode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut);
        public abstract int DecodeLength(byte[] input, int offsetIn, int length);

        public virtual byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodeLength(input, 0, input.Length)];
            Encode(input, 0, input.Length, output, 0);
            return output;
        }
        
        public virtual void Encode(Stream input, Stream output)
        {
            int n = Convert.ToInt32(input.Length - input.Position);
            byte[] b = new byte[n];
            input.Read(b, 0, n);
            b = Encode(b);
            output.Write(b, 0, b.Length);
        }

        public virtual byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodeLength(input, 0, input.Length)];
            Decode(input, 0, input.Length, output, 0);
            return output;
        }

        public virtual void Decode(Stream input, Stream output)
        {
            int n = Convert.ToInt32(input.Length - input.Position);
            byte[] b = new byte[n];
            input.Read(b, 0, n);
            b = Decode(b);
            output.Write(b, 0, b.Length);
        }
    }

    /// <summary> 
    /// MIME Content-Transfer-Encoding: 7bit
    /// For more information, see RFC 1521.
    /// </summary>
    public class Bit7TransferEncoding : TransferEncoding
    {
        public override string Name
        {
            get { return "7bit"; }
        }
        public override int Encode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            System.Buffer.BlockCopy(input, offsetIn, output, offsetOut, length);
            return length;
        }
        public override int EncodeLength(byte[] input, int offsetIn, int length)
        {
            return length;
        }
        public override int Decode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            System.Buffer.BlockCopy(input, offsetIn, output, offsetOut, length);
            return length;
        }
        public override int DecodeLength(byte[] input, int offsetIn, int length)
        {
            return length;
        }
    }

    /// <summary> 
    /// MIME Content-Transfer-Encoding: 8bit
    /// For more information, see RFC 1521.
    /// </summary>
    public class Bit8TransferEncoding : TransferEncoding
    {
        public override string Name
        {
            get { return "8bit"; }
        }
        public override int Encode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            System.Buffer.BlockCopy(input, offsetIn, output, offsetOut, length);
            return length;
        }
        public override int EncodeLength(byte[] input, int offsetIn, int length)
        {
            return length;
        }
        public override int Decode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            System.Buffer.BlockCopy(input, offsetIn, output, offsetOut, length);
            return length;
        }
        public override int DecodeLength(byte[] input, int offsetIn, int length)
        {
            return length;
        }
    }

    /// <summary> 
    /// MIME Content-Transfer-Encoding: binary
    /// For more information, see RFC 1521.
    /// </summary>
    public class BinaryTransferEncoding : TransferEncoding
    {
        public override string Name
        {
            get { return "binary"; }
        }
        public override int Encode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            System.Buffer.BlockCopy(input, offsetIn, output, offsetOut, length);
            return length;
        }
        public override int EncodeLength(byte[] input, int offsetIn, int length)
        {
            return length;
        }
        public override int Decode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            System.Buffer.BlockCopy(input, offsetIn, output, offsetOut, length);
            return length;
        }
        public override int DecodeLength(byte[] input, int offsetIn, int length)
        {
            return length;
        }
    }

    /// <summary> 
    /// MIME Content-Transfer-Encoding: quoted-printable
    /// For more information, see RFC 1521.
    /// </summary>
    public class QuotedPrintableTransferEncoding : TransferEncoding
    {
        public const int QP_MAX_LINE_LENGTH = 76;

        private bool encodeSmtpDot = true;
        private bool trailingSoftCrLf = true;

        public override string Name
        {
            get { return "quoted-printable"; }
        }
        public override int Encode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            char[] map = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};
			
            if (input == null)
                return 0;

            int end = offsetIn + length;
            int i = offsetIn;
            int j = offsetOut;
            int linelen = 0;

            while (i < end)
            {
                byte ch = input[i++];
                if (linelen == 0 && ch == '.' && encodeSmtpDot)
                {
                    output[j++] = ch;
                    linelen++;
                }
                if ((ch > 32 && ch < 61) || (ch > 61 && ch < 127))
                {
                    output[j++] = ch;
                    linelen++;
                }
                else if ((ch == ' ' || ch == '\t') && (linelen < QP_MAX_LINE_LENGTH - 12))
                {
                    output[j++] = ch;
                    linelen++;
                }
                else
                {
                    output[j++] = (byte)'=';
                    output[j++] = (byte)map[(ch >> 4) & 0x0F];
                    output[j++] = (byte)map[ch & 0x0F];
                    linelen += 3;
                }
                if (linelen >= QP_MAX_LINE_LENGTH - 11)
                {
                    output[j++] = (byte)'=';
                    output[j++] = (byte)'\r';
                    output[j++] = (byte)'\n';
                    linelen = 0;
                }
            }
            if (trailingSoftCrLf)
            {
                output[j++] = (byte)'=';
                output[j++] = (byte)'\r';
                output[j++] = (byte)'\n';
            }
            return j - offsetOut;
        }
        public override int EncodeLength(byte[] input, int offsetIn, int length)
        {
            char[] map = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};
			
            if (input == null)
                return 0;

            int end = offsetIn + length;
            int i = offsetIn;
            int j = 0;
            int linelen = 0;

            while (i < end)
            {
                byte ch = input[i++];
                if (linelen == 0 && ch == '.' && encodeSmtpDot)
                {
                    j++;
                    linelen++;
                }
                if ((ch > 32 && ch < 61) || (ch > 61 && ch < 127))
                {
                    j++;
                    linelen++;
                }
                else if ((ch == ' ' || ch == '\t') && (linelen < QP_MAX_LINE_LENGTH - 12))
                {
                    j++;
                    linelen++;
                }
                else
                {
                    j++;
                    j++;
                    j++;
                    linelen += 3;
                }
                if (linelen >= QP_MAX_LINE_LENGTH - 11)
                {
                    j++;
                    j++;
                    j++;
                    linelen = 0;
                }
            }
            if (trailingSoftCrLf)
            {
                j++;
                j++;
                j++;
            }
            return j;
        }
        public override int Decode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            char[] map = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};
			
            if (input == null)
                return 0;

            int end = offsetIn + length;
            int i = offsetIn;
            int j = offsetOut;
            int linelen = -1;

            while (i < end)
            {
                byte ch = input[i++];
                linelen++;
                if (ch == '=')
                {
                    if (i < end && char.IsLetterOrDigit((char)input[i]))
                    {
                        output[j++] = (byte)(
                            ((input[i+0] >= 'A' ? (input[i+0] & 0x0F) + 9 : input[i+0] & 0x0F) << 4) +
                            (input[i+1] >= 'A' ? (input[i+1] & 0x0F) + 9 : input[i+1] & 0x0F)
                            );
                        i += 2;
                        continue;
                    }
                    if (i < end && input[i] == '\r' && i + 1 < end && input[i+1] == '\n')
                    {
                        i += 2;
                        linelen = -1;
                        continue;
                    }
                    return 0;
                }
                if (ch == '\r' || ch == '\n')
                {
                    linelen = -1;
                }
                if (ch == '.' && linelen==0 && encodeSmtpDot)
                {
                    continue;
                }
                output[j++] = ch;
            }
            
            return j - offsetOut;
        }
        public override int DecodeLength(byte[] input, int offsetIn, int length)
        {
            char[] map = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};
			
            if (input == null)
                return 0;

            int end = offsetIn + length;
            int i = offsetIn;
            int j = 0;
            int linelen = -1;

            while (i < end)
            {
                byte ch = input[i++];
                linelen++;
                if (ch == '=')
                {
                    if (i < end && char.IsLetterOrDigit((char)input[i]))
                    {
                        j++;
                        i += 2;
                        continue;
                    }
                    if (i < end && input[i] == '\r' && i + 1 < end && input[i+1] == '\n')
                    {
                        i += 2;
                        linelen = -1;
                        continue;
                    }
                    return 0;
                }
                if (ch == '\r' || ch == '\n')
                {
                    linelen = -1;
                }
                if (ch == '.' && linelen==0 && encodeSmtpDot)
                {
                    continue;
                }
                j++;
            }
            
            return j;
        }
    }

    /// <summary> 
    /// MIME Content-Transfer-Encoding: base64
    /// For more information, see RFC 1521.
    /// </summary>
    public class Base64TransferEncoding : TransferEncoding
    {
        public override string Name
        {
            get { return "base64"; }
        }
        public override int Encode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            MemoryStream inputStream = new MemoryStream(input, offsetIn, length, false);
            MemoryStream outputStream = new MemoryStream();
            Encode(inputStream, outputStream);
            byte[] b = outputStream.ToArray();
            Buffer.BlockCopy(b, 0, output, offsetOut, b.Length);
            return b.Length;
        }
        public override byte[] Encode(byte[] input)
        {
            MemoryStream inputStream = new MemoryStream(input);
            MemoryStream outputStream = new MemoryStream();
            Encode(inputStream, outputStream);
            return outputStream.ToArray();
        }
        public override int EncodeLength(byte[] input, int offsetIn, int length)
        {
            throw new NotImplementedException();
        }
        public override void Encode(Stream input, Stream output)
        {
            if ( (input == null) || (output == null) )
                throw new ArgumentNullException("The input and output streams may not be null.");
	    
            ICryptoTransform base64 = new ToBase64Transform();
                    
            byte[] inputBuf = new byte[base64.InputBlockSize];
            byte[] outputBuf = new byte[base64.OutputBlockSize];

            int count = 0;
            byte[] newln = new byte[] { 13 , 10 }; //CR LF with mail

            // Read through the stream until there 
            // are no more bytes left
            while (true) 
            {
                // Read some bytes, break on EOF
                int read = input.Read(inputBuf, 0, inputBuf.Length);
                if (read < 1) 
                    break;
            
                // Transfrom and write the blocks. If the block size
                // is less than the InputBlockSize then write the final block
                if (read == inputBuf.Length) 
                {
                    // Transform and write
                    // int transformed = base64.TransformBlock(inputBuf, 0, inputBuf.Length, outputBuf, 0);
                    // output.Write(outputBuf, 0, transformed );
                    base64.TransformBlock(inputBuf, 0, inputBuf.Length, outputBuf, 0);
                    output.Write(outputBuf, 0, outputBuf.Length);
                        
                    // Do this to output lines that are max 60 chars long
                    count += outputBuf.Length;
                    if (count == 60) 
                    {
                        output.Write( newln , 0 , newln.Length );
                        count = 0;
                    }
                } 
                else 
                {
                    // Convert the final blocks of bytes and write them.
                    outputBuf = base64.TransformFinalBlock(inputBuf, 0, read);
                    output.Write(outputBuf, 0, outputBuf.Length);
                }
            } 
            // Append newline
            output.Write(newln, 0, newln.Length);
        }
        public override int Decode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut)
        {
            byte[] b = Convert.FromBase64CharArray(MimeUtility.BytesToChars(input, offsetIn, length), 0, length);
            Buffer.BlockCopy(b, 0, output, offsetOut, b.Length);
            return b.Length;
        }
        public override int DecodeLength(byte[] input, int offsetIn, int length)
        {
            byte[] b = Convert.FromBase64CharArray(MimeUtility.BytesToChars(input, offsetIn, length), 0, length);
            return b.Length;
        }
        public override void Decode(Stream input, Stream output)
        {
            ICryptoTransform base64 = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
            byte[] inputBuf = new byte[base64.InputBlockSize];
            byte[] outputBuf = new byte[base64.OutputBlockSize];

            // Read through the stream until there 
            // are no more bytes left
            while (true) 
            {
                // Read some bytes, break on EOF
                int read = input.Read(inputBuf, 0, inputBuf.Length);
                if (read < 1) 
                    break;
            
                // Transfrom and write the blocks. If the block size
                // is less than the InputBlockSize then write the final block
                if (read == inputBuf.Length)
                {
                    // Tansform and write
                    // int transformed = base64.TransformBlock(inputBuf, 0, inputBuf.Length, outputBuf, 0);
                    // output.Write(outputBuf, 0, transformed );
                    base64.TransformBlock(inputBuf, 0, inputBuf.Length, outputBuf, 0);
                    output.Write(outputBuf, 0, outputBuf.Length);
                } 
                else 
                {
                    // Convert the final blocks of bytes and write them
                    outputBuf = base64.TransformFinalBlock(inputBuf, 0, read);
                    output.Write(outputBuf, 0, outputBuf.Length);
                }
            } 
        }
    }
}
