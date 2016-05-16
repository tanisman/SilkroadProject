namespace SR_GameServer.Data.RefData
{
    using System;
    using System.Collections.Generic;

    public class RefRegion
    {
        #region Public Properties and Fields

        public ushort Region;
        public byte X, Z;
        public string ContinentName, AreaName;
        public bool IsBattleField;
        public int Climate, MaxCapacity, AssocObjID, AssocServer;
        public string AssocFile256;

        #endregion
    }

    public class RefRegionBindAssocServer
    {
        #region Public Properties and Fields

        public string AreaName;
        public byte AssocServer;

        #endregion
    }

    public static class RefRegionExtensions
    {
        public static void Load(this RefRegion[] list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefRegion"))
            {
                while (reader.Read())
                {
                    if (Globals.Ref.RegionBindAssocServer.Exists(p => p.AreaName == (string)reader["ContinentName"] && p.AssocServer != 0))
                    {
                        ushort region = Convert.ToUInt16((short)reader["wRegionID"]);
                        list[region] = new RefRegion();
                        list[region].Region = region;
                        list[region].X = (byte)reader["X"];
                        list[region].Z = (byte)reader["Z"];
                        list[region].ContinentName = (string)reader["ContinentName"];
                        list[region].AreaName = (string)reader["AreaName"];
                        list[region].IsBattleField = Convert.ToBoolean((byte)reader["IsBattleField"]);
                        list[region].Climate = (int)reader["Climate"];
                        list[region].MaxCapacity = (int)reader["MaxCapacity"];
                        list[region].AssocObjID = (int)reader["AssocObjID"];
                        list[region].AssocServer = Globals.Ref.RegionBindAssocServer.Find(p => p.AreaName == (string)reader["ContinentName"] && p.AssocServer != 0).AssocServer;
                        list[region].AssocFile256 = (string)reader["AssocFile256"];
                    }
                }
            }
        }

        public static void Load(this List<RefRegionBindAssocServer> list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefRegionBindAssocServer"))
            {
                while (reader.Read())
                {
                    list.Add(new RefRegionBindAssocServer
                    {
                        AreaName = (string)reader["AreaName"],
                        AssocServer = (byte)reader["AssocServer"]
                    });
                }
            }
        }
    }
}
