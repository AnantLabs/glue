using System;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// Summary description for SmtpServer.
	/// </summary>
    public class SmtpConnection : TcpConnection
    {
        protected static readonly Regex regexAddress = new Regex(@"<?[\w\-\.]+@[\w\-\.]+>?", RegexOptions.IgnoreCase);
        
        protected string sender = null;
        protected StringBuilder message = new StringBuilder();
        protected StringCollection recipients = new StringCollection();
        private StringBuilder inputBuffer = new StringBuilder();

        /// <summary>
        /// Public constructor
        /// </summary>
        public SmtpConnection(SmtpServer server, Socket socket) : base(server, socket)
        {
        }

        /// <summary>
        /// The owning server
        /// </summary>
        public SmtpServer Server
        {
            get { return base.server as SmtpServer; }
        }

        /// <summary>
        /// Process a single conversation. 
        /// </summary>
        public override void Process()
        {
            try
            {
                if (!socket.Connected)
                    return;

                Log.Debug("ClientIP={0}", socket.RemoteEndPoint);
                
                // Socket timeout here...
                // socket.SetSocketOption(
                //    SocketOptionLevel.Socket, 
                //    SocketOptionName.ReceiveTimeout, 
                //    Settings.Incoming.SocketTimeout
                //    );

                WriteLine("220 Glue SMTP Server.");
        
                bool processing = true;
        
                while (processing)
                {
                    string line = ReadLine();
                    Log.Debug("<< " + line);
                    string[] parts = line.Split(new char[]{':',' '}, 3);
                    switch (parts[0].ToLower())
                    {
                        case "helo":
                        case "ehlo":
                            WriteLine("250 Hello");
                            break;
                        case "rset":
                            Rset();
                            WriteLine("250 OK");
                            break;
                        case "noop":
                            WriteLine("250 OK");
                            break;
                        case "quit":
                            WriteLine("221 Goodbye.");
                            processing = false;
                            break;
                        case "mail":
                            if (parts[1].Trim().ToLower() != "from")
                            {
                                WriteLine("500 Command Unrecognized.");
                                break;
                            }
                            Mail(parts[2]);
                            break;
                        case "rcpt":
                            if (parts[1].Trim().ToLower() != "to")
                            {
                                WriteLine("500 Command Unrecognized.");
                                break;
                            }
                            Rcpt(parts[2]);
                            break;
                        case "data":
                            Data();
                            break;
                        default:
                            WriteLine("500 Command Unrecognized: " + parts[0]);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            socket.Close();
        }

        protected virtual void Rset()
        {
            message.Length = 0;
            sender = null;
            recipients.Clear();
        }

        protected virtual void Mail(string input)
        {
            Match m =regexAddress.Match(input);
            if (!m.Success)
            {
                Log.Info("Invalid sender address: " + input);
                WriteLine("451 Address is invalid.");
                return;
            }
            string email = m.Value.Trim('<','>',' ','\t');
            if (email == null || email == "")
            {
                Log.Info("Empty sender address");
                WriteLine("451 Address is invalid.");
                return;
            }
            Log.Debug("Sender: " + email);
            sender = email;
            WriteLine("250 OK");
        }

        protected virtual void Rcpt(string input)
        {
            if (sender == null)
            {
                WriteLine("503 Need mail command");
                return;
            }
            Match m =regexAddress.Match(input);
            if (!m.Success)
            {
                Log.Info("Invalid recipient address: " + input);
                WriteLine("451 Address is invalid.");
                return;
            }
            string email = m.Value.Trim('<','>',' ','\t');
            if (email == null || email == "")
            {
                Log.Info("Empty recipient address");
                WriteLine("451 Address is invalid.");
                return;
            }
            recipients.Add(email);
            WriteLine("250 OK");
        }
        
        protected virtual void Data()
        {
            if (sender == null)
            {
                WriteLine("503 Need mail command.");
                return;
            }
            if (recipients.Count == 0)
            {
                WriteLine("503 Need recipients.");
                return;
            }
            WriteLine("354 Start mail input; end with <CRLF>.<CRLF>");

            // Read in message data
            ReadData();
            
            // Call store to do 'something' with the message
            Store();

            Rset();
            WriteLine("250 OK");
        }

        /// <summary>
        /// Override ReadData in your class to perform custom
        /// reading of the message. 
        /// </summary>
        protected virtual void ReadData()
        {
            string line = ReadLine();
            while (line != ".")
            {
                message.Append(line);
                message.Append("\r\n");
                line = ReadLine();
            }
        }

        /// <summary>
        /// Override store in your class to perform some action
        /// on the message
        /// </summary>
        protected virtual void Store()
        {
            Guid msgid = Guid.NewGuid();
            Log.Debug("Saving mail: {0}", msgid);
            string path = msgid.ToString("N") + ".txt";
            using (StreamWriter writer = new StreamWriter(path, false, ISO))
            {
                writer.Write(message.ToString());
            }
        }

        /// <summary>
        /// Reads an entire line from the socket.  This method
        /// will block until an entire line has been read.
        /// </summary>
        public string ReadLine()
        {
            // If we already buffered another line, just return
            // from the buffer.            
            string output = ReadLineFromBuffer();
            if( output != null )
                return output;
                        
            // Otherwise, read more input.
            byte[] bytes = new byte[80];
            while (output == null)
            {
                // Read the input data.
                int count = socket.Receive(bytes);
                if (count == 0)
                    break;
                inputBuffer.Append(ISO.GetString(bytes, 0, count));
                output = ReadLineFromBuffer();
            }
            
            return output;
        }

        /// <summary>
        /// Helper method that returns the first full line in
        /// the input buffer, or null if there is no line in the buffer.
        /// If a line is found, it will also be removed from the buffer.
        /// </summary>
        private string ReadLineFromBuffer()
        {
            // If the buffer has data, check for a full line.
            int n = inputBuffer.Length;
            for (int i = 0; i < n; i++)
                if (inputBuffer[i] == '\n')
            {
                    string line;
                    if (i > 0 && inputBuffer[i-1] == '\r')
                        line = inputBuffer.ToString(0, i - 1);
                    else
                        line = inputBuffer.ToString(0, i);
                    i++;
                    inputBuffer.Remove(0, i);
                    return line;
            }
            return null;
        }

        /// <summary>
        /// Writes the string to the socket as an entire line.  This
        /// method will append the end of line characters, so the data
        /// parameter should not contain them.
        /// </summary>
        public void WriteLine(string data)
        {
            Log.Debug(">> " + data);
            socket.Send(ISO.GetBytes(data + "\r\n"));
        }
    }
}
