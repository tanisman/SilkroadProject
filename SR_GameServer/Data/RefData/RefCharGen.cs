namespace SR_GameServer.Data.RefData
{
    using System.Collections.Generic;

    public class RefCharGen
    {
        #region Public Properties and Fields

        public int RefObjID { get; private set; }

        #endregion

        #region Constructors & Destructors

        public RefCharGen(int id)
        {
            RefObjID = id;
        }

        #endregion
    }

    public static class RefCharGenExtensions
    {
        public static void Load(this List<RefCharGen> list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefCharGen WHERE Service = 1"))
            {
                while (reader.Read())
                    list.Add(new RefCharGen((int)reader["RefObjID"]));
            }
        }
    }
}
