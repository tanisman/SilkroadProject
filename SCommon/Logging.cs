namespace SCommon
{
    using System;

    public enum LogType
    {
        Console,
        File,
        Packet,
    }

    public enum LogLevel
    {
        Notify,
        Warning,
        Error,
        Info,
        Success,
    }

    
    public static class Logging
    {
        #region Public Properties and Fields

        public delegate void LogDelegateT(string arg1, LogLevel arg2 = LogLevel.Notify);

        public static bool EnablePacketLogging { get; set; }

        #endregion

        #region Public Methods

        public static LogDelegateT Log(LogType type = LogType.Console)
        {
            switch(type)
            {
                case LogType.Console:
                    return LogToConsole;
                case LogType.File:
                    return LogToFile;
                case LogType.Packet:
                    return LogPacket;
            }
            return null;
        }

        #endregion

        #region Private Methods

        private static void LogToConsole(string str, LogLevel level)
        {            
            var backupcolor = Console.ForegroundColor;
            Console.ForegroundColor = GetConsoleColor(level);
            Console.WriteLine("[{0}]{1}", level, str);
            Console.ForegroundColor = backupcolor;
        }

        private static void LogToFile(string str, LogLevel level)
        {
            //log to file
        }

        private static void LogPacket(string str, LogLevel level = LogLevel.Notify)
        {
            //if not enabled
            if (!EnablePacketLogging)
                return;

            //for level if notify/info to console if error/warning to file & console etc.
            //for now not supported only log to console
            Console.WriteLine(str);
        }

        private static ConsoleColor GetConsoleColor(LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Warning:
                    return ConsoleColor.DarkMagenta;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Info:
                    return ConsoleColor.Cyan;
                case LogLevel.Success:
                    return ConsoleColor.Green;
                case LogLevel.Notify:
                default:
                    return ConsoleColor.Gray;
            }
        }
        
        #endregion
    }
}
