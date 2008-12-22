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

namespace Glue.Lib.Mail {

    /// represents a conntection to a smtp server
    public class SmtpClient
    {
        private string server;
        private TcpClient tcpConnection;
        private Encoding encoding;
        private Stream stream;
        private SmtpResponse lastResponse;
        private string command = "";
	
        //Initialise the variables and connect
        public SmtpClient(string server) 
        {
            this.server = server;
            encoding = new ASCIIEncoding();

            Connect();
        }
	
        // make the actual connection
        // and HELO handshaking
        public void Connect() 
        {
            // open connection
            tcpConnection = new TcpClient(server, 25);
            stream = tcpConnection.GetStream();

            // read the server greeting
            ReadResponse();
            CheckForStatusCode(220);
	   
            // write the HELO command to the server
            WriteHelo(Dns.GetHostName());
        }
	
        public void Send(MailMessage msg) 
        {

            if (msg.From == null) 
            {
                throw new SmtpException( "From property must be set." );
            }

            if (msg.To == null) 
            {
                if (msg.To.Count < 1) throw new SmtpException( "At least one recipient must be set." );
            }

            // start with a reset incase old data
            // is present at the server in this session
            WriteRset();

            // write the mail from command
            WriteMailFrom(msg.From.Address);

            // write the rcpt to command for the To addresses
            foreach (MailAddress addr in msg.To) 
            {
                WriteRcptTo(addr.Address);
            }

            // write the rcpt to command for the Cc addresses
            foreach (MailAddress addr in msg.Cc) 
            {
                WriteRcptTo(addr.Address);
            }

            // write the rcpt to command for the Bcc addresses
            foreach (MailAddress addr in msg.Bcc) 
            {
                WriteRcptTo(addr.Address);
            }

            // write the data command and then
            // send the email
            WriteData();

            // send mail data
            msg.Write(stream);

            // write the data end tag "."
            WriteDataEndTag();

        }
	
        // send quit command and
        // closes the connection
        public void Close() 
        {
            WriteQuit();
            tcpConnection.Close();
        }

        protected SmtpResponse LastResponse 
        {
            get { return lastResponse; }
        }

        protected void WriteRset() 
        {
            command = "RSET";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );
        }

        protected void WriteHelo( string hostName ) 
        { 
            command = "HELO " + hostName;
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );
        }

        protected void WriteMailFrom( string from ) 
        {
            command = "MAIL FROM: <" + from + ">";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );
        }

        protected void WriteRcptTo( string to ) 
        {
            command = "RCPT TO: <" + to + ">";  
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );
        }

        protected void WriteData() 
        {
            command = "DATA";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 354 );
        }

        protected void WriteQuit() 
        {
            command = "QUIT";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 221 );
        }

        protected void WriteBoundary( string boundary ) 
        {
            WriteLine( "\r\n--{0}" , boundary );
        }

        protected void WriteFinalBoundary( string boundary ) 
        {
            WriteLine( "\r\n--{0}--" , boundary );
        }

        // single dot by itself
        protected void WriteDataEndTag() 
        {
            command = "\r\n.";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );
	
        }

        protected void CheckForStatusCode( int statusCode ) 
        {

            if( LastResponse.StatusCode != statusCode ) 
            {

                string msg = "" + 
                    "Server reponse: '" + lastResponse.RawResponse + "';" +
                    "Status code: '" +  lastResponse.StatusCode + "';" + 
                    "Expected status code: '" + statusCode + "';" + 
                    "Last command: '" + command + "'";

                throw new SmtpException( msg ); 

            }
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
        protected void ReadResponse() 
        {
            string line = null;
	    
            byte[] buffer = new byte[ 4096 ];
	    
            int readLength = stream.Read( buffer , 0 , buffer.Length );
	    
            if( readLength > 0 ) 
            { 
	    
                line = encoding.GetString( buffer , 0 , readLength );
		
                line = line.TrimEnd( new Char[] { '\r' , '\n' , ' ' } );
			
            }
	   
            // parse the line to the lastResponse object
            lastResponse = SmtpResponse.Parse( line );
        }

    }

}
