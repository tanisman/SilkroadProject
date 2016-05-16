namespace SR_GameServer.GameWorld
{
    using System;

    using SCommon;
    using SCommon.Security;

    using SharpDX;

    public static class GObjUtils
    {
        private static Random s_rnd;

        static GObjUtils()
        {
            s_rnd = new Random();
        }

        public static void WriteGObj(this Packet packet, GObj gobj, bool single = false)
        {
            packet.WriteUInt32(gobj.m_model);

            if (gobj.m_model == -1) //event zones
            {
                packet.WriteUInt16(0);

                packet.WriteInt32(gobj.m_refSkillId);

                packet.WriteInt32(gobj.m_uniqueId);

                packet.WriteUInt16(gobj.m_region);
                packet.WriteSingle(gobj.Position.X);
                packet.WriteSingle(gobj.Position.Y);
                packet.WriteSingle(gobj.Position.Z);
                packet.WriteUInt16(gobj.m_angle);
            }
            else
            {
                if (Data.Globals.Ref.ObjCommon[gobj.m_model].TypeID1 == 1) //bionics
                {
                    if (gobj.IsCharacter)
                    {
                        GObjChar character = (GObjChar)gobj;
                        packet.WriteAscii(character.m_name);
                        packet.WriteByte(character.m_scale);
                        packet.WriteByte(character.m_equipItems.Count);
                        for (int i = 0; i < character.m_equipItems.Count; i++)
                        {
                            packet.WriteByte(character.m_equipItems[i].Slot);
                            packet.WriteInt32(character.m_equipItems[i].RefItemID);
                        }
                    }

                    packet.WriteInt32(gobj.m_uniqueId);

                    packet.WriteUInt16(gobj.m_region);
                    packet.WriteSingle(gobj.Position.X);
                    packet.WriteSingle(gobj.Position.Y);
                    packet.WriteSingle(gobj.Position.Z);
                    packet.WriteUInt16(gobj.m_angle);
                    packet.WriteByte(gobj.m_hasDestination);
                    packet.WriteByte(gobj.m_walkState);
                    if (gobj.m_hasDestination)
                    {
                        packet.WriteUInt16(gobj.m_destinationRegion);
                        packet.WriteUInt16(gobj.m_destination.X);
                        packet.WriteUInt16(gobj.m_destination.Y);
                        packet.WriteUInt16(gobj.m_destination.Z);
                    }
                    else
                    {
                        packet.WriteByte(gobj.m_hasAngleMovement);
                        packet.WriteUInt16(gobj.m_angle);
                    }

                    packet.WriteByte(gobj.m_lifeState);
                    packet.WriteByte(gobj.m_movementType);
                    packet.WriteByte(gobj.m_status);
                    packet.WriteSingle(gobj.m_baseWalkSpeed);
                    packet.WriteSingle(gobj.m_baseRunSpeed);
                    packet.WriteSingle(gobj.m_baseHwanSpeed);
                    packet.WriteByte(0); //reserved for buff count
                    byte buff_count = 0;
                    for (byte b = 0; b < 10; b++)
                    {
                        if (!gobj.m_buffs[b].IsFree)
                        {
                            packet.WriteInt32(gobj.m_buffs[b].SkillID);
                            packet.WriteInt32(gobj.m_buffs[b].CastingID);
                            buff_count++;
                        }
                    }
                    packet.GoBackAndWrite((buff_count * 8) + 1, buff_count);

                    if (gobj.IsCharacter)
                    {
                        GObjChar character = (GObjChar)gobj;
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0); // Stall Flag = 4
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        if (single)
                            packet.WriteByte(4);
                    }
                    else if (gobj.IsMonster)
                    {
                        GObjMob mob = (GObjMob)gobj;
                        packet.WriteByte(0); //nametype
                        packet.WriteByte(mob.m_rarity);
                        if (single)
                            packet.WriteByte(2);
                    }
                    else if (gobj.IsNPC)
                    {
                        GObjNPC npc = (GObjNPC)gobj;
                        packet.WriteByte(npc.m_talkFlag);
                        packet.WriteByte(0x0B);
                        packet.WriteByte(0x00);
                        packet.WriteByte(0x00);
                        packet.WriteByte(0x80);

                        if (single)
                            packet.WriteByte(2);
                    }
                }
                else if (Data.Globals.Ref.ObjCommon[gobj.m_model].TypeID1 == 3) //non bionics
                {
                    if (gobj.IsItem)
                    {
                        GObjItem item = (GObjItem)gobj;
                        if (item.IsGold)
                            packet.WriteInt32(item.m_data);

                        if (item.IsQuest || item.IsGoods)
                            packet.WriteAscii(item.m_owner.m_name);

                        packet.WriteInt32(gobj.m_uniqueId);

                        packet.WriteUInt16(gobj.m_region);
                        packet.WriteSingle(gobj.Position.X);
                        packet.WriteSingle(gobj.Position.Y);
                        packet.WriteSingle(gobj.Position.Z);
                        packet.WriteUInt16(gobj.m_angle);
                        packet.WriteByte(0);
                        packet.WriteByte(0);
                        packet.WriteByte(5);
                        packet.WriteUInt32(item.m_owner == null ? -1 : item.m_owner.m_accountInfo.SID);
                    }
                }
                else if (Data.Globals.Ref.ObjCommon[gobj.m_model].TypeID1 == 4) //portals
                {
                    packet.WriteInt32(gobj.m_uniqueId);

                    packet.WriteUInt16(gobj.m_region);
                    packet.WriteSingle(gobj.Position.X);
                    packet.WriteSingle(gobj.Position.Y);
                    packet.WriteSingle(gobj.Position.Z);
                    packet.WriteUInt16(gobj.m_angle);
                    if (single)
                        packet.WriteByte(2);
                }
            }
        }

        public static GObjMob AllocGObjMob(int refObjId, short region, Vector3 pos, GObjMobRarity rarity)
        {
            GObjMob obj = new GObjMob()
            {
                m_walkState = WalkState.Walking,
                m_movementType = MovementType.NotMoving,
                m_lifeState = LifeState.Alive,
                m_status = StatusType.None,
                m_model = refObjId,
                m_refSkillId = -1,
                m_uniqueId = Services.UniqueID.GenerateGObjID(),
                m_baseWalkSpeed = Data.Globals.Ref.ObjChar[refObjId].Speed1,
                m_baseRunSpeed = Data.Globals.Ref.ObjChar[refObjId].Speed2,
                m_baseHwanSpeed = 100,
                m_region = region,
                m_position = pos,
                m_generatedRegion = -1,
                m_nestId = -1,
                m_rarity = rarity,
                m_attackType = AttackType.None,
            };
            obj.m_currentHealthPoints = (int)obj.MaxHP;

            return obj;
        }

        public static GObjItem AllocGObjItem(int refObjId, short region, Vector3 pos)
        {
            GObjItem obj = new GObjItem()
            {
                m_walkState = WalkState.Walking,
                m_movementType = MovementType.NotMoveable,
                m_lifeState = LifeState.Alive,
                m_status = StatusType.None,
                m_model = refObjId,
                m_refSkillId = -1,
                m_uniqueId = Services.UniqueID.GenerateGObjID(),
                m_baseWalkSpeed = 0,
                m_baseRunSpeed = 0,
                m_baseHwanSpeed = 0,
                m_region = region,
                m_position = pos,
            };
            obj.m_position = (pos.ToVector2() + new Vector2((float)Math.Cos(s_rnd.Next(0, 360) * (Math.PI / 180)), (float)Math.Sin(s_rnd.Next(0, 360) * (Math.PI / 180))) * s_rnd.Next(10, 20)).ToVector3(obj.m_position.Y);

            return obj;
        }

        public static void CreateGObjMobGroup(int refObjId, short region, Vector3 pos, GObjMobRarity rarity, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                GObjMob obj = AllocGObjMob(refObjId, region, pos, rarity);
                obj.m_angle = unchecked((short)((i % 36) * 1820));
                obj.m_assignedListNode = Data.Globals.GObjList.push(obj);
                obj.SightCheck();
            }
        }

        public static GObj FindGObjByUniqueID(int uid)
        {
            var node = Data.Globals.GObjList.Head.next_;
            while (node != null)
            {
                lock (node.locker_)
                {
                    if (node.value_ != null)
                    {
                        if (node.value_.m_uniqueId == uid)
                            return node.value_;
                    }
                    node = node.next_;
                }
            }
            return null;
        }

        public static GObjChar FindGObjCharByName(string name)
        {
            var node = Data.Globals.GObjList.Head.next_;
            while (node != null)
            {
                lock (node.locker_)
                {
                    if (node.value_ != null && node.value_.IsCharacter)
                    {
                        var pc = (GObjChar)node.value_;
                        if (pc.m_name == name)
                            return pc;
                    }
                    node = node.next_;
                }
            }
            return null;
        }
        
        public static void BroadcastPacket(Packet pkt)
        {
            var node = Data.Globals.GObjList.Head.next_;
            while (node != null)
            {
                lock (node.locker_)
                {
                    if (node.value_ != null && node.value_.IsCharacter)
                    {
                        var pc = (GObjChar)node.value_;
                        pc.SendPacket(pkt);
                    }
                    node = node.next_;
                }
            }
        }
    }
}
