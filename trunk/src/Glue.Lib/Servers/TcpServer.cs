using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Specialized;
using System.Xml;

namespace Glue.Lib.Servers
{
    /// <summary>
    /// Single threaded server
    /// </summary>
    public abstract class TcpServer
    {
        protected const string EOL = "\r\n";
        protected string[] allowedIP = null;
        protected IPEndPoint localEP;
        protected Socket listener;
        protected bool started = false;
        protected bool stopped = true;
        protected WaitCallback onSocketListen;
        protected WaitCallback onSocketAccept;

        /// <summary>
        /// Creates a connection object. Override this in
        /// your server class to create a protocol specific
        /// connection class.
        /// </summary>
        public abstract TcpConnection CreateConnection(Socket socket);

        /// <summary>
        /// Public constructor
        /// </summary>
        public TcpServer(IPEndPoint localEP)
        {
            this.localEP = localEP;
            onSocketAccept = new WaitCallback(OnSocketAccept);
            onSocketListen = new WaitCallback(OnSocketListen);
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public virtual void Start()
        {
            try
            {
                // Start listing on the end point.
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEP);
                listener.Listen((int)SocketOptionName.MaxConnections);
                Log.Info("Listening on {0}", localEP);

                // Stuff the listening thread in the pool
                started = true;
                stopped = false;

                ThreadPool.QueueUserWorkItem(onSocketListen);
            }
            catch (Exception e)
            {
                started = false;
                stopped = true;
                Log.Error("Error starting: " + e);
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public virtual void Stop()
        {
            if (!started)
                return;
            
            started = false;
            try 
            {
                listener.Close();
            }
            catch 
            {
            }
            listener = null;

            while (!stopped)
                Thread.Sleep(100);
        }

        /// <summary>
        /// IsAllowed
        /// </summary>
        public virtual bool IsAllowed(IPAddress ip)
        {
            if (allowedIP == null)
                return true;
            string test = ip.ToString();
            foreach (string allowed in allowedIP)
                if (test.StartsWith(allowed))
                    return true;
            return false;
        }

        /// <summary>
        /// Asynchronous callback on socket accept.
        /// </summary>
        /// <param name="acceptedSocket"></param>
        protected void OnSocketAccept(Object acceptedSocket) 
        {
            TcpConnection connection = CreateConnection((Socket)acceptedSocket);
            connection.Process();
        }

        /// <summary>
        /// Asynchronous callback to start listening
        /// </summary>
        protected void OnSocketListen(Object unused) 
        {
            while (started) 
            {
                try 
                {
                    Socket socket = listener.Accept();
                    ThreadPool.QueueUserWorkItem(onSocketAccept, socket);
                }
                catch 
                {
                    Thread.Sleep(100);
                }
            }
            stopped = true;
        }
    }
}
