namespace SR_GameServer.Data
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using SCommon;
    using SCommon.Database;

    using RefData;

    internal static class Globals
    {
        public static Dictionary<string, string> Config;
        public static MSSQL GlobalDB;
        public static MSSQL ShardDB;
        public static SRGame SRGameService;
        public static string AllowedCharacters = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuopasdfghjklizxcvbnm1234567890_";
        public static _refData Ref;
        public static PooledList<GObj> GObjList;

        public static bool ConnectGlobalDB()
        {
            try
            {
                GlobalDB = new MSSQL("SR_Global Connection String");
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static bool ConnectShardDB()
        {
            try
            {
                ShardDB = new MSSQL("SR_OPEN (aka shard) Connection String");
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static bool LoadConfigTable()
        {
            try
            {
                Config = new Dictionary<string, string>();
                using (var reader = GlobalDB.ExecuteReader("SELECT * FROM _ServerConfig"))
                {
                    while (reader.Read())
                        Config.Add(Convert.ToString(reader["Key"]), Convert.ToString(reader["Value"]));
                }
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static bool LoadRefData()
        {
            try
            {
                GObjList = new PooledList<GObj>();
                Ref = new _refData();

                Ref.CharGen = new List<RefCharGen>();
                Ref.CharGen.Load();

                Ref.FmnTidGroupMap = new List<RefFmnTidGroupMap>();
                Ref.FmnTidGroupMap.Load();

                Ref.ObjItem = new RefObjItem[45000];
                Ref.ObjItemByCodeName = new Dictionary<string, RefObjItem>();
                Ref.ObjItem.Load();
                Ref.ObjItem.ToList().ForEach(p => { if (p != null) Ref.ObjItemByCodeName.Add(p.CodeName128, p); });

                Ref.ObjChar = new RefObjChar[45000];
                Ref.ObjCharByCodeName = new Dictionary<string, RefObjChar>();
                Ref.ObjChar.Load();
                Ref.ObjChar.ToList().ForEach(p => { if (p != null) Ref.ObjCharByCodeName.Add(p.CodeName128, p); });

                Ref.Skill = new RefSkill[45000];
                Ref.Skill.Load();

                Ref.RegionBindAssocServer = new List<RefRegionBindAssocServer>();
                Ref.RegionBindAssocServer.Load();

                Ref.Region = new RefRegion[65535];
                Ref.Region.Load();

                Ref.DropGold = new RefDropGold[255];
                Ref.DropGold.Load();

                Ref.Level = new RefLevel[255];
                Ref.Level.Load();

                Ref.TeleportData = new List<RefTeleport>();
                Ref.TeleportData.Load();

                Ref.TeleportBuilding = new RefTeleportBuilding[45000];
                Ref.TeleportBuilding.Load();

                Ref.TeleportLink = new List<RefTeleportLink>();
                Ref.TeleportLink.Load();

                Ref.ObjCommon = new RefObjCommon[45000];
                Ref.ObjItem.ToList().ForEach(p => { if (p != null) Ref.ObjCommon[p.ID] = p; });
                Ref.ObjChar.ToList().ForEach(p => { if (p != null) Ref.ObjCommon[p.ID] = p; });
                Ref.TeleportBuilding.ToList().ForEach(p => { if (p != null) Ref.ObjCommon[p.ID] = p; });

                Ref.Shop = new List<RefShop>();
                Ref.Shop.Load();

                Ref.Hive = new Dictionary<int, Tab_RefHive>();
                Ref.Hive.Load();

                Ref.Tactics = new Dictionary<int, Tab_RefTactics>();
                Ref.Tactics.Load();

                Ref.Nest = new Dictionary<int, Tab_RefNest>();
                Ref.Nest.Load();

                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static T GetConfigValue<T>(string key)
        {
            return (T)Convert.ChangeType(Config[key], typeof(T));
        }


        internal struct _refData
        {
            public List<RefCharGen> CharGen;
            public RefObjItem[] ObjItem;
            public Dictionary<string, RefObjItem> ObjItemByCodeName;
            public RefObjChar[] ObjChar;
            public Dictionary<string, RefObjChar> ObjCharByCodeName;
            public RefSkill[] Skill;
            public List<RefFmnTidGroupMap> FmnTidGroupMap;
            public RefRegion[] Region;
            public List<RefRegionBindAssocServer> RegionBindAssocServer;
            public RefDropGold[] DropGold;
            public RefLevel[] Level;
            public RefObjCommon[] ObjCommon;
            public Dictionary<int, Tab_RefHive> Hive;
            public Dictionary<int, Tab_RefNest> Nest;
            public Dictionary<int, Tab_RefTactics> Tactics;
            public List<RefTeleport> TeleportData;
            public RefTeleportBuilding[] TeleportBuilding;
            public List<RefTeleportLink> TeleportLink;
            public List<RefShop> Shop;
        }
    }
}
