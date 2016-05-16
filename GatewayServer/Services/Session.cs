namespace GatewayServer.Services
{
    using System.Threading;

    public static class Session
    {
        #region Private Properties and Fields

        /// <summary>
        /// Stores the last generated session id
        /// </summary>
        private static volatile int s_LastSessionID;

        #endregion

        #region Initializer Methods

        public static void Initialize()
        {
            s_LastSessionID = int.MinValue;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the new session id
        /// </summary>
        /// <returns></returns>
        public static int Generate()
        {
            if (PreventOverflows())
                return int.MinValue;

            return Interlocked.Increment(ref s_LastSessionID);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks for the overflows
        /// </summary>
        /// <returns></returns>
        private static bool PreventOverflows()
        {
            return Interlocked.CompareExchange(ref s_LastSessionID, int.MinValue, int.MaxValue) == int.MaxValue;
        }

        #endregion
    }
}
