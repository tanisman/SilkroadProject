namespace SR_GameServer
{
    public enum _Char
    {
        CharID,
        CharName16,
        RefObjID,
        Scale,
        CurLevel,
        MaxLevel,
        CurHP,
        CurMP,
        ExpOffset,
        SExpOffset,
        Strength,
        Intellect,
        LatestRegion,
        PosX,
        PosY,
        PosZ,
        Angle,
        RemainGold,
        RemainSkillPoint,
        RemainStatPoint,
        RemainHwanCount,
        Deleting,
        DeleteEndTime,
        TraderLvl,
        TraderExp,
        HunterLvl,
        HunterExp,
        ThiefLvl,
        ThiefExp,
        GGData,
        AppointedTeleport,
        Deleted,
        FieldCount,
    }

    public enum _CharSkillMastery
    {
        CharID,
        MasteryID,
        Level,
        FieldCount,
    }

    public enum _CharSkill
    {
        CharID,
        SkillID,
        Enabled,
        FieldCount,
    }

    public enum _Inventory
    {
        CharID,
        Slot,
        ItemID,
        FieldCount,
    }

    public enum _Items
    {
        ID64,
        RefItemID,
        OptLevel,
        Variance,
        Data,
        CreaterName,
        MagParamCount,
        MagParam1,
        MagParam2,
        MagParam3,
        MagParam4,
        MagParam5,
        MagParam6,
        MagParam7,
        MagParam8,
        MagParam9,
        MagParam10,
        MagParam11,
        MagParam12,
        ItemSerial,
        FieldCount,
    }
}
