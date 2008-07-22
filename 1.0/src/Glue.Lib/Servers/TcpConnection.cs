using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Glue.Lib.Servers
{
	/// <summary>
	/// Summary description for TcpConnection.
	/// </summary>
	public abstract class TcpConnection
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
        protected static readonly Encoding ISO = Encoding.GetEncoding("iso-8859-1");
        protected TcpServer server;
        protected Socket socket;

        public TcpConnection(TcpServer server, Socket socket) 
        {
            this.server = server;
            this.socket = socket;
        }

        public bool Connected 
        {
            get { return socket.Connected; }
        }

        public IPEndPoint LocalEP
        {
            get { return (IPEndPoint)socket.LocalEndPoint; }
        }

        public string LocalIP 
        {
            get 
            {
                IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
                if (endPoint != null && endPoint.Address != null)
                    return endPoint.Address.ToString();
                else
                    return "127.0.0.1";
            }
        }

        public IPEndPoint RemoteEP
        {
            get { return (IPEndPoint)socket.RemoteEndPoint; }
        }

        public string RemoteIP 
        {
            get 
            {
                IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;
                if (endPoint != null && endPoint.Address != null)
                    return endPoint.Address.ToString();
                else
                    return "127.0.0.1";
            }
        }

        public bool IsLocal 
        {
            get { return (LocalIP == RemoteIP); }
        }

        public void Close() 
        {
            try 
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch 
            {
            }
            socket = null;
        }

        public abstract void Process();

        public byte[] ReadBytes(int maxBytes) 
        {
            try 
            {
                int numBytes = WaitForBytes();
                if (numBytes == 0)
                    return null;

                if (numBytes > maxBytes)
                    numBytes = maxBytes;

                int numReceived = 0;
                byte[] buffer = new byte[numBytes];

                if (numBytes > 0) 
                {
                    numReceived = socket.Receive(buffer, 0, numBytes, SocketFlags.None);
                }

                if (numReceived < numBytes) 
                {
                    byte[] tempBuffer = new byte[numReceived];
                    if (numReceived > 0) 
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    buffer = tempBuffer;
                }

                return buffer;
            }
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// Sends data to the client
        /// </summary>
        public void SendBytes(byte[] data)
        {
            SendBytes(data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to the client
        /// </summary>
        public void SendBytes(byte[] data, int offset, int length)
        {
            int num = 0;
            
            try
            {
                if (socket.Connected)
                {
                    if ((num = socket.Send(data, offset, length, SocketFlags.None)) == -1)
                        Log.Error("Cannot send packet");
                    else
                    {
                        Log.Debug("No. of bytes send {0}" , num);
                    }
                }
                else
                    Log.Debug("Connection dropped....");
            }
            catch (Exception  e)
            {
                Log.Error("Error Occurred : {0} ", e );
            }
        }

        public int WaitForBytes() 
        {
            int availBytes = 0;
            try 
            {
                if (socket.Available == 0) 
                {
                    // poll until there is data
                    socket.Poll(100000 /* 100ms */, SelectMode.SelectRead);
                    //if (socket.Available == 0 && socket.Connected)
                    //    socket.Poll(10000000 /* 10sec */, SelectMode.SelectRead);
                }

                availBytes = socket.Available;
            }
            catch 
            {
            }
            return availBytes;
        }

        /*
        /// <summary>
        /// SendContent
        /// </summary>
        public void SendTextReader(System.IO.TextReader reader)
        {
            try
            {
                char[] data = new char[8192];
                byte[] bytes;
                int tot = 0;
                int len = 0;
                while ((len = reader.ReadBlock(data, 0, data.Length)) > 0)
                {
                    tot += len;
                    if (!socket.Connected)
                    {
                        Log("  Connection dropped...");
                        break;
                    }
                    int num;
                    bytes = System.Text.Encoding.UTF8.GetBytes(data);
                    if ((num = socket.Send(bytes, bytes.Length, 0)) == -1)
                    {
                        Log("  Cannot send packet");
                        break;
                    }
                }
                Log("  No. of bytes send {0}" , tot);
            }
            catch (Exception  e)
            {
                Log("  Error Occurred : {0} ", e );
            }
        }

        public void SendFile(string path)
        {
            throw new NotImplementedException();
        }
        */

    }
}
