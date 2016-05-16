namespace GatewayServer.Data
{
    using System;
    using System.Collections.Generic;

    using SCommon.Database;

    public struct _wrongpass_item
    {
        public int Tries;
        public int LastTick;
    }

    internal static class Globals
    {
        public const int MAX_WRONG_PASSWORD = 3;

        public static Gateway GatewayService;
        public static Dictionary<string, string> Config;
        public static Dictionary<string, _wrongpass_item> WrongPasswordTries;
        public static MSSQL GlobalDB;
        public static MSSQL AccountDB;

        public static bool ConnectGlobalDB()
        {
            try
            {
                GlobalDB = new MSSQL("SR_Global Connection String");
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static bool ConnectAccountDB()
        {
            try
            {
                AccountDB = new MSSQL("SR_Account Connection String");
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static bool LoadConfigTable()
        {
            try
            {
                Config = new Dictionary<string, string>();
                using (var reader = GlobalDB.ExecuteReader("SELECT * FROM _ServerConfig"))
                {
                    while (reader.Read())
                        Data.Globals.Config.Add(Convert.ToString(reader["Key"]), Convert.ToString(reader["Value"]));
                }
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                return false;
            }
        }

        public static T GetConfigValue<T>(string key)
        {
            return (T)Convert.ChangeType(Config[key], typeof(T));
        }
    }
}
