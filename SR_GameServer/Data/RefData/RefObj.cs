namespace SR_GameServer.Data.RefData
{
    using System;
    using System.Collections.Generic;

    using SCommon;

    public abstract class RefObjCommon
    {
        #region Public Properties and Fields

        public int ID;
        public string CodeName128, ObjName128, OrgObjCodeName128, NameStrID128, DescStrID128;
        public byte CashItem, Bionic, TypeID1, TypeID2, TypeID3, TypeID4;
        public int DecayTime;
        public byte Country, Rarity, CanTrade, CanSell, CanBuy, CanBorrow, CanDrop, CanPick, CanRepair, CanRevive, CanUse, CanThrow;
        public int Price, CostRepair, CostRevive, CostBorrow, KeepingFee, SellPrice, ReqLevelType1, ReqLevelType2, ReqLevelType3, ReqLevelType4;
        public byte ReqLevel1, ReqLevel2, ReqLevel3, ReqLevel4;
        public int MaxContain;
        public short RegionID, Dir, OffsetX, OffsetY, OffsetZ, Speed1, Speed2;
        public int Scale;
        public short BCHeight, BCRadius;
        public int EventID;
        public string AssocFileObj128, AssocFileDrop128, AssocFileIcon128, AssocFile1_128, AssocFile2_128;
        public int Link;

        #endregion
    }

    public class RefObjChar : RefObjCommon
    {
        #region Public Properties and Fields

        public byte Lvl, CharGender;
        public int MaxHP, MaxMP, ResistFrozen, ResistFrostbite, ResistBurn, ResistEShock, ResistPoison, ResistZombie, ResistSleep, ResistRoot, ResistSlow, ResistFear, ResistMyopia, ResistBlood, ResistStone, ResistDark, ResistStun, ResistDisea, ResistChaos, ResistCsePD, ResistCseMD, ResistCseSTR, ResistCseINT, ResistCseHP, ResistCseMP, Resist24, ResistBomb, Resist26, Resist27, Resist28, Resist29, Resist30, Resist31, Resist32;
        public byte InventorySize, CanStore_TID1, CanStore_TID2, CanStore_TID3, CanStore_TID4, CanBeVehicle, CanControl, DamagePortion;
        public short MaxPassenger;
        public int AssocTactics, PD, MD, PAR, MAR, ER, BR, HR, CHR, ExpToGive, CreepType;
        public byte Knockdown;
        public int[] DefaultSkill;
        public byte TextureType;
        public int[] Except;
        public int KO_RecoverTime, c_ID;
        public List<_drop_item> Drops;

        #endregion

        #region Constructors & Destructors

        public RefObjChar()
        {
            DefaultSkill = new int[10];
            Except = new int[10];
            Drops = new List<_drop_item>();
        }

        #endregion

        public struct _drop_item
        {
            public float ItemID, Prob;
            public _drop_item(int iid, float prb)
            {
                ItemID = iid;
                Prob = prb;
            }
        }
    }

    public class RefObjItem : RefObjCommon
    {
        #region Public Properties and Fields

        public int MaxStack;
        public byte ReqGender;
        public int ReqStr, ReqInt;
        public byte ItemClass;
        public int SetID;
        public double Dur_L, Dur_U, PD_L, PD_U, PDInc, ER_L, ER_U, ERInc, PAR_L, PAR_U, PARInc, BR_L, BR_U, MD_L, MD_U, MDInc, MAR_L, MAR_U, MARInc, PDStr_L, PDStr_U, MDInt_L, MDInt_U;
        public byte Quivered, Ammo1_TID4, Ammo2_TID4, Ammo3_TID4, Ammo4_TID4, Ammo5_TID4, SpeedClass, TwoHanded;
        public short Range;
        public double PAttackMin_L, PAttackMin_U, PAttackMax_L, PAttackMax_U, PAttackInc, MAttackMin_L, MAttackMin_U, MAttackMax_L, MAttackMax_U, MAttackInc, PAStrMin_L, PAStrMin_U, PAStrMax_L, PAStrMax_U, MAInt_Min_L, MAInt_Min_U, MAInt_Max_L, MAInt_Max_U, HR_L, HR_U, HRInc, CHR_L, CHR_U;
        public byte MaxMagicOptCount, ChildItemCount;
        public int i_ID;
        public byte Slot;
        public JobType JobClass;
        public _itemParam[] Params;
        public ItemType Type;

        #endregion

        #region Constructors & Destructors

        public RefObjItem()
        {
            Params = new _itemParam[6];
        }

        #endregion

        public struct _itemParam
        {
            public int Param;
            public string Desc;
        }
    }

    public static class RefObjExtensions
    {
        public static void Load(this RefObjChar[] list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefObjChar WHERE Service = 1 ORDER BY ID"))
            {
                while (reader.Read())
                {
                    int dmp_id = (int)reader["ID"];
                    list[dmp_id] = new RefObjChar();
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
                    list[dmp_id].AssocFile2_128 = (string)reader["AssocFile2_128"]; //last refobjcommon field
                    list[dmp_id].Lvl = (byte)reader["Lvl"]; //first refobjchar field
                    list[dmp_id].CharGender = (byte)reader["CharGender"];
                    list[dmp_id].MaxHP = (int)reader["MaxHP"];
                    list[dmp_id].MaxMP = (int)reader["MaxMP"];
                    list[dmp_id].ResistFrostbite = (int)reader["ResistFrostbite"];
                    list[dmp_id].ResistBurn = (int)reader["ResistBurn"];
                    list[dmp_id].ResistEShock = (int)reader["ResistEShock"];
                    list[dmp_id].ResistPoison = (int)reader["ResistPoison"];
                    list[dmp_id].ResistZombie = (int)reader["ResistZombie"];
                    list[dmp_id].InventorySize = (byte)reader["InventorySize"];
                    list[dmp_id].CanStore_TID1 = (byte)reader["CanStore_TID1"];
                    list[dmp_id].CanStore_TID2 = (byte)reader["CanStore_TID2"];
                    list[dmp_id].CanStore_TID3 = (byte)reader["CanStore_TID3"];
                    list[dmp_id].CanStore_TID4 = (byte)reader["CanStore_TID4"];
                    list[dmp_id].CanBeVehicle = (byte)reader["CanBeVehicle"];
                    list[dmp_id].CanControl = (byte)reader["CanControl"];
                    list[dmp_id].DamagePortion = (byte)reader["DamagePortion"];
                    list[dmp_id].MaxPassenger = (short)reader["MaxPassenger"];
                    list[dmp_id].AssocTactics = (int)reader["AssocTactics"];
                    list[dmp_id].PD = (int)reader["PD"];
                    list[dmp_id].MD = (int)reader["MD"];
                    list[dmp_id].PAR = (int)reader["PAR"];
                    list[dmp_id].MAR = (int)reader["MAR"];
                    list[dmp_id].ER = (int)reader["ER"];
                    list[dmp_id].BR = (int)reader["BR"];
                    list[dmp_id].HR = (int)reader["HR"];
                    list[dmp_id].CHR = (int)reader["CHR"];
                    list[dmp_id].ExpToGive = (int)reader["ExpToGive"];
                    list[dmp_id].CreepType = (int)reader["CreepType"];
                    list[dmp_id].Knockdown = (byte)reader["Knockdown"];
                    list[dmp_id].KO_RecoverTime = (int)reader["KO_RecoverTime"];
                    list[dmp_id].DefaultSkill[0] = (int)reader["DefaultSkill_1"];
                    list[dmp_id].DefaultSkill[1] = (int)reader["DefaultSkill_2"];
                    list[dmp_id].DefaultSkill[2] = (int)reader["DefaultSkill_3"];
                    list[dmp_id].DefaultSkill[3] = (int)reader["DefaultSkill_4"];
                    list[dmp_id].DefaultSkill[4] = (int)reader["DefaultSkill_5"];
                    list[dmp_id].DefaultSkill[5] = (int)reader["DefaultSkill_6"];
                    list[dmp_id].DefaultSkill[6] = (int)reader["DefaultSkill_7"];
                    list[dmp_id].DefaultSkill[7] = (int)reader["DefaultSkill_8"];
                    list[dmp_id].DefaultSkill[8] = (int)reader["DefaultSkill_9"];
                    list[dmp_id].DefaultSkill[9] = (int)reader["DefaultSkill_10"];
                    list[dmp_id].TextureType = (byte)reader["TextureType"];
                    list[dmp_id].Except[0] = (int)reader["Except_1"];
                    list[dmp_id].Except[1] = (int)reader["Except_2"];
                    list[dmp_id].Except[2] = (int)reader["Except_3"];
                    list[dmp_id].Except[3] = (int)reader["Except_4"];
                    list[dmp_id].Except[4] = (int)reader["Except_5"];
                    list[dmp_id].Except[5] = (int)reader["Except_6"];
                    list[dmp_id].Except[6] = (int)reader["Except_7"];
                    list[dmp_id].Except[7] = (int)reader["Except_8"];
                    list[dmp_id].Except[8] = (int)reader["Except_9"];
                    list[dmp_id].Except[9] = (int)reader["Except_10"];

                    for (byte b = 1; b <= 5; b++)
                    {
                        int itemid = (int)reader["AssignedItem" + b.ToString()];
                        float prob = (float)reader["DropProb" + b.ToString()];
                        if (itemid != 0 && Globals.Ref.ObjItem[itemid] != null)
                        {
                            Logging.Log()("local overlap job exception!!", LogLevel.Error);
                            Console.ReadLine();
                            Environment.Exit(0);
                        }
                        if (prob != 0 && itemid != 0) list[dmp_id].Drops.Add(new RefObjChar._drop_item(itemid, prob));
                    }
                }
            }
        }

        public static void Load(this RefObjItem[] list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefObjItem WHERE Service = 1 ORDER BY ID"))
            {
                while (reader.Read())
                {
                    int dmp_id = (int)reader["ID"];
                    list[dmp_id] = new RefObjItem();
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
                    list[dmp_id].AssocFile2_128 = (string)reader["AssocFile2_128"]; //last refobjcommon field 
                    list[dmp_id].MaxStack = (int)reader["MaxStack"]; //first refobjitem field
                    list[dmp_id].ReqGender = (byte)reader["ReqGender"];
                    list[dmp_id].ReqStr = (int)reader["ReqStr"];
                    list[dmp_id].ReqInt = (int)reader["ReqInt"];
                    list[dmp_id].ItemClass = (byte)reader["ItemClass"];
                    list[dmp_id].SetID = (int)reader["SetID"];
                    list[dmp_id].Dur_L = (float)reader["Dur_L"];
                    list[dmp_id].Dur_U = (float)reader["Dur_U"];
                    list[dmp_id].PD_L = (float)reader["PD_L"];
                    list[dmp_id].PD_U = (float)reader["PD_U"];
                    list[dmp_id].PDInc = (float)reader["PDInc"];
                    list[dmp_id].ER_L = (float)reader["ER_L"];
                    list[dmp_id].ER_U = (float)reader["ER_U"];
                    list[dmp_id].ERInc = (float)reader["ERInc"];
                    list[dmp_id].PAR_L = (float)reader["PAR_L"];
                    list[dmp_id].PAR_U = (float)reader["PAR_U"];
                    list[dmp_id].PARInc = (float)reader["PARInc"];
                    list[dmp_id].BR_L = (float)reader["BR_L"];
                    list[dmp_id].BR_U = (float)reader["BR_U"];
                    list[dmp_id].MD_L = (float)reader["MD_L"];
                    list[dmp_id].MD_U = (float)reader["MD_U"];
                    list[dmp_id].MDInc = (float)reader["MDInc"];
                    list[dmp_id].MAR_L = (float)reader["MAR_L"];
                    list[dmp_id].MAR_U = (float)reader["MAR_U"];
                    list[dmp_id].MARInc = (float)reader["MARInc"];
                    list[dmp_id].Quivered = (byte)reader["Quivered"];
                    list[dmp_id].Ammo1_TID4 = (byte)reader["Ammo1_TID4"];
                    list[dmp_id].Ammo2_TID4 = (byte)reader["Ammo2_TID4"];
                    list[dmp_id].Ammo3_TID4 = (byte)reader["Ammo3_TID4"];
                    list[dmp_id].Ammo4_TID4 = (byte)reader["Ammo4_TID4"];
                    list[dmp_id].Ammo5_TID4 = (byte)reader["Ammo5_TID4"];
                    list[dmp_id].SpeedClass = (byte)reader["SpeedClass"];
                    list[dmp_id].TwoHanded = (byte)reader["TwoHanded"];
                    list[dmp_id].Range = (short)reader["Range"];
                    list[dmp_id].PAttackMin_L = (float)reader["PAttackMin_L"];
                    list[dmp_id].PAttackMin_U = (float)reader["PAttackMin_U"];
                    list[dmp_id].PAttackMax_L = (float)reader["PAttackMax_L"];
                    list[dmp_id].PAttackMax_U = (float)reader["PAttackMax_U"];
                    list[dmp_id].PAttackInc = (float)reader["PAttackInc"];
                    list[dmp_id].MAttackMin_L = (float)reader["MAttackMin_L"];
                    list[dmp_id].MAttackMin_U = (float)reader["MAttackMin_U"];
                    list[dmp_id].MAttackMax_L = (float)reader["MAttackMax_L"];
                    list[dmp_id].MAttackMax_U = (float)reader["MAttackMax_U"];
                    list[dmp_id].MAttackInc = (float)reader["MAttackInc"];
                    list[dmp_id].HR_L = (float)reader["HR_L"];
                    list[dmp_id].HR_U = (float)reader["HR_U"];
                    list[dmp_id].HRInc = (float)reader["HRInc"];
                    list[dmp_id].CHR_L = (float)reader["CHR_L"];
                    list[dmp_id].CHR_U = (float)reader["CHR_U"];

                    for (byte i = 1; i <= 6; i++)
                    {
                        list[dmp_id].Params[i - 1] = new RefObjItem._itemParam();
                        list[dmp_id].Params[i - 1].Param = (int)reader[String.Format("Param{0}", i)];
                        list[dmp_id].Params[i - 1].Desc = (string)reader[String.Format("Desc{0}_128", i)];
                    }

                    try
                    {
                        list[dmp_id].Type = (ItemType)Globals.Ref.FmnTidGroupMap.Find(
                            p => p != null &&
                            p.TID1 == list[dmp_id].TypeID1 &&
                            p.TID2 == list[dmp_id].TypeID2 &&
                            p.TID3 == list[dmp_id].TypeID3 &&
                            p.TID4 == list[dmp_id].TypeID4).TidGroup;
                    }
                    catch
                    {
                        list[dmp_id].Type = ItemType.None;
                    }

                    if (list[dmp_id].CodeName128.Contains("_FRPVP_")) list[dmp_id].Type = Data.ItemType.COSTUME_FRPVP;

                    if (list[dmp_id].Type == ItemType.COSTUME_TRIANGLE)
                    {
                        if (list[dmp_id].CodeName128.Contains("_THIEF_")) list[dmp_id].JobClass = JobType.Thief;
                        if (list[dmp_id].CodeName128.Contains("_TRADER_")) list[dmp_id].JobClass = JobType.Trader;
                        if (list[dmp_id].CodeName128.Contains("_HUNTER_")) list[dmp_id].JobClass = JobType.Hunter;
                    }

                    switch (list[dmp_id].Type)
                    {
                        case ItemType.CH_ARMOR_CLOTHES:
                        case ItemType.CH_ARMOR_LIGHT:
                        case ItemType.CH_ARMOR_HEAVY:
                            switch (list[dmp_id].TypeID4)
                            {
                                case 1:
                                    list[dmp_id].Slot = 0;
                                    break;
                                case 2:
                                    list[dmp_id].Slot = 2;
                                    break;
                                case 3:
                                    list[dmp_id].Slot = 1;
                                    break;
                                case 4:
                                    list[dmp_id].Slot = 4;
                                    break;
                                case 5:
                                    list[dmp_id].Slot = 3;
                                    break;
                                case 6:
                                    list[dmp_id].Slot = 5;
                                    break;
                            }
                            break;
                        case ItemType.CH_ACCESSORY_EARRING:
                            list[dmp_id].Slot = 9;
                            break;
                        case Data.ItemType.CH_ACCESSORY_RING:
                            list[dmp_id].Slot = 11;
                            break;
                        case Data.ItemType.CH_ACCESSORY_NECKLACE:
                            list[dmp_id].Slot = 10;
                            break;
                        case ItemType.CH_WEAPON_BLADE:
                        case ItemType.CH_WEAPON_BOW:
                        case ItemType.CH_WEAPON_SPEAR:
                        case ItemType.CH_WEAPON_SWORD:
                        case ItemType.CH_WEAPON_TBLADE:
                            list[dmp_id].Slot = 6;
                            break;
                        case ItemType.CH_WEAPON_SHIELD:
                        case ItemType.ARTICLES_ARROW:
                            list[dmp_id].Slot = 7;
                            break;
                        default:
                            list[dmp_id].Slot = 255;
                            break;
                    }
                }
            }
        }
    }
}
