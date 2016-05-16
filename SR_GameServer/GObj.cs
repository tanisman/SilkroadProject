namespace SR_GameServer
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using SCommon;
    using SCommon.Security;
    
    using SCore;

    using GameWorld;

    using SharpDX;

    public abstract class GObj
    {
        #region Public Properties and Fields 

        public GObjType m_type;
        public WalkState m_walkState;
        public MovementType m_movementType;
        public LifeState m_lifeState;
        public StatusType m_status;
        public List<_sightGObj> m_inSightGObjList;
        public int m_model;
        public int m_refSkillId;
        public int m_uniqueId;
        public bool m_isBusy;
        public float m_baseWalkSpeed;
        public float m_baseRunSpeed;
        public float m_baseHwanSpeed;
        public volatile int m_bonusSpeed;
        public volatile int m_bonusSpeedPercent;
        public short m_region;
        public Vector3 m_position;
        public bool m_hasDestination;
        public short m_destinationRegion;
        public Vector3 m_destination;
        public bool m_hasAngleMovement;
        public Vector2 m_direction;
        public int m_lastMovementUpdate;
        public AsyncTimer m_movementTimer;
        public float m_angle;
        public GObj m_selectedGObj;
        public _buff_item[] m_buffs;
        public int m_lastEmoteTick;
        public Random m_rnd;
        public AsyncTimer m_disapperTimer;
        public object m_assignedListNode;
        public object m_lock;

        public float Speed
        {
            get
            {
                lock (m_lock)
                {
                    float baseSpeed = m_baseRunSpeed;
                    if (m_walkState == WalkState.Walking)
                        baseSpeed = m_baseWalkSpeed;
                    if (m_status == StatusType.Hwan)
                        baseSpeed = m_baseHwanSpeed;

                    return (baseSpeed + Interlocked.CompareExchange(ref m_bonusSpeed, 0, 0)) * (1.0f + Interlocked.CompareExchange(ref m_bonusSpeedPercent, 0, 0) / 100f);
                }
            }
        }

        public Vector3 Position
        {
            get
            {
                if (m_movementType == MovementType.Moving)
                    return (m_position.ToVector2() + m_direction * this.Speed * (Environment.TickCount - m_lastMovementUpdate) / 1000f).ToVector3(m_position.Y);
                else
                    return m_position;
            }
        }

        public bool IsMonster => m_type == GObjType.GObjMob;
        public bool IsItem => m_type == GObjType.GObjItem;
        public bool IsCharacter => m_type == GObjType.GObjChar;
        public bool IsCOS => m_type == GObjType.GObjCOS;
        public bool IsStructure => m_type == GObjType.GObjStructure;
        public bool IsNPC => m_type == GObjType.GObjNPC;

        #endregion

        #region Constructors & Destructors

        public GObj(GObjType type)
        {
            m_type = type;
            m_inSightGObjList = new List<_sightGObj>();
            m_position = new Vector3();
            m_movementTimer = new AsyncTimer(MovementTimer_Callback);
            m_disapperTimer = new AsyncTimer(DisappearTimer_Callback);
            m_selectedGObj = null;
            m_buffs = new _buff_item[10];
            for (int i = 0; i < m_buffs.Length; i++)
                m_buffs[i].IsFree = true;
            m_rnd = new Random();
            m_lock = new object();
            m_lastEmoteTick = Environment.TickCount;
        }

        #endregion

        #region Public Methods

        public void SightCheck()
        {
            Packet group_spawn = default(Packet);
            Packet group_despawn = default(Packet);
            List<GObj> obj_added = new List<GObj>();
            List<GObj> obj_removed = new List<GObj>();

            if (this.IsCharacter)
            {
                group_spawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_DATA);
                group_despawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_DATA);
            }

            node<GObj> node = Data.Globals.GObjList.Head.next_;
            while (node != null)
            {
                lock (node.locker_)
                {
                    if (node.value_ != null)
                    {
                        if (this.IsCharacter || node.value_.IsCharacter)
                        {
                            if (/*Formula.IsInSightRegion(this.m_region, node.value_.m_region) && */this.m_uniqueId != node.value_.m_uniqueId)
                            {
                                if (this.Position.ToGameWorld(this.m_region).Distance(node.value_.Position.ToGameWorld(node.value_.m_region)) <= 50f + (float)Data.Globals.Ref.ObjCommon[node.value_.m_model].BCRadius / Formula.WORLD_SCALE) //spawn
                                {
                                    if (node.value_.TryAddSightList(this))
                                    {
                                        if (this.IsCharacter)
                                        {
                                            if (node.value_.IsItem)
                                            {
                                                Packet spawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_SINGLE);
                                                spawn.WriteGObj(node.value_, true);
                                                spawn.m_data = node.value_;
                                                this.SendGObj(true, spawn, node.value_);
                                            }
                                            else
                                            {
                                                group_spawn.WriteGObj(node.value_);
                                                obj_added.Add(node.value_);
                                            }
                                        }
                                        else
                                            obj_added.Add(node.value_);
                                    }

                                    if (this.TryAddSightList(node.value_))
                                    {
                                        Packet spawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_SINGLE);
                                        spawn.WriteGObj(this, true);
                                        spawn.m_data = this;
                                        node.value_.SendGObj(true, spawn, this);
                                    }
                                }
                                else //despawn
                                {
                                    if (node.value_.TryRemoveSightList(this))
                                    {
                                        if (this.IsCharacter)
                                            group_despawn.WriteInt32(node.value_.m_uniqueId);
                                        obj_removed.Add(node.value_);
                                    }

                                    if (this.TryRemoveSightList(node.value_))
                                    {
                                        Packet despawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_DESPAWN_SINGLE);
                                        despawn.WriteInt32(this.m_uniqueId);
                                        despawn.m_data = this;
                                        node.value_.SendGObj(false, despawn, this);
                                    }
                                }
                            }
                        }
                    }
                    node = node.next_;
                }
            }


            if (obj_added.Count > 0)
                this.SendGObj(true, group_spawn, obj_added);

            if (obj_removed.Count > 0)
                this.SendGObj(false, group_despawn, obj_removed);

            if (this.m_type == GObjType.GObjMob)
            {
                if (!Data.Globals.Ref.Region[m_region].IsBattleField)
                {
                    this.Disappear();
                    return;
                }
            }
        }

        public void Disappear(bool sendme = true)
        {
            if (!Data.Globals.GObjList.pop((node<GObj>)m_assignedListNode))
                return;
            m_assignedListNode = null;

            Packet group_despawn = default(Packet);
            List<GObj> obj_removed = new List<GObj>();

            if (this.IsCharacter)
            {
                group_despawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_DATA);
            }

            node<GObj> node = Data.Globals.GObjList.Head.next_;
            while (node != null)
            {
                lock (node.locker_)
                {
                    if (node.value_ != null)
                    {
                        if (this.IsCharacter || node.value_.IsCharacter)
                        {
                            if (/*Formula.IsInSightRegion(this.m_region, node.value.m_region) &&*/ this.m_uniqueId != node.value_.m_uniqueId)
                            {
                                if (node.value_.TryRemoveSightList(this))
                                {
                                    if (this.IsCharacter)
                                        group_despawn.WriteInt32(node.value_.m_uniqueId);

                                    obj_removed.Add(node.value_);
                                }

                                if (this.TryRemoveSightList(node.value_))
                                {
                                    Packet despawn = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_DESPAWN_SINGLE);
                                    despawn.WriteInt32(this.m_uniqueId);
                                    despawn.m_data = this;
                                    node.value_.SendGObj(false, despawn, this);
                                }
                            }
                        }
                    }
                    node = node.next_;
                }
            }

            if (this.IsCharacter && sendme)
            {
                if (obj_removed.Count > 0)
                {
                    Packet start = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_START);
                    start.WriteByte(2); //despawn
                    start.WriteUInt16(obj_removed.Count);

                    Packet end = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_SPAWN_GROUP_END);

                    this.SendPacket(start);
                    this.SendPacket(group_despawn);
                    this.SendPacket(end);
                }
            }

            for(int i = 0; i < obj_removed.Count; i++)
            {
                lock (obj_removed[i].m_lock)
                {
                    _sightGObj obj = new _sightGObj { Object = this, Seen = false };
                    int idx = obj_removed[i].m_inSightGObjList.IndexOf(obj);
                    if (idx != -1)
                        obj_removed[i].m_inSightGObjList.RemoveAt(idx);
                }
            }
        }
        
        public bool InSight(GObj obj)
        {
            lock (m_lock)
            {
                return m_inSightGObjList.Exists(p => p.Object.m_uniqueId == obj.m_uniqueId);
            }
        }

        public bool TryAddSightList(GObj obj)
        {
            lock (m_lock)
            {
                if (!InSight(obj))
                {
                    var sight_item = new _sightGObj { Object = obj, Seen = false };
                    if (obj.IsCharacter)
                        sight_item.PacketQueue = new Queue<Packet>();

                    m_inSightGObjList.Add(sight_item);
                    return true;
                }
            }
            return false;
        }

        public bool TryRemoveSightList(GObj obj)
        {
            lock (m_lock)
            {
                for(int i = 0; i < m_inSightGObjList.Count; i++)
                {
                    var sight_obj = m_inSightGObjList[i];
                    if (sight_obj.Object.m_uniqueId == obj.m_uniqueId && sight_obj.Seen)
                    {
                        sight_obj.Seen = false;
                        m_inSightGObjList[i] = sight_obj;
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void SendGObj(bool type, Packet packet, GObj gobj)
        {
            lock (gobj.m_lock)
            {
                _sightGObj obj = new _sightGObj { Object = this, Seen = false };
                int idx = gobj.m_inSightGObjList.IndexOf(obj);

                if (idx != -1)
                {
                    if (type)
                    {
                        obj.Seen = true;
                        gobj.m_inSightGObjList[idx] = obj;
                        OnEnterVisibility(gobj);
                        gobj.OnEnterVisibility(this);
                    }
                    else
                    {
                        gobj.m_inSightGObjList.RemoveAt(idx);
                        OnLeaveVisibility(gobj);
                        gobj.OnLeaveVisibility(this);
                    }
                }
            }
        }

        public virtual void SendGObj(bool type, Packet packet, List<GObj> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                var gobj = objects[i];
                lock (gobj.m_lock)
                {
                    _sightGObj obj = new _sightGObj { Object = this, Seen = false };
                    int idx = gobj.m_inSightGObjList.IndexOf(obj);

                    if (idx != -1)
                    {
                        if (type)
                        {
                            obj.Seen = true;
                            gobj.m_inSightGObjList[idx] = obj;
                            OnEnterVisibility(gobj);
                            gobj.OnEnterVisibility(this);
                        }
                        else if (!type)
                        {
                            gobj.m_inSightGObjList.RemoveAt(idx);
                            OnLeaveVisibility(gobj);
                            gobj.OnLeaveVisibility(this);
                        }
                    }
                }
            }
        }

        public virtual void SendPacket(Packet packet)
        {
            //
        }

        public virtual void OnEnterVisibility(GObj obj)
        {
            //
        }

        public virtual void OnLeaveVisibility(GObj obj)
        {
            //
        }
        
        public void BroadcastToSightList(Packet packet)
        {
            lock (m_lock)
            {
                for (int i = 0; i < m_inSightGObjList.Count; i++)
                {
                    var sight_obj = m_inSightGObjList[i];

                    if (sight_obj.Seen)
                        sight_obj.Object.SendPacket(packet);
                    else if (sight_obj.Object.IsCharacter && sight_obj.PacketQueue != null)
                        sight_obj.PacketQueue.Enqueue(packet);

                    m_inSightGObjList[i] = sight_obj;
                }
            }
        }

        public void MoveTo(short region, Vector3 position)
        {
            lock (m_lock)
            {
                if (m_isBusy || m_movementType == MovementType.Seat || m_movementType == MovementType.NotMoveable || this.Speed < float.Epsilon || Data.Globals.Ref.Region[m_region] == null)
                    return;

                StopMovement();

                m_destinationRegion = region;
                m_destination = position;
                m_angle = Formula.Angle(m_position.ToGameWorld(m_region).ToVector3(m_position.Y), m_destination.ToGameWorld(m_destinationRegion).ToVector3(m_destination.Y));
                
                m_hasDestination = true;
                StartMovement((m_destination.ToGameWorld(m_destinationRegion) - m_position.ToGameWorld(m_region)).Normalized());
            }

            Packet pkt = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_MOVEMENT);
            pkt.WriteInt32(m_uniqueId);
            pkt.WriteByte(1);
            pkt.WriteInt16(m_destinationRegion);
            pkt.WriteUInt16(m_destination.X);
            pkt.WriteUInt16(m_destination.Y);
            pkt.WriteUInt16(m_destination.Z);
            pkt.WriteUInt16(m_region);
            pkt.WriteSingle(m_position.X);
            pkt.WriteSingle(m_position.Y);
            pkt.WriteSingle(m_position.Z);
            pkt.WriteByte(0);
            pkt.WriteByte(0);
            SendPacket(pkt);
            BroadcastToSightList(pkt);
        }

        public void Move(ushort angle)
        {
            lock (m_lock)
            {
                if (m_isBusy || m_movementType == MovementType.Seat || m_movementType == MovementType.NotMoveable || this.Speed < float.Epsilon || Data.Globals.Ref.Region[m_region] == null)
                    return;

                StopMovement();

                m_angle = angle;

                m_hasAngleMovement = true;
                StartMovement(new Vector2((float)Math.Cos((m_angle / 182f) * (Math.PI / 180f)), (float)Math.Sin((m_angle / 182f) * (Math.PI / 180f))));
            }

            Packet pkt = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_MOVEMENT);
            pkt.WriteInt32(m_uniqueId);
            pkt.WriteInt32(0);
            pkt.WriteUInt16(m_region);
            pkt.WriteSingle(m_position.X);
            pkt.WriteSingle(m_position.Y);
            pkt.WriteSingle(m_position.Z);
            pkt.WriteUInt16(angle);
            SendPacket(pkt);
            BroadcastToSightList(pkt);
        }

        public void ChangeDirection(Vector2 direction)
        {
            lock (m_lock)
            {
                if (m_movementType == MovementType.Moving)
                {
                    Vector3 old_pos = m_position;
                    m_position = (m_position.ToVector2() + m_direction * this.Speed * (Environment.TickCount - m_lastMovementUpdate) / 1000f).ToVector3(m_position.Y);
                    Vector3 result;
                    if (Data.NavMesh.Collision.Test(m_region, old_pos, m_position, out result))
                    {
                        m_position = old_pos;
                        Logging.Log()("collision detected 4", LogLevel.Info);
                    }
                    UpdatePosition();
                }
                m_direction = direction;
            }
        }

        public void StartMovement(Vector2 direction)
        {
            lock (m_lock)
            {
                m_movementType = MovementType.Moving;
                m_direction = direction;
                m_lastMovementUpdate = Environment.TickCount;
                if (m_movementTimer.IsRunning)
                    m_movementTimer.Change(0, 1000, false);
                else
                    m_movementTimer.Start(0, 1000, false);
            }
        }

        public void StopMovement(bool packet = false)
        {
            lock (m_lock)
            {
                if (m_movementType == MovementType.Moving)
                {
                    if (m_movementTimer.IsRunning)
                        m_movementTimer.Stop();
                    Vector3 old_pos = m_position;
                    m_position = (m_position.ToVector2() + m_direction * this.Speed * (Environment.TickCount - m_lastMovementUpdate) / 1000f).ToVector3(m_position.Y);
                    Vector3 result;
                    if (Data.NavMesh.Collision.Test(m_region, old_pos, m_position, out result))
                    {
                        m_position = old_pos;
                        Logging.Log()("collision detected 3", LogLevel.Info);
                    }
                    UpdatePosition();
                }

                if (packet)
                {
                    Packet pkt = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_WARP);
                    pkt.WriteInt32(m_uniqueId);
                    pkt.WriteInt16(m_region);
                    pkt.WriteSingle(m_position.X);
                    pkt.WriteSingle(m_position.Y);
                    pkt.WriteSingle(m_position.Z);
                    pkt.WriteUInt16(m_angle);
                    SendPacket(pkt);
                    BroadcastToSightList(pkt);
                }

                m_movementType = MovementType.NotMoving;
                m_hasDestination = false;
                m_hasAngleMovement = false;
                m_direction = Vector2.Zero;
            }
        }

        public void UpdatePosition()
        {
            lock (m_lock)
            {
                short old_region = m_region;
                Vector3 old_position = m_position;
                if (m_position.X > 1920)
                {
                    m_region += (short)(m_position.X / 1920);
                    m_position.X = m_position.X % 1920;
                }
                else if (this.m_position.X < 0)
                {
                    m_region -= (short)((short)((-m_position.X - 1) / 1920) + 1);
                    m_position.X = 1920 + (m_position.X % 1920);
                }

                if (this.m_position.Z > 1920)
                {
                    m_region += (short)(Formula.REGION_SCALE * (short)(m_position.Z / 1920));
                    m_position.Z = m_position.Z % 1920;
                }
                else if (this.m_position.Z < 0)
                {
                    m_region -= (short)(Formula.REGION_SCALE * ((short)((-m_position.Z - 1) / 1920) + 1));
                    m_position.Z = 1920 + (m_position.Z % 1920);
                }

                if (Data.Globals.Ref.Region[m_region] == null)
                {
                    m_region = old_region;
                    m_position = old_position;
                }

                m_lastMovementUpdate = Environment.TickCount;
            }
        }

        public void Emote(byte type)
        {
            if (Environment.TickCount - m_lastEmoteTick < 1000)
                return;

            StopMovement(true);

            m_lastEmoteTick = Environment.TickCount;

            Packet pkt = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_EMOTE);
            pkt.WriteInt32(m_uniqueId);
            pkt.WriteByte(type);
            SendPacket(pkt);
            BroadcastToSightList(pkt);
        }
        
        public void StartDisappear(int time)
        {
            m_disapperTimer.Start(time, 0);
        }

        #endregion

        #region Private Methods

        private void MovementTimer_Callback(object sender, object state)
        {
            bool last = (bool)state;
            lock (m_lock)
            {
                Vector2 old_pos = m_position.ToVector2();

                //make a simple update to position for some test
                //before the NavMesh tests
                m_position = (m_position.ToVector2() + m_direction * this.Speed * (Environment.TickCount - m_lastMovementUpdate) / 1000f).ToVector3(m_position.Y);

                if (m_hasDestination)
                {
                    if (last)
                    {
                        m_movementType = MovementType.NotMoving;
                        m_hasDestination = false;
                        m_hasAngleMovement = false;
                        m_direction = Vector2.Zero;
                        m_movementTimer.Reset(-1, 0);
                        Logging.Log()("movement has ended", LogLevel.Success);
                    }
                    else
                    {
                        float arrivalT = m_destination.ToGameWorld(m_destinationRegion).Distance(m_position.ToGameWorld(m_region)) / (this.Speed / Formula.WORLD_SCALE) * 1000f;
                        if (arrivalT < 1000f)
                            m_movementTimer.Reset((int)arrivalT, 0, true);
                    }
                }



                Vector3 result;
                if (Data.NavMesh.Collision.Test(m_region, old_pos.ToVector3(m_position.Y), m_position, out result))
                {
                    m_position = old_pos.ToVector3(m_position.Y);
                    m_movementTimer.Reset(-1, 0);
                    m_movementType = MovementType.NotMoving;
                    m_hasDestination = false;
                    m_hasAngleMovement = false;
                    m_direction = Vector2.Zero;
                    m_lastMovementUpdate = Environment.TickCount;
                    Logging.Log()("collision detected 1", LogLevel.Info);
                    return;
                }
                else
                {
                    m_position.Y = result.Y;

                    if (Data.NavMesh.Collision.Test(m_region, m_position, (m_position.ToVector2() + m_direction * this.Speed).ToVector3(m_position.Y), out result))
                    {
                        float arrivalT = result.ToGameWorld(m_region).Distance(m_position.ToGameWorld(m_region)) / (this.Speed / Formula.WORLD_SCALE) * 1000f - 100f;
                        if (arrivalT > 250)
                            m_movementTimer.Reset((int)arrivalT, 0, true);
                        else
                        {
                            m_movementTimer.Reset(0, 0, true);
                            m_movementType = MovementType.NotMoving;
                            m_hasDestination = false;
                            m_hasAngleMovement = false;
                            m_direction = Vector2.Zero;
                            m_lastMovementUpdate = Environment.TickCount;
                        }
                        m_lastMovementUpdate = Environment.TickCount;
                        Logging.Log()("collision detected 2", LogLevel.Info);
                        return;
                    }
                }
                UpdatePosition();
            }
            SightCheck();

            if (last)
            {
                Packet pkt = new Packet(SCommon.Opcode.Agent.GAMEOBJECT_WARP);
                pkt.WriteInt32(m_uniqueId);
                pkt.WriteInt16(m_region);
                pkt.WriteSingle(m_position.X);
                pkt.WriteSingle(m_position.Y);
                pkt.WriteSingle(m_position.Z);
                pkt.WriteUInt16(m_angle);
                SendPacket(pkt);
                BroadcastToSightList(pkt);
            }
        }

        protected virtual void DisappearTimer_Callback(object sender, object state)
        {
            Disappear(false);
        }

        #endregion
    }
}
