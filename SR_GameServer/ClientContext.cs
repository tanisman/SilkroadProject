namespace SR_GameServer
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using SCore;

    using SCommon;
    using SCommon.Security;
    using SCommon.Networking;

    using Data;
    using GameWorld;

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

        /// <summary>
        /// Stores the player's account info.
        /// </summary>
        private AccountInfo m_AccountInfo;

        /// <summary>
        /// Stores the player's game object
        /// </summary>
        private GObjChar m_Character;

        #endregion

        #region Constructors & Destructors

        public ClientContext()
        {
            m_PingTimer = new AsyncTimer(PingTimerCallback);
            m_blDisconnected = false;
        }

        public ClientContext(SocketContext context)
            : this()
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
        /// Gets the account info
        /// </summary>
        public AccountInfo AccountInfo => m_AccountInfo;

        /// <summary>
        /// Get the character
        /// </summary>
        public GObjChar Character => m_Character;

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
        public async Task PacketTransferredNotify(Packet packet)
        {
            if (packet.Opcode == SCommon.Opcode.Agent.CHARACTER_ENTERWORLD)
            {
                m_Character.m_isBusy = false;
                m_Character.m_isIngame = true;
                m_Character.m_isTeleporting = false;
                m_Character.m_assignedListNode = Globals.GObjList.push(m_Character);
                m_Character.SightCheck();
            }
            packet.m_data = null;
        }

        public async Task ProcessLoginResult(string id, string pass, int session)
        {
            m_AccountInfo = new AccountInfo();
            m_AccountInfo.Characters = new List<Tuple<int, int>>();
            m_AccountInfo.StrUserID = id;
            m_AccountInfo.Password = pass;

            using (var reader = await Globals.ShardDB.ExecuteReaderAsync("exec _CertifyUser '{0}', '{1}', {2}", id, Utility.MD5Hash(pass, true), session))
            {
                await reader.ReadAsync();
                
                int type = await reader.GetFieldValueAsync<int>(0);

                if (type == 0)
                {
                    m_AccountInfo.SID = await reader.GetFieldValueAsync<int>(1);
                    m_AccountInfo.Auth = (AuthType) await reader.GetFieldValueAsync<byte>(2);

                    Packet resp = new Packet(SCommon.Opcode.Agent.Response.CONNECTION);
                    resp.WriteByte(1);
                    m_SocketContext.Send(resp);
                }
                else
                    Disconnect();
            }
        }

        public async Task ProcessIngameRequest(int charid)
        {
            if (m_Character == null)
                m_Character = new GObjChar(this);

            m_Character.m_charId = charid;
            m_Character.m_accountInfo = m_AccountInfo;
            m_Character.m_uniqueId = Services.UniqueID.GenerateGObjID();

            using (var reader = await Globals.ShardDB.ExecuteReaderAsync("SELECT * FROM _Char WHERE CharID = {0}", charid))
            {
                while (await reader.ReadAsync())
                {
                    m_Character.m_name = await reader.GetFieldValueAsync<string>((int)_Char.CharName16);
                    m_Character.m_model = await reader.GetFieldValueAsync<int>((int)_Char.RefObjID);
                    m_Character.m_scale = await reader.GetFieldValueAsync<byte>((int)_Char.Scale);
                    m_Character.m_level = await reader.GetFieldValueAsync<byte>((int)_Char.CurLevel);
                    m_Character.m_maxLevel = await reader.GetFieldValueAsync<byte>((int)_Char.MaxLevel);
                    m_Character.m_experience = await reader.GetFieldValueAsync<long>((int)_Char.ExpOffset);
                    m_Character.m_skillPoint = await reader.GetFieldValueAsync<int>((int)_Char.RemainSkillPoint);
                    m_Character.m_skillExperience = await reader.GetFieldValueAsync<int>((int)_Char.SExpOffset);
                    m_Character.m_hwanCount = await reader.GetFieldValueAsync<byte>((int)_Char.RemainHwanCount);
                    m_Character.m_gold = await reader.GetFieldValueAsync<long>((int)_Char.RemainGold);
                    m_Character.m_appointedTeleport = await reader.GetFieldValueAsync<int>((int)_Char.AppointedTeleport);
                    m_Character.m_gameGuideData = await reader.GetFieldValueAsync<int>((int)_Char.GGData);
                    m_Character.m_baseWalkSpeed = 16f;
                    m_Character.m_baseRunSpeed = 100f;//75
                    m_Character.m_baseHwanSpeed = 100f;
                    m_Character.m_currentHealthPoints = await reader.GetFieldValueAsync<int>((int)_Char.CurHP);
                    m_Character.m_currentManaPoints = await reader.GetFieldValueAsync<int>((int)_Char.CurMP);
                    m_Character.m_baseStrenght = await reader.GetFieldValueAsync<short>((int)_Char.Strength);
                    m_Character.m_baseIntellect = await reader.GetFieldValueAsync<short>((int)_Char.Intellect);
                    m_Character.m_attributePoints = await reader.GetFieldValueAsync<short>((int)_Char.RemainStatPoint);
                    m_Character.m_baseMaxHealth = Formula.BaseMaxHealthManaByStats(m_Character.m_level, m_Character.m_baseStrenght);
                    m_Character.m_baseMaxMana = Formula.BaseMaxHealthManaByStats(m_Character.m_level, m_Character.m_baseIntellect);
                    m_Character.m_basePhyDef = 6 + Formula.BaseDefByStats(m_Character.m_baseStrenght);
                    m_Character.m_basePhyDef = 3 + Formula.BaseDefByStats(m_Character.m_baseIntellect);
                    m_Character.m_baseParryRate = 11;
                    m_Character.m_baseHitRate = 11;
                    m_Character.m_baseMinPhyAtk = 6 + Formula.BaseMinAtkByStats(m_Character.m_baseStrenght);
                    m_Character.m_baseMaxPhyAtk = 9 + Formula.BaseMaxAtkByStats(m_Character.m_baseStrenght);
                    m_Character.m_baseMinMagAtk = 6 + Formula.BaseMinAtkByStats(m_Character.m_baseIntellect);
                    m_Character.m_baseMaxMagAtk = 10 + Formula.BaseMinAtkByStats(m_Character.m_baseIntellect);
                    m_Character.m_phyBalancePercent = Formula.PhyBalance(m_Character.m_level, m_Character.m_baseStrenght);
                    m_Character.m_magBalancePercent = Formula.MagBalance(m_Character.m_level, m_Character.m_baseIntellect);
                    m_Character.m_phyAbsorbPercent = 0;
                    m_Character.m_magAbsorbPercent = 0;
                    m_Character.m_bonusStrenght = 0;
                    m_Character.m_bonusIntellect = 0;
                    m_Character.m_bonusPhyDefPercent = 0;
                    m_Character.m_bonusMagDefPercent = 0;
                    m_Character.m_bonusPhyAtkPercent = 0;
                    m_Character.m_bonusMagAtkPercent = 0;
                    m_Character.m_bonusParryPercent = 0;
                    m_Character.m_bonusHitPercent = 0;
                    m_Character.m_bonusMaxHealthPercent = 0;
                    m_Character.m_bonusMaxManaPercent = 0;
                    m_Character.m_bonusBlockRatioPercent = 0;
                    m_Character.m_bonusCritialRatioPercent = 0;
                    m_Character.m_bonusPhyDef = 0;
                    m_Character.m_bonusMagDef = 0;
                    m_Character.m_bonusMinPhyAtk = 0;
                    m_Character.m_bonusMinMagAtk = 0;
                    m_Character.m_bonusMaxPhyAtk = 0;
                    m_Character.m_bonusMaxMagAtk = 0;
                    m_Character.m_bonusParryRate = 0;
                    m_Character.m_bonusHitRate = 0;
                    m_Character.m_bonusMaxHealth = 0;
                    m_Character.m_bonusMaxMana = 0;
                    m_Character.m_bonusBlockRatio = 0;
                    m_Character.m_bonusCriticalRatio = 0;
                    m_Character.m_region = await reader.GetFieldValueAsync<short>((int)_Char.LatestRegion);
                    m_Character.m_position.X = Convert.ToSingle(await reader.GetFieldValueAsync<short>((int)_Char.PosX));
                    m_Character.m_position.Y = Convert.ToSingle(await reader.GetFieldValueAsync<short>((int)_Char.PosY));
                    m_Character.m_position.Z = Convert.ToSingle(await reader.GetFieldValueAsync<short>((int)_Char.PosZ));
                    m_Character.m_angle = Convert.ToSingle(await reader.GetFieldValueAsync<int>((int)_Char.Angle));
                    m_Character.m_triJob.TraderLvl = await reader.GetFieldValueAsync<byte>((int)_Char.TraderLvl);
                    m_Character.m_triJob.ThiefLvl = await reader.GetFieldValueAsync<byte>((int)_Char.ThiefLvl);
                    m_Character.m_triJob.HunterLvl = await reader.GetFieldValueAsync<byte>((int)_Char.HunterLvl);
                    m_Character.m_triJob.TraderExp = await reader.GetFieldValueAsync<int>((int)_Char.TraderExp);
                    m_Character.m_triJob.ThiefExp = await reader.GetFieldValueAsync<int>((int)_Char.ThiefExp);
                    m_Character.m_triJob.HunterExp = await reader.GetFieldValueAsync<int>((int)_Char.HunterExp);
                    m_Character.m_walkState = WalkState.Running;
                    m_Character.m_lifeState = LifeState.Alive;
                    m_Character.m_movementType = MovementType.NotMoving;

                    if (m_Character.m_currentHealthPoints > m_Character.MaxHP)
                        m_Character.m_currentHealthPoints = (int)m_Character.MaxHP;

                    if (m_Character.m_currentManaPoints > m_Character.MaxMP)
                        m_Character.m_currentManaPoints = (int)m_Character.MaxMP;

                    m_Character.UpdatePosition();
                }
            }

            //Mastery
            using (var reader = await Globals.ShardDB.ExecuteReaderAsync("SELECT * FROM _CharSkillMastery WHERE CharID = {0}", charid))
            {
                while (await reader.ReadAsync())
                {
                    _skill_mastery m = new _skill_mastery();
                    m.ID = await reader.GetFieldValueAsync<int>((int)_CharSkillMastery.MasteryID);
                    m.Level = await reader.GetFieldValueAsync<byte>((int)_CharSkillMastery.Level);
                    m_Character.m_masteryTree[m.ID] = m;
                }
            }

            //Skills
            using (var reader = await Globals.ShardDB.ExecuteReaderAsync("SELECT * FROM _CharSkill WHERE CharID = {0} AND Enabled = 1", charid))
            {
                while (await reader.ReadAsync())
                    m_Character.m_skills.Add(await reader.GetFieldValueAsync<int>((int)_CharSkill.SkillID));
            }

            Packet resp = new Packet(SCommon.Opcode.Agent.Response.CHARACTER_SELECT);
            resp.WriteByte(1);
            m_SocketContext.Send(resp);

            Packet start_load = new Packet(SCommon.Opcode.Agent.CHARACTER_LOAD_START);
            m_SocketContext.Send(start_load);

            Packet load = new Packet(SCommon.Opcode.Agent.CHARACTER_LOAD_DATA);
            load.WriteInt32(m_Character.m_model);
            load.WriteAscii(m_Character.m_name);
            load.WriteByte(m_Character.m_scale);
            load.WriteByte(m_Character.m_level);
            load.WriteByte(m_Character.m_maxLevel);
            load.WriteInt64(m_Character.m_experience);
            load.WriteInt32(m_Character.m_skillExperience);
            load.WriteInt64(m_Character.m_gold);
            load.WriteInt32(m_Character.m_skillPoint);
            load.WriteInt16(m_Character.m_attributePoints);
            load.WriteByte(m_Character.m_hwanCount);
            load.WriteInt32(0); //gathered exp point ? wtf is that
            load.WriteInt32(m_Character.m_currentHealthPoints);
            load.WriteInt32(m_Character.m_currentManaPoints);
            load.WriteByte(m_Character.m_begginerIcon ? 1 : 0);
            load.WriteByte(m_Character.m_playerKillInfo.Dailiy);
            load.WriteInt16(m_Character.m_playerKillInfo.Total);
            load.WriteInt32(m_Character.m_playerKillInfo.MurdererLevel * 60); //in minutes
            load.WriteInt16(0);
            load.WriteInt16(m_Character.m_triJob.ArrangeLevel);
            load.WriteInt32(m_Character.m_triJob.ArrangePoint1);
            load.WriteInt32(m_Character.m_triJob.ArrangePoint2);
            load.WriteByte(m_Character.m_triJob.TraderLvl);
            load.WriteInt32(m_Character.m_triJob.TraderExp);
            load.WriteByte(m_Character.m_triJob.ThiefLvl);
            load.WriteInt32(m_Character.m_triJob.ThiefExp);
            load.WriteByte(m_Character.m_triJob.HunterLvl);
            load.WriteInt32(m_Character.m_triJob.HunterExp);

            using (var reader = await Globals.ShardDB.ExecuteReaderAsync("select * from _Items as A inner join _Inventory as B on A.ID64 = B.ItemID and B.CharID = {0} and A.RefItemID <> 0 AND A.ItemSerial > 0", m_Character.m_charId))
            {
                int item_count = 0;
                int skip_bytes = 1;

                m_Character.m_equipItems.Clear();

                load.WriteByte(0); //reserved for item count
                while (await reader.ReadAsync())
                {
                    _item item = new _item();
                    await item.ReadAsync(reader);

                    if (item.Slot < 13)
                        m_Character.EquipItemEffect(true, item, item.Slot);

                    skip_bytes += item.Write(load);
                    item_count++;
                }
                if (item_count > 0)
                    load.GoBackAndWrite(skip_bytes, (byte)item_count);
            }

            load.WriteByte(0);

            foreach (var mastery in m_Character.m_masteryTree)
            {
                if (mastery.ID != 0)
                {
                    load.WriteByte(1);
                    load.WriteInt32(mastery.ID);
                    load.WriteByte(mastery.Level);
                }
            }
            load.WriteByte(2); //masteries end

            load.WriteByte(0);

            foreach (int skill in m_Character.m_skills)
            {
                if (skill != 0 && Globals.Ref.Skill[skill].UI_SkillTab != 255)
                {
                    load.WriteByte(1);
                    load.WriteInt32(skill);
                    load.WriteByte(1); //enabled
                }
            }
            load.WriteByte(2); //skills end

            load.WriteByte(0); //completed quest count
            load.WriteByte(0); //active quest count

            load.WriteInt32(m_Character.m_uniqueId);

            load.WriteUInt16(m_Character.m_region);
            load.WriteSingle(m_Character.Position.X);
            load.WriteSingle(m_Character.Position.Y);
            load.WriteSingle(m_Character.Position.Z);
            load.WriteUInt16(m_Character.m_angle);

            load.WriteByte(m_Character.m_hasDestination); //has destination
            load.WriteByte(m_Character.m_walkState); //walking, running etc..
            
            if (m_Character.m_hasDestination)
            {
                load.WriteUInt16(m_Character.m_destinationRegion);
                load.WriteUInt16(m_Character.m_destination.X);
                load.WriteUInt16(m_Character.m_destination.Y);
                load.WriteUInt16(m_Character.m_destination.Z);
            }
            else
            {
                load.WriteByte(m_Character.m_hasAngleMovement);
                load.WriteUInt16(m_Character.m_angle);
            }
            
            load.WriteByte(m_Character.m_lifeState);
            load.WriteByte(m_Character.m_movementType);
            load.WriteByte(m_Character.m_status);


            load.WriteSingle(m_Character.m_baseWalkSpeed);
            load.WriteSingle(m_Character.m_baseRunSpeed);
            load.WriteSingle(m_Character.m_baseHwanSpeed);

            load.WriteByte(0); //reserved for buff count

            byte buff_count = 0;
            for (byte b = 0; b < 10; b++)
            {
                if (!m_Character.m_buffs[b].IsFree)
                {
                    load.WriteInt32(m_Character.m_buffs[b].SkillID);
                    load.WriteInt32(m_Character.m_buffs[b].CastingID);
                    buff_count++;
                }
            }
            load.GoBackAndWrite((buff_count * 8) + 1, buff_count);

            load.WriteByte(m_Character.m_playerKillInfo.Type);

            load.WriteByte(0);

            load.WriteInt32(m_Character.m_gameGuideData);
            load.WriteInt32(m_Character.m_accountInfo.SID);
            load.WriteInt16(0);
            load.WriteByte(m_Character.m_accountInfo.Auth.HasFlag(AuthType.GM));

            m_SocketContext.Send(load);

            Packet resp3 = new Packet(SCommon.Opcode.Agent.CHARACTER_LOAD_END);
            m_SocketContext.Send(resp3);

            Packet resp4 = new Packet(SCommon.Opcode.Agent.CHARACTER_CELESTICAL_POS);
            resp4.WriteUInt32(m_Character.m_uniqueId);
            resp4.WriteUInt16(0);//moon position
            resp4.WriteByte(DateTime.Now.Hour);
            resp4.WriteByte(DateTime.Now.Minute);
            m_SocketContext.Send(resp4);
        }

        public void Teleport(short region, short x, short y, short z, short r)
        {
            m_Character.StopMovement();
            
            //pop from GObjList and dissapper first
            m_Character.Disappear();
            m_Character.m_isIngame = false;

            m_Character.m_isBusy = false;
            

            lock (m_Character.m_lock)
            {
                m_Character.m_region = region;
                m_Character.m_position.X = x + m_Character.m_rnd.Next(-r, r) / 10f;
                m_Character.m_position.Y = y;
                m_Character.m_position.Z = z + m_Character.m_rnd.Next(-r, r) / 10f;
            }

            Globals.ShardDB.ExecuteCommand("UPDATE _Char SET LatestRegion = {0}, PosX = {1}, PosY = {2}, PosZ = {3} WHERE CharID = {4}", m_Character.m_region, m_Character.m_position.X, m_Character.m_position.Y, m_Character.m_position.Z, m_Character.m_charId);

            Packet pkt = new Packet(SCommon.Opcode.Agent.TELEPORT_SCREEN);
            pkt.WriteInt16(region);
            m_SocketContext.Send(pkt);

            ProcessIngameRequest(m_Character.m_charId);
        }

        public bool ExecuteCommand(Packet pkt)
        {
            if (!m_AccountInfo.Auth.HasFlag(AuthType.GM))
                return false;

            byte type = pkt.ReadByte();
            switch (type)
            {
                case 6: //loadmonster
                {
                    int id = pkt.ReadInt32();
                    byte count = pkt.ReadByte();
                    GObjMobRarity rarity = (GObjMobRarity)pkt.ReadByte();
                    if (rarity > GObjMobRarity.Elite)
                        rarity = GObjMobRarity.General;

                    if (rarity == GObjMobRarity.General)
                        rarity = (GObjMobRarity)Globals.Ref.ObjChar[id].Rarity;

                    GObjUtils.CreateGObjMobGroup(id, m_Character.m_region, m_Character.m_position, rarity, count);
                }
                break;
                case 7: //makeitem
                {
                    var obj = GObjUtils.AllocGObjItem(pkt.ReadInt32(), m_Character.m_region, m_Character.m_position);
                    obj.m_data = 1;

                    if (Globals.Ref.ObjItem[obj.m_model].TypeID1 == 3 && Globals.Ref.ObjItem[obj.m_model].TypeID2 == 1)
                        obj.m_optLevel = pkt.ReadByte();
                    else if (Globals.Ref.ObjItem[obj.m_model].Type == ItemType.GOLD)
                        obj.m_data = pkt.ReadInt32();
                    else
                        obj.m_data = pkt.ReadByte();
                    
                    obj.m_assignedListNode = Globals.GObjList.push(obj);
                    obj.SightCheck();
                }
                break;
                case 11: //mobkill
                {
                    var obj = GObjUtils.FindGObjByUniqueID(pkt.ReadInt32());
                    if (obj.m_type == GObjType.GObjMob)
                    {
                        byte delete = pkt.ReadByte();
                        if (delete == 1)
                            obj.Disappear(false);
                        else
                            obj.StartDisappear(1);
                    }
                }
                break;
                default:
                    Logging.Log()(String.Format("Unknown command (type: {0})", type), LogLevel.Warning);
                    break;
            }

            return true;
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
                    Globals.GlobalDB.ExecuteCommandAsync("DELETE FROM _ActiveSessions WHERE UserSID = {0}", m_AccountInfo.SID);

                    if (m_PingTimer.IsRunning)
                        m_PingTimer.Stop();

                    if (m_Character != null && m_Character.m_isIngame)
                    {
                        lock (m_Character.m_lock)
                        {
                            if (m_Character.m_movementTimer.IsRunning)
                                m_Character.m_movementTimer.Stop();
                        }
                        m_Character.Disappear(false);
                        Globals.ShardDB.ExecuteCommandAsync(String.Format("UPDATE _Char SET LatestRegion = {0}, PosX = {1}, PosY = {2}, PosZ = {3} WHERE CharID = {4}", m_Character.m_region, m_Character.m_position.X, m_Character.m_position.Y, m_Character.m_position.Z, m_Character.m_charId));
                    }

                    m_Character = null;
                    m_AccountInfo.Characters = null;
                    m_AccountInfo = default(AccountInfo);

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
                Console.WriteLine("client timeout, disconnecting (last: {0}, elapsed: {1})", LastPingTick, Environment.TickCount - LastPingTick);
                Disconnect();
            }
        }

        #endregion
    }
}
