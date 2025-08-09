// src/Util/Log.cs
using System;

namespace CityTimelineMod.Util
{
    internal static class Log
    {
        internal static void Info(string msg)  => Console.WriteLine($"[CityTimelineMod] {msg}");
        internal static void Error(string msg) => Console.WriteLine($"[CityTimelineMod][ERR] {msg}");
    }
}
