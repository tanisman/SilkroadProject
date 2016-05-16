namespace GatewayServer
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

            if (Data.Globals.ConnectAccountDB())
                Logging.Log()("Connected to account db", LogLevel.Notify);
            else
                Logging.Log()("Cannot connect to account db", LogLevel.Error);

            //truncate the sessions
            Data.Globals.GlobalDB.ExecuteCommand("TRUNCATE TABLE _ActiveSessions");

            #endregion

            #region Load Configs

            if (Data.Globals.LoadConfigTable())
                Logging.Log()("Server config has loaded", LogLevel.Notify);
            else
                Logging.Log()("Cannot load server config", LogLevel.Error);

            Data.Globals.WrongPasswordTries = new Dictionary<string, Data._wrongpass_item>();

            #endregion

            #region Start Service

            Data.Globals.GatewayService = new Gateway(Data.Globals.GetConfigValue<string>("GatewayServerIPAddress"), Data.Globals.GetConfigValue<int>("GatewayServerPort"));
            Data.Globals.GatewayService.UpdateServiceState(true);
            Logging.Log()("Gateway Service has started", LogLevel.Info);

            #endregion

            #region Start GC CollectorThread
            
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    GC.Collect();
                    Thread.Sleep(10000);
                }
            })).Start();

            #endregion

            #region Console command helper
            
            while (true)
            {
                Console.Write("gateway@{0}:~# ", Data.Globals.GetConfigValue<string>("GatewayServerIPAddress"));
                var commands = Console.ReadLine().ToLower().Split(' ');
                switch(commands[0])
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
                        Logging.Log()(String.Format("Current Active Connections Count is {0}", Data.Globals.GatewayService.ActiveConnectionCount), LogLevel.Info);
                        break;
                }
                Thread.Sleep(1);
            }

            #endregion
        }
    }
}
