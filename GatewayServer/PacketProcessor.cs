namespace GatewayServer
{
    using System;
    using System.Linq;

    using SCommon.Security;

    using Opcode = SCommon.Opcode;

    public static class PacketProcessor
    {
        #region Private Properties and Fields

        /// <summary>
        /// The opcode map.
        /// </summary>
        private static Func<ClientContext, Packet, bool>[] s_OpcodeMap = new Func<ClientContext, Packet, bool>[256 * 256];

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Gets the opcode map.
        /// </summary>
        public static Func<ClientContext, Packet, bool>[] OpcodeMap => s_OpcodeMap;

        #endregion

        #region Public Methods

        /// <summary>
        /// Fills the opcode map.
        /// </summary>
        public static void FillTable()
        {
            s_OpcodeMap[0] = Unhandled;
            s_OpcodeMap[Opcode.General.PING] = Ping;
            s_OpcodeMap[Opcode.General.IDENTITY] = Identity;
            s_OpcodeMap[Opcode.Gateway.Request.PATCH] = Patch;
            s_OpcodeMap[Opcode.Gateway.Request.SERVERLIST] = ServerList;
            s_OpcodeMap[Opcode.Gateway.Request.LOGIN] = Login;
            s_OpcodeMap[Opcode.Gateway.Request.LAUNCHER] = Launcher;
        }

        #endregion

        #region Private Methods

        private static bool Unhandled(ClientContext Me, Packet packet)
        {
            Console.WriteLine("Unhandled msg 0x{0:X4}", packet.Opcode);
#if DEBUG
            return true;
#endif
            return false;
        }

        private static bool Ping(ClientContext Me, Packet packet)
        {
            return true;
        }

        private static bool Identity(ClientContext Me, Packet packet)
        {
             if (packet.ReadAscii() == "SR_Client")
             {
                 Packet resp = new Packet(Opcode.General.IDENTITY);
                 resp.WriteAscii("GatewayServer");
                 resp.WriteByte(0);
                 Me.SocketContext.Send(resp);

                 return true;
             }

             return false;
        }

        private static bool Patch(ClientContext Me, Packet packet)
        {
            bool ret = true;

            byte locale = packet.ReadByte();
            string identity_name = packet.ReadAscii();
            uint version = packet.ReadUInt32();

            Packet seed1 = new Packet(Opcode.General.SEED_1, false, true, new byte[] { 0x01, 0x00, 0x01, 0x47, 0x01, 0x05, 0x00, 0x00, 0x00, 0x02 });
            Packet seed2 = new Packet(Opcode.General.SEED_2, false, true, new byte[] { 0x03, 0x00, 0x02, 0x00, 0x02 });

            Me.SocketContext.EnqueuePacket(seed1);
            Me.SocketContext.EnqueuePacket(seed2);

            if (identity_name == "SR_Client")
            {
                uint current_version = Data.Globals.GetConfigValue<uint>("CurrentVersion");
                uint latest_version = Data.Globals.GetConfigValue<uint>("LatestVersion");

                Packet resp = new Packet(Opcode.Gateway.Response.PATCH, false, true);

                if (version == current_version) //no patches
                    resp.WriteByte(1);
                else
                {
                    resp.WriteByte(2);

                    if (version < latest_version) //too old client, cannot update
                        resp.WriteByte(5);
                    else if (version > current_version) //too new client ? its illegal operation !
                    {
                        resp.WriteByte(1);
                        ret = false;
                        Console.WriteLine("Illegal operation ! Current Version: {0}, Requested: {1}", current_version, version);
                    }
                    else //send patches
                    {
                        resp.WriteByte(2);
                        resp.WriteAscii(Data.Globals.GetConfigValue<string>("DownloadServerIPAddress"));
                        resp.WriteUInt32(Data.Globals.GetConfigValue<uint>("DownloadServerPort"));
                        resp.WriteUInt32(current_version);
                        foreach(var file in Services.Patch.Items)
                        {
                            resp.WriteByte(1);
                            resp.WriteUInt32(file.ID);
                            resp.WriteAscii(file.FileName);
                            resp.WriteAscii(file.Path);
                            resp.WriteUInt32(file.FileSize);
                            resp.WriteByte(file.ToBePacked);
                        }
                        resp.WriteByte(0);
                    }
                }

                Me.SocketContext.EnqueuePacket(resp);
                Me.SocketContext.ProcessSendQueue();
            }
            else
                return false;

            if (ret)
                Me.PingTimer.Start(10000, 10000);
            
            return ret;
        }

        private static bool ServerList(ClientContext Me, Packet packet)
        {
            if (Environment.TickCount - Me.LastServerListTick >= 4000)
            {
                Packet resp = new Packet(Opcode.Gateway.Response.SERVERLIST);
                resp.WriteByte(1);
                resp.WriteByte(0x15);
                resp.WriteAscii("SRO_Global_TestBed");
                resp.WriteByte(0);
                foreach(var shard in Services.Shard.Items)
                {
                    resp.WriteUInt8(1);
                    resp.WriteUInt16(shard.ID);
                    resp.WriteAscii(shard.Name);
                    resp.WriteUInt16(shard.CurrentUsers);
                    resp.WriteUInt16(shard.MaxUsers);
                    resp.WriteUInt8(shard.State);
                }
                resp.WriteByte(0);

                Me.SocketContext.Send(resp);
                return true;
            }

            //too much requests, disconnect the client
            return false;
        }

        #pragma warning disable 4014
        private static bool Login(ClientContext Me, Packet packet)
        {
            byte locale = packet.ReadByte();
            string id = packet.ReadAscii();
            string password = SCommon.Utility.MD5Hash(packet.ReadAscii(), true);
            ushort server = packet.ReadUInt16();
            Data._wrongpass_item wrong;

            Packet resp = new Packet(Opcode.Gateway.Response.LOGIN);

            if (Services.Shard.Items.Exists(p => p.ID == server))
            {
                var shard = Services.Shard.Items.FirstOrDefault(p => p.ID == server);
                resp.WriteByte(2);
                if (shard.State != 1)
                {
                    resp.WriteByte(2);
                    resp.WriteByte(2);
                }
                else if (shard.ContentID != locale)
                {
                    resp.WriteByte(7);
                }
                else if (id.Contains('\''))
                {
                    resp.WriteByte(6);
                }
                else if (Data.Globals.WrongPasswordTries.ContainsKey(id) && (wrong = Data.Globals.WrongPasswordTries[id]).Tries >= Data.Globals.MAX_WRONG_PASSWORD)
                {
                    if (Environment.TickCount - wrong.LastTick < 5 * 60 * 1000)
                    {
                        resp.WriteByte(1);
                        resp.WriteInt32(wrong.Tries);
                        resp.WriteInt32(Data.Globals.MAX_WRONG_PASSWORD);
                    }
                    else
                        Data.Globals.WrongPasswordTries.Remove(id);
                }
                else
                {
                    Me.ProcessLoginResult(shard, id, password);
                    return true;
                }

                Me.SocketContext.Send(resp);
            }
            else
                return false;

            return true;
        }

        private static bool Launcher(ClientContext Me, Packet packet)
        {
            byte locale = packet.ReadByte();
            byte written_news = 0;

            Packet resp = new Packet(Opcode.Gateway.Response.LAUNCHER, false, true);
            resp.WriteByte(0); //news count
            foreach(var news in Services.Launcher.Items)
            {
                if (news.ContentID == locale)
                {
                    resp.WriteAscii(news.Subject);
                    resp.WriteAscii(news.Article);
                    resp.WriteUInt16(news.EditDate.Year);
                    resp.WriteUInt16(news.EditDate.Month);
                    resp.WriteUInt16(news.EditDate.Day);
                    resp.WriteUInt16(news.EditDate.Hour);
                    resp.WriteUInt16(news.EditDate.Minute);
                    resp.WriteUInt16(news.EditDate.Second);
                    resp.WriteUInt32(news.EditDate.Millisecond);

                    written_news++;
                }
            }
            resp.ReWriteAt(0, written_news);

            Me.SocketContext.Send(resp);

            return true;
        }

        #endregion
    }
}
