using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoreMonarchy.MarketplacePlugin.Logging
{
    public static class ServiceLogger 
    {
        public static bool IsDebug { get; set; } = false;

        public static void LogInformation<T>(string msg)
        {
            Logger.Log($"{typeof(T).Name} >> {msg}", ConsoleColor.Green);
        }

        public static void LogDebug<T>(string msg)
        {
            if (IsDebug)
                Logger.Log($"{typeof(T).Name} >> {msg}", ConsoleColor.DarkGreen);
        }

        public static void LogError<T>(Exception e)
        {
            Logger.Log($"{typeof(T).Name} >> {e.Message}", ConsoleColor.Red);
            if (IsDebug)
                Logger.LogException(e);
        }
    }
}
