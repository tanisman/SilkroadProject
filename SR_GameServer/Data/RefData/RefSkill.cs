using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_GameServer.Data.RefData
{
    public class RefSkill
    {
        #region Public Properties and Fields

        public int ID, GroupID;
        public string Basic_Code, Basic_Name, Basic_Group;
        public int Basic_Original;
        public byte Basic_Level, Basic_Activity;
        public int Basic_ChainCode, Basic_RecycleCost, Action_PreparingTime, Action_CastingTime, Action_ActionDuration, Action_ReuseDelay, Action_CoolTime, Action_FlyingSpeed;
        public byte Action_Interruptable;
        public int Action_Overlap;
        public byte Action_AutoAttackType, Action_InTown;
        public short Action_Range;
        public byte Target_Required, TargetType_Animal, TargetType_Land, TargetType_Building, TargetGroup_Self, TargetGroup_Ally, TargetGroup_Party, TargetGroup_Enemy_M, TargetGroup_Enemy_P, TargetGroup_Neutral, TargetGroup_DontCare, TargetEtc_SelectDeadBody, TargetGroup_Enemy;
        public int ReqCommon_Mastery1, ReqCommon_Mastery2;
        public byte ReqCommon_MasteryLevel1, ReqCommon_MasteryLevel2;
        public short ReqCommon_Str, ReqCommon_Int;
        public int ReqLearn_Skill1, ReqLearn_Skill2, ReqLearn_Skill3;
        public byte ReqLearn_SkillLevel1, ReqLearn_SkillLevel2, ReqLearn_SkillLevel3;
        public short ReqLearn_SP;
        public byte ReqLearn_Race, Req_Restriction1, Req_Restriction2;
        public byte ReqCast_Weapon1, ReqCast_Weapon2;
        public short Consume_HP, Consume_MP, Consume_HPRatio, Consume_MPRatio;
        public byte Consume_WHAN, UI_SkillTab, UI_SkillPage, UI_SkillColumn, UI_SkillRow;
        public string UI_IconFile, UI_SkillName, UI_SkillToolTip, UI_SkillToolTip_Desc, UI_SkillStudy_Desc;
        public short AI_AttackChance;
        public byte AI_SkillType;
        public int Duration, NumAttack = 1;
        public Dictionary<string, _param_item> Params;

        #endregion

        #region Constructors and Destructors

        public RefSkill()
        {
            Params = new Dictionary<string, _param_item>();
        }

        #endregion

        public struct _param_item
        {
            public string Name;
            public int[] Value;
        }
    }

    public static class RefSkillExtensions
    {
        public static void Load(this RefSkill[] list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefSkill WHERE Service = 1 ORDER BY ID"))
            {
                while (reader.Read())
                {
                    int dmp_id = (int)reader["ID"];
                    list[dmp_id] = new RefSkill();
                    list[dmp_id].ID = dmp_id;
                    list[dmp_id].GroupID = (int)reader["GroupID"];
                    list[dmp_id].Basic_Code = (string)reader["Basic_Code"];
                    list[dmp_id].Basic_Name = (string)reader["Basic_Name"];
                    list[dmp_id].Basic_Group = (string)reader["Basic_Group"];
                    list[dmp_id].Basic_Original = (int)reader["Basic_Original"];
                    list[dmp_id].Basic_Level = (byte)reader["Basic_Level"];
                    list[dmp_id].Basic_Activity = (byte)reader["Basic_Activity"];
                    list[dmp_id].Basic_ChainCode = (int)reader["Basic_ChainCode"];
                    list[dmp_id].Basic_RecycleCost = (int)reader["Basic_RecycleCost"];
                    list[dmp_id].Action_PreparingTime = (int)reader["Action_PreparingTime"];
                    list[dmp_id].Action_CastingTime = (int)reader["Action_CastingTime"];
                    list[dmp_id].Action_ActionDuration = (int)reader["Action_ActionDuration"];
                    list[dmp_id].Action_ReuseDelay = (int)reader["Action_ReuseDelay"];
                    list[dmp_id].Action_CoolTime = (int)reader["Action_CoolTime"];
                    list[dmp_id].Action_FlyingSpeed = (int)reader["Action_FlyingSpeed"];
                    list[dmp_id].Action_Interruptable = (byte)reader["Action_Interruptable"];
                    list[dmp_id].Action_Overlap = (int)reader["Action_Overlap"];
                    list[dmp_id].Action_AutoAttackType = (byte)reader["Action_AutoAttackType"];
                    list[dmp_id].Action_InTown = (byte)reader["Action_InTown"];
                    list[dmp_id].Action_Range = (short)reader["Action_Range"];
                    list[dmp_id].Target_Required = (byte)reader["Target_Required"];
                    list[dmp_id].TargetType_Animal = (byte)reader["TargetType_Animal"];
                    list[dmp_id].TargetType_Land = (byte)reader["TargetType_Land"];
                    list[dmp_id].TargetType_Building = (byte)reader["TargetType_Building"];
                    list[dmp_id].TargetGroup_Self = (byte)reader["TargetGroup_Self"];
                    list[dmp_id].TargetGroup_Ally = (byte)reader["TargetGroup_Ally"];
                    list[dmp_id].TargetGroup_Enemy = (byte)reader["TargetGroup_Enemy"];
                    list[dmp_id].TargetGroup_Neutral = (byte)reader["TargetGroup_Neutral"];
                    list[dmp_id].TargetGroup_DontCare = (byte)reader["TargetGroup_DontCare"];
                    list[dmp_id].TargetEtc_SelectDeadBody = (byte)reader["TargetEtc_SelectDeadBody"];
                    list[dmp_id].ReqCommon_Mastery1 = (int)reader["ReqCommon_Mastery1"];
                    list[dmp_id].ReqCommon_Mastery2 = (int)reader["ReqCommon_Mastery2"];
                    list[dmp_id].ReqCommon_MasteryLevel1 = (byte)reader["ReqCommon_MasteryLevel1"];
                    list[dmp_id].ReqCommon_MasteryLevel2 = (byte)reader["ReqCommon_MasteryLevel2"];
                    list[dmp_id].ReqCommon_Str = (short)reader["ReqCommon_Str"];
                    list[dmp_id].ReqCommon_Int = (short)reader["ReqCommon_Int"];
                    list[dmp_id].ReqLearn_Skill1 = (int)reader["ReqLearn_Skill1"];
                    list[dmp_id].ReqLearn_Skill2 = (int)reader["ReqLearn_Skill2"];
                    list[dmp_id].ReqLearn_Skill3 = (int)reader["ReqLearn_Skill3"];
                    list[dmp_id].ReqLearn_SkillLevel1 = (byte)reader["ReqLearn_SkillLevel1"];
                    list[dmp_id].ReqLearn_SkillLevel2 = (byte)reader["ReqLearn_SkillLevel2"];
                    list[dmp_id].ReqLearn_SkillLevel3 = (byte)reader["ReqLearn_SkillLevel3"];
                    list[dmp_id].ReqLearn_SP = (short)reader["ReqLearn_SP"];
                    list[dmp_id].ReqLearn_Race = (byte)reader["ReqLearn_Race"];
                    list[dmp_id].Req_Restriction1 = (byte)reader["Req_Restriction1"];
                    list[dmp_id].Req_Restriction2 = (byte)reader["Req_Restriction2"];
                    list[dmp_id].ReqCast_Weapon1 = (byte)reader["ReqCast_Weapon1"];
                    list[dmp_id].ReqCast_Weapon2 = (byte)reader["ReqCast_Weapon2"];
                    list[dmp_id].Consume_HP = (short)reader["Consume_HP"];
                    list[dmp_id].Consume_MP = (short)reader["Consume_MP"];
                    list[dmp_id].Consume_HPRatio = (short)reader["Consume_HPRatio"];
                    list[dmp_id].Consume_MPRatio = (short)reader["Consume_MPRatio"];
                    list[dmp_id].Consume_WHAN = (byte)reader["Consume_WHAN"];
                    list[dmp_id].UI_SkillTab = (byte)reader["UI_SkillTab"];
                    list[dmp_id].UI_SkillPage = (byte)reader["UI_SkillPage"];
                    list[dmp_id].UI_SkillColumn = (byte)reader["UI_SkillColumn"];
                    list[dmp_id].UI_SkillRow = (byte)reader["UI_SkillRow"];
                    list[dmp_id].UI_IconFile = (string)reader["UI_IconFile"];
                    list[dmp_id].UI_SkillName = (string)reader["UI_SkillName"];
                    list[dmp_id].UI_SkillToolTip = (string)reader["UI_SkillToolTip"];
                    list[dmp_id].UI_SkillToolTip_Desc = (string)reader["UI_SkillToolTip_Desc"];
                    list[dmp_id].UI_SkillStudy_Desc = (string)reader["UI_SkillStudy_Desc"];
                    list[dmp_id].AI_AttackChance = (short)reader["AI_AttackChance"];

                    int pIndex = 2;
                    string paramdmp_name = "";
                    int sparam = 0;
                    int[] pvalue = null;
                    try
                    {
                        while ((sparam = (int)reader["Param" + pIndex.ToString()]) != 0)
                        {
                            paramdmp_name = GetSkillParamStr(sparam); pIndex++;

                            byte vCount = ParamValueCount(paramdmp_name);
                            if (!paramdmp_name.ToCharArray().ToList().Exists(p => (p >= 48 && p <= 57) || (p >= 65 && p <= 90) || (p >= 97 && p <= 122)) || paramdmp_name.Trim().Length <= 1)
                            {
                                pIndex -= 2;
                                continue;
                            }
                            if (vCount == 255)
                            {
                                Console.WriteLine("invalid param {0} ({1})", sparam, paramdmp_name);
                                return;
                            }
                            if (paramdmp_name == "setv")
                            {
                                paramdmp_name = GetSkillParamStr((int)reader["Param" + pIndex.ToString()]); pIndex++;
                            }
                            if (paramdmp_name == "ssou")
                            {
                                while ((int)reader["Param" + pIndex.ToString()] != 0)
                                {
                                    pvalue = new int[4];
                                    pvalue[0] = (Convert.ToInt32(reader["Param" + pIndex.ToString()])); pIndex++;
                                    pvalue[1] = (Convert.ToInt32(reader["Param" + pIndex.ToString()])); pIndex++;
                                    pvalue[2] = (Convert.ToInt32(reader["Param" + pIndex.ToString()])); pIndex++;
                                    pvalue[3] = (Convert.ToInt32(reader["Param" + pIndex.ToString()])); pIndex++;
                                }
                            }
                            else if (paramdmp_name == "reqi")
                            {
                                pvalue = new int[2];
                                pvalue[0] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //wep type1
                                pvalue[1] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //wep type2
                            }
                            else if (paramdmp_name == "efr")
                            {
                                pvalue = new int[4];
                                pvalue[0] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //simult attack type
                                pvalue[1] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //effect range
                                pvalue[2] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //sekme (simultattack)
                                pvalue[3] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //+distance
                            }
                            else if (paramdmp_name == "att")
                            {
                                pvalue = new int[5];
                                pvalue[0] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //type => if(type == 10) { mag } else { phy }
                                pvalue[1] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //skill %
                                pvalue[2] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //min atk
                                pvalue[3] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //max atk
                                pvalue[4] = Convert.ToInt32(reader["Param" + pIndex.ToString()]); pIndex++; //?
                            }
                            else
                            {
                                if (vCount != 0)
                                    pvalue = new int[vCount];

                                for (byte i = 0; i < vCount; i++)
                                    pvalue[i] = (Convert.ToInt32(reader["Param" + pIndex.ToString()]));

                                pIndex += vCount;
                            }
                            if (paramdmp_name == "dura")
                                list[dmp_id].Duration = pvalue[0];
                            else if (paramdmp_name == "mc")
                                list[dmp_id].NumAttack = pvalue[1];
                            else
                            {
                                if (paramdmp_name != "getv")
                                {
                                    list[dmp_id].Params.Add(paramdmp_name,
                                        new RefSkill._param_item
                                        {
                                            Name = paramdmp_name,
                                            Value = pvalue
                                        });
                                }
                            }
                        }

                       /* if (list[dmp_id].Basic_Code.Contains("SKILL_CH_SWORD_GEOMGI_"))
                        {
                            pvalue = new int[4];
                            pvalue[0] = 0;
                            pvalue[1] = 0;
                            pvalue[2] = 0;
                            pvalue[3] = 50;
                            list[dmp_id].Params.Add(paramdmp_name,
                                new RefSkill._param_item
                                {
                                    Name = "efr",
                                    Value = pvalue
                                });
                        }*/
                        /*
                        if (list[dmp_id].UI_SkillTab != 255)
                            list_g[list[dmp_id].GroupID, list[dmp_id].Basic_Level] = list[dmp_id];
                        */
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine("local overlap job exception!! : {1} ({0})", GetSkillParamStr(sparam), sparam);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                }
            }
        }

        public static string GetSkillParamStr(int intparam)
        {
            return Encoding.ASCII.GetString(BitConverter.GetBytes(intparam).Reverse().ToList().FindAll(p => p != 0).ToArray());
        }

        public static byte ParamValueCount(string param)
        {
            switch (param)
            {
                case "setv":
                    return 2;
                case "getv":
                case "MAAT":
                // warrior
                case "E2AH":
                case "E2AA":
                case "E1SA":
                case "E2SA":
                // rogue
                case "CBAT":
                case "CBRA":
                case "DGAT":
                case "DGHR":
                case "DGAA":
                case "STDU":
                case "STSP":
                case "RPDU":
                case "RPTU":
                case "RPBU":
                // wizard
                case "WIMD":
                case "WIRU":
                case "EAAT":
                case "COAT":
                case "FIAT":
                case "LIAT":
                // warlock
                case "DTAT":
                case "DTDR":
                case "BLAT":
                case "TRAA":
                case "BSHP":
                case "SAAA":
                // bard
                case "MUAT":
                case "BDMD":
                case "MUER":
                case "MUCR":
                case "DSER":
                case "DSCR":
                // cleric
                case "HLAT":
                case "HLRU":
                case "HLMD":
                case "HLFS":
                case "HLMI":
                case "HLBP":
                case "HLSM":
                // attribute only - no effect
                case "nmh": // Healing stone (socket stone)
                case "nmf": // Movement stone (socket stone)
                case "eshp":
                case "reqn":
                case "fitp":
                case "ao":   // fortress ??
                case "rpkt": // fortress repair kit
                case "hitm": // Taunt the enemy into attacking only you
                case "efta": // bard tambour
                case "lks2": // warlock damage buff
                case "hntp": // tag point
                case "trap": // csapda??
                case "cbuf": // itembuff
                case "nbuf": // ??(ticketnél volt) nem látszika  buff másnak?
                case "bbuf": //debuff
                case null:
                    return 0;
                case "tant":
                case "rcur": // randomly cure number of bad statuses
                case "ck":
                case "ovl2":
                case "mwhs":
                case "scls":
                case "mwmh":
                case "mwhh":
                case "rmut":
                case "abnb":
                case "mscc":
                case "bcnt": // cos bag count [slot]
                case "chpi": // cos hp increase [%]
                case "chst": // cos speed increase [%]
                case "csum": // cos summon [coslevel]
                case "jobs": // job skill [charlevel]
                case "hwit": // ITEM_ETC_SOCKET_STONE_HWAN ?? duno what is it
                case "spi": // Gives additional skill points when hunting monsters. [%inc]
                case "dura": // skill duration
                case "msid": // mod def ignore prob%
                case "hwir": // honor buff new zerk %
                case "hst3": // honor buff speed %inc
                case "hst2": // rogue haste speed %inc
                case "lkdd": // Share % damage taken from a selected person. (link damage)
                case "gdr":  // gold drop rate %inc
                case "chcr": // target loses % HP
                case "cmcr": // target loses % MP
                case "dcmp": // MP Cost % Decrease
                case "mwdt": // Weapon Magical Attack Power %Reflect
                case "pdmg": // Absolute Damage
                case "lfst": // life steal Absorb HP
                case "puls": // pulsing skill frequenzy
                case "pwtt": // Weapon Physical Attack Power reflect.
                case "pdm2": // ghost killer
                case "luck": // lucky %inc
                case "alcu": // alchemy lucky %inc
                case "terd": // parry reduce
                case "thrd": // Attack ratio reduce
                case "ru": // range incrase
                case "hste": // speed %inc
                case "da": // downattack %inc
                case "reqc": // required status?
                case "dgmp": // damage mana absorb
                case "dcri": // critical parry inc
                    return 1;
                case "mc": //num attack
                case "atca":
                case "reat":
                case "defr":
                case "msr": // Triggered at a certain chance, the next spell cast does not cost MP. [%chance to trigger|%mana reduce]
                case "kb": // knockback
                case "ko":  // knockdown
                case "zb": // zombie
                case "fz":  // frozen
                case "fb":  // frostbite
                case "spda": // Shield physical defence % reduce. Physical attack power increase.
                case "expi": // sp|exp %inc PET?
                case "stri": // str increase
                case "inti": // int increase
                case "rhru": // Increase the amount of HP healed. %
                case "dmgt": // Absorption %? 
                case "dtnt": // Aggro Reduce
                case "mcap": // monster mask lvl cap
                case "apau": // attack power inc [phy|mag]
                case "lkag": // Share aggro
                case "dttp": // detect stealth [|maxstealvl]
                case "tnt2": // taunt inc | aggro %increase
                case "expu": // exp|sp %inc
                case "msch": // monster transform
                case "dtt": // detect invis [ | maxinvilvl]
                case "hpi": // hp incrase [inc|%inc]
                case "mpi": // mp incrase [inc|%inc]
                case "odar": // damage absorbation
                case "resu": // resurrection [lvl|expback%]
                case "er": // evasion | parry %inc 
                case "hr": // hit rating inc | attack rating inc 
                case "tele": // light teleport [sec|meter*10]
                case "tel2": // wizard teleport [sec|meter*10]
                case "tel3": // warrior sprint teleport [sec|m]
                case "onff": // mana consume per second
                case "br":  // blocking ratio [|%inc]
                case "cr":  // critical inc
                case "dru": // damage %inc [phy|mag]
                case "irgc": // reincarnation [hp|mp]
                case "pola": // Preemptive attack prevention [hp|mp]
                case "curt": // negative status effect reduce target player
                case "dar": //shield & spear buff w/o move (old client)
                case "ri": //?
                case "st": // stun
                    return 2;
                case "curl": //anti effect: cure [|effect cure amount|effect level]
                case "real":
                case "skc":
                case "bldr": // Reflects damage upon successful block.
                case "ca": // confusion
                case "rt":  // restraint (wizard) << restraint i guess it should restrain the target or put to ground as in same spot like chains on feet :) 
                case "fe": // fear 
                case "sl": // dull
                case "se": // sleep
                case "es":  // lightening
                case "bu":  // burn
                case "ps":  // poison
                case "lkdh": // link Damage % MP Absorption (Mana Switch)
                case "stns": // Petrified status
                case "hide": // stealth hide
                case "lkdr": // Share damage
                case "defp": // defensepower inc [phy|mag]
                case "bgra": // negative status effect reduce
                case "cnsm": // consume item
                    return 3;
                case "csit": // division
                case "tb": // hidden
                case "my": // short sight
                case "ds": // disease
                case "csmd":  // weaken
                case "cspd":  // decay
                case "cssr": // impotent
                case "dn": // darkness
                case "mom": // duration | Berserk mode Attack damage/Defence/Hit rate/Parry rate will increase % | on every X mins
                case "pmdp": // maximum physical defence strength decrease %
                case "pmhp": // hp reduce
                case "dmgr": // damage return [prob%|return%||]
                case "lnks": // Connection between players
                case "pmdg": // damage reduce [dura|phy%|mag%|?]
                case "qest": // some quest related skill?
                case "heal": // heal [hp|hp%|mp|mp%]
                case "pw": // player wall
                case "summ": // summon bird
                    return 4;
                case "bl": // bleed
                    return 5;
                case "cshp": // panic
                case "csmp": // combustion
                    return 6;
                case "reqi": // required item
                    return 2;
                case "att": // if attack skill
                    return 5;
                case "efr":
                    return 6;
                case "nst":
                case "cura": //water cure [effect]
                    return 1;
                case "abno":
                    return 2;
                case "apru": //attack inc [phy%|mag%]
                case "tgzn": //water harmony [hpinc%]
                    return 2;
                case "tctc":
                    return 0;
                default:
                    return 255;
            }
        }

    }
}
