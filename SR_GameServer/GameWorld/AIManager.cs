namespace SR_GameServer.GameWorld
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using SCommon;

    using Data.NavMesh;

    using SharpDX;

    public static class AIManager
    {
        private static Random s_rnd;

        static AIManager()
        {
            s_rnd = new Random();
            Logging.Log()("A.I. Manager has initialized.");
        }

        public static void Initialize()
        {
            new Thread(() =>
            {
                foreach (var kvp in Data.Globals.Ref.Nest)
                {
                    int k = kvp.Value.dwMaxTotalCount;
                    if (Data.Globals.Ref.Hive[kvp.Value.dwHiveID].HatchObjType == 2 && k > 0) //prevent more than 1 npc spawns
                        k = 1;

                    for (int a = 0; a < k; a++)
                    {
                        int count = (int)(((Data.Globals.Ref.Hive[kvp.Value.dwHiveID].fMonsterCountPerPC / 100f) * Data.Globals.Ref.Hive[kvp.Value.dwHiveID].dwMaxIncreaseRate) / 2f);
                        if (Data.Globals.Ref.Hive[kvp.Value.dwHiveID].HatchObjType == 2 || count <= 0)
                            count = 1;

                        for (int b = 0; b < count; b++)
                        {
                            if (Data.Globals.Ref.Hive[kvp.Value.dwHiveID].HatchObjType != 2)
                            {
                                GObjMob obj = new GObjMob()
                                {
                                    m_walkState = WalkState.Walking,
                                    m_movementType = MovementType.NotMoving,
                                    m_lifeState = LifeState.Alive,
                                    m_status = StatusType.None,
                                    m_model = Data.Globals.Ref.Tactics[kvp.Value.dwTacticsID].dwObjID,
                                    m_refSkillId = -1,
                                    m_uniqueId = Services.UniqueID.GenerateGObjID(),
                                    m_baseWalkSpeed = Data.Globals.Ref.ObjChar[Data.Globals.Ref.Tactics[kvp.Value.dwTacticsID].dwObjID].Speed1,
                                    m_baseRunSpeed = Data.Globals.Ref.ObjChar[Data.Globals.Ref.Tactics[kvp.Value.dwTacticsID].dwObjID].Speed2,
                                    m_baseHwanSpeed = 100,
                                    m_region = kvp.Value.nRegionDBID,
                                    m_angle = kvp.Value.wInitialDir,
                                    m_generatedRegion = kvp.Value.nRegionDBID,
                                    m_attackType = AttackType.None,
                                };                                
                                obj.m_position = new Vector3(kvp.Value.fLocalPosX, kvp.Value.fLocalPosY, kvp.Value.fLocalPosZ);
                                obj.m_position = (obj.m_position.ToVector2() + new Vector2((float)Math.Cos(s_rnd.Next(0, 360) * (Math.PI / 180)), (float)Math.Sin(s_rnd.Next(0, 360) * (Math.PI / 180))) * kvp.Value.nGenerateRadius).ToVector3(kvp.Value.fLocalPosY);
                                obj.m_generatedPosition = obj.m_position;
                                obj.UpdatePosition();
                                int x = 0;
                                while (!obj.m_position.IsValid(obj.m_region) && x < 50)
                                {
                                    obj.m_position = new Vector3(kvp.Value.fLocalPosX, kvp.Value.fLocalPosY, kvp.Value.fLocalPosZ);
                                    obj.m_position = (obj.m_position.ToVector2() + new Vector2((float)Math.Cos(s_rnd.Next(0, 360) * (Math.PI / 180)), (float)Math.Sin(s_rnd.Next(0, 360) * (Math.PI / 180))) * kvp.Value.nGenerateRadius).ToVector3(kvp.Value.fLocalPosY);
                                    obj.UpdatePosition();
                                    x++;
                                }
                                if (x >= 50)
                                {
                                    obj.m_position = new Vector3(kvp.Value.fLocalPosX, kvp.Value.fLocalPosY, kvp.Value.fLocalPosZ);
                                    obj.UpdatePosition();
                                }
                                if (!obj.m_position.IsValid(obj.m_region))
                                {
                                    obj = null;
                                    break;
                                }
                                obj.m_nestId = kvp.Value.dwNestID;
                                obj.m_rarity = (GObjMobRarity)Data.Globals.Ref.ObjChar[obj.m_model].Rarity;

                                if (s_rnd.Next(0, 100) <= kvp.Value.nChampionGenPercentage)
                                {
                                    if (Data.Globals.Ref.ObjChar[obj.m_model].Lvl >= 14 && s_rnd.Next(0, 3) == 2)
                                        obj.m_rarity = GObjMobRarity.Giant;
                                    else
                                        obj.m_rarity = GObjMobRarity.Champion;
                                }
                                obj.m_currentHealthPoints = (int)obj.MaxHP;

                                obj.m_assignedListNode = Data.Globals.GObjList.push(obj);
                                obj.SightCheck();
                            }
                            else
                            {
                                GObjNPC obj = new GObjNPC()
                                {
                                    m_walkState = WalkState.Walking,
                                    m_movementType = MovementType.NotMoveable,
                                    m_lifeState = LifeState.Alive,
                                    m_status = StatusType.None,
                                    m_model = Data.Globals.Ref.Tactics[kvp.Value.dwTacticsID].dwObjID,
                                    m_refSkillId = -1,
                                    m_uniqueId = Services.UniqueID.GenerateGObjID(),
                                    m_baseWalkSpeed = Data.Globals.Ref.ObjChar[Data.Globals.Ref.Tactics[kvp.Value.dwTacticsID].dwObjID].Speed1,
                                    m_baseRunSpeed = Data.Globals.Ref.ObjChar[Data.Globals.Ref.Tactics[kvp.Value.dwTacticsID].dwObjID].Speed2,
                                    m_baseHwanSpeed = 100,
                                    m_region = kvp.Value.nRegionDBID,
                                    m_position = new Vector3(kvp.Value.fLocalPosX, kvp.Value.fLocalPosY, kvp.Value.fLocalPosZ),
                                    m_angle = kvp.Value.wInitialDir,
                                };
                                obj.m_talkFlag = GObjTalkFlags.Talkable;
                                switch (Data.Globals.Ref.ObjCommon[obj.m_model].CodeName128)
                                {
                                    case "NPC_CH_SMITH":
                                    case "NPC_WC_SMITH":
                                    case "NPC_KT_SMITH":
                                    case "NPC_CH_ARMOR":
                                    case "NPC_WC_ARMOR":
                                    case "NPC_KT_ARMOR":
                                    case "NPC_KT_POTION":
                                    case "NPC_WC_POTION":
                                    case "NPC_CH_ACCESSORY":
                                    case "NPC_WC_ACCESSORY":
                                    case "NPC_KT_ACCESSORY":
                                    case "NPC_CH_SPECIAL":
                                    case "NPC_WC_SPECIAL":
                                    case "NPC_KT_SPECIAL":
                                    case "NPC_CH_POTION":
                                    case "NPC_CH_POTION2":
                                    case "NPC_CH_POTION3":
                                    case "NPC_CH_HORSE":
                                    case "NPC_CH_HORSE1":
                                    case "NPC_CH_HORSE2":
                                    case "NPC_CH_HORSE3":
                                    case "NPC_CH_HORSE4":
                                    case "NPC_WC_HORSE":
                                    case "NPC_KT_HORSE":
                                    case "NPC_CH_DOCTOR":
                                    case "NPC_WC_DOCTOR":
                                    case "NPC_KT_DESIGNER":
                                    case "NPC_TD_THIEF_BUY":
                                    case "NPC_TD_THIEF_SELL":
                                    case "NPC_TD_THIEF_A":
                                    case "NPC_TD_THIEF_B":
                                    case "NPC_TD_THIEF_C":
                                    case "NPC_TD_THIEF_D":
                                    case "NPC_CH_MINISTER":
                                    case "NPC_KT_MINISTER":
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(1);
                                        obj.m_talkOptions.Add(1);
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0);
                                        break;
                                    case "NPC_CH_WAREHOUSE_M":
                                    case "NPC_CH_WAREHOUSE_W":
                                    case "NPC_WC_WAREHOUSE_M":
                                    case "NPC_WC_WAREHOUSE_W":
                                    case "NPC_KT_WAREHOUSE":
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0x20);
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0);
                                        break;
                                    case "NPC_CH_FERRY":
                                    case "NPC_WC_FERRY":
                                    case "NPC_KT_FERRY":
                                    case "NPC_CH_FERRY2":
                                    case "NPC_WC_FERRY2":
                                    case "NPC_CH_KISAENG6":
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(2);
                                        obj.m_talkOptions.Add(8);
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0);
                                        break;
                                    default:
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(1);
                                        obj.m_talkOptions.Add(0);
                                        obj.m_talkOptions.Add(0);
                                        break;
                                }

                                obj.m_assignedListNode = Data.Globals.GObjList.push(obj);
                                obj.SightCheck();
                            }
                        }
                    }
                }

                for (int i = 0; i < Data.Globals.Ref.TeleportBuilding.Length; i++)
                {
                    if (Data.Globals.Ref.TeleportBuilding[i] != null)
                    {
                        GObjNPC obj = new GObjNPC()
                        {
                            m_movementType = MovementType.NotMoveable,
                            m_lifeState = LifeState.Alive,
                            m_status = StatusType.None,
                            m_model = i,
                            m_refSkillId = -1,
                            m_uniqueId = Services.UniqueID.GenerateGObjID(),
                            m_baseWalkSpeed = 0,
                            m_baseRunSpeed = 0,
                            m_baseHwanSpeed = 100,
                            m_region = Data.Globals.Ref.TeleportBuilding[i].RegionID,
                            m_position = new Vector3(Data.Globals.Ref.TeleportBuilding[i].OffsetX, Data.Globals.Ref.TeleportBuilding[i].OffsetY, Data.Globals.Ref.TeleportBuilding[i].OffsetZ),
                            m_angle = 0,
                        };
                        obj.m_talkFlag = GObjTalkFlags.Talkable;
                        obj.m_talkOptions = new List<byte>();
                        obj.m_talkOptions.Add(0);
                        obj.m_talkOptions.Add(12);
                        obj.m_talkOptions.Add(8);
                        obj.m_talkOptions.Add(0);

                        obj.m_assignedListNode = Data.Globals.GObjList.push(obj);
                        obj.SightCheck();
                    }
                }
            }).Start();
        }
    }
}
