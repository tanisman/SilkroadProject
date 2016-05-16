namespace SR_GameServer
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using SCommon.Security;

    public struct _buff_item
    {
        public int SkillID;
        public int CastingID;
        public byte Slot;
        public object Data;
        public bool IsFree;

        public _buff_item(int sid, int cid, byte slt)
        {
            SkillID = sid;
            CastingID = cid;
            Slot = slt;
            Data = null;
            IsFree = false;
        }
    }

    public struct _trijob
    {
        public int TraderExp;
        public int HunterExp;
        public int ThiefExp;
        public byte TraderLvl;
        public byte HunterLvl;
        public byte ThiefLvl;
        public short ArrangeLevel;
        public int ArrangePoint1;
        public int ArrangePoint2;
    }

    public struct _skill_mastery
    {
        public int ID;
        public byte Level;
    }

    public class _item
    {
        public int CharID;
        public int RefItemID;
        public byte Slot;
        public byte OptLvl;
        public long ID64;
        public long Variance;
        public int Data;
        public string CreaterName;
        public byte MagParamCount;
        public int[] MagParams;
        public long ItemSerial;

        public void Read(SqlDataReader reader)
        {
            this.ID64 = reader.GetFieldValue<long>((int)_Items.ID64);
            this.RefItemID = reader.GetFieldValue<int>((int)_Items.RefItemID);
            this.OptLvl = reader.GetFieldValue<byte>((int)_Items.OptLevel);
            this.Variance = reader.GetFieldValue<long>((int)_Items.Variance);
            this.Data = reader.GetFieldValue<int>((int)_Items.Data);
            this.CreaterName = reader.GetFieldValue<string>((int)_Items.CreaterName);
            this.MagParamCount = reader.GetFieldValue<byte>((int)_Items.MagParamCount);
            if (this.MagParamCount > 0)
            {
                this.MagParams = new int[this.MagParamCount];
                for (int i = 0; i < this.MagParamCount; i++)
                    this.MagParams[i] = reader.GetFieldValue<int>((int)_Items.MagParam1 + i);
            }
            this.ItemSerial = reader.GetFieldValue<long>((int)_Items.ItemSerial);
            this.CharID = reader.GetFieldValue<int>((int)_Items.FieldCount + (int)_Inventory.CharID);
            this.Slot = reader.GetFieldValue<byte>((int)_Items.FieldCount + (int)_Inventory.Slot);
        }

        #pragma warning disable 1998
        public async Task ReadAsync(SqlDataReader reader)
        {
            this.ID64 = await reader.GetFieldValueAsync<long>((int)_Items.ID64);
            this.RefItemID = await reader.GetFieldValueAsync<int>((int)_Items.RefItemID);
            this.OptLvl = await reader.GetFieldValueAsync<byte>((int)_Items.OptLevel);
            this.Variance = await reader.GetFieldValueAsync<long>((int)_Items.Variance);
            this.Data = await reader.GetFieldValueAsync<int>((int)_Items.Data);
            this.CreaterName = await reader.GetFieldValueAsync<string>((int)_Items.CreaterName);
            this.MagParamCount = await reader.GetFieldValueAsync<byte>((int)_Items.MagParamCount);
            if (this.MagParamCount > 0)
            {
                this.MagParams = new int[this.MagParamCount];
                for (int i = 0; i < this.MagParamCount; i++)
                    this.MagParams[i] = await reader.GetFieldValueAsync<int>((int)_Items.MagParam1 + i);
            }
            this.ItemSerial = await reader.GetFieldValueAsync<long>((int)_Items.ItemSerial);
            this.CharID = await reader.GetFieldValueAsync<int>((int)_Items.FieldCount + (int)_Inventory.CharID);
            this.Slot = await reader.GetFieldValueAsync<byte>((int)_Items.FieldCount + (int)_Inventory.Slot);
        }

        public int Write(Packet p)
        {
            p.WriteByte(Slot);
            p.WriteInt32(RefItemID);
            if (SR_GameServer.Data.Globals.Ref.ObjItem[RefItemID].TypeID1 == 3 && SR_GameServer.Data.Globals.Ref.ObjItem[RefItemID].TypeID2 == 1)
            {
                p.WriteByte(OptLvl);
                p.WriteInt32(0);
                p.WriteInt32(0);
                p.WriteInt16(Data);
                return 16;
            }
            else if (SR_GameServer.Data.Globals.Ref.ObjItem[RefItemID].TypeID1 == 3 && SR_GameServer.Data.Globals.Ref.ObjItem[RefItemID].TypeID2 == 3)
            {
                p.WriteByte((byte)Data);
                return 6;
            }
            return 5;
        }
    }

    public struct _pk_info
    {
        public byte Dailiy;
        public short Total;
        public int MurdererLevel;
        public DateTime Last;
        public PKType Type;
    }

    public struct _action_info
    {
        public GObj Target;
        public int SkillID;
        public int CastingID;
    }

    public struct _sightGObj
    {
        public GObj Object;
        public bool Seen;
        public Queue<Packet> PacketQueue;

        public override bool Equals(object obj)
        {
            try
            {
                _sightGObj sight = (_sightGObj)obj;
                return this.Object.m_uniqueId == sight.Object.m_uniqueId && this.Seen == sight.Seen;
            }
            catch
            {
                return false;
            }
        }
    }
}