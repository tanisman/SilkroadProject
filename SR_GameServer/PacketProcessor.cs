namespace SR_GameServer
{
    using System;
    using System.Linq;
    using System.Threading;

    using SCommon.Security;

    using SharpDX;

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
            s_OpcodeMap[Opcode.Agent.Request.CONNECTION] = Login;
            s_OpcodeMap[Opcode.Agent.Request.CHARACTER_SCREEN] = CharacterScreen;
            s_OpcodeMap[Opcode.Agent.Request.CHARACTER_SELECT] = CharacterSelect;
            s_OpcodeMap[Opcode.Agent.Request.CHARACTER_ENTERWORLD] = CharacterEnterWorld;
            s_OpcodeMap[Opcode.Agent.Request.GAMEOBJECT_MOVEMENT] = CharacterMove;
            s_OpcodeMap[Opcode.Agent.Request.GAMEOBJECT_STOP] = CharacterStop;
            s_OpcodeMap[Opcode.Agent.Request.GAMEOBJECT_SET_ANGLE] = CharacterSetAngle;
            s_OpcodeMap[Opcode.Agent.GAMEOBJECT_EMOTE] = GObjEmote;
            s_OpcodeMap[Opcode.Agent.Request.SELECT_GAMEOBJECT] = SelectGObj;
            s_OpcodeMap[Opcode.Agent.Request.TELEPORT] = Teleport;
            s_OpcodeMap[Opcode.Agent.Request.TELEPORT_LOADING] = TeleportLoading;
            s_OpcodeMap[Opcode.Agent.Request.TELEPORT_APPOINT] = TeleportAppoint;
            s_OpcodeMap[Opcode.Agent.Request.GAMEGUIDE] = GameGuide;
            s_OpcodeMap[Opcode.Agent.Request.CHAT] = Chat;
            s_OpcodeMap[Opcode.Agent.Request.INVENTORY_OPERATION] = InventoryOperations;
            s_OpcodeMap[Opcode.Agent.Request.SHOP_OPEN] = OpenShop;
            s_OpcodeMap[Opcode.Agent.Request.SHOP_CLOSE] = CloseShop;
            s_OpcodeMap[Opcode.Agent.Request.GM_COMMAND] = ExecuteCommand;
        }

        #endregion

        #region Private Methods

        private static bool Unhandled(ClientContext Me, Packet pkt)
        {
            //Me.Teleport(5934, 1138, 250, 153, 10);
            Console.WriteLine("Unhandled msg 0x{0:X4}", pkt.Opcode);
#if DEBUG
            return true;
#endif
            return false;
        }

        private static bool Ping(ClientContext Me, Packet pkt)
        {
            return true;
        }

        private static bool Identity(ClientContext Me, Packet packet)
        {
            if (packet.ReadAscii() == "SR_Client")
            {
                Packet resp = new Packet(Opcode.General.IDENTITY);
                resp.WriteAscii("AgentServer");
                resp.WriteByte(0);
                Me.SocketContext.Send(resp);

                return true;
            }

            return false;
        }

        #pragma warning disable 4014
        private static bool Login(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
                return false;

            int session_id = pkt.ReadInt32();
            string id = pkt.ReadAscii();
            string pw = pkt.ReadAscii();

            Me.ProcessLoginResult(id, pw, session_id);
            return true;
        }

        private static bool CharacterScreen(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
                return false;

            Packet resp = new Packet(Opcode.Agent.Response.CHARACTER_SCREEN);

            switch (pkt.ReadByte())
            {
                case 1: //character create
                {
                        resp.WriteByte(1);

                        string charname = pkt.ReadAscii();

                        if (charname.Length < 3 || charname.Length > 12)
                        {
                            resp.WriteByte(2);
                            resp.WriteByte(12);
                            Me.SocketContext.Send(resp);
                            return true;
                        }

                        foreach (var c in charname)
                        {
                            if (Data.Globals.AllowedCharacters.IndexOf(c) == -1)
                            {
                                resp.WriteByte(2);
                                resp.WriteByte(13);
                                Me.SocketContext.Send(resp);
                                return true;
                            }
                        }

                        int model = pkt.ReadInt32();
                        byte volume = pkt.ReadByte();
                        if (volume > 68)
                            volume = 68;
                        int item1 = pkt.ReadInt32();
                        int item2 = pkt.ReadInt32();
                        int item3 = pkt.ReadInt32();
                        int item4 = pkt.ReadInt32();

                        if (
                            !Data.Globals.Ref.CharGen.Exists(p => p.RefObjID == model) ||
                            !Data.Globals.Ref.CharGen.Exists(p => p.RefObjID == item1) ||
                            !Data.Globals.Ref.CharGen.Exists(p => p.RefObjID == item2) ||
                            !Data.Globals.Ref.CharGen.Exists(p => p.RefObjID == item3) ||
                            !Data.Globals.Ref.CharGen.Exists(p => p.RefObjID == item4)
                            )
                        {
                            //invalid operation
                            return false;
                        }

                        int charid = Data.Globals.ShardDB.Result<int>("exec _AddNewChar {0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}", Me.AccountInfo.SID, charname, model, volume, item1, item2, item3, item4);

                        if (charid <= 0)
                        {
                            resp.WriteByte(2);
                            switch (charid)
                            {
                                case -1:
                                    resp.WriteByte(5);
                                    break;

                                case -2:
                                    resp.WriteByte(16);
                                    break;

                                default: //other stuff, unexpected
                                    return false;
                            }
                        }
                        else
                            resp.WriteByte(1);

                        Me.SocketContext.Send(resp);
                        return true;
                    }
                case 2: //char listing
                {
                        Me.AccountInfo.Characters.Clear();

                        resp.WriteByte(2);
                        resp.WriteByte(1);
                        using (var reader = Data.Globals.ShardDB.ExecuteReader("SELECT * FROM _Char WHERE EXISTS(SELECT * FROM _User WHERE UserSID = {0} AND _User.CharID = _Char.CharID) ORDER BY CharID", Me.AccountInfo.SID))
                        {
                            int idx = 0;
                            while (reader.Read())
                            {
                                int item_count = 0;
                                int CharID = (int)reader["CharID"];
                                resp.WriteByte(1); //char exists
                                resp.WriteInt32(reader["RefObjID"]);
                                resp.WriteAscii(reader["CharName16"]);
                                resp.WriteByte(reader["Scale"]);
                                resp.WriteByte(reader["CurLevel"]);
                                resp.WriteInt64(reader["ExpOffset"]);
                                resp.WriteInt16(reader["Strength"]);
                                resp.WriteInt16(reader["Intellect"]);
                                resp.WriteInt16(reader["RemainStatPoint"]);
                                resp.WriteInt32(reader["CurHP"]);
                                resp.WriteInt32(reader["CurMP"]);

                                using (var items = Data.Globals.ShardDB.ExecuteReader("select RefItemID from _Items as A inner join _Inventory as B on A.ID64 = B.ItemID and B.CharID = {0} and B.Slot between 0 and 7 and A.RefItemID <> 0 AND A.ItemSerial > 0", CharID))
                                {
                                    resp.WriteByte(0);
                                    while (items.Read())
                                    {
                                        resp.WriteInt32(items["RefItemID"]);
                                        item_count++;
                                    }  //no plus values ?
                                    if (item_count > 0)
                                        resp.GoBackAndWrite((item_count * 4) + 1, (byte)item_count);
                                }

                                Me.AccountInfo.Characters.Add(new Tuple<int, int>(CharID, idx++));
                            }

                            for (int i = 0; i < 3 - idx; i++)
                                resp.WriteByte(0);
                        }
                        Me.SocketContext.Send(resp);
                        return true;
                }
                case 3: //deletechar, warning: It deletes character direct no waits
                {
                        resp.WriteByte(3);

                        byte idx = pkt.ReadByte();
                        
                        var character = Me.AccountInfo.Characters.FirstOrDefault(p => p.Item2 == idx);
                        if (character != null)
                        {
                            resp.WriteByte(1);

                            Data.Globals.ShardDB.ExecuteCommandAsync("UPDATE _Char SET Deleted = 1 WHERE CharID = {0}", character.Item1);
                            Data.Globals.ShardDB.ExecuteCommandAsync("DELETE FROM _User WHERE CharID = {0}", character.Item1);

                            Me.AccountInfo.Characters.Remove(character);
                            character = null;
                        }
                        else
                        {
                            resp.WriteByte(2);
                            resp.WriteByte(18); //S18 dunno what is mean but its an error code anyway XD
                        }

                        Me.SocketContext.Send(resp);
                        return true;
                }
                case 4: //charname check
                {
                        resp.WriteByte(4);

                        string charname = pkt.ReadAscii();

                        if (charname.Length < 3 || charname.Length > 12)
                        {
                            resp.WriteByte(2);
                            resp.WriteByte(12);
                            Me.SocketContext.Send(resp);
                            return true;
                        }

                        foreach (var c in charname)
                        {
                            if (Data.Globals.AllowedCharacters.IndexOf(c) == -1)
                            {
                                resp.WriteByte(2);
                                resp.WriteByte(13);
                                Me.SocketContext.Send(resp);
                                return true;
                            }
                        }

                        if (Data.Globals.ShardDB.Result<int>("SELECT COUNT(CharName16) FROM _CharNameList WHERE CharName16 = '{0}'", charname) != 0)
                        {
                            resp.WriteByte(2);
                            resp.WriteByte(16);
                            Me.SocketContext.Send(resp);
                            return true;
                        }

                        resp.WriteByte(1);
                        Me.SocketContext.Send(resp);
                        return true;
                }
            }
            return false;
        }

        private static bool CharacterSelect(ClientContext Me, Packet pkt)
        {
            if (Me.Character == null)
            {
                byte idx = pkt.ReadByte();
                var character = Me.AccountInfo.Characters.FirstOrDefault(p => p.Item2 == idx);
                if (character != null)
                {
                    Me.ProcessIngameRequest(character.Item1);
                    return true;
                }
            }
            return false;
        }

        private static bool CharacterEnterWorld(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null && !Me.Character.m_isIngame)
            {
                Packet resp1 = new Packet(Opcode.Agent.CHARACTER_STAT);
                resp1.WriteInt32(Me.Character.TotalMinPhyAtk);
                resp1.WriteInt32(Me.Character.TotalMaxPhyAtk);
                resp1.WriteInt32(Me.Character.TotalMinMagAtk);
                resp1.WriteInt32(Me.Character.TotalMaxMagAtk);
                resp1.WriteUInt16(Me.Character.TotalPhyDef);
                resp1.WriteUInt16(Me.Character.TotalMagDef);
                resp1.WriteUInt16(Me.Character.TotalHitRate);
                resp1.WriteUInt16(Me.Character.TotalParryRate);
                resp1.WriteInt32(Me.Character.MaxHP);
                resp1.WriteInt32(Me.Character.MaxMP);
                resp1.WriteInt16(Me.Character.TotalStrenght);
                resp1.WriteInt16(Me.Character.TotalIntellect);
                Me.SocketContext.Send(resp1);

                Packet resp2 = new Packet(Opcode.Agent.CHARACTER_ENTERWORLD);
                Me.SocketContext.Send(resp2);

                return true;
            }
            return false;
        }

        private static bool CharacterMove(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    byte type = pkt.ReadByte();

                    if (type == 0 && pkt.ReadByte() == 1)
                        Me.Character.Move(pkt.ReadUInt16());
                    else if (type == 1)
                        Me.Character.MoveTo(pkt.ReadInt16(), new Vector3(pkt.ReadInt16(), pkt.ReadInt16(), pkt.ReadInt16()));
                }
                return true;
            }
            return false;
        }

        private static bool CharacterStop(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    if (Me.Character.m_movementType == MovementType.Moving)
                    {
                        ushort angle = pkt.ReadUInt16();
                        lock (Me.Character.m_lock)
                            Me.Character.m_angle = angle;

                        Me.Character.StopMovement(true);
                    }
                }
                return true;
            }
            return false;
        }

        private static bool CharacterSetAngle(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    ushort angle = pkt.ReadUInt16();
                    lock (Me.Character.m_lock)
                    {
                        Me.Character.m_angle = angle;
                        if (Me.Character.m_hasAngleMovement)
                            Me.Character.ChangeDirection(new Vector2((float)Math.Cos((angle / 182f) * (Math.PI / 180f)), (float)Math.Sin((angle / 182f) * (Math.PI / 180f))));
                    }

                    Packet resp = new Packet(Opcode.Agent.Response.GAMEOBJECT_SET_ANGLE);
                    resp.WriteInt32(Me.Character.m_uniqueId);
                    resp.WriteUInt16(angle);
                    Me.SocketContext.Send(resp);
                    Me.Character.BroadcastToSightList(resp);
                }
                return true;
            }
            return false;
        }

        private static bool GObjEmote(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    byte type = pkt.ReadByte();

                    Me.Character.Emote(type);
                }
                return true;
            }
            return false;
        }

        private static bool SelectGObj(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    int uniqueId = pkt.ReadInt32();

                    Packet resp = new Packet(Opcode.Agent.Response.SELECT_GAMEOBJECT);

                    if (uniqueId == Me.Character.m_uniqueId)
                    {
                        Me.Character.m_selectedGObj = Me.Character;
                        return true;
                    }

                    lock (Me.Character.m_lock)
                    {
                        var obj = Me.Character.m_inSightGObjList.FirstOrDefault(p => p.Object.m_uniqueId == uniqueId && p.Seen);
                        if (obj.Object != null)
                        {
                            resp.WriteByte(1);
                            resp.WriteInt32(uniqueId);
                            switch (obj.Object.m_type)
                            {
                                case GObjType.GObjNPC:
                                    var npc = (GObjNPC)obj.Object;
                                    for (int i = 0; i < npc.m_talkOptions.Count; i++)
                                        resp.WriteByte(npc.m_talkOptions[i]);
                                    break;
                                case GObjType.GObjMob:
                                    var mob = (GObjMob)obj.Object;
                                    resp.WriteByte(1);
                                    resp.WriteInt32(Interlocked.CompareExchange(ref mob.m_currentHealthPoints, 0, 0));
                                    resp.WriteByte(1);
                                    resp.WriteByte(1);
                                    resp.WriteByte(1);
                                    resp.WriteByte(1);
                                    break;
                                case GObjType.GObjChar:
                                    var pc = (GObjChar)obj.Object;
                                    resp.WriteInt32(0);
                                    resp.WriteByte(pc.m_triJob.TraderLvl);
                                    resp.WriteByte(pc.m_triJob.HunterLvl);
                                    resp.WriteByte(pc.m_triJob.ThiefLvl);
                                    resp.WriteByte(0);
                                    break;
                            }
                            Me.Character.m_selectedGObj = obj.Object;
                        }
                        else
                        {
                            resp.WriteByte(0);
                            resp.WriteByte(0);
                            Me.Character.m_selectedGObj = null;
                        }
                    }

                    Me.SocketContext.Send(resp);
                }
                return true;
            }
            return false;
        }

        private static bool Teleport(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame && !Me.Character.m_isBusy)
                {
                    int uniqueId = pkt.ReadInt32();
                    byte flag = pkt.ReadByte();
                    int dest = pkt.ReadInt32();
                    if (flag == 2 && Me.Character.m_selectedGObj != null && Me.Character.m_selectedGObj.m_uniqueId == uniqueId)
                    {
                        var teleport_data = Data.Globals.Ref.TeleportData.FirstOrDefault(p => p.AssocRefObjID == Me.Character.m_selectedGObj.m_model);
                        var link = Data.Globals.Ref.TeleportLink.FirstOrDefault(p => p.OwnerTeleport == teleport_data.ID && p.TargetTeleport == dest);
                        if (link != null)
                        {
                            Packet resp;
                            lock (Me.Character.m_lock)
                            {
                                if (Me.Character.m_gold < link.Fee)
                                    return true; //not enough gold packet

                                Me.Character.UpdateGold(-link.Fee, out resp);
                            }
                            Me.SocketContext.Send(resp);
                            teleport_data = Data.Globals.Ref.TeleportData.FirstOrDefault(p => p.ID == dest);
                            Me.Teleport(teleport_data.GenRegionID, teleport_data.GenPos_X, teleport_data.GenPos_Y, teleport_data.GenPos_Z, teleport_data.GenAreaRadius);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private static bool TeleportLoading(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isTeleporting)
                {
                    Console.WriteLine("called TeleportLoading :R");
                    return true;
                }
            }
            return false;
        }

        private static bool TeleportAppoint(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    int uniqueId = pkt.ReadInt32();

                    Packet resp = new Packet(Opcode.Agent.Response.TELEPORT_APPOINT);

                    lock (Me.Character.m_lock)
                    {
                        var obj = Me.Character.m_inSightGObjList.FirstOrDefault(p => p.Object.m_uniqueId == uniqueId && p.Seen);
                        if (obj.Object != null)
                        {
                            var teleport_data = Data.Globals.Ref.TeleportData.FirstOrDefault(p => p.AssocRefObjID == obj.Object.m_model);
                            if (teleport_data == null)
                                return true;

                            if (teleport_data.CanBeResurrectPos == 1)
                            {
                                Me.Character.m_appointedTeleport = teleport_data.ID;
                                resp.WriteByte(1);
                            }
                            else
                                resp.WriteByte(0);
                        }
                    }

                    Me.SocketContext.Send(resp);
                }
                return true;
            }
            return false;
        }

        private static bool GameGuide(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                int data = pkt.ReadByte();
                Me.Character.m_gameGuideData |= (int)Math.Pow(2, data);
                Data.Globals.ShardDB.ExecuteCommandAsync(String.Format("UPDATE _Char SET GGData = {0} WHERE CharID = {1}", Me.Character.m_gameGuideData, Me.Character.m_charId));

                Packet resp = new Packet(Opcode.Agent.Response.GAMEGUIDE);
                resp.WriteByte(1);
                Me.SocketContext.Send(resp);

                return true;
            }
            return false;
        }

        private static bool Chat(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    byte type = pkt.ReadByte();
                    byte idx = pkt.ReadByte();

                    if ((type == 3 || type == 7) && !Me.Character.m_accountInfo.Auth.HasFlag(Data.AuthType.GM))
                        return false;

                    Packet resp = new Packet(Opcode.Agent.SEND_MESSAGE);
                    resp.WriteByte(type);

                    string msg = String.Empty;
                    GObjChar sendTo = Me.Character;
                    bool succeded = true;

                    switch (type)
                    {
                        case 1: //normal all chat
                        case 3: //gm all chat
                        {
                            msg = pkt.ReadAscii();
                            
                            resp.WriteInt32(Me.Character.m_uniqueId);
                        }
                        break;
                        case 2: //pm
                        {
                            string to = pkt.ReadAscii();
                            msg = pkt.ReadAscii();
                            sendTo = GameWorld.GObjUtils.FindGObjCharByName(to);
                            if (sendTo == null)
                                succeded = false;

                            resp.WriteAscii(Me.Character.m_name);
                        }
                        break;
                        case 7: //notice
                        {
                            msg = pkt.ReadAscii();
                            sendTo = null;
                        }
                        break;
                        case 4:
                        case 5:
                        case 6:
                        case 9:
                        case 11:
                        {
                            msg = pkt.ReadAscii();
                            
                            resp.WriteAscii(Me.Character.m_name);
                        }
                        break;
                        default:
                            return false;
                    }

                    //check msg content ?
                    resp.WriteAscii(msg);

                    if (sendTo == null)
                    {
                        if (type == 3)
                        {
                            //send cannot find
                        }
                        else if (type == 7)
                        {
                            GameWorld.GObjUtils.BroadcastPacket(resp);
                        }
                    }
                    else
                    {
                        sendTo.SendPacket(resp);
                    }

                    Packet resp2 = new Packet(Opcode.Agent.Response.CHAT);
                    resp2.WriteByte(succeded);
                    if (succeded)
                    {
                        resp2.WriteByte(type);
                        resp2.WriteByte(idx);
                    }
                    Me.SocketContext.Send(resp2);
                }
                return true;
            }
            return false;
        }

        private static bool InventoryOperations(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                    Me.Character.OperateInventory(pkt.GetBytes());

                return true;
            }
            return false;
        }

        private static bool OpenShop(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    int uniqueId = pkt.ReadInt32();

                    Packet resp = new Packet(Opcode.Agent.Response.SHOP_OPEN);
                    Me.Character.StopMovement(true);
                    lock (Me.Character.m_lock)
                    {
                        if (!Me.Character.m_isBusy)
                        {
                            var obj = Me.Character.m_inSightGObjList.FirstOrDefault(p => p.Object.m_uniqueId == uniqueId && p.Seen);
                            if (obj.Object != null && obj.Object.m_type == GObjType.GObjNPC)
                            {
                                resp.WriteByte(1);
                                resp.WriteInt32(pkt.ReadByte());
                                Me.Character.m_openedShop = (GObjNPC)obj.Object;
                                Me.Character.m_isBusy = true;
                            }
                            else
                                resp.WriteByte(0);
                        }
                        else
                            return true;
                    }
                    Me.SocketContext.Send(resp);
                }
                return true;
            }
            return false;
        }

        private static bool CloseShop(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                {
                    Packet resp = new Packet(Opcode.Agent.Response.SHOP_CLOSE);
                    lock (Me.Character.m_lock)
                    {
                        if (Me.Character.m_isBusy && Me.Character.m_openedShop != null)
                        {
                            resp.WriteByte(1);
                            Me.Character.m_openedShop = null;
                            Me.Character.m_isBusy = false;
                        }
                        else
                            resp.WriteByte(0);
                    }
                    Me.SocketContext.Send(resp);
                }
                return true;
            }
            return false;
        }

        private static bool ExecuteCommand(ClientContext Me, Packet pkt)
        {
            if (Me.Character != null)
            {
                if (Me.Character.m_isIngame)
                    return Me.ExecuteCommand(pkt);

                return true;
            }
            return false;
        }
        #endregion
    }
}
