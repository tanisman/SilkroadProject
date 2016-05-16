namespace SR_GameServer.Data.RefData
{
    public class RefDropGold
    {
        #region Public Properties and Fields

        public byte MonLevel;
        public double DropProb;
        public int GoldMin, GoldMax;

        #endregion
    }

    public static class RefDropGoldExtensions
    {
        public static void Load(this RefDropGold[] list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefDropGold"))
            {
                while (reader.Read())
                {
                    byte tmp_monlvl = (byte)reader["MonLevel"];
                    list[tmp_monlvl] = new RefDropGold();
                    list[tmp_monlvl].MonLevel = tmp_monlvl;
                    list[tmp_monlvl].DropProb = (float)reader["DropProb"];
                    list[tmp_monlvl].GoldMin = (int)reader["GoldMin"];
                    list[tmp_monlvl].GoldMax = (int)reader["GoldMax"];
                }
            }
        }
    }
}
