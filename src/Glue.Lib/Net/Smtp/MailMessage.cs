//
// Glue.Lib.Mail.MailMessage.cs
//
//
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Glue.Lib.Mail
{
	/// <remarks>
	/// </remarks>
    public class MailMessage
    {
        private ArrayList attachments;
        private MailAddressCollection bcc = new MailAddressCollection();
        private string textBody;
        private string htmlBody;
        private Encoding bodyEncoding;
        private MailAddressCollection cc = new MailAddressCollection();		
        private MailAddress from;
        private ListDictionary headers;
        private MailPriority priority;
        private string subject = "";
        private MailAddress replyTo;
        private MailAddressCollection to = new MailAddressCollection();
        private string urlContentBase;
        private string urlContentLocation;
        private Hashtable fields;


        // Constructor		
        public MailMessage()
        {
            attachments = new ArrayList(8);
            headers = new ListDictionary();
            bodyEncoding = Encoding.UTF8;
            fields = new Hashtable();
        }		


        // Properties
        public IList Attachments 
        {
            get { return (IList) attachments; }
        }		

		
        public MailAddressCollection Bcc 
        {
            get { return bcc; } 
            set { bcc = value; }
        }


        public string HtmlBody 
        {
            get { return htmlBody; } 
            set { htmlBody = value; }
        }


        public string TextBody 
        {
            get { return textBody; } 
            set { textBody = value; }
        }


        public Encoding BodyEncoding 
        {
            get { return bodyEncoding; } 
            set { bodyEncoding = value; }
        }


        public MailAddressCollection  Cc 
        {
            get { return cc; } 
            set { cc = value; }
        }


        public MailAddress From 
        {
            get { return from; } 
            set { from = value; }
        }


        public IDictionary Headers 
        {
            get { return (IDictionary) headers; }
        }

		
        public MailPriority Priority 
        {
            get { return priority; } 
            set { priority = value; }
        }


        public MailAddress ReplyTo
        {
            get { return replyTo; }
            set { replyTo = value; }
        }

        public string Subject 
        {
            get { return subject; } 
            set { subject = value; }
        }


        public MailAddressCollection To 
        {
            get { return to; }   
            set { to = value; }
        }


        public string UrlContentBase 
        {
            get { return urlContentBase; } 
            set { urlContentBase = value; }
        }

        public string UrlContentLocation 
        {
            get { return urlContentLocation; } 
            set { urlContentLocation = value; }
        }

        public IDictionary Fields 
        {
            get 
            {
                return (IDictionary) fields;
            }
        }

        public void Write(Stream output)
        {
            // write header

            WriteHeader(output, "Reply-To", replyTo);
            WriteHeader(output, "From", from);
            WriteHeader(output, "To", to);
            WriteHeader(output, "CC", cc);
            WriteHeader(output, "Subject", subject, true);
            WriteHeader(output, "Date", DateTime.Now.ToUniversalTime().ToString("R"));
            WriteHeader(output, "X-Mailer", "Glue.Lib.Mail");
            switch (priority)
            {
                case MailPriority.High:
                    WriteHeader(output, "X-Priority", "1");
                    WriteHeader(output, "Priority", "Urgent");
                    break;
                case MailPriority.Low:
                    WriteHeader(output, "X-Priority", "5");
                    WriteHeader(output, "Priority", "Non-Urgent");
                    break;
            }
            // TODO: Disposition-Notification-To:
            // TODO: [custom headers]
            
            WriteHeader(output, "MIME-Version", "1.0");

            if (attachments.Count > 0)
            {
                WriteMultiPartStart(output, "multipart/mixed");
                WriteMultiPartBoundary(output);
            }
            
            if (htmlBody != null && textBody != null)
            {
                WriteMultiPartStart(output, "multipart/alternative");
                WriteMultiPartBoundary(output);
            }
            if (textBody != null)
            {
                WriteTextMessageBody(output, textBody, "text/plain");
            }
            if (htmlBody != null && textBody != null)
            {
                WriteMultiPartBoundary(output);
            }
            if (htmlBody != null)
            {
                WriteTextMessageBody(output, htmlBody, "text/html");
            }
            if (htmlBody != null && textBody != null)
            {
                WriteMultiPartEnd(output);
            }

            foreach (MailAttachment atm in attachments)
            {
                WriteMultiPartBoundary(output);
                atm.Write(output);
            }

            if (attachments.Count > 0)
            {
                WriteMultiPartEnd(output);
            }

        }

        // Internal rendering methods

        #region Mime message rendering
        
        // Keeps track of (nested) multipart mime boundaries
        private Stack boundaries = null;

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

        private void WriteHeader(Stream output, string name, MailAddress value)
        {
            if (value != null)
                WriteLine(output, string.Concat(name, ": ", value.ToString(true)));
        }

        private void WriteHeader(Stream output, string name, MailAddressCollection value)
        {
            if (value != null && value.Count > 0)
                WriteLine(output, string.Concat(name, ": ", value.ToString(true)));
        }

        // Writes body text
        private void WriteTextMessageBody(Stream output, string messageBody, string mimeType)
        {
            WriteLine(output, "Content-Type: " + mimeType + ";");
            WriteLine(output, "   charset=\"" + bodyEncoding.BodyName + "\"");
            if (MailUtil.NeedEncoding(messageBody))
            {
                WriteLine(output, "Content-Transfer-Encoding: quoted-printable");
                WriteLine(output, "");

                byte[] inbuf = bodyEncoding.GetBytes(messageBody);
                int len = MailUtil.QPEncodeMaxBytes(inbuf.Length);
                byte[] outbuf = new byte[len];
                len = MailUtil.QPEncode(inbuf, 0, inbuf.Length, outbuf, 0, true, true);
                output.Write(outbuf, 0, len);
            }
            else
            {
                WriteLine(output, "Content-Transfer-Encoding: 7bit");
                WriteLine(output, "");
                WriteLine(output, messageBody);
            }
        }

        private void WriteMultiPartStart(Stream output, string multipartType)
        {
            if (boundaries == null)
            {
                boundaries = new Stack();
            }
            boundaries.Push(MailUtil.GenerateBoundary());
            
            WriteLine(output, "Content-Type: " + multipartType + ";");
            WriteLine(output, "   boundary=\"" + (string)boundaries.Peek() + "\"");
            WriteLine(output, "");
            WriteLine(output, "This is a multi-part message in MIME format.");
        }

        private void WriteMultiPartBoundary(Stream output)
        {
            Write(output, "\r\n--" + (string)boundaries.Peek() + "\r\n");
        }
        
        private void WriteMultiPartEnd(Stream output)
        {
            Write(output, "\r\n--" + (string)boundaries.Pop() + "--\r\n");
        }

        private void Write(Stream output, string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            output.Write(data, 0, data.Length);
        }
        
        private void WriteLine(Stream output, string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text + "\r\n");
            output.Write(data, 0, data.Length);
        }
        
        #endregion 
    }

} 

