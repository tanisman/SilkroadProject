namespace SR_GameServer
{
    using System;

    using SCommon;

    using SharpDX;

    public class GObjMob : GObj
    {
        #region Public Properties and Fields

        public GObjMobRarity m_rarity;
        public int m_nestId;
        public short m_generatedRegion;
        public Vector3 m_generatedPosition;

        #region Stats

        public volatile int m_bonusPhyDefPercent;
        public volatile int m_bonusMagDefPercent;
        public volatile int m_bonusPhyAtkPercent;
        public volatile int m_bonusMagAtkPercent;
        public volatile int m_bonusParryPercent;
        public volatile int m_bonusHitPercent;
        public volatile int m_bonusMaxHealthPercent;
        public volatile int m_bonusBlockRatioPercent;
        public volatile int m_bonusPhyDef;
        public volatile int m_bonusMagDef;
        public volatile int m_bonusPhyAtk;
        public volatile int m_bonusMagAtk;
        public volatile int m_bonusParryRate;
        public volatile int m_bonusHitRate;
        public volatile int m_bonusMaxHealth;
        public volatile int m_bonusBlockRatio;
        public volatile int m_currentHealthPoints;
        //absorb values doesnt effect def values but absorbs incoming raw dmg by %
        public volatile int m_phyAbsorbPercent;
        public volatile int m_magAbsorbPercent;
        //balance values doesnt effect atk values but modifies outgoing raw dmg by %
        public volatile int m_phyBalancePercent;
        public volatile int m_magBalancePercent;

        public float TotalPhyDef => (Data.Globals.Ref.ObjChar[this.m_model].PD * StatsMultipler + this.m_bonusPhyDef) * (1.0f + this.m_bonusPhyDefPercent / 100f);
        public float TotalMagDef => (Data.Globals.Ref.ObjChar[this.m_model].MD * StatsMultipler + this.m_bonusMagDef) * (1.0f + this.m_bonusMagDefPercent / 100f);
        public float TotalPhyAtk => (Data.Globals.Ref.ObjChar[this.m_model].PAR * StatsMultipler + this.m_bonusPhyAtk) * (1.0f + this.m_bonusPhyAtkPercent / 100f);
        public float TotalMagAtk => (Data.Globals.Ref.ObjChar[this.m_model].MAR * StatsMultipler + this.m_bonusMagAtk) * (1.0f + this.m_bonusMagAtkPercent / 100f);
        public float TotalParryRate => (Data.Globals.Ref.ObjChar[this.m_model].ER * StatsMultipler + this.m_bonusParryRate) * (1.0f + this.m_bonusParryPercent / 100f);
        public float TotalHitRate => (Data.Globals.Ref.ObjChar[this.m_model].HR * StatsMultipler + this.m_bonusHitRate) * (1.0f + this.m_bonusHitPercent / 100f);
        public float MaxHP => (Data.Globals.Ref.ObjChar[this.m_model].MaxHP * HealthMultipler + this.m_bonusMaxHealth) * (1.0f + this.m_bonusMaxHealthPercent / 100f);
        public float BlockRatio => (Data.Globals.Ref.ObjChar[this.m_model].BR * StatsMultipler + this.m_bonusBlockRatio) * (1.0f + this.m_bonusBlockRatioPercent / 100f);

        #endregion
        public AttackType m_attackType;

        #endregion

        #region Constructors & Destructors

        public GObjMob()
            : base (GObjType.GObjMob)
        {
            
        }

        #endregion

        #region Private Properties and Fields

        private int HealthMultipler
        {
            get
            {
                switch (m_rarity)
                {
                    case GObjMobRarity.Champion:
                        return 10;

                    case GObjMobRarity.Unique:
                        return 20;

                    case GObjMobRarity.Giant:
                        return 20;
                    default:
                        return 1;
                }
            }
        }

        private int StatsMultipler
        {
            get
            {
                switch (m_rarity)
                {
                    case GObjMobRarity.Champion:
                        return 2;

                    case GObjMobRarity.Unique:
                        return 5;

                    case GObjMobRarity.Giant:
                        return 4;
                    default:
                        return 1;
                }
            }
        }

        #endregion
    }
}
