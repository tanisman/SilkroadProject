namespace SR_GameServer.Data.RefData
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class RefTeleport
    {
        public int ID, AssocRefObjID;
        public string CodeName128, ZoneName128;
        public short GenRegionID, GenPos_X, GenPos_Y, GenPos_Z, GenAreaRadius;
        public byte CanBeResurrectPos, CanGotoResurrectPos;
    }

    public class RefTeleportBuilding : RefObjCommon
    {
        public RefTeleport TeleportData;
    }

    public class RefTeleportLink
    {
        public int OwnerTeleport, TargetTeleport, Fee;
        public byte RestrictBindMethod, CheckResult;
        public _teleport_restrict[] Restrict;

        public RefTeleportLink()
        {
            Restrict = new _teleport_restrict[5];
        }
    }

    public static class RefTeleportExtensions
    {
        public static void Load(this List<RefTeleport> list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefTeleport ORDER BY ID"))
            {
                while (reader.Read())
                {
                    var t = new RefTeleport();
                    t.ID = (int)reader["ID"];
                    t.AssocRefObjID = (int)reader["AssocRefObjID"];
                    t.CodeName128 = (string)reader["CodeName128"];
                    t.ZoneName128 = (string)reader["ZoneName128"];
                    t.GenRegionID = (short)reader["GenRegionID"];
                    t.GenPos_X = (short)reader["GenPos_X"];
                    t.GenPos_Y = (short)reader["GenPos_Y"];
                    t.GenPos_Z = (short)reader["GenPos_Z"];
                    t.GenAreaRadius = (short)reader["GenAreaRadius"];
                    t.CanBeResurrectPos = (byte)reader["CanBeResurrectPos"];
                    t.CanGotoResurrectPos = (byte)reader["CanGotoResurrectPos"];
                    list.Add(t);
                }
            }
        }

        public static void Load(this RefTeleportBuilding[] list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefTeleportBuilding ORDER BY ID"))
            {
                while (reader.Read())
                {
                    int dmp_id = (int)reader["ID"];
                    list[dmp_id] = new RefTeleportBuilding();
                    list[dmp_id].ID = dmp_id;
                    list[dmp_id].CodeName128 = (string)reader["CodeName128"];
                    list[dmp_id].ObjName128 = (string)reader["ObjName128"];
                    list[dmp_id].OrgObjCodeName128 = (string)reader["OrgObjCodeName128"];
                    list[dmp_id].NameStrID128 = (string)reader["NameStrID128"];
                    list[dmp_id].DescStrID128 = (string)reader["DescStrID128"];
                    list[dmp_id].Bionic = (byte)reader["Bionic"];
                    list[dmp_id].TypeID1 = (byte)reader["TypeID1"];
                    list[dmp_id].TypeID2 = (byte)reader["TypeID2"];
                    list[dmp_id].TypeID3 = (byte)reader["TypeID3"];
                    list[dmp_id].TypeID4 = (byte)reader["TypeID4"];
                    list[dmp_id].Country = (byte)reader["Country"];
                    list[dmp_id].Rarity = (byte)reader["Rarity"];
                    list[dmp_id].CanTrade = (byte)reader["CanTrade"];
                    list[dmp_id].CanSell = (byte)reader["CanSell"];
                    list[dmp_id].CanBuy = (byte)reader["CanBuy"];
                    list[dmp_id].CanBorrow = (byte)reader["CanBorrow"];
                    list[dmp_id].CanDrop = (byte)reader["CanDrop"];
                    list[dmp_id].CanPick = (byte)reader["CanPick"];
                    list[dmp_id].CanRepair = (byte)reader["CanRepair"];
                    list[dmp_id].CanRevive = (byte)reader["CanRevive"];
                    list[dmp_id].CanUse = (byte)reader["CanUse"];
                    list[dmp_id].CanThrow = (byte)reader["CanThrow"];
                    list[dmp_id].DecayTime = (int)reader["DecayTime"];
                    list[dmp_id].Price = (int)reader["Price"];
                    list[dmp_id].CostRepair = (int)reader["CostRepair"];
                    list[dmp_id].CostRevive = (int)reader["CostRevive"];
                    list[dmp_id].CostBorrow = (int)reader["CostBorrow"];
                    list[dmp_id].KeepingFee = (int)reader["KeepingFee"];
                    list[dmp_id].SellPrice = (int)reader["SellPrice"];
                    list[dmp_id].ReqLevelType1 = (int)reader["ReqLevelType1"];
                    list[dmp_id].ReqLevelType2 = (int)reader["ReqLevelType2"];
                    list[dmp_id].ReqLevel1 = (byte)reader["ReqLevel1"];
                    list[dmp_id].ReqLevel2 = (byte)reader["ReqLevel2"];
                    list[dmp_id].MaxContain = (int)reader["MaxContain"];
                    list[dmp_id].RegionID = (short)reader["RegionID"];
                    list[dmp_id].Dir = (short)reader["Dir"];
                    list[dmp_id].OffsetX = (short)reader["OffsetX"];
                    list[dmp_id].OffsetY = (short)reader["OffsetY"];
                    list[dmp_id].OffsetZ = (short)reader["OffsetZ"];
                    list[dmp_id].Speed1 = (short)reader["Speed1"];
                    list[dmp_id].Speed2 = (short)reader["Speed2"];
                    list[dmp_id].Scale = (int)reader["Scale"];
                    list[dmp_id].BCHeight = (short)reader["BCHeight"];
                    list[dmp_id].BCRadius = (short)reader["BCRadius"];
                    list[dmp_id].EventID = (int)reader["EventID"];
                    list[dmp_id].AssocFileObj128 = (string)reader["AssocFileObj128"];
                    list[dmp_id].AssocFileDrop128 = (string)reader["AssocFileDrop128"];
                    list[dmp_id].AssocFileIcon128 = (string)reader["AssocFileIcon128"];
                    list[dmp_id].AssocFile1_128 = (string)reader["AssocFile1_128"];
                    list[dmp_id].AssocFile2_128 = (string)reader["AssocFile2_128"];
                    list[dmp_id].TeleportData = Globals.Ref.TeleportData.FirstOrDefault(p => p != null && p.AssocRefObjID == dmp_id);
                }
            }
        }

        public static void Load(this List<RefTeleportLink> list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefTeleportLink"))
            {
                while (reader.Read())
                {
                    RefTeleportLink l = new RefTeleportLink();
                    l.OwnerTeleport = (int)reader["OwnerTeleport"];
                    l.TargetTeleport = (int)reader["TargetTeleport"];
                    l.Fee = (int)reader["Fee"];
                    l.RestrictBindMethod = (byte)reader["RestrictBindMethod"];
                    l.CheckResult = (byte)reader["CheckResult"];
                    for (byte b = 0; b < 5; b++)
                    {
                        l.Restrict[b] = new _teleport_restrict();
                        l.Restrict[b].Restrict = (int)reader[String.Format("Restrict{0}", b + 1)];
                        l.Restrict[b].Data1 = (int)reader[String.Format("Data{0}_1", b + 1)];
                        l.Restrict[b].Data2 = (int)reader[String.Format("Data{0}_2", b + 1)];
                    }
                    list.Add(l);
                }
            }
        }
    }
}
