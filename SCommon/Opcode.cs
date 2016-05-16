namespace SCommon.Opcode
{
    public struct General
    {
       public const ushort HANDSHAKE = 0x5000;
       public const ushort HANDSHAKE_ACCEPT = 0x9000;
       public const ushort IDENTITY = 0x2001;
       public const ushort SEED_1 = 0x2005;
       public const ushort SEED_2 = 0x6005;
       public const ushort PING = 0x2002;
    }
    
    public struct Gateway
    {
        public const ushort CHECKVERSION = 0x6000;

        public struct Request
        {
            public const ushort PATCH = 0x6100;
            public const ushort LAUNCHER = 0x6104;
            public const ushort SERVERLIST = 0x6101;
            public const ushort LOGIN = 0x6102;
        }

        public struct Response
        {
            public const ushort PATCH = 0xA100;
            public const ushort LAUNCHER = 0xA104;
            public const ushort SERVERLIST = 0xA101;
            public const ushort LOGIN = 0xA102;
        }
    }

    public struct Download
    {
        public struct Request
        {
            public const ushort FILE = 0x6004;
        }

        public struct Response
        {
            public const ushort FILE = 0x1001;
            public const ushort SUCCEEDED = 0xA004;
        }
    }

    public struct Agent
    {
        public struct Request
        {
            public const ushort CONNECTION = 0x6103;
            public const ushort INGAME = 0x7001;
            public const ushort CHARACTER_SCREEN = 0x7007;
            public const ushort CHARACTER_SELECT = 0x7001;
            public const ushort CHARACTER_ENTERWORLD = 0x3012;
            public const ushort GAMEOBJECT_MOVEMENT = 0x7021;
            public const ushort GAMEOBJECT_STOP = 0x7023;
            public const ushort GAMEOBJECT_SET_ANGLE = 0x7024;
            public const ushort SELECT_GAMEOBJECT = 0x7045;
            public const ushort TELEPORT = 0x705A;
            public const ushort TELEPORT_LOADING = 0x34B6;
            public const ushort CHAT = 0x7025;
            public const ushort INVENTORY_OPERATION = 0x7034;
            public const ushort SHOP_OPEN = 0x7046;
            public const ushort SHOP_CLOSE = 0x704B;
            public const ushort TELEPORT_APPOINT = 0x7059;
            public const ushort GAMEGUIDE = 0x70EA;
            public const ushort GM_COMMAND = 0x7010;
        }

        public struct Response
        {
            public const ushort CONNECTION = 0xA103;
            public const ushort INGAME = 0xB001;
            public const ushort CHARACTER_SCREEN = 0xB007;
            public const ushort CHARACTER_SELECT = 0xB001;
            public const ushort GAMEOBJECT_SET_ANGLE = 0xB024;
            public const ushort SELECT_GAMEOBJECT = 0xB045;
            public const ushort CHAT = 0xB025;
            public const ushort INVENTORY_OPERATION = 0xB034;
            public const ushort UPDATE_INVENTORY_SLOT = 0xB04C;
            public const ushort SHOP_OPEN = 0xB046;
            public const ushort SHOP_CLOSE = 0xB04B;
            public const ushort TELEPORT_APPOINT = 0xB059;
            public const ushort GAMEGUIDE = 0xB0EA;
        }

        public const ushort CHARACTER_LOAD_START = 0x34A5;
        public const ushort CHARACTER_LOAD_DATA = 0x3013;
        public const ushort CHARACTER_LOAD_END = 0x34A6;
        public const ushort CHARACTER_CELESTICAL_POS = 0x3020;
        public const ushort CHARACTER_STAT = 0x303D;
        public const ushort CHARACTER_ENTERWORLD = 0x3077;
        public const ushort GAMEOBJECT_SPAWN_SINGLE = 0x3015;
        public const ushort GAMEOBJECT_DESPAWN_SINGLE = 0x3016;
        public const ushort GAMEOBJECT_SPAWN_GROUP_START = 0x3017;
        public const ushort GAMEOBJECT_SPAWN_GROUP_DATA = 0x3019;
        public const ushort GAMEOBJECT_SPAWN_GROUP_END = 0x3018;
        public const ushort GAMEOBJECT_WARP = 0xB023;
        public const ushort GAMEOBJECT_MOVEMENT = 0xB021;
        public const ushort GAMEOBJECT_EMOTE = 0x3091;
        public const ushort TELEPORT_SCREEN = 0x3055;
        public const ushort SEND_MESSAGE = 0x3026;
        public const ushort EQUIP_ITEM = 0x3038;
        public const ushort UNEQUIP_ITEM = 0x3039;
        public const ushort DROP_ITEM = 0x304D;
        public const ushort UPDATE_PC = 0x304E;
    }
}