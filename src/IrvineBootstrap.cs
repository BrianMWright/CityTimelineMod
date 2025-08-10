// src/IrvineBootstrap.cs
using System;
using System.IO;
using CityTimelineMod.Importers;
using CityTimelineMod.Util;

namespace CityTimelineMod
{
    internal static class IrvineBootstrap
    {
        private static bool _ran = false;

        internal static void RunOnce()
        {
            if (_ran) return;
            _ran = true;

            try
            {
                // 🔧 Update if you move the repo; or set env var CITYTIMELINE_ROOT to override
                var root = Environment.GetEnvironmentVariable("CITYTIMELINE_ROOT");
                if (string.IsNullOrWhiteSpace(root))
                    root = @"C:\Projects\CitySkylines2\Irvine\cs2-realcity";

                // Path helpers
                string P(params string[] parts) => Path.Combine(parts);
                string Lines(string suffix) => P(root, "dist", "irvine-ca", "networks", $"water_lines{suffix}.geojson");
                string Areas(string suffix) => P(root, "dist", "irvine-ca", "networks", $"water_areas{suffix}.geojson");

                // Prefer simplified EPSG:2230
                var linesPath = Lines("_2230_simplified");
                var areasPath = Areas("_2230_simplified");

                string SizeStr(string p) => File.Exists(p)
                    ? new FileInfo(p).Length.ToString("N0") + " bytes"
                    : "MISSING";

                Log.Info($"[Paths] Lines: {linesPath}  ({SizeStr(linesPath)})");
                Log.Info($"[Paths] Areas: {areasPath}  ({SizeStr(areasPath)})");

                // Count features
                int lineFeatures = File.Exists(linesPath) ? GeoJson.CountFeatures(linesPath) : 0;
                int areaFeatures = File.Exists(areasPath) ? GeoJson.CountFeatures(areasPath) : 0;

                // Fallback to full _2230 if simplified has 0 features
                if (lineFeatures == 0 && File.Exists(Lines("_2230")))
                {
                    Log.Info("Simplified lines had 0 features — falling back to water_lines_2230.geojson");
                    linesPath = Lines("_2230");
                    lineFeatures = GeoJson.CountFeatures(linesPath);
                    Log.Info($"[Using] {linesPath}  ({SizeStr(linesPath)})");
                }
                if (areaFeatures == 0 && File.Exists(Areas("_2230")))
                {
                    Log.Info("Simplified areas had 0 features — falling back to water_areas_2230.geojson");
                    areasPath = Areas("_2230");
                    areaFeatures = GeoJson.CountFeatures(areasPath);
                    Log.Info($"[Using] {areasPath}  ({SizeStr(areasPath)})");
                }

                Log.Info($"Loaded OK. Water lines features: {lineFeatures}, water areas features: {areaFeatures}");

                // === Sample geometry preview ===
                // Lines
                if (lineFeatures > 0)
                {
                    var lines = GeoJson.ReadLineStrings(linesPath);
                    Log.Info($"Parsed {lines.Count} LineString parts.");

                    if (lines.Count > 0)
                    {
                        var first = lines[0];
                        var preview = Math.Min(8, first.Count);
                        Log.Info($"First LineString has {first.Count} points (EPSG:2230 feet). Preview {preview}:");
                        for (int i = 0; i < preview; i++)
                            Log.Info($"  L{i}: {first[i].x:F2}, {first[i].y:F2}");
                    }
                }
                else
                {
                    Log.Info("No line features to parse.");
                }

                // Areas (outer rings)
                if (areaFeatures > 0)
                {
                    var rings = GeoJson.ReadPolygonOuterRings(areasPath);
                    Log.Info($"Parsed {rings.Count} polygon outer rings.");

                    if (rings.Count > 0)
                    {
                        var r = rings[0];
                        var preview = Math.Min(8, r.Count);
                        Log.Info($"First polygon outer ring has {r.Count} points (EPSG:2230 feet). Preview {preview}:");
                        for (int i = 0; i < preview; i++)
                            Log.Info($"  A{i}: {r[i].x:F2}, {r[i].y:F2}");
                    }
                }
                else
                {
                    Log.Info("No area features to parse.");
                }

                // NEXT: transform EPSG:2230 (ft) -> game world coords and spawn splines.
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
