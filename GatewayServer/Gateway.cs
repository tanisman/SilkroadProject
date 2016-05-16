namespace GatewayServer
{
    using System;
    using System.Threading;

    using SCommon;
    using SCommon.Networking;
    using SCommon.Security;

    internal class Gateway
    {
        #region Private Properties and Fields

        /// <summary>
        /// The server's ip address
        /// </summary>
        private string m_IPAddress;

        /// <summary>
        /// The server's port
        /// </summary>
        private int m_Port;

        /// <summary>
        /// The tcp server
        /// </summary>
        private TCPServer m_Server;

        /// <summary>
        /// The pool for <see cref="ClientContext"/> class
        /// </summary>
        private ObjectPool<ClientContext> m_ClientContextPool;

        #endregion

        #region Constructors and Destructors

        public Gateway(string ip, int port)
        {
            m_IPAddress = ip;
            m_Port = port;
            m_ClientContextPool = new ObjectPool<ClientContext>(() => new ClientContext(), () => m_ClientContextPool.Count > 200);

            Services.Patch.Initialize();
            Services.Shard.Initialize();
            Services.Launcher.Initialize();
            Services.Session.Initialize();
        }

        #endregion

        #region Public Properties and Fileds

        public int ActiveConnectionCount => Interlocked.Exchange(ref m_Server.m_ConnectionCount, m_Server.m_ConnectionCount);

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the gateway service's state
        /// </summary>
        /// <param name="state"></param>
        public void UpdateServiceState(bool state)
        {
            if (state)
            {
                //initialize the server and start the tcp service
                if (m_Server == null)
                {
                    m_Server = new TCPServer(m_IPAddress, m_Port, SecurityFlags.Blowfish | SecurityFlags.SecurityBytes);
                    TCPServer.OnNewConnection += Gateway_NewConnection;
                    SocketContext.OnPacketReceived += SocketContext_PacketReceived;
                    SocketContext.OnLostConnection += SocketContext_ConnectionLost;
                    m_Server.StartService();
                }
            }
            else
            {
                //
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The OnNewConnection event.
        /// </summary>
        /// <param name="sender">The client context.</param>
        /// <param name="context">The socket context.</param>
        private void Gateway_NewConnection(object sender, SocketContext context)
        {
#if DEBUG
            Logging.Log()(String.Format("A new connection has arrived. (SocketContext Hash Code:{0:X8})", context.GetHashCode()), LogLevel.Info);
#endif
            ClientContext client = m_ClientContextPool.GetObject();
            client.SetContext(context);

            context.SetContext(client);
        }

        /// <summary>
        /// The OnPacketReceived event.
        /// </summary>
        /// <param name="sender">The client context.</param>
        /// <param name="packet">The socket context.</param>
        private void SocketContext_PacketReceived(object sender, Packet packet)
        {
            Func<ClientContext, Packet, bool> fn;
            ClientContext client = (ClientContext)((SocketContext)sender).Context;

            client.LastPingTick = Environment.TickCount;

            if (PacketProcessor.OpcodeMap[packet.Opcode] != null)
                fn = PacketProcessor.OpcodeMap[packet.Opcode];
            else
                fn = PacketProcessor.OpcodeMap[0];

            if (!fn(client, packet))
                client.Disconnect();
        }

        /// <summary>
        /// The OnLostConnection event.
        /// </summary>
        /// <param name="sender">The client context.</param>
        /// <param name="args">The event args.</param>
        private void SocketContext_ConnectionLost(object sender, EventArgs args)
        {
#if DEBUG
            Logging.Log()(String.Format("A connection has dropped. (SocketContext Hash Code:{0:X8})", sender.GetHashCode()), LogLevel.Info);
#endif
            ClientContext client = (ClientContext)((SocketContext)sender).Context;
            client.Disconnect(true);
            m_ClientContextPool.PutObject(client);
        }

        #endregion
    }
}
