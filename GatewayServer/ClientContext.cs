namespace GatewayServer
{
    using System;
    using System.Threading.Tasks;

    using SCore;

    using SCommon.Security;
    using SCommon.Networking;

    public class ClientContext : IDisposable
    {
        #region Private Properties and Fields

        /// <summary>
        /// Stores if the class has disposed
        /// </summary>
        private bool m_blDisposed;

        /// <summary>
        /// 
        /// </summary>
        private bool m_blDisconnected;

        /// <summary>
        /// The context.
        /// </summary>
        private SocketContext m_SocketContext;

        /// <summary>
        /// The ping timer.
        /// </summary>
        private AsyncTimer m_PingTimer;

        #endregion

        #region Constructors & Destructors

        public ClientContext()
        {
            m_PingTimer = new AsyncTimer(PingTimerCallback);
            m_blDisconnected = false;
        }

        public ClientContext(SocketContext context)
            : this ()
        {
            m_SocketContext = context;
        }

        ~ClientContext()
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
            if (!m_blDisposed)
            {
                if (disposing)
                {
                    //
                }
                m_PingTimer = null;
                m_blDisposed = true;
            }
        }
        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Gets the context.
        /// </summary>
        public SocketContext SocketContext => m_SocketContext;

        /// <summary>
        /// Gets the ping timer.
        /// </summary>
        public AsyncTimer PingTimer => m_PingTimer;

        /// <summary>
        /// Gets or sets the last ping tick.
        /// </summary>
        public int LastPingTick { get; set; }

        /// <summary>
        /// Gets or sets the last server list send tick
        /// </summary>
        public int LastServerListTick { get; set; } = Environment.TickCount;
 
        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the socket context.
        /// </summary>
        /// <param name="context">The socket context.</param>
        public void SetContext(SocketContext context)
        {
            m_SocketContext = context;
            m_blDisconnected = false;
        }

        #pragma warning disable 1998, 4014
        public async Task ProcessLoginResult(Services._shard_item shard, string id, string password)
        {
            using (var reader = await Data.Globals.AccountDB.ExecuteReaderAsync("exec _CertifyTB_User '{0}', '{1}'", id, password))
            {
                await reader.ReadAsync();

                Packet resp = new Packet(SCommon.Opcode.Gateway.Response.LOGIN);

                int type = await reader.GetFieldValueAsync<int>(0);
                switch (type)
                {
                    case 0:
                    {
                            int sid = await reader.GetFieldValueAsync<int>(1);
                            byte sec_content = await reader.GetFieldValueAsync<byte>(2);
                            int session_id = -1;
                            lock (shard.m_lock)
                            {
                                if (shard.CurrentUsers < shard.MaxUsers)
                                    session_id = Services.Session.Generate();
                            }
                    
                            if (session_id != -1)
                            {
                                Data.Globals.GlobalDB.ExecuteCommandAsync("INSERT INTO _ActiveSessions (UserSID, SessionID, Processed) VALUES ({0}, {1}, 0)", sid, session_id);
                    
                                resp.WriteByte(1);
                                resp.WriteInt32(session_id);
                                resp.WriteAscii(Data.Globals.GetConfigValue<string>("GameServerIPAddress"));
                                resp.WriteUInt16(Data.Globals.GetConfigValue<ushort>("GameServerPort"));
                            }
                            else
                            {
                                resp.WriteByte(2);
                                resp.WriteByte(5);
                            }
                    }
                    break;
                    case 1:
                    {
                            int sid = await reader.GetFieldValueAsync<int>(1);
                            string ban_reason = await reader.GetFieldValueAsync<string>(2);
                            DateTime ban_endTime = await reader.GetFieldValueAsync<DateTime>(3);

                            resp.WriteByte(2);
                            resp.WriteByte(2);
                            resp.WriteByte(1);
                            resp.WriteAscii(ban_reason);
                            resp.WriteUInt16(ban_endTime.Year);
                            resp.WriteUInt16(ban_endTime.Month);
                            resp.WriteUInt16(ban_endTime.Day);
                            resp.WriteUInt16(ban_endTime.Hour);
                            resp.WriteUInt16(ban_endTime.Minute);
                            resp.WriteUInt16(ban_endTime.Second);
                            resp.WriteInt32(ban_endTime.Millisecond);
                    }
                    break;
                    case 2:
                    {
                            resp.WriteByte(2);
                            resp.WriteByte(1);

                            if (Data.Globals.WrongPasswordTries.ContainsKey(id))
                            {
                                var wrong = Data.Globals.WrongPasswordTries[id];

                                resp.WriteInt32(++wrong.Tries);

                                wrong.LastTick = Environment.TickCount;
                                Data.Globals.WrongPasswordTries[id] = wrong;
                            }
                            else
                            {
                                var wrong = new Data._wrongpass_item
                                {
                                    LastTick = Environment.TickCount,
                                    Tries = 1
                                };
                                Data.Globals.WrongPasswordTries.Add(id, wrong);

                                resp.WriteInt32(1);
                            }

                            resp.WriteInt32(Data.Globals.MAX_WRONG_PASSWORD);
                    }
                    break;
                    case 3:
                    {
                            //already in game packet
                    }
                    break;
                }

                m_SocketContext.Send(resp);
                Disconnect();
            }
        }

        /// <summary>
        /// Disconnects the client
        /// </summary>
        /// <param name="disconnecting">if <c>true</c>, the function won't call Socket.Disconnect</param>
        public void Disconnect(bool disconnecting = false)
        {
            lock (this)
            {
                if (!disconnecting)
                    m_SocketContext.Disconnect();

                if (!m_blDisconnected)
                {
                    if (m_PingTimer.IsRunning)
                        m_PingTimer.Stop();
                    m_blDisconnected = true;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The ping timer's callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="state">The state.</param>
        private void PingTimerCallback(object sender, object state)
        {
            if (Environment.TickCount - LastPingTick >= 10000)
            {
                LastPingTick = Environment.TickCount;
                Console.WriteLine("client timeout, disconnecting (last: {0}, elapsed: {1})", LastPingTick, Environment.TickCount - LastPingTick);
                m_PingTimer.Reset(-1, 0);
                Disconnect();
            }
        }

        #endregion
    }
}
