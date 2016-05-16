namespace SR_GameServer
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using SCore;

    using SCommon;

    internal class Program
    {
        static void Main(string[] args)
        {
            #region Assert Debug Mode
#if DEBUG
            Logging.Log()("Debug mode is active", LogLevel.Info);
#endif
            #endregion

            #region Initialize Services

            TimerService.Initialize();
            Logging.Log()("Timer service has initialized", LogLevel.Success);

            PacketProcessor.FillTable();
            Logging.Log()("Packet Processor has initialized", LogLevel.Success);

            #endregion

            #region Connect to Database Services

            if (Data.Globals.ConnectGlobalDB())
                Logging.Log()("Connected to global db", LogLevel.Notify);
            else
                Logging.Log()("Cannot connect to global db", LogLevel.Error);

            if (Data.Globals.ConnectShardDB())
                Logging.Log()("Connected to shard db", LogLevel.Notify);
            else
                Logging.Log()("Cannot connect to shard db", LogLevel.Error);

            //truncate the sessions
            Data.Globals.GlobalDB.ExecuteCommand("TRUNCATE TABLE _ActiveSessions");

            #endregion

            #region Load Configs

            if (Data.Globals.LoadConfigTable())
                Logging.Log()("Server config has loaded", LogLevel.Notify);
            else
                Logging.Log()("Cannot load server config", LogLevel.Error);

            #endregion

            #region Load RefData

            Data.Globals.LoadRefData();

            #endregion

            #region Start Services

            Data.Globals.SRGameService = new SRGame(Data.Globals.GetConfigValue<string>("GameServerIPAddress"), Data.Globals.GetConfigValue<int>("GameServerPort"));
            Data.Globals.SRGameService.UpdateServiceState(true);
            Logging.Log()("SRGame Service has started", LogLevel.Info);

            #endregion

            #region Start GC Collector Thread

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    GC.Collect();
                    Thread.Sleep(10000);
                }
            })).Start();

            #endregion

            #region GameWorld Status Thread

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    long c = Data.Globals.GObjList == null ? 0 : Data.Globals.GObjList.Count;
                    Console.Title = String.Format("GObj Count: {0}", c);
                    Thread.Sleep(1000);
                }
            })).Start();

            #endregion

            #region Console command helper

            while (true)
            {
                Console.Write("srgame@{0}:~# ", Data.Globals.GetConfigValue<string>("GameServerIPAddress"));
                var commands = Console.ReadLine().ToLower().Split(' ');
                switch (commands[0])
                {
                    case "exit":
                        Environment.Exit(0);
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "enablepacketlogging":
                        Logging.EnablePacketLogging = Convert.ToBoolean(commands[1]);
                        Logging.Log()(String.Format("Set EnablePacketLogging = {0}", Logging.EnablePacketLogging ? "TRUE" : "FALSE"), LogLevel.Success);
                        break;
                    case "connectioncount":
                        Logging.Log()(String.Format("Current Active Connections Count is {0}", Data.Globals.SRGameService.ActiveConnectionCount), LogLevel.Info);
                        break;
                }
                Thread.Sleep(1);
            }

            #endregion
        }
    }
}
