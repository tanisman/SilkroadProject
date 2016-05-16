#define SOCKETTHREADPOOL
namespace SCommon.Networking
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using SCommon;

    public class TCPServer
    {
        #region Private Properties and Fields

        /// <summary>
        /// The TCP server socket.
        /// </summary>
        private Socket m_Socket;

        /// <summary>
        /// The connection allowed field.
        /// </summary>
        private bool m_blConnectionAllowed;

        /// <summary>
        /// The security flags field for <see cref="SocketContext"/> class
        /// </summary>
        private SecurityFlags m_SecurityFlags;

        /// <summary>
        /// The pool for <see cref="SocketContext"/> class
        /// </summary>
        private ObjectPool<SocketContext> m_SocketContextPool;

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Stores the current connection count
        /// </summary>
        public volatile int m_ConnectionCount;

        /// <summary>
        /// Gets the SocketContextPool.
        /// </summary>
        public ObjectPool<SocketContext> SocketContextPool => m_SocketContextPool;

        /// <summary>
        /// The event fired when new connection has arrived.
        /// </summary>
        public static EventHandler<SocketContext> OnNewConnection;

        #endregion

        #region Constructors & Destructors

        public TCPServer(string ip, int port, SecurityFlags flags)
        {
            m_blConnectionAllowed = false;
            m_SecurityFlags = flags;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_SocketContextPool = new ObjectPool<SocketContext>(() => new SocketContext(), () => m_SocketContextPool.Count > 200);
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                m_Socket.Bind(endPoint);
                m_Socket.Listen(1000);
            }
            catch (SocketException)
            {
                Console.WriteLine("Cannot listen socket ({0}:{1})", ip, port);
            }
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Begins accept sockets to the TCP Server.
        /// </summary>
        public void StartService(int acceptThreadCount = 8)
        {
            for(int i = 0; i < acceptThreadCount; i++)
                m_Socket.BeginAccept(AsyncAccept, i);

            m_blConnectionAllowed = true;   
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The event fired when the new connection arrived
        /// </summary>
        /// <param name="res">The result as <see cref="IAsyncResult"/></param>
        private void AsyncAccept(IAsyncResult res)
        {
            try
            {
                //extract the pending socket
                Socket pending = m_Socket.EndAccept(res);

                if (pending != null) //if socket is valid
                {
                    if (!m_blConnectionAllowed || !pending.Connected) //if it isnt allowed to connect server or pending client has disconnected
                    {
                        pending.Disconnect(true);
                        return;
                    }

                    //if there is nothing wrong with new connection;
                    //notify the event subscribers so we can create a real context between socket & user
                    FireOnNewConnection(pending);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("cannot accept socket");
            }
            finally
            {
                //begin async accepting again
                m_Socket.BeginAccept(AsyncAccept, null);
            }
        }

        /// <summary>
        /// Fires the OnNewConnectionEvent
        /// </summary>
        /// <param name="client">The client socket.</param>
        private void FireOnNewConnection(Socket client)
        {
            //build context
            SocketContext context = m_SocketContextPool.GetObject();
            context.SetSocket(client);
            context.SetOwner(this);
            context.SetSecurity(m_SecurityFlags);
            
            //notify the event subscribers
            OnNewConnection(this, context);

            //begin async receiving from context's socket
            context.Begin();

            Interlocked.Increment(ref m_ConnectionCount);
        }

        #endregion
    }
}
