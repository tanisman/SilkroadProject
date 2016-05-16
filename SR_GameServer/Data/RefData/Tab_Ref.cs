namespace SR_GameServer.Data.RefData
{
    using System;
    using System.Collections.Generic;

    using SCommon;

    public class Tab_RefNest
    {
        #region Public Properties and Fields

        public int dwNestID, dwHiveID, dwTacticsID;
        public short nRegionDBID;
        public float fLocalPosX, fLocalPosY, fLocalPosZ;
        public short wInitialDir;
        public int nRadius, nGenerateRadius, nChampionGenPercentage, dwDelayTimeMin, dwDelayTimeMax, dwMaxTotalCount;
        public byte btFlag, btRespawn, btType;

        #endregion
    }

    public class Tab_RefHive
    {
        #region Public Properties and Fields

        public int dwHiveID, dwOverwriteMaxTotalCount, dwSpawnSpeedIncreaseRate, dwMaxIncreaseRate;
        public byte btKeepMonsterCountType, btFlag;
        public float fMonsterCountPerPC;
        public short GameWorldID, HatchObjType;
        public string szDescString128;

        #endregion
    }

    public class Tab_RefTactics
    {
        #region Public Properties and Fields

        public int dwTacticsID, dwObjID;
        public byte btAIQoS;
        public int nMaxStamina;
        public byte btMaxStaminaVariance;
        public int nSightRange;
        public byte btAggressType;
        public int AggressData;
        public byte btChangeTarget, btHelpRequestTo, btHelpResponseTo, btBattleStyle;
        public int BattleStyleData;
        public byte btDiversionBasis;
        public int[] DiversionBasisData;
        public byte btDiversionKeepBasis;
        public int[] DiversionKeepBasisData;
        public byte btKeepDistance;
        public int KeepDistanceData;
        public byte btTraceType, btTraceBoundary;
        public int TraceData;
        public byte btHomingType;
        public int HomingData;
        public byte btAggressTypeOnHoming, btFleeType;
        public int dwChampionTacticsID, AdditionOptionFlag;
        public string szDescString128;

        #endregion

        #region Constructors & Destructors

        public Tab_RefTactics()
        {
            DiversionBasisData = new int[8];
            DiversionKeepBasisData = new int[8];
        }

        #endregion
    }

    public static class Tab_RefExtensions
    {
        public static void Load(this Dictionary<int, Tab_RefNest> nest)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM Tab_RefNest"))
            {
                while (reader.Read())
                {
                    int dw_NestID = (int)reader["dwNestID"];
                    if (nest.ContainsKey(dw_NestID))
                    {
                        Logging.Log()("locallocal overlap job exception!!", LogLevel.Error);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }

                    nest[dw_NestID] = new Tab_RefNest();
                    nest[dw_NestID].dwNestID = dw_NestID;
                    nest[dw_NestID].dwHiveID = (int)reader["dwHiveID"];
                    nest[dw_NestID].dwTacticsID = (int)reader["dwTacticsID"];
                    if (Globals.Ref.Hive.ContainsKey(nest[dw_NestID].dwHiveID) && Globals.Ref.Tactics.ContainsKey(nest[dw_NestID].dwTacticsID))
                    {
                        nest[dw_NestID].nRegionDBID = (short)reader["nRegionDBID"];
                        if (Globals.Ref.Region[nest[dw_NestID].nRegionDBID] == null)
                            Console.WriteLine("Invalid region {0}, this nest never be spawned ({1})", nest[dw_NestID].nRegionDBID, dw_NestID);
                        nest[dw_NestID].fLocalPosX = (float)reader["fLocalPosX"];
                        nest[dw_NestID].fLocalPosY = (float)reader["fLocalPosY"];
                        nest[dw_NestID].fLocalPosZ = (float)reader["fLocalPosZ"];
                        nest[dw_NestID].wInitialDir = (short)reader["wInitialDir"];
                        nest[dw_NestID].nRadius = (int)reader["nRadius"];
                        nest[dw_NestID].nGenerateRadius = (int)reader["nGenerateRadius"];
                        nest[dw_NestID].nChampionGenPercentage = (int)reader["nChampionGenPercentage"];
                        nest[dw_NestID].dwDelayTimeMin = (int)reader["dwDelayTimeMin"];
                        nest[dw_NestID].dwDelayTimeMax = (int)reader["dwDelayTimeMax"];
                        nest[dw_NestID].dwMaxTotalCount = (int)reader["dwMaxTotalCount"];
                        nest[dw_NestID].btFlag = (byte)reader["btFlag"];
                        nest[dw_NestID].btRespawn = (byte)reader["btRespawn"];
                        nest[dw_NestID].btType = (byte)reader["btType"];
                    }
                    else
                    {
                        Console.WriteLine("Invalid NEST {0}", dw_NestID);
                        nest.Remove(dw_NestID);
                    }
                }
            }
        }
        public static void Load(this Dictionary<int, Tab_RefHive> hive)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM Tab_RefHive"))
            {
                while (reader.Read())
                {
                    int dw_HiveID = (int)reader["dwHiveID"];
                    if (hive.ContainsKey(dw_HiveID))
                    {
                        Logging.Log()("locallocal overlap job exception!!", LogLevel.Error);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    hive[dw_HiveID] = new Tab_RefHive();
                    hive[dw_HiveID].dwHiveID = dw_HiveID;
                    hive[dw_HiveID].dwOverwriteMaxTotalCount = (int)reader["dwOverwriteMaxTotalCount"];
                    hive[dw_HiveID].dwSpawnSpeedIncreaseRate = (int)reader["dwSpawnSpeedIncreaseRate"];
                    hive[dw_HiveID].dwMaxIncreaseRate = (int)reader["dwMaxIncreaseRate"];
                    hive[dw_HiveID].btKeepMonsterCountType = (byte)reader["btKeepMonsterCountType"];
                    hive[dw_HiveID].btFlag = (byte)reader["btFlag"];
                    hive[dw_HiveID].fMonsterCountPerPC = (float)reader["fMonsterCountPerPC"];
                    hive[dw_HiveID].GameWorldID = (short)reader["GameWorldID"];
                    hive[dw_HiveID].HatchObjType = (short)reader["HatchObjType"];
                    hive[dw_HiveID].szDescString128 = (string)reader["szDescString128"];
                }
            }
        }

        public static void Load(this Dictionary<int, Tab_RefTactics> tactics)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM Tab_RefTactics"))
            {
                while (reader.Read())
                {
                    int dw_TacticsID = (int)reader["dwTacticsID"];
                    if (tactics.ContainsKey(dw_TacticsID))
                    {
                        Logging.Log()("locallocal overlap job exception!!", LogLevel.Error);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    tactics[dw_TacticsID] = new Tab_RefTactics();
                    tactics[dw_TacticsID].dwTacticsID = dw_TacticsID;
                    tactics[dw_TacticsID].dwObjID = (int)reader["dwObjID"];
                    tactics[dw_TacticsID].btAIQoS = (byte)reader["btAIQoS"];
                    tactics[dw_TacticsID].nMaxStamina = (int)reader["nMaxStamina"];
                    tactics[dw_TacticsID].btMaxStaminaVariance = (byte)reader["btMaxStaminaVariance"];
                    tactics[dw_TacticsID].nSightRange = (int)reader["nSightRange"];
                    tactics[dw_TacticsID].btAggressType = (byte)reader["btAggressType"];
                    tactics[dw_TacticsID].AggressData = (int)reader["AggressData"];
                    tactics[dw_TacticsID].btChangeTarget = (byte)reader["btChangeTarget"];
                    tactics[dw_TacticsID].btHelpRequestTo = (byte)reader["btHelpRequestTo"];
                    tactics[dw_TacticsID].btHelpResponseTo = (byte)reader["btHelpResponseTo"];
                    tactics[dw_TacticsID].btBattleStyle = (byte)reader["btBattleStyle"];
                    tactics[dw_TacticsID].BattleStyleData = (int)reader["BattleStyleData"];
                    tactics[dw_TacticsID].btDiversionBasis = (byte)reader["btDiversionBasis"];
                    for (byte b = 0; b < 7; b++)
                        tactics[dw_TacticsID].DiversionBasisData[b] = (int)reader[String.Format("DiversionBasisData{0}", b + 1)];
                    tactics[dw_TacticsID].btDiversionKeepBasis = (byte)reader["btDiversionKeepBasis"];
                    for (byte b = 0; b < 7; b++)
                        tactics[dw_TacticsID].DiversionKeepBasisData[b] = (int)reader[String.Format("DiversionKeepBasisData{0}", b + 1)];
                    tactics[dw_TacticsID].btKeepDistance = (byte)reader["btKeepDistance"];
                    tactics[dw_TacticsID].KeepDistanceData = (int)reader["KeepDistanceData"];
                    tactics[dw_TacticsID].btTraceType = (byte)reader["btTraceType"];
                    tactics[dw_TacticsID].btTraceBoundary = (byte)reader["btTraceBoundary"];
                    tactics[dw_TacticsID].TraceData = (int)reader["TraceData"];
                    tactics[dw_TacticsID].btHomingType = (byte)reader["btHomingType"];
                    tactics[dw_TacticsID].HomingData = (int)reader["HomingData"];
                    tactics[dw_TacticsID].btAggressTypeOnHoming = (byte)reader["btAggressTypeOnHoming"];
                    tactics[dw_TacticsID].btFleeType = (byte)reader["btFleeType"];
                    tactics[dw_TacticsID].dwChampionTacticsID = (int)reader["dwChampionTacticsID"];
                    tactics[dw_TacticsID].AdditionOptionFlag = (int)reader["AdditionOptionFlag"];
                    tactics[dw_TacticsID].szDescString128 = (string)reader["szDescString128"];
                }
            }
        }
    }
}