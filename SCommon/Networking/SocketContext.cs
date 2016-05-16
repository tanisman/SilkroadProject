#define SOCKETTHREADPOOL
namespace SCommon.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using SCommon;
    using SCommon.Security;

    public class SocketContext : IDisposable
    {
        #region Private Properties and Fields

        /// <summary>
        /// Stores if the class has disposed
        /// </summary>
        private bool m_Disposed;
        
        /// <summary>
        /// The tcp server which own the this socket
        /// </summary>
        private TCPServer m_Owner;

        /// <summary>
        /// The socket.
        /// </summary>
        private Socket m_Socket;

        /// <summary>
        /// The security.
        /// </summary>
        private Security m_Security;

        /// <summary>
        /// The recv buffer
        /// </summary>
        private byte[] m_Buffer;

        /// <summary>
        /// The context.
        /// </summary>
        private object m_Context;

        /// <summary>
        /// Stores if the OnLostConnection event has fired
        /// </summary>
        private volatile int m_FiredConnectionLost;

        /// <summary>
        /// Stores if the sending is in progress
        /// </summary>
        private bool m_SendingInProgress;

        /// <summary>
        /// Stores if the client should dc after the transfer completed.
        /// </summary>
        private bool m_DisconnectAfterTransfer;

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// The eventhandler fired when packet has received
        /// </summary>
        public static EventHandler<Packet> OnPacketReceived;

        /// <summary>
        /// The eventhandler fired when connection has lost
        /// </summary>
        public static EventHandler OnLostConnection;

        /// <summary>
        /// The eventhandler fired when packet has arrived to client
        /// </summary>
        public static EventHandler<Packet> OnPacketSent;

        #endregion

        #region Constructors & Destructors

        public SocketContext()
        {
            m_Buffer = new byte[4096];
        }

        public SocketContext(Socket client, SecurityFlags flags)
             : this()
        {
            SetSocket(client);
            SetSecurity(flags);
        }

        ~SocketContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //
                }
                if (m_Security != null)
                {
                    m_Security.Dispose();
                    m_Security = null;
                }
                m_Buffer = null;
                m_Disposed = true;
            }
        }

        private void Clean()
        {
            m_Security.Dispose();
            m_Security = null;
            m_Socket = null;
        }

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Gets the socket.
        /// </summary>
        public Socket Socket => m_Socket;

        /// <summary>
        /// Gets the context.
        /// </summary>
        public object Context => m_Context;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the owner server
        /// </summary>
        /// <param name="server">The owner server.</param>
        public void SetOwner(TCPServer server)
        {
            m_Owner = server;
        }

        /// <summary>
        /// Sets the socketç
        /// </summary>
        /// <param name="client">The socket.</param>
        public void SetSocket(Socket client)
        {
            m_FiredConnectionLost = 0;
            m_DisconnectAfterTransfer = false;
            m_SendingInProgress = false;
            m_Socket = client;
        }

        /// <summary>
        /// Sets the security
        /// </summary>
        /// <param name="flags"></param>
        public void SetSecurity(SecurityFlags flags)
        {
            m_Security = new Security();
            m_Security.GenerateSecurity(flags.HasFlag(SecurityFlags.Blowfish), flags.HasFlag(SecurityFlags.SecurityBytes), flags.HasFlag(SecurityFlags.Handshake));
            m_Security.ChangeIdentity("GatewayServer", 0);
        }

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void SetContext(object context)
        {
            m_Context = context;
        }

        #pragma warning disable 4014
        /// <summary>
        /// Begins receiving from the socket
        /// </summary>
        public void Begin()
        {
            if (m_Socket == null || m_Security == null)
                throw new InvalidOperationException("[SCommon.Networking][SocketContext] You must set socket & security before calling Begin()");

            SocketError ec;
            m_Socket.BeginReceive(this.m_Buffer, 0, this.m_Buffer.Length, SocketFlags.None, out ec, AsyncReceive, null);

            if (ec != SocketError.Success && ec != SocketError.IOPending)
            {
                Disconnect();
                FireOnLostConnection();
                return;
            }

            ProcessSendQueue();
        }

        /// <summary>
        /// Enqueues packet to send queue and immediatly processes the send queue.
        /// </summary>
        /// <param name="p">The packet to send.</param>
        public void Send(Packet p)
        {
            m_Security.Send(p);
            ProcessSendQueue();
        }

        /// <summary>
        /// Enqueues ds packet to send queue
        /// </summary>
        /// <param name="p">The packet.</param>
        public void EnqueuePacket(Packet p)
        {
            m_Security.Send(p);
        }

        /// <summary>
        /// Sends the outgoing packet queue
        /// </summary>
        public void ProcessSendQueue()
        {
            try
            {
                lock (this)
                {
                    if (m_SendingInProgress) //check if still there is sending progress
                        return;

                    if (m_Security.HasPacketToSend()) //check the send queue is empty 
                    {
                        //set the field true so if ProcessSendQueue called,
                        //it should wait for complete current send progress.
                        m_SendingInProgress = true;

                        //get next packet and send to the client
                        var packet = m_Security.GetPacketToSend();

                        SocketError ec;
                        m_Socket.BeginSend(packet.Key.Buffer, 0, packet.Key.Size, SocketFlags.None, out ec, AsyncSend, packet);

                        if (ec != SocketError.Success && ec != SocketError.IOPending)
                        {
                            Disconnect();
                            FireOnLostConnection();
                            return;
                        }                        
                    }
                }
            }
            catch(Exception)
            {
                Disconnect();
                FireOnLostConnection();
            }
        }

        /// <summary>
        /// Disconnects the sockets
        /// </summary>
        public void Disconnect()
        {
            try
            {
                lock (this)
                {
                    if (m_Socket != null)
                    {
                        if (m_Socket.Connected)
                        {
                            m_Socket.Disconnect(true);
                            m_Socket.Close();
                            /*if (m_SendingInProgress)
                            {
                                m_DisconnectAfterTransfer = true;
                                return;
                            }
                            else
                                m_Socket.Disconnect(true);*/
                        }
                        FireOnLostConnection();
                    }
                }
            }
            catch(Exception)
            {

            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The event fired when data has received
        /// </summary>
        /// <param name="res">The result as <see cref="IAsyncResult"/></param>
        private void AsyncReceive(IAsyncResult res)
        {
            try
            {
                if (!m_Socket.Connected) //if socket already disconnected
                {
                    FireOnLostConnection();
                    return;
                }

                SocketError ec;

                int size = m_Socket.EndReceive(res, out ec);

                if(ec != SocketError.Success)
                {
                    FireOnLostConnection();
                    return;
                }

                if (size <= 0) //received size was 0 (means disconnect too)
                {
                    FireOnLostConnection();
                    return;
                }

                //if all things are OK;

                m_Security.Recv(m_Buffer, 0, size);
                List<Packet> packets = m_Security.TransferIncoming();
                foreach (var p in packets)
                    FireOnPacketReceived(p);

                //begin async receiving again
                m_Socket.BeginReceive(m_Buffer, 0, m_Buffer.Length, SocketFlags.None, out ec, AsyncReceive, null);

                if (ec != SocketError.Success && ec != SocketError.IOPending)
                {
                    Disconnect();
                    FireOnLostConnection();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Disconnect();
                FireOnLostConnection();
            }
        }

        /// <summary>
        /// The event fired when data has sent
        /// </summary>
        /// <param name="res">The result as <see cref="IAsyncResult"/></param>
        private void AsyncSend(IAsyncResult res)
        {
            try
            {
                lock (this)
                {
                    if (!m_Socket.Connected) //if socket is already disconnected
                    {
                        FireOnLostConnection();
                        return;
                    }

                    SocketError ec;
                    int size = m_Socket.EndSend(res, out ec);

                    if (ec != SocketError.Success)
                    {
                        FireOnLostConnection();
                        return;
                    }

                    if (size <= 0) //if nothing could transferred
                    {
                        FireOnLostConnection();
                        return;
                    }

                    KeyValuePair<TransferBuffer, Packet> transferedPacket = (KeyValuePair<TransferBuffer, Packet>)res.AsyncState;
                    FireOnPacketSent(transferedPacket.Value);

                    if (!m_Security.HasPacketToSend()) //check if send queue is empty
                    {
                        m_SendingInProgress = false; //set the field false so ProcessSendQueue can call beginsend next time.
                        if (m_DisconnectAfterTransfer) //if socket flagged to dc
                            Disconnect();
                        return;
                    }

                    //if still there is queued packets
                    //send next one
                    var packet = m_Security.GetPacketToSend();
                    m_Socket.BeginSend(packet.Key.Buffer, 0, packet.Key.Size, SocketFlags.None, out ec, AsyncSend, packet);
                    
                    if (ec != SocketError.Success && ec != SocketError.IOPending)
                    {
                        Disconnect();
                        FireOnLostConnection();
                        return;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
                FireOnLostConnection();
            }
        }

        /// <summary>
        /// Fires the OnLostConnection event
        /// </summary>
        private void FireOnLostConnection()
        {
            if (Interlocked.CompareExchange(ref m_FiredConnectionLost, 1, 0) == 0)
            {
                if (OnLostConnection != null)
                    OnLostConnection(this, null);
                this.Clean();
                m_Owner.SocketContextPool.PutObject(this);
                Interlocked.Decrement(ref m_Owner.m_ConnectionCount);
            }
        }

        /// <summary>
        /// Fires the OnPacketReceivedEvent
        /// </summary>
        /// <param name="p">The packet.</param>
        private void FireOnPacketReceived(Packet p)
        {
#if DEBUG
            Logging.Log(LogType.Packet)(p.HexDump(false));
#endif
            if (p.Opcode == 0x5000 || p.Opcode == 0x9000)
                ProcessSendQueue();
            else
            {
                if (OnPacketReceived != null)
                    OnPacketReceived(this, p);
            }
        }

        /// <summary>
        /// Fire the OnPacketSent event
        /// </summary>
        /// <param name="p">The packet.</param>
        private void FireOnPacketSent(Packet p)
        {
#if DEBUG
            Logging.Log(LogType.Packet)(p.HexDump(true));
#endif
            if (OnPacketSent != null)
                OnPacketSent(this, p);
        }
        #endregion
    }
}
