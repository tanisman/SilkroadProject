namespace GatewayServer.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The launcher news item structure
    /// </summary>
    public struct _news_item
    {
        public uint ID;
        public byte ContentID;
        public string Subject;
        public string Article;
        public DateTime EditDate;
    }

    /// <summary>
    /// The launcher service class.
    /// </summary>
    public static class Launcher
    {
        #region Private Properties and Fields
         
        /// <summary>
        /// The launcher news items.
        /// </summary>
        private static List<_news_item> s_Items;

        #endregion

        #region Initializer Method

        public static void Initialize()
        {
            s_Items = new List<_news_item>();
            using (var reader = Data.Globals.AccountDB.ExecuteReader("SELECT TOP 4 * FROM _Notice ORDER BY EditDate DESC"))
            {
                while(reader.Read())
                {
                    _news_item item = new _news_item
                    {
                        ID = (uint)(int)reader["ID"],
                        ContentID = (byte)reader["ContentID"],
                        Subject = (string)reader["Subject"],
                        Article = (string)reader["Article"],
                        EditDate = (DateTime)reader["EditDate"]
                    };

                    s_Items.Add(item);
                }
            }
        }

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Gets the launcher news item list
        /// </summary>
        public static List<_news_item> Items => s_Items;

        #endregion
    }
}
