namespace SR_GameServer
{
    using System;
    using System.Runtime.CompilerServices;

    using SharpDX;

    public static class Formula
    {
        public const byte REGION_SCALE = 128;
        public const byte WORLD_SCALE = 10;
        public const byte REGION_HEIGHT = 192;
        public const byte REGION_WIDTH = 192;
        public const byte X_START_SECTOR = 46;
        public const byte Z_START_SECTOR = 46;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BaseMaxHealthManaByStats(byte level, int stats)
        {
            return (int)(Math.Pow(1.02f, (level - 1)) * stats * 10);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BaseDefByStats(int stats)
        {
            return (int)(0.4f * stats);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BaseMinAtkByStats(int stats)
        {
            return (int)(0.45f * stats);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BaseMaxAtkByStats(int stats)
        {
            return (int)(0.65f * stats);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PhyBalance(byte level, int stats)
        {
            return (int)(100f - (100f * 2f / 3f * ((28f + level * 4f) - stats) / (28f + level * 4f)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MagBalance(byte level, int stats)
        {
            return (int)(100f * stats / (28f + level * 4f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetSectorX(short region)
        {
            return (byte)(region % REGION_SCALE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetSectorZ(short region)
        {
            return (byte)((region - GetSectorX(region)) / REGION_SCALE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short MakeRegion(byte x, byte z)
        {
            return (short)(z * REGION_SCALE + x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRegionalX(short region, float x)
        {
            return (x - (GetSectorX(region) - X_START_SECTOR) * REGION_HEIGHT) * WORLD_SCALE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRegionalZ(short region, float z)
        {
            return (z - (GetSectorZ(region) - Z_START_SECTOR) * REGION_WIDTH) * WORLD_SCALE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetWorldX(short region, float x)
        {
            return (GetSectorX(region) - X_START_SECTOR) * REGION_HEIGHT + (x / WORLD_SCALE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetWorldZ(short region, float z)
        {
            return (GetSectorZ(region) - Z_START_SECTOR) * REGION_WIDTH + (z / WORLD_SCALE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short GetRegion(float x, float z)
        {
            return MakeRegion((byte)(Math.Floor(x / REGION_HEIGHT) + X_START_SECTOR), (byte)(Math.Floor(z / REGION_WIDTH) + Z_START_SECTOR));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInSightRegion(short r1, short r2)
        {

            /*              
                *************************
                *       *       *       *
                * +127	* +128  *  +129 *
                *       *		*	    *
                *************************
                *       *		*	    *
                *  -1	*   0	*   +1  *
                *       *		*	    *
                *************************
                *       *		*	    *
                * -129	* -128	*  -127 *
                *       *       *       *
                *************************  
            */
            int region_gap = Math.Abs(r1 - r2);
            return (region_gap >= REGION_SCALE - 1 && region_gap <= REGION_SCALE + 1) || region_gap == 1 || region_gap == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(Vector3 p1, Vector3 p2)
        {
            return (float)(Math.Atan2(p2.Z - p1.Z, p2.X - p1.X) * 180 / Math.PI * 182);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToGameWorld(this Vector3 v, short r)
        {
            return new Vector2(GetWorldX(r, v.X), GetWorldZ(r, v.Z));
        }       
    }
}
