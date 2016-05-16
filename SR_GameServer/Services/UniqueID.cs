namespace SR_GameServer.Services
{
    using System.Threading;

    public static class UniqueID
    {
        #region Private Properties and Fields

        /// <summary>
        /// Stores the last generated GObj UniqueID
        /// </summary>
        private static volatile int s_LastGObjID;

        /// <summary>
        /// Stores the last generated Casting UniqueID
        /// </summary>
        private static volatile int s_LastCastingID;

        #endregion

        #region Initializer Methods

        public static void Initialize()
        {
            s_LastGObjID = 100;
            s_LastCastingID = 150000000;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the new GObj UniqueID
        /// </summary>
        /// <returns></returns>
        public static int GenerateGObjID()
        {
            if (PreventGObjIDOverflows())
                return 100;

            return Interlocked.Increment(ref s_LastGObjID);
        }

        /// <summary>
        /// Generates the new Casting UniqueID
        /// </summary>
        /// <returns></returns>
        public static int GenerateCastingID()
        {
            if (PreventCastingIDOverflows())
                return 150000000;

            return Interlocked.Increment(ref s_LastCastingID);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks for the GObj UniqueID overflows
        /// </summary>
        /// <returns></returns>
        private static bool PreventGObjIDOverflows()
        {
            return Interlocked.CompareExchange(ref s_LastGObjID, 100, 147483646) == 147483646;
        }

        /// <summary>
        /// Checks for the Casting UniqueID overflows
        /// </summary>
        /// <returns></returns>
        private static bool PreventCastingIDOverflows()
        {
            return Interlocked.CompareExchange(ref s_LastCastingID, 150000000, 2000000100) == 2000000100;
        }

        #endregion
    }
}
