//
// Glue.Lib.Mail.SmtpClient.cs
//
//
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using Glue.Lib.Mime;

namespace Glue.Lib.Net.Pop3 {

    // Represents a conntection to a POP3 server
    public class Pop3Client
    {
        private string server;
        private string username;
        private string password;
        private TcpClient tcpConnection;
        private Encoding encoding;
        private Stream stream;
        private StreamReader reader;
	
        // Initialise the variables and connect
        public Pop3Client(string server, string username, string password) 
        {
            this.server = server;
            this.username = username;
            this.password = password;
            encoding = new ASCIIEncoding();
        }
	
        // Make the actual connection and authenticate
        public void Connect() 
        {
            // Open connection
            tcpConnection = new TcpClient(server, 110);
            stream = tcpConnection.GetStream();
            reader = new StreamReader(stream, encoding, false, 4096);

            // Read the server greeting
            CheckResponse();

            // authenticate
            WriteLine("USER " + username);
            CheckResponse();
            WriteLine("PASS " + password);
            CheckResponse();
        }
	
        // Send quit command and close the connection
        public void Close() 
        {
            WriteLine("QUIT");
            try { CheckResponse(); }
            catch {}
            tcpConnection.Close();
            reader.Close();
            stream.Close(); 
        }

        /// <summary>
        /// List message numbers
        /// </summary>
        public int[] List()
        {
            WriteLine("UIDL");
            CheckResponse();
            ArrayList list = new ArrayList();
            string line;
            while ((line = ReadLine()) != ".")
            {
                int key = Convert.ToInt32(StringHelper.Slice(line, 0));
                list.Add(key);
            }
            return (int[])list.ToArray(typeof(int));
        }

        /// <summary>
        /// List message numbers and unique identifiers
        /// 
        /// IDictionary ids = pop3.ListUniqueIds();
        /// ids[3] = "ARFEOIRUTLPTEP443E"
        /// </summary>
        public Hashtable ListUniqueIds()
        {
            WriteLine("UIDL");
            CheckResponse();
            Hashtable result = new Hashtable();
            string line = ReadLine();
            while (line != null && line != ".")
            {
                int key = Convert.ToInt32(StringHelper.Slice(line, 0));
                result[key] = StringHelper.Slice(line, 1);
            }
            return result;
        }

        /// <summary>
        /// Retrieve raw message with given number
        /// </summary>
        public string Retrieve(int key)
        {
            StringBuilder data = new StringBuilder();
            WriteLine("RETR " + key);
            CheckResponse();
            string line;
            while ((line = ReadLine()) != ".")
                data.Append(line + "\r\n");
            return data.ToString();
        }

        /// <summary>
        /// Deletes given message from server.
        /// </summary>
        public void Delete(int key)
        {
            WriteLine("DELE " + key);
            CheckResponse();
        }

        /// <summary>
        /// Returns the message with given number
        /// </summary>
        public MimePart RetrieveMessage(int key)
        {
            return MimePart.Parse(Retrieve(key));
        }

        protected void CheckResponse() 
        {
            string response = ReadLine();
            if (StringHelper.Slice(response, 0) != "+OK")
                throw new Pop3Exception("Unexpected response: '" + response + "'");
        }

        // write buffer's bytes to the stream
        protected void WriteBytes(byte[] buffer) 
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        // writes a formatted line to the server
        protected void WriteLine(string format, params object[] args) 
        {
            WriteLine( string.Format(format, args) );
        }

        // writes a line to the server
        protected void WriteLine(string line) 
        {
            byte[] buffer = encoding.GetBytes( line + "\r\n" );
            stream.Write( buffer , 0 , buffer.Length );
        }

        // read a line from the server
        protected string ReadLine()
        {
            return reader.ReadLine();
        }
    }

}
