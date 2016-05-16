namespace SR_GameServer
{
    using System;

    public enum GObjType
    {
        GObjMob,
        GObjItem,
        GObjNPC,
        GObjChar,
        GObjCOS,
        GObjStructure,
    }

    public enum WalkState
    {
        Walking,
        Running,
        Momentum = 7,
    }

    public enum MovementType
    {
        NotMoveable,
        NotMoving,
        Moving,
        Seat,
        //0 = None, 2 = Walking, 3 = Running, 4 = Sitting << to do: try
    }

    public enum LifeState
    {
        None,
        Alive,
        Dead,
    }

    public enum StatusType
    {
        None,
        Hwan,
        Untouchable,
        GameMasterInvincible,
        GameMasterInvisible,
        Stealth,
        Invisible,
    }

    public enum AttackType
    {
        None,
        Normal,
        Skill,
    }

    public enum GObjMobRarity
    {
        General,
        Champion,
        Unknown,
        Unique,
        Giant,
        Titan,
        Elite,
    }

    [Flags]
    public enum GObjTalkFlags
    {
        None = 0,
        Unknown1 = 1,
        Talkable = 2,
    }

    public enum PKType
    {
        None,
        Attacked,
        Killed,
    }

    [Flags]
    public enum GObjUpdateType
    {
        HP = 1,
        MP = 2,
        BadEffect = 4
    }

    [Flags]
    public enum GObjUpdateSource
    {
        None = 0,
        Damage = 1,
        DotDamage = 2,
        Consume = 4,
        Reverse = 8,
        Regeneration = 16,
        Potion = 32,
        Heal = 64,
        Unknown128 = 128, //TODO: RESEARCH
        BadEffect = 256,
    }
}
