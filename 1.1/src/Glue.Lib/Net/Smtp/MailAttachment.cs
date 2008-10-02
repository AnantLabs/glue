//
// Glue.Lib.Mail.MailAttachment.cs
//
using System;
using System.IO;
using System.Text;

namespace Glue.Lib.Mail
{
    public class MailAttachment
    {
        private string path;
        private string name;
        private string mimeType;
        private TransferEncoding transferEncoding;
        private Stream cached;
		
        public MailAttachment(string path) : this(path, false, TransferEncoding.Base64)
        {
        }

        public MailAttachment(string path, bool cache) : this(path, cache, TransferEncoding.Base64)
        {
        }

        public MailAttachment(string path, bool cache, TransferEncoding transferEncoding)
        {
            this.path = path;
            this.name = Path.GetFileName(path);
            this.transferEncoding = transferEncoding;
            this.mimeType = Mime.MimeMapping.GetMimeMapping(Path.GetExtension(path));
            
            if (cache)
            {
                using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    LoadCache(input);
                    input.Close();
                }
            }
        }

        public MailAttachment(Stream input, string name) : this(input, name, null, TransferEncoding.Base64)
        {
        }

        public MailAttachment(Stream input, string name, string mimeType) : this(input, name, mimeType, TransferEncoding.Base64)
        {
        }

        public MailAttachment(Stream input, string name, string mimeType, TransferEncoding transferEncoding)
        {
            this.name = name;
            this.transferEncoding = transferEncoding;
            if (mimeType == null)
                this.mimeType = Mime.MimeMapping.GetMimeMapping(Path.GetExtension(path));
            else
                this.mimeType = mimeType;
            LoadCache(input);
        }

        public void Write(Stream output)
        {
            // write headers
            WriteHeader(output, "Content-Type", MimeType);
            WriteHeader(output, "Content-Transfer-Encoding", "base64");
            WriteHeader(output, "Content-Disposition", "attachment; filename=\"" + Name + "\"");
            WriteLine(output, "");

            // write content
            if (cached == null)
            {
                using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    switch (transferEncoding)
                    {
                        case TransferEncoding.Base64:
                            MailUtil.Base64Encode(input, output);
                            break;
                        case TransferEncoding.UUEncode:
                            MailUtil.UUEncode(input, output, 644, Name);
                            break;
                        case TransferEncoding.QuotedPrintable:
                            MailUtil.QPEncode(input, output, true, true);
                            break;
                        default:
                            MailUtil.StreamCopy(input, output);
                            break;
                    }
                }
            }
            else
            {
                cached.Position = 0;
                MailUtil.StreamCopy(cached, output);
            }
        }

        public string MimeType
        {
            get { return mimeType; }
            set { mimeType = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Filename
        {
            get { return this.path; }
        }

        public TransferEncoding TransferEncoding 
        {
            get { return transferEncoding; } 
        }

        private void LoadCache(Stream input)
        {
            this.cached = new MemoryStream();
            switch (transferEncoding)
            {
                case TransferEncoding.Base64:
                    MailUtil.Base64Encode(input, cached);
                    break;
                case TransferEncoding.UUEncode:
                    MailUtil.UUEncode(input, cached, 644, Name);
                    break;
                case TransferEncoding.QuotedPrintable:
                    MailUtil.QPEncode(input, cached, true, true);
                    break;
                default:
                    MailUtil.StreamCopy(input, cached);
                    break;
            }
        }

        private void WriteHeader(Stream output, string name, string value)
        {
            if (value != null)
                WriteLine(output, name + ": " + value);
        }
        
        private void WriteHeader(Stream output, string name, string value, bool allowEncoding)
        {
            if (value != null)
            {
                if (allowEncoding && MailUtil.NeedEncoding(value))
                    WriteLine(output, name + ": =?utf-8?Q?" + MailUtil.QPEncodeToString(Encoding.UTF8.GetBytes(value)) + "?=");
                else
                    WriteLine(output, name + ": " + value);
            }
        }

        private void WriteLine(Stream output, string line)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(line + "\r\n");
            output.Write(bytes, 0, bytes.Length);
        }
      
    }
} 

