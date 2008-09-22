//
// Glue.Lib.Mail.MailUtil.cs
//
//
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Glue.Lib.Mail 
{
    
    /// <summary>
    /// This class contains some utillity functions
    /// that dont fit in other classes and to keep
    /// high cohesion on the other classes.
    /// </summary>
    public class MailUtil 
    {
	
        /// <summary>
        /// determines if a string needs to
        /// be encoded for transfering over
        /// the smtp protocol without risking
        /// that it would be changed.
        /// </summary>
        public static bool NeedEncoding(string str) 
        {
            foreach (char chr in str)
            {
                if ( ! ( (chr > 61) && (chr < 127) || (chr>31) && (chr<61) ) ) 
                {
                    return true;
                }
            }
            return false;
        }

        static Random random = new Random();

        /// <summary>
        /// Generate a unique boundary
        /// </summary>
        public static string GenerateBoundary() 
        {
            StringBuilder  boundary = new StringBuilder("__Part__");
	    
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


        //
        // No encoding (plain copy)
        //

        /// <summary>
        /// Copies from one stream to another.
        /// </summary>
        public static void StreamCopy(Stream ins , Stream outs) 
        {
            int chunk = 0x2000;
            byte[] buffer = new byte[chunk];
            int read = ins.Read(buffer, 0, chunk);
            while (read > 0)
            {
                outs.Write(buffer, 0, read);
                if (chunk < 0x10000)
                {
                    chunk *= 2;
                    buffer = new byte[chunk];
                }
                read = ins.Read(buffer, 0, chunk);
            }
        }

        //
        // Quoted-Printable encoding
        //

        const int QP_MAX_LINE_LENGTH  = 76;

        /// <summary>
        /// Returns max number of bytes necessary for storing QP encoded string
        /// </summary>
        internal static int QPEncodeMaxBytes(int length)
        {
            int n = 3*((3*length)/(QP_MAX_LINE_LENGTH -8));
            n += 3*length;
            n += 3;
            return n;
        }

        /// <summary>
        /// Encodes to Quoted-Printable string (see RFC 1521)
        /// No special smtp dot handling is done.
        /// </summary>
        public static string QPEncodeToString(byte[] input)
        {
            return QPEncodeToString(input, 0, input.Length);
        }
        
        /// <summary>
        /// Encodes to Quoted-Printable string (see RFC 1521)
        /// No special smtp dot handling is done.
        /// </summary>
        public static string QPEncodeToString(byte[] input, int offset, int length)
        {
            byte[] output = new byte[QPEncodeMaxBytes(length)];
            int len = QPEncode(input, offset, length, output, 0, false, false);
            return Encoding.ASCII.GetString(output, 0, len);
        }

        /// <summary>
        /// Encodes from one stream to another using Quoted-Printable encoding (see RFC 1521)
        /// </summary>
        public static void QPEncode(Stream ins , Stream outs, bool encodeSmtpDot, bool trailingSoftCrLf)
        {
            int chunk = 0x4000;
            byte[] input = new byte[chunk];
            byte[] output = new byte[QPEncodeMaxBytes(chunk)];
            int read = ins.Read(input, 0, chunk);
            while (read > 0)
            {
                int write = QPEncode(input, 0, read, output, 0, encodeSmtpDot, trailingSoftCrLf);
                outs.Write(output, 0, write);
                read = ins.Read(input, 0, chunk);
            }
        }

        /// <summary> 
        /// Encodes a string using Quoted-Printable encoding (see RFC 1521)
        /// </summary>
        /// <returns>Quoted-Printable encoded string</returns>
        internal static int QPEncode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut, bool encodeSmtpDot, bool trailingSoftCrLf)
        {
            char[] map = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};
			
            // input 
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
        
        /// <summary> 
        /// Decodes a string in Quoted-Printable encoding (see RFC 1521)
        /// </summary>
        /// <returns>Quoted-Printable encoded string</returns>
        internal static int QPDecode(byte[] input, int offsetIn, int length, byte[] output, int offsetOut, bool encodeSmtpDot, bool trailingSoftCrLf)
        {
            char[] map = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};
			
            // input 
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
                    continue;
                }
                if (ch == '.' && linelen==0 && encodeSmtpDot)
                {
                    continue;
                }
                output[j++] = ch;
            }
            
            return j - offsetOut;
        }

        //
        // Base64 encoding
        //

        /// <summary>
        /// reads bytes from a stream and writes the encoded
        /// as base64 encoded characters. ( 60 chars on each row) 
        /// </summary>
        public static void Base64Encode(Stream ins , Stream outs) 
        {
            if ( (ins == null) || (outs == null) )
                throw new ArgumentNullException("The input and output streams may not be null.");
	    
            ICryptoTransform base64 = new ToBase64Transform();
                    
            // the buffers
            byte[] plainText = new byte[ base64.InputBlockSize ];
            byte[] cipherText = new byte[ base64.OutputBlockSize ];

            int readLength = 0;
            int trLength = 0;
            int count = 0;
            byte[] newln = new byte[] { 13 , 10 }; //CR LF with mail

            // read through the stream until there 
            // are no more bytes left
            while (true) 
            {
                // read some bytes
                readLength = ins.Read( plainText , 0 , plainText.Length );
            
                // break when there is no more data
                if( readLength < 1 ) break;
            
                // transfrom and write the blocks. If the block size
                // is less than the InputBlockSize then write the final block
                if( readLength == plainText.Length ) 
                {
                    trLength = base64.TransformBlock( plainText , 0 , 
                        plainText.Length ,
                        cipherText , 0 );
                		    
                    // write the data
                    outs.Write( cipherText , 0 , cipherText.Length );
                        
                    // do this to output lines that
                    // are 60 chars long
                    count += cipherText.Length;
                    if( count == 60 ) 
                    {
                        outs.Write( newln , 0 , newln.Length );
                        count = 0;
                    }
                } 
                else 
                {
                    // convert the final blocks of bytes and write them
                    cipherText = base64.TransformFinalBlock( plainText , 0 , readLength );
                    outs.Write( cipherText , 0 , cipherText.Length );
                }
            
            } 
	    
            outs.Write( newln , 0 , newln.Length );
        }
	
        //
        // UU encoding
        //

        /// <summary>
        /// uu encodes a stream in to another stream 
        /// </summary>
        public static void UUEncode(Stream ins , Stream outs, int mode, string fileName)
        {
            string endlstr = "\r\n";

            byte[] beginTag = Encoding.ASCII.GetBytes( "begin " + mode + " " + fileName + endlstr); 
            byte[] endTag = Encoding.ASCII.GetBytes( "`" + endlstr + "end" + endlstr ); 
            byte[] endl = Encoding.ASCII.GetBytes( endlstr );
	    	    
            // write the start tag
            outs.Write( beginTag , 0 , beginTag.Length );	   
	    
            // create the uu transfom and the buffers
            ToUUEncodingTransform tr = new ToUUEncodingTransform();
            byte[] input = new byte[ tr.InputBlockSize ];
            byte[] output = new byte[ tr.OutputBlockSize ];
	    
            while( true ) 
            {
			
                // read from the stream until no more data is available
                int check = ins.Read( input , 0 , input.Length );
                if( check < 1 ) break;
		
                // if the read length is not InputBlockSize
                // write a the final block
                if( check == tr.InputBlockSize ) 
                {
                    tr.TransformBlock( input , 0 , check , output , 0 );
                    outs.Write( output , 0 , output.Length );
                    outs.Write( endl , 0 , endl.Length );
                } 
                else 
                {
                    byte[] finalBlock = tr.TransformFinalBlock( input , 0 , check );
                    outs.Write( finalBlock , 0 , finalBlock.Length );
                    outs.Write( endl , 0 , endl.Length );
                    break;
                }
				
            }
	    
            // write the end tag.
            outs.Write( endTag , 0 , endTag.Length );
        }
	
    }
}
