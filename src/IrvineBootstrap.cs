// src/IrvineBootstrap.cs
using System;
using System.IO;
using CityTimelineMod.Util;
using CityTimelineMod.Importers;

namespace CityTimelineMod
{
    internal static class IrvineBootstrap
    {
        static bool _ran = false;

        internal static void RunOnce()
        {
            if (_ran) return; _ran = true;

            try
            {
                // 🔧 Adjust only this root if you move the repo
                var root = @"C:\Projects\CitySkylines2\Irvine\cs2-realcity";
                var lines = Path.Combine(root, @"dist\irvine-ca\networks\water_lines_2230_simplified.geojson");
                var areas = Path.Combine(root, @"dist\irvine-ca\networks\water_areas_2230_simplified.geojson");

                var lc = GeoJson.CountFeatures(lines);
                var ac = GeoJson.CountFeatures(areas);

                Log.Info($"Loaded OK. Water lines: {lc}, water areas: {ac}");
                Log.Info($"lines={lines}");
                Log.Info($"areas={areas}");

                // NEXT STEP: replace with real placement into the map (networks & water areas)
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
