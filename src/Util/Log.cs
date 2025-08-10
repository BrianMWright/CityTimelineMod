// src/Util/Log.cs
using System;
using Colossal.Logging;

namespace CityTimelineMod.Util
{
    internal static class Log
    {
        private static readonly ILog _log =
            LogManager.GetLogger(nameof(CityTimelineMod)).SetShowsErrorsInUI(true);

        internal static void Info(string msg)
        {
            Console.WriteLine($"[CityTimelineMod] {msg}");
            _log.Info(msg);
        }

        internal static void Error(string msg)
        {
            Console.WriteLine($"[CityTimelineMod][ERR] {msg}");
            _log.Error(msg);
        }
    }
}
