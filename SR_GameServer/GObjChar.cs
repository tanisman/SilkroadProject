namespace SR_GameServer
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using SCommon;
    using SCommon.Security;

    using GameWorld;

    public class GObjChar : GObj
    {
        #region Private Properties and Fields

        private ClientContext m_pc;

        #endregion

        #region Public Properties and Fields

        public int m_charId;
        public string m_name;
        public Data.AccountInfo m_accountInfo;
        public int m_guildId;
        public byte m_level;
        public byte m_maxLevel;
        public byte m_hwanCount;
        public byte m_scale;
        public long m_experience;
        public long m_gold;
        public int m_skillPoint;
        public int m_skillExperience;
        public int m_gameGuideData;
        public int m_appointedTeleport;
        public bool m_isIngame;
        public bool m_leaving;
        public bool m_canMove;
        public bool m_isDoingAction;
        public bool m_begginerIcon;
        public bool m_isTeleporting;
        public GObjNPC m_openedShop;

        #region Stats

        public volatile int m_baseStrenght;
        public volatile int m_baseIntellect;
        public volatile int m_attributePoints;
        public volatile int m_baseBlockRatio;
        public volatile int m_baseCriticalRatio;
        public volatile int m_basePhyDef;
        public volatile int m_baseMagDef;
        public volatile int m_baseMinPhyAtk;
        public volatile int m_baseMaxPhyAtk;
        public volatile int m_baseMinMagAtk;
        public volatile int m_baseMaxMagAtk;
        public volatile int m_baseParryRate;
        public volatile int m_baseHitRate;
        public volatile int m_baseMaxHealth;
        public volatile int m_baseMaxMana;
        public volatile int m_bonusStrenght;
        public volatile int m_bonusIntellect;
        public volatile int m_bonusPhyDefPercent;
        public volatile int m_bonusMagDefPercent;
        public volatile int m_bonusPhyAtkPercent;
        public volatile int m_bonusMagAtkPercent;
        public volatile int m_bonusParryPercent;
        public volatile int m_bonusHitPercent;
        public volatile int m_bonusMaxHealthPercent;
        public volatile int m_bonusMaxManaPercent;
        public volatile int m_bonusBlockRatioPercent;
        public volatile int m_bonusCritialRatioPercent;
        public volatile int m_bonusPhyDef;
        public volatile int m_bonusMagDef;
        public volatile int m_bonusMinPhyAtk;
        public volatile int m_bonusMinMagAtk;
        public volatile int m_bonusMaxPhyAtk;
        public volatile int m_bonusMaxMagAtk;
        public volatile int m_bonusParryRate;
        public volatile int m_bonusHitRate;
        public volatile int m_bonusMaxHealth;
        public volatile int m_bonusMaxMana;
        public volatile int m_bonusBlockRatio;
        public volatile int m_bonusCriticalRatio;
        public volatile int m_currentHealthPoints;
        public volatile int m_currentManaPoints;
        //absorb values doesnt effect def values but absorbs incoming raw dmg by %
        public volatile int m_phyAbsorbPercent;
        public volatile int m_magAbsorbPercent;
        //balance values doesnt effect atk values but modifies outgoing raw dmg by %
        public volatile int m_phyBalancePercent;
        public volatile int m_magBalancePercent;

        public float TotalStrenght => this.m_baseStrenght + this.m_bonusStrenght;
        public float TotalIntellect => this.m_baseIntellect + this.m_bonusIntellect;
        public float TotalPhyDef => (this.m_basePhyDef + this.m_bonusPhyDef) * (1.0f + this.m_bonusPhyDefPercent / 100f);
        public float TotalMagDef => (this.m_baseMagDef + this.m_bonusMagDef) * (1.0f + this.m_bonusMagDefPercent / 100f);
        public float TotalMinPhyAtk => (this.m_baseMinPhyAtk + this.m_bonusMinPhyAtk) * (1.0f + this.m_bonusPhyAtkPercent / 100f);
        public float TotalMaxPhyAtk => (this.m_baseMaxPhyAtk + this.m_bonusMaxPhyAtk) * (1.0f + this.m_bonusPhyAtkPercent / 100f);
        public float TotalMinMagAtk => (this.m_baseMinMagAtk + this.m_bonusMinMagAtk) * (1.0f + this.m_bonusMagAtkPercent / 100f);
        public float TotalMaxMagAtk => (this.m_baseMaxMagAtk + this.m_bonusMaxMagAtk) * (1.0f + this.m_bonusMagAtkPercent / 100f);
        public float TotalParryRate => (this.m_baseParryRate + this.m_bonusParryRate) * (1.0f + this.m_bonusParryPercent / 100f);
        public float TotalHitRate => (this.m_baseHitRate + this.m_bonusHitRate) * (1.0f + this.m_bonusHitPercent / 100f);
        public float MaxHP => (this.m_baseMaxHealth + this.m_bonusMaxHealth) * (1.0f + this.m_bonusMaxHealthPercent / 100f);
        public float MaxMP => (this.m_baseMaxMana + this.m_bonusMaxMana) * (1.0f + this.m_bonusMaxManaPercent / 100f);
        public float BlockRatio => (this.m_baseBlockRatio + this.m_bonusBlockRatio) * (1.0f + this.m_bonusBlockRatioPercent / 100f);
        public float CriticalRatio => (this.m_baseCriticalRatio + this.m_bonusCriticalRatio) * (1.0f + this.m_bonusCritialRatioPercent / 100f);

        #endregion
        public _trijob m_triJob;
        public _skill_mastery[] m_masteryTree;
        public List<int> m_skills;
        public List<_item> m_equipItems;
        public _pk_info m_playerKillInfo;
        public AttackType m_attackType;

        #endregion

        #region Constructors & Destructors

        public GObjChar(ClientContext pc)
            : base (GObjType.GObjChar)
        {
            m_masteryTree = new _skill_mastery[300];
            m_skills = new List<int>();
            m_equipItems = new List<_item>(13);
            m_pc = pc;
        }

        #endregion

        #region Public Methods

        public override void SendGObj(bool type, Packet packet, GObj gobj)
        {
            m_pc.SocketContext.Send(packet);

            #region Sight List Update
            lock (gobj.m_lock)
            {
                _sightGObj obj = new _sightGObj { Object = this, Seen = false };
                int idx = gobj.m_inSightGObjList.IndexOf(obj);

                if (type)
                {
                    if (idx != -1)
                    {
                        if (this.Position.ToGameWorld(this.m_region).Distance(gobj.Position.ToGameWorld(gobj.m_region)) > 50f + (float)Data.Globals.Ref.ObjCommon[gobj.m_model].BCRadius / Formula.WORLD_SCALE //should have despawned
                            || gobj.m_assignedListNode == null) //or removed from gobjlist
                        {
                            Packet despawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_DESPAWN_SINGLE);
                            despawn.WriteInt32(gobj.m_uniqueId);
                            despawn.m_data = gobj;
                            m_pc.SocketContext.Send(despawn);

                            gobj.m_inSightGObjList.RemoveAt(idx);
                        }
                        else //ok to set seen
                        {
                            while (gobj.m_inSightGObjList[idx].PacketQueue.Count > 0)
                                m_pc.SocketContext.Send(gobj.m_inSightGObjList[idx].PacketQueue.Dequeue());
                            m_pc.SocketContext.ProcessSendQueue();

                            obj.Seen = true;
                            gobj.m_inSightGObjList[idx] = obj;
                            OnEnterVisibility(gobj);
                            gobj.OnEnterVisibility(this);
                        }
                    }
                    else
                        Logging.Log()("wat ? m_inSightGObjList.IndexOf returns -1 in 0x3015 sent notify", LogLevel.Error);
                }
                else
                {
                    if (idx != -1)
                    {
                        if (this.Position.ToGameWorld(this.m_region).Distance(gobj.Position.ToGameWorld(gobj.m_region)) <= 49f + (float)Data.Globals.Ref.ObjCommon[gobj.m_model].BCRadius / Formula.WORLD_SCALE) //should have spawned
                        {
                            if (gobj.m_assignedListNode != null)
                            {
                                Packet spawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_SINGLE);
                                spawn.WriteGObj(gobj, true);
                                spawn.m_data = gobj;
                                m_pc.SocketContext.Send(spawn);

                                while (gobj.m_inSightGObjList[idx].PacketQueue.Count > 0)
                                    m_pc.SocketContext.Send(gobj.m_inSightGObjList[idx].PacketQueue.Dequeue());
                                m_pc.SocketContext.ProcessSendQueue();

                                obj.Seen = true;
                                gobj.m_inSightGObjList[idx] = obj;
                                OnEnterVisibility(gobj);
                                gobj.OnEnterVisibility(this);
                            }
                        }
                        else //ok to remove
                        {
                            gobj.m_inSightGObjList.RemoveAt(idx);
                            OnLeaveVisibility(gobj);
                            gobj.OnLeaveVisibility(this);
                        }
                    }
                    else
                        Logging.Log()("wat ? m_inSightGObjList.IndexOf returns -1 in 0x3016 ({0}) sent notify", LogLevel.Error);
                }
            }
            #endregion
        }

        public override void SendGObj(bool type, Packet packet, List<GObj> objects)
        {
            //start
            Packet start = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_START);
            if (type)
                start.WriteByte(1); //spawn
            else
                start.WriteByte(2); //despawn
            start.WriteUInt16(objects.Count);
            m_pc.SocketContext.Send(start);
            
            //data
            if (packet.Size > 4090 + 6)
            {
                byte[] buffer = packet.GetBytes();
                int index = 0;

                while (buffer.Length - index > 4090)
                {
                    Packet data = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_DATA);
                    data.WriteUInt8Array(buffer, index, 4090);
                    m_pc.SocketContext.Send(data);
                    index += 4090;
                }

                if (index < buffer.Length)
                {
                    Packet data = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_DATA);
                    data.WriteUInt8Array(buffer, index, buffer.Length - index);
                    m_pc.SocketContext.Send(data);
                }
            }
            else
                m_pc.SocketContext.Send(packet);

            //end
            Packet end = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_END);
            end.m_data = new object[] { type, objects };
            m_pc.SocketContext.Send(end);
            
            #region Sight List Update
            for (int i = 0; i < objects.Count; i++)
            {
                lock (objects[i].m_lock)
                {
                    _sightGObj obj = new _sightGObj { Object = this, Seen = false };
                    int idx = objects[i].m_inSightGObjList.IndexOf(obj);
                    if (idx != -1)
                    {
                        if (type)
                        {
                            if (this.Position.ToGameWorld(this.m_region).Distance(objects[i].Position.ToGameWorld(objects[i].m_region)) > 50f + (float)Data.Globals.Ref.ObjCommon[objects[i].m_model].BCRadius / Formula.WORLD_SCALE //should have despawned
                                || objects[i].m_assignedListNode == null) //or removed from gobjlist
                            {
                                return;
                                Packet despawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_DESPAWN_SINGLE);
                                despawn.WriteInt32(objects[i].m_uniqueId);
                                despawn.m_data = objects[i];
                                m_pc.SocketContext.Send(despawn);

                                objects[i].m_inSightGObjList.RemoveAt(idx);
                            }
                            else //ok to set seen
                            {
                                while (objects[i].m_inSightGObjList[idx].PacketQueue.Count > 0)
                                    m_pc.SocketContext.EnqueuePacket(objects[i].m_inSightGObjList[idx].PacketQueue.Dequeue());
                                m_pc.SocketContext.ProcessSendQueue();

                                obj.Seen = true;
                                objects[i].m_inSightGObjList[idx] = obj;
                                OnEnterVisibility(objects[i]);
                                objects[i].OnEnterVisibility(this);
                            }
                        }
                        else
                        {
                            if (this.Position.ToGameWorld(this.m_region).Distance(objects[i].Position.ToGameWorld(objects[i].m_region)) <= 49f + (float)Data.Globals.Ref.ObjCommon[objects[i].m_model].BCRadius / Formula.WORLD_SCALE) //should have spawned
                            {
                                Packet spawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_SINGLE);
                                spawn.WriteGObj(objects[i], true);
                                spawn.m_data = objects[i];
                                m_pc.SocketContext.Send(spawn);

                                while (objects[i].m_inSightGObjList[idx].PacketQueue.Count > 0)
                                    m_pc.SocketContext.EnqueuePacket(objects[i].m_inSightGObjList[idx].PacketQueue.Dequeue());
                                m_pc.SocketContext.ProcessSendQueue();

                                obj.Seen = true;
                                objects[i].m_inSightGObjList[idx] = obj;
                                OnEnterVisibility(objects[i]);
                                objects[i].OnEnterVisibility(this);
                            }
                            else //ok to remove
                            {
                                if (objects[i].m_inSightGObjList[idx].PacketQueue != null)
                                    objects[i].m_inSightGObjList[idx].PacketQueue.Clear();
                                objects[i].m_inSightGObjList.RemoveAt(idx);

                                OnLeaveVisibility(objects[i]);
                                objects[i].OnLeaveVisibility(this);
                            }
                        }
                    }
                    else
                        Logging.Log()(String.Format("wat ? m_inSightGObjList.IndexOf returns -1 in 0x3019 ({0}) sent notify", type ? "spawn" : "despawn"));
                }
            }
            #endregion
        }

        public override void SendPacket(Packet packet)
        {
            m_pc.SocketContext.Send(packet);
        }

        #pragma warning disable 1998
        public async Task<bool> OperateInventory(byte[] data)
        {
            lock (m_lock)
            {
                if (m_lifeState != LifeState.Alive)
                    return true;

                StopMovement(true);

                switch (data[0])
                {
                    case 0:
                        if (data.Length != 4)
                            return false;
                        return MoveItemInInventory(data[1], data[2], data[3]);
                    case 7:
                        if (data.Length != 2)
                            return false;
                        return DropItem(data[1]);
                    case 8:
                        if (data.Length != 8)
                            return false;
                        return BuyItemFromShop(data[1], data[2], data[3], BitConverter.ToInt32(data, 4));
                    case 9:
                        if (data.Length != 7)
                            return false;
                        return SellItemToShop(data[1], data[2], BitConverter.ToInt32(data, 3));
                    case 10:
                        if (data.Length != 5)
                            return false;
                        return DropGold(BitConverter.ToInt32(data, 1));
                    default:
                        return false;
                }
            }
        }

        public void EquipItemEffect(bool equipped, _item item, byte slot)
        {
            lock (m_lock)
            {
                if (equipped)
                {
                    Interlocked.Add(ref m_bonusPhyDef, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].PD_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PDInc));
                    Interlocked.Add(ref m_bonusMagDef, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].MD_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MDInc));
                    Interlocked.Add(ref m_bonusParryRate, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].ER_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].ERInc));
                    Interlocked.Add(ref m_bonusHitRate, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].HR_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].HRInc));
                    Interlocked.Add(ref m_bonusMinPhyAtk, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].PAttackMin_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PAttackInc));
                    Interlocked.Add(ref m_bonusMaxPhyAtk, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].PAttackMax_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PAttackInc));
                    Interlocked.Add(ref m_bonusMinMagAtk, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].MAttackMin_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MAttackInc));
                    Interlocked.Add(ref m_bonusMaxMagAtk, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].MAttackMax_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MAttackInc));
                    Interlocked.Add(ref m_phyAbsorbPercent, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].PAR_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PARInc));
                    Interlocked.Add(ref m_magAbsorbPercent, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].MAR_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MARInc));
                    Interlocked.Add(ref m_bonusCriticalRatio, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].CHR_L));
                    Interlocked.Add(ref m_bonusBlockRatio, (int)(Data.Globals.Ref.ObjItem[item.RefItemID].BR_L));
                    item.Slot = slot;
                    m_equipItems.Add(item);
                }
                else
                {
                    Interlocked.Add(ref m_bonusPhyDef, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].PD_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PDInc));
                    Interlocked.Add(ref m_bonusMagDef, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].MD_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MDInc));
                    Interlocked.Add(ref m_bonusParryRate, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].ER_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].ERInc));
                    Interlocked.Add(ref m_bonusHitRate, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].HR_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].HRInc));
                    Interlocked.Add(ref m_bonusMinPhyAtk, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].PAttackMin_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PAttackInc));
                    Interlocked.Add(ref m_bonusMaxPhyAtk, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].PAttackMax_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PAttackInc));
                    Interlocked.Add(ref m_bonusMinMagAtk, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].MAttackMin_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MAttackInc));
                    Interlocked.Add(ref m_bonusMaxMagAtk, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].MAttackMax_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MAttackInc));
                    Interlocked.Add(ref m_phyAbsorbPercent, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].PAR_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].PARInc));
                    Interlocked.Add(ref m_magAbsorbPercent, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].MAR_L + item.OptLvl * Data.Globals.Ref.ObjItem[item.RefItemID].MARInc));
                    Interlocked.Add(ref m_bonusCriticalRatio, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].CHR_L));
                    Interlocked.Add(ref m_bonusBlockRatio, -(int)(Data.Globals.Ref.ObjItem[item.RefItemID].BR_L));
                    m_equipItems.RemoveAll(p => p.Slot == slot);
                }
            }
        }
        
        public void UpdateGold(long gold, out Packet pkt)
        {
            pkt = new Packet(SCommon.Opcode.Agent.UPDATE_PC);
            pkt.WriteByte(1);

            lock (m_lock)
            {
                m_gold += gold;
                Data.Globals.ShardDB.ExecuteCommand("UPDATE _Char SET RemainGold = {0} WHERE CharID = {1}", m_gold, m_charId);
                pkt.WriteInt64(m_gold);
            }

            pkt.WriteByte(0);
        }

        #endregion

        #region Private Methods

        private bool MoveItemInInventory(byte fSlot, byte tSlot, byte amount)
        {
            bool lockWasTaken = false;
            var temp = m_lock;
            Queue<Tuple<bool, Packet>> packets = null;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);

                if (m_isBusy)
                    return true;

                if (fSlot < 13 && tSlot < 13) //both from and target slots cant be equip slots
                    return false;

                if (fSlot == tSlot) //from and target slots cant be same
                    return false;

                _item fItem = new _item();
                using (var reader = Data.Globals.ShardDB.ExecuteReader("SELECT TOP 1 B.*, A.* FROM _Inventory A INNER JOIN _Items B ON B.ID64 = A.ItemID WHERE A.CharID = {0} AND A.Slot = {1} AND B.ItemSerial > 0", m_charId, fSlot))
                {
                    if (reader.Read())
                        fItem.Read(reader);
                }

                if (fItem.ID64 == 0) //empty slot
                    return false;

                _item tItem = new _item();
                using (var reader = Data.Globals.ShardDB.ExecuteReader("SELECT TOP 1 B.*, A.* FROM _Inventory A INNER JOIN _Items B ON B.ID64 = A.ItemID WHERE A.CharID = {0} AND A.Slot = {1} AND B.ItemSerial > 0", m_charId, tSlot))
                {
                    if (reader.Read())
                        tItem.Read(reader);
                }

                if (tItem.ID64 == 0)
                    tItem.Slot = tSlot;

                if (tSlot < 13) //equip item
                    return EquipItem(true, fItem, tItem, out packets);

                if (fSlot < 13) //unequip item
                    return EquipItem(false, fItem, tItem, out packets);

                if (Data.Globals.Ref.ObjItem[fItem.RefItemID].TypeID2 == 3 && (amount < 1 || amount > fItem.Data)) //amount cannot be smaller than 1
                    return false;

                if (amount < fItem.Data) //split item
                    return SplitItem(fItem, amount, out packets);

                if (tItem.ID64 != 0 && fItem.RefItemID == tItem.RefItemID && Data.Globals.Ref.ObjItem[fItem.RefItemID].TypeID2 == 3) //merge item
                {
                    if (Data.Globals.Ref.ObjItem[fItem.RefItemID].MaxStack > fItem.Data && Data.Globals.Ref.ObjItem[tItem.RefItemID].MaxStack > tItem.Data) //can merge
                        return MergeItem(fItem, tItem, out packets);
                }

                return SwapItemSlots(fItem, tItem, out packets);
            }
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(temp);

                if (packets != null)
                {
                    while (packets.Count > 0)
                    {
                        var it = packets.Dequeue();
                        if (it.Item1)
                            BroadcastToSightList(it.Item2);
                        SendPacket(it.Item2);
                    }
                }

                packets = null;
            }
        }

        private bool DropItem(byte fSlot)
        {
            _item fItem;
            lock (m_lock)
            {
                if (m_isBusy)
                    return false;

                fItem = new _item();
                using (var reader = Data.Globals.ShardDB.ExecuteReader("SELECT TOP 1 B.*, A.* FROM _Inventory A INNER JOIN _Items B ON B.ID64 = A.ItemID WHERE A.CharID = {0} AND A.Slot = {1} AND B.ItemSerial > 0", m_charId, fSlot))
                {
                    if (reader.Read())
                        fItem.Read(reader);
                }

                if (fItem.ID64 == 0) //empty slot
                    return false;

                var result = Data.Globals.ShardDB.Result<int>("EXEC _DEL_ITEM {0}, {1}, {2}", m_charId, fItem.ID64, fItem.Slot);
                if (result != 1)
                    return false;
            }

            var obj = GObjUtils.AllocGObjItem(fItem.RefItemID, m_region, m_position);
            obj.m_owner = this;
            obj.m_assignedListNode = Data.Globals.GObjList.push(obj);
            obj.SightCheck();

            Packet pkt = new Packet(SCommon.Opcode.Agent.DROP_ITEM);
            pkt.WriteInt32(obj.m_uniqueId);
            SendPacket(pkt);

            pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
            pkt.WriteByte(1);
            pkt.WriteByte(7);
            pkt.WriteByte(fSlot);
            SendPacket(pkt);

            return true;
        }

        private bool BuyItemFromShop(byte tab, byte slot, byte amount, int uid)
        {
            Packet pkt;
            byte tSlot = 255;
            lock (m_lock)
            {
                if (tab >= 10)
                    return false;

                if (!m_isBusy)
                    return false;

                if (m_selectedGObj == null || m_selectedGObj.m_uniqueId != uid)
                    return false;

                var shop = Data.Globals.Ref.Shop.FirstOrDefault(p => p.NpcID == m_selectedGObj.m_model);
                if (shop == null)
                    return false;

                if (shop.Tabs[tab].ID == 0)
                    return false;

                if (shop.Tabs[tab].Items.Count < slot)
                    return false;

                if (shop.Tabs[tab].Items[slot].RefItemID == 0)
                    return false;

                if (Data.Globals.Ref.ObjItem[shop.Tabs[tab].Items[slot].RefItemID].MaxStack < amount)
                    return false;

                if (amount < 1)
                    amount = 1;

                long gold = shop.Tabs[tab].Items[slot].GoldPrice * amount;

                if (m_gold < gold)
                    return true; //not enough gold

                if (Data.Globals.Ref.ObjItem[shop.Tabs[tab].Items[slot].RefItemID].Type == Data.ItemType.ETC_TRADE_ITEM)
                {
                    //check pet
                    //get free slot
                }
                else
                    tSlot = GetFreeSlot();

                if (tSlot == 255)
                    return true; //inventory full

                int data = 0;
                if (Data.Globals.Ref.ObjItem[shop.Tabs[tab].Items[slot].RefItemID].TypeID2 == 3)
                    data = amount;
                else
                    data = (int)Data.Globals.Ref.ObjItem[shop.Tabs[tab].Items[slot].RefItemID].Dur_L;

                var result = Data.Globals.ShardDB.Result<int>("exec _ADD_ITEM {0}, {1}, {2}, {3}, {4}", m_charId, tSlot, shop.Tabs[tab].Items[slot].RefItemID, shop.Tabs[tab].Items[slot].OptLevel, data);
                if (result <= 0)
                    return false;

                UpdateGold(-gold, out pkt);
            }

            SendPacket(pkt); //send gold update

            pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
            pkt.WriteByte(1);
            pkt.WriteByte(8);
            pkt.WriteByte(tab);
            pkt.WriteByte(slot);
            pkt.WriteByte(tSlot);
            pkt.WriteByte(amount);

            SendPacket(pkt);

            return true;
        }

        private bool SellItemToShop(byte fSlot, byte amount, int uid)
        {
            Packet pkt;
            lock (m_lock)
            {
                if (!m_isBusy)
                    return false;

                if (m_selectedGObj == null || m_selectedGObj.m_uniqueId != uid)
                    return false;

                var shop = Data.Globals.Ref.Shop.FirstOrDefault(p => p.NpcID == m_selectedGObj.m_model);
                if (shop == null)
                    return false;

                _item fItem = new _item();
                using (var reader = Data.Globals.ShardDB.ExecuteReader("SELECT TOP 1 B.*, A.* FROM _Inventory A INNER JOIN _Items B ON B.ID64 = A.ItemID WHERE A.CharID = {0} AND A.Slot = {1} AND B.ItemSerial > 0", m_charId, fSlot))
                {
                    if (reader.Read())
                        fItem.Read(reader);
                }

                if (fItem.ID64 == 0) //empty slot
                    return false;

                if (Data.Globals.Ref.ObjItem[fItem.RefItemID].MaxStack < amount)
                    return false;

                if (amount < 1)
                    amount = 1;

                long gold = Data.Globals.Ref.ObjItem[fItem.RefItemID].SellPrice * amount;

                var result = Data.Globals.ShardDB.Result<int>("EXEC _UPDATE_ITEM {0}, {1}, {2}, {3}", m_charId, fItem.ID64, fItem.Slot, amount);
                if (result != 1)
                    return false;

                UpdateGold(gold, out pkt);
            }

            SendPacket(pkt);

            pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
            pkt.WriteByte(1);
            pkt.WriteByte(9);
            pkt.WriteByte(fSlot);
            pkt.WriteByte(amount);
            pkt.WriteInt32(uid);

            SendPacket(pkt);

            return true;
        }

        private bool DropGold(int amount)
        {
            Packet pkt;
            int model = -1;
            lock (m_lock)
            {
                if (m_isBusy)
                    return false;

                if (amount < 1)
                    return false;

                if (amount < 10000)
                    model = Data.Globals.Ref.ObjItemByCodeName["ITEM_ETC_GOLD_01"].ID;
                else if (amount < 100000)
                    model = Data.Globals.Ref.ObjItemByCodeName["ITEM_ETC_GOLD_02"].ID;
                else
                    model = Data.Globals.Ref.ObjItemByCodeName["ITEM_ETC_GOLD_03"].ID;

                UpdateGold(-amount, out pkt);
            }

            var obj = GObjUtils.AllocGObjItem(model, m_region, m_position);
            obj.m_owner = this;
            obj.m_data = amount;
            obj.m_assignedListNode = Data.Globals.GObjList.push(obj);
            obj.SightCheck();

            SendPacket(pkt);

            pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
            pkt.WriteByte(1);
            pkt.WriteByte(10);
            pkt.WriteInt32(amount);
            SendPacket(pkt);

            return true;
        }

        private bool SwapItemSlots(_item fItem, _item tItem, out Queue<Tuple<bool, Packet>> packets)
        {
            lock (m_lock)
            {
                var result = Data.Globals.ShardDB.Result<int>("exec _ITEM_MOVE 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_accountInfo.SID, m_charId, fItem.ID64, tItem.ID64, fItem.Slot, tItem.Slot, fItem.Data);

                if (result == 1)
                {
                    Queue<Tuple<bool, Packet>> pktQueue = new Queue<Tuple<bool, Packet>>();

                    Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                    pkt.WriteByte(1);
                    pkt.WriteByte(0);
                    pkt.WriteByte(fItem.Slot);
                    pkt.WriteByte(tItem.Slot);
                    pkt.WriteByte(fItem.Data);
                    pkt.WriteByte(0);
                    pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                    packets = pktQueue;
                    return true;
                }
                packets = null;
                return false;
            }
        }

        private bool EquipItem(bool equipped, _item fItem, _item tItem, out Queue<Tuple<bool, Packet>> packets)
        {
            lock (m_lock)
            {
                packets = null;
                var pktQueue = new Queue<Tuple<bool, Packet>>();

                if (equipped)
                {
                    if (Data.Globals.Ref.ObjItem[fItem.RefItemID].Slot == 11 && tItem.Slot != 11 && tItem.Slot != 12)
                        return false; //disconnect

                    if (Data.Globals.Ref.ObjItem[fItem.RefItemID].Slot != tItem.Slot)
                        return false; //disconnect

                    if (Data.Globals.Ref.ObjItem[fItem.RefItemID].ReqLevel1 > m_level)
                        return false; //level is too low packet

                    if (Data.Globals.Ref.ObjItem[fItem.RefItemID].ReqGender != 2 && Data.Globals.Ref.ObjItem[fItem.RefItemID].ReqGender != Data.Globals.Ref.ObjChar[m_model].CharGender)
                        return true; //different sex packet

                    if (tItem.Slot < 6)
                    {
                        var item = m_equipItems.FirstOrDefault(p => p.Slot < 6);
                        if (item != null)
                        {
                            if (Data.Globals.Ref.ObjItem[item.RefItemID].Type == Data.ItemType.CH_ARMOR_CLOTHES && Data.Globals.Ref.ObjItem[fItem.RefItemID].Type != Data.ItemType.CH_ARMOR_CLOTHES)
                                return true; //cannot equip packet
                            else if (Data.Globals.Ref.ObjItem[item.RefItemID].Type != Data.ItemType.CH_ARMOR_CLOTHES && Data.Globals.Ref.ObjItem[fItem.RefItemID].Type == Data.ItemType.CH_ARMOR_CLOTHES)
                                return true; //cannot equip packet
                        }
                    }
                    else if (tItem.Slot == 6)
                    {
                        _item shield = m_equipItems.FirstOrDefault(p => p.Slot == 7);
                        if (shield != null)
                        {
                            if (
                                    (Data.Globals.Ref.ObjItem[shield.RefItemID].Type == Data.ItemType.ARTICLES_ARROW && Data.Globals.Ref.ObjItem[fItem.RefItemID].Type != Data.ItemType.CH_WEAPON_BOW) //if slot 7 is arrow and equipping item is not arrow
                                || (Data.Globals.Ref.ObjItem[shield.RefItemID].Type == Data.ItemType.CH_WEAPON_SHIELD && Data.Globals.Ref.ObjItem[fItem.RefItemID].TwoHanded == 1) //or slot 7 is shield and equipping item is twohanded
                                )
                            {
                                //unequip shield/arrows
                                byte slot = GetFreeSlot();
                                if (slot == 255)
                                    return true; //inventory full packet

                                var result = Data.Globals.ShardDB.Result<int>("exec _ITEM_MOVE 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_accountInfo.SID, m_charId, shield.ID64, 0, 7, slot, shield.Data);

                                if (result != 1)
                                    return false; //disconnect

                                Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                                pkt.WriteByte(1);
                                pkt.WriteByte(0);
                                pkt.WriteByte(7);
                                pkt.WriteByte(slot);
                                pkt.WriteByte(shield.Data);
                                pkt.WriteByte(0);
                                pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                                pkt = new Packet(SCommon.Opcode.Agent.UNEQUIP_ITEM);
                                pkt.WriteInt32(m_uniqueId);
                                pkt.WriteByte(7);
                                pkt.WriteByte(0);
                                pkt.WriteByte(0);
                                pktQueue.Enqueue(new Tuple<bool, Packet>(true, pkt));

                                EquipItemEffect(false, shield, 7);
                            }
                        }
                    }
                    else if (tItem.Slot == 7)
                    {
                        _item weapon = m_equipItems.FirstOrDefault(p => p.Slot == 6);
                        if (weapon != null)
                        {
                            if (Data.Globals.Ref.ObjItem[fItem.RefItemID].Type == Data.ItemType.ARTICLES_ARROW && Data.Globals.Ref.ObjItem[weapon.RefItemID].Type != Data.ItemType.CH_WEAPON_BOW)
                                return true; //wrong operation packet

                            if (Data.Globals.Ref.ObjItem[fItem.RefItemID].Type == Data.ItemType.CH_WEAPON_SHIELD && Data.Globals.Ref.ObjItem[weapon.RefItemID].TwoHanded == 1)
                            {
                                //unequip weapon
                                byte slot = GetFreeSlot();
                                if (slot == 255)
                                    return true; //inventory full packet

                                var result = Data.Globals.ShardDB.Result<int>("exec _ITEM_MOVE 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_accountInfo.SID, m_charId, weapon.ID64, 0, 6, slot, weapon.Data);
                                if (result != 1)
                                    return false; //disconnect

                                Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                                pkt.WriteByte(1);
                                pkt.WriteByte(0);
                                pkt.WriteByte(6);
                                pkt.WriteByte(slot);
                                pkt.WriteByte(weapon.Data);
                                pkt.WriteByte(0);
                                pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                                pkt = new Packet(SCommon.Opcode.Agent.UNEQUIP_ITEM);
                                pkt.WriteInt32(m_uniqueId);
                                pkt.WriteByte(6);
                                pkt.WriteByte(0);
                                pkt.WriteByte(0);
                                pktQueue.Enqueue(new Tuple<bool, Packet>(true, pkt));

                                EquipItemEffect(false, weapon, 6);
                            }
                        }
                        else
                        {
                            if (Data.Globals.Ref.ObjItem[fItem.RefItemID].Type == Data.ItemType.ARTICLES_ARROW)
                                return true; //wrong operation packet
                        }
                    }

                    {
                        var result = Data.Globals.ShardDB.Result<int>("exec _ITEM_MOVE 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_accountInfo.SID, m_charId, fItem.ID64, tItem.ID64, fItem.Slot, tItem.Slot, fItem.Data);

                        if (result != 1)
                            return false; //disconnect

                        Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                        pkt.WriteByte(1);
                        pkt.WriteByte(0);
                        pkt.WriteByte(fItem.Slot);
                        pkt.WriteByte(tItem.Slot);
                        pkt.WriteByte(fItem.Data);
                        pkt.WriteByte(0);
                        pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                        pkt = new Packet(SCommon.Opcode.Agent.EQUIP_ITEM);
                        pkt.WriteInt32(m_uniqueId);
                        pkt.WriteByte(tItem.Slot);
                        pkt.WriteInt32(fItem.RefItemID);
                        pktQueue.Enqueue(new Tuple<bool, Packet>(true, pkt));

                        if (tItem.ID64 != 0)
                            EquipItemEffect(false, tItem, tItem.Slot);

                        EquipItemEffect(true, fItem, tItem.Slot);

                        pkt = new Packet(SCommon.Opcode.Agent.CHARACTER_STAT);
                        pkt.WriteInt32(this.TotalMinPhyAtk);
                        pkt.WriteInt32(this.TotalMaxPhyAtk);
                        pkt.WriteInt32(this.TotalMinMagAtk);
                        pkt.WriteInt32(this.TotalMaxMagAtk);
                        pkt.WriteUInt16(this.TotalPhyDef);
                        pkt.WriteUInt16(this.TotalMagDef);
                        pkt.WriteUInt16(this.TotalHitRate);
                        pkt.WriteUInt16(this.TotalParryRate);
                        pkt.WriteInt32(this.MaxHP);
                        pkt.WriteInt32(this.MaxMP);
                        pkt.WriteInt16(this.TotalStrenght);
                        pkt.WriteInt16(this.TotalIntellect);
                        pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                        packets = pktQueue;
                        return true;
                    }
                }
                else
                {
                    if (tItem.ID64 != 0)
                        tItem.Slot = GetFreeSlot();

                    if (tItem.Slot == 255)
                        return true; //inventory full packet

                    if (fItem.Slot == 6)
                    {
                        _item shield = m_equipItems.FirstOrDefault(p => p.Slot == 7);
                        if (shield != null && Data.Globals.Ref.ObjItem[shield.RefItemID].Type == Data.ItemType.ARTICLES_ARROW)
                        {
                            //unequip shield/arrows
                            byte slot = GetFreeSlot(tItem.Slot);
                            if (slot == 255)
                                return true; //inventory full packet

                            var result = Data.Globals.ShardDB.Result<int>("exec _ITEM_MOVE 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_accountInfo.SID, m_charId, shield.ID64, 0, 7, slot, shield.Data);

                            if (result != 1)
                                return false; //disconnect

                            Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                            pkt.WriteByte(1);
                            pkt.WriteByte(0);
                            pkt.WriteByte(7);
                            pkt.WriteByte(slot);
                            pkt.WriteByte(shield.Data);
                            pkt.WriteByte(0);
                            pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                            pkt = new Packet(SCommon.Opcode.Agent.UNEQUIP_ITEM);
                            pkt.WriteInt32(m_uniqueId);
                            pkt.WriteByte(7);
                            pkt.WriteByte(0);
                            pkt.WriteByte(0);
                            pktQueue.Enqueue(new Tuple<bool, Packet>(true, pkt));

                            EquipItemEffect(false, shield, 7);
                        }
                    }

                    {
                        var result = Data.Globals.ShardDB.Result<int>("exec _ITEM_MOVE 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_accountInfo.SID, m_charId, fItem.ID64, 0, fItem.Slot, tItem.Slot, fItem.Data);

                        if (result != 1)
                            return false; //disconnect

                        Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                        pkt.WriteByte(1);
                        pkt.WriteByte(0);
                        pkt.WriteByte(fItem.Slot);
                        pkt.WriteByte(tItem.Slot);
                        pkt.WriteByte(fItem.Data);
                        pkt.WriteByte(0);
                        pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                        pkt = new Packet(SCommon.Opcode.Agent.UNEQUIP_ITEM);
                        pkt.WriteInt32(m_uniqueId);
                        pkt.WriteByte(fItem.Slot);
                        pkt.WriteByte(0);
                        pkt.WriteByte(0);
                        pktQueue.Enqueue(new Tuple<bool, Packet>(true, pkt));

                        EquipItemEffect(false, fItem, fItem.Slot);

                        pkt = new Packet(SCommon.Opcode.Agent.CHARACTER_STAT);
                        pkt.WriteInt32(this.TotalMinPhyAtk);
                        pkt.WriteInt32(this.TotalMaxPhyAtk);
                        pkt.WriteInt32(this.TotalMinMagAtk);
                        pkt.WriteInt32(this.TotalMaxMagAtk);
                        pkt.WriteUInt16(this.TotalPhyDef);
                        pkt.WriteUInt16(this.TotalMagDef);
                        pkt.WriteUInt16(this.TotalHitRate);
                        pkt.WriteUInt16(this.TotalParryRate);
                        pkt.WriteInt32(this.MaxHP);
                        pkt.WriteInt32(this.MaxMP);
                        pkt.WriteInt16(this.TotalStrenght);
                        pkt.WriteInt16(this.TotalIntellect);
                        pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                        packets = pktQueue;
                        return true;
                    }
                }
            }
        }

        private bool SplitItem(_item fItem, byte amount, out Queue<Tuple<bool, Packet>> packets)
        {
            lock (m_lock)
            {
                packets = null;
                var pktQueue = new Queue<Tuple<bool, Packet>>();
                byte slot = GetFreeSlot();
                if (slot == 255)
                    return true; //inventory full packet

                var result = Data.Globals.ShardDB.Result<int>("exec _SPLIT_ITEM 0, {0}, {1}, {2}, {3}, {4}, {5}", m_charId, fItem.ID64, slot, fItem.Data - amount, amount, fItem.RefItemID);
                if (result <= 0)
                    return false; //disconnect

                Packet pkt = new Packet(SCommon.Opcode.Agent.Response.INVENTORY_OPERATION);
                pkt.WriteByte(1);
                pkt.WriteByte(0);
                pkt.WriteByte(fItem.Slot);
                pkt.WriteByte(slot);
                pkt.WriteByte(amount);
                pkt.WriteByte(0);
                pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                packets = pktQueue;
                return true;
            }
        }

        private bool MergeItem(_item fItem, _item tItem, out Queue<Tuple<bool, Packet>> packets)
        {
            lock (m_lock)
            {
                packets = null;
                var pktQueue = new Queue<Tuple<bool, Packet>>();
                if (fItem.Data > Data.Globals.Ref.ObjItem[fItem.RefItemID].MaxStack - tItem.Data)
                {
                    int left = fItem.Data - (Data.Globals.Ref.ObjItem[fItem.RefItemID].MaxStack - tItem.Data);
                    var result = Data.Globals.ShardDB.Result<int>("exec _MERGE_ITEM 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", m_accountInfo.SID, m_charId, fItem.Slot, tItem.Slot, fItem.ID64, tItem.ID64, left, Data.Globals.Ref.ObjItem[fItem.RefItemID].MaxStack);
                    if (result != 1)
                        return false; //disconnect

                    Packet pkt = new Packet(SCommon.Opcode.Agent.Response.UPDATE_INVENTORY_SLOT);
                    pkt.WriteByte(1);
                    pkt.WriteByte(fItem.Slot);
                    pkt.WriteByte(left);
                    pkt.WriteUInt16(0);
                    pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                    pkt = new Packet(SCommon.Opcode.Agent.Response.UPDATE_INVENTORY_SLOT);
                    pkt.WriteByte(1);
                    pkt.WriteByte(tItem.Slot);
                    pkt.WriteByte(Data.Globals.Ref.ObjItem[fItem.RefItemID].MaxStack);
                    pkt.WriteUInt16(0);
                    pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));
                }
                else
                {
                    var result = Data.Globals.ShardDB.Result<int>("exec _MERGE_ITEM 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", m_accountInfo.SID, m_charId, fItem.Slot, tItem.Slot, fItem.ID64, tItem.ID64, 0, tItem.Data + fItem.Data);
                    if (result != 1)
                        return false; //disconnect

                    Packet pkt = new Packet(SCommon.Opcode.Agent.Response.UPDATE_INVENTORY_SLOT);
                    pkt.WriteByte(1);
                    pkt.WriteByte(fItem.Slot);
                    pkt.WriteByte(0);
                    pkt.WriteUInt16(0);
                    pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));

                    pkt = new Packet(SCommon.Opcode.Agent.Response.UPDATE_INVENTORY_SLOT);
                    pkt.WriteByte(1);
                    pkt.WriteByte(tItem.Slot);
                    pkt.WriteByte(tItem.Data + fItem.Data);
                    pkt.WriteUInt16(0);
                    pktQueue.Enqueue(new Tuple<bool, Packet>(false, pkt));
                }
                packets = pktQueue;
                return true;
            }
        }
        
        private byte GetFreeSlot()
        {
            return Data.Globals.ShardDB.Result<byte>("SELECT ISNULL((SELECT TOP 1 Slot FROM _Inventory WHERE CharID = {0} AND ItemID = 0 AND Slot >= 13 AND Slot < {1} ORDER BY Slot), 255)", m_charId, 45);
        }

        private byte GetFreeSlot(byte except)
        {
            return Data.Globals.ShardDB.Result<byte>("SELECT ISNULL((SELECT TOP 1 Slot FROM _Inventory WHERE CharID = {0} AND ItemID = 0 AND Slot >= 13 AND Slot < {1} AND Slot != {2} ORDER BY Slot), 255)", m_charId, 45, except);
        }

        #endregion

    }
}
