namespace GatewayServer.Services
{
    using System.Collections.Generic;

    /// <summary>
    /// The shard item structure
    /// </summary>
    public struct _shard_item
    {
        public int ID;
        public string Name;
        public byte ContentID;
        public ushort CurrentUsers;
        public ushort MaxUsers;
        public byte State;
        public object m_lock;
    }


    /// <summary>
    /// The shard service class
    /// </summary>
    public static class Shard
    {
        #region Private Properties and Fields

        /// <summary>
        /// The shard list.
        /// </summary>
        private static List<_shard_item> s_Items;

        #endregion

        #region Initializer Method

        public static void Initialize()
        {
            s_Items = new List<_shard_item>();
            using (var reader = Data.Globals.AccountDB.ExecuteReader("SELECT * FROM _Shard"))
            {
                while(reader.Read())
                {
                    _shard_item item = new _shard_item
                    {
                        ID = (int)reader["ID"],
                        Name = (string)reader["Name"],
                        ContentID = (byte)reader["ContentID"],
                        CurrentUsers = 0,
                        MaxUsers = (ushort)(short)reader["MaxUsers"],
                        State = (byte)reader["State"],
                        m_lock = new object()
                    };

                    s_Items.Add(item);
                }
            }
        }

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Gets the shard list
        /// </summary>
        public static List<_shard_item> Items => s_Items;

        #endregion
    }
}
