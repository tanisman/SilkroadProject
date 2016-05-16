namespace SR_GameServer.Data.RefData
{
    using System.Collections.Generic;

    public class RefFmnTidGroupMap
    {
        #region Public Properties and Fields

        public int TidGroup;
        public byte TID1, TID2, TID3, TID4;

        #endregion
    }

    public static class RefFmnTidGroupExtensions
    {
        public static void Load(this List<RefFmnTidGroupMap> list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefFmnTidGroupMap WHERE Service = 1"))
            {
                while (reader.Read())
                {
                    int dmp_id = (int)reader["TidGroupID"];
                    RefFmnTidGroupMap tid = new RefFmnTidGroupMap();
                    tid.TidGroup = dmp_id;
                    tid.TID1 = (byte)reader["TypeID1"];
                    tid.TID2 = (byte)reader["TypeID2"];
                    tid.TID3 = (byte)reader["TypeID3"];
                    tid.TID4 = (byte)reader["TypeID4"];
                    list.Add(tid);
                }
            }
        }
    }
}
