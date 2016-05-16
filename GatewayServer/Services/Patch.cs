namespace GatewayServer.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The patch item structure
    /// </summary>
    public struct _patch_item
    {
        public uint ID;
        public uint Version;
        public uint ModuleID;
        public string FileName;
        public string Path;
        public uint FileSize;
        public byte FileType;
        public uint FileTypeVersion;
        public bool ToBePacked;
        public DateTime TimeModified;
        public bool IsValid;
    }

    /// <summary>
    /// The patch service class
    /// </summary>
    public static class Patch
    {
        #region Private Properties and Fields

        /// <summary>
        /// The patch file items.
        /// </summary>
        private static List<_patch_item> s_Items;

        #endregion

        #region Initializer Method

        public static void Initialize()
        {
            s_Items = new List<_patch_item>();
            using (var reader = Data.Globals.AccountDB.ExecuteReader("SELECT * FROM _ModuleVersionFile WHERE nVersion > {0} AND nValid = 1 AND nModuleID = '{1}' ORDER BY nID", Data.Globals.GetConfigValue<uint>("LatestVersion"), 9))
            {
                while(reader.Read())
                {
                    _patch_item item = new _patch_item
                    {
                        ID = (uint)reader["nID"],
                        Version = (uint)reader["nVersion"],
                        ModuleID = (uint)reader["nModuleID"],
                        FileName = (string)reader["szFilename"],
                        Path = ((string)reader["szPath"]).Replace(".", ""),
                        FileSize = (uint)reader["nFileSize"],
                        FileType = (byte)reader["nFileType"],
                        FileTypeVersion = (uint)reader["nFileTypeVersion"],
                        ToBePacked = Convert.ToBoolean(reader["nToBePacked"]),
                        TimeModified = (DateTime)reader["timeModified"],
                        IsValid = Convert.ToBoolean(reader["nValid"])
                    };

                    s_Items.Add(item);
                }
            }
        }

        #endregion

        #region Public Properties and Fields

        /// <summary>
        /// Gets the patch file items list.
        /// </summary>
        public static List<_patch_item> Items => s_Items;

        #endregion
    }
}
