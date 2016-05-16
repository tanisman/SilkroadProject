namespace SR_GameServer.Data.RefData
{
    public class RefLevel
    {
        #region Public Properties and Fields

        public byte Lvl;
        public long Exp_C;
        public int Exp_M, Cost_M, Cost_ST, GUST_Mob_Exp, JobExp_Trader, JobExp_Robber, JobExp_Hunter;

        #endregion
    }

    public static class RefLevelExtensions
    {
        public static void Load(this RefLevel[] list)
        {
            list[0] = new RefLevel(); //dummy

            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefLevel"))
            {
                while (reader.Read())
                {
                    byte dmp_lvl = (byte)reader["Lvl"];
                    list[dmp_lvl] = new RefLevel();
                    list[dmp_lvl].Lvl = dmp_lvl;
                    list[dmp_lvl].Exp_C = (long)reader["Exp_C"];
                    list[dmp_lvl].Exp_M = (int)reader["Exp_M"];
                    list[dmp_lvl].Cost_M = (int)reader["Cost_M"];
                    list[dmp_lvl].Cost_ST = (int)reader["Cost_ST"];
                    list[dmp_lvl].GUST_Mob_Exp = (int)reader["GUST_Mob_Exp"];
                    list[dmp_lvl].JobExp_Trader = (int)reader["JobExp_Trader"];
                    list[dmp_lvl].JobExp_Robber = (int)reader["JobExp_Robber"];
                    list[dmp_lvl].JobExp_Hunter = (int)reader["JobExp_Hunter"];
                }
            }
        }
    }
}
