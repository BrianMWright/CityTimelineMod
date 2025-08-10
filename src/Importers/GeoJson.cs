// src/Importers/GeoJson.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CityTimelineMod.Importers
{
    internal static class GeoJson
    {
        /// Quick count for sanity/logging.
        internal static int CountFeatures(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            var root = JObject.Parse(File.ReadAllText(path));
            var feats = (JArray?)root["features"] ?? throw new InvalidDataException("Not a FeatureCollection");
            return feats.Count;
        }

        /// Return a list of LineString parts. MultiLineStrings are expanded into multiple parts.
        internal static List<List<(double x, double y)>> ReadLineParts(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            var root = JObject.Parse(File.ReadAllText(path));
            var feats = (JArray?)root["features"] ?? throw new InvalidDataException("Not a FeatureCollection");

            var parts = new List<List<(double x, double y)>>();

            foreach (var f in feats)
            {
                var geom = f?["geometry"] as JObject;
                if (geom is null) continue;
                var type = (string?)geom["type"] ?? "";
                var coords = geom["coordinates"];

                switch (type)
                {
                    case "LineString":
                    {
                        var line = new List<(double x, double y)>();
                        foreach (var p in (JArray)coords!)
                        {
                            // coordinates are [x, y] (lon, lat) or projected feet for 2230
                            double x = p[0]!.Value<double>();
                            double y = p[1]!.Value<double>();
                            line.Add((x, y));
                        }
                        if (line.Count > 1) parts.Add(line);
                        break;
                    }
                    case "MultiLineString":
                    {
                        foreach (var seg in (JArray)coords!)
                        {
                            var line = new List<(double x, double y)>();
                            foreach (var p in (JArray)seg!)
                            {
                                double x = p[0]!.Value<double>();
                                double y = p[1]!.Value<double>();
                                line.Add((x, y));
                            }
                            if (line.Count > 1) parts.Add(line);
                        }
                        break;
                    }
                }
            }

            return parts;
        }

        /// Return the **outer** ring of each polygon (first ring). MultiPolygons → many outers.
        internal static List<List<(double x, double y)>> ReadAreaOuterRings(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            var root = JObject.Parse(File.ReadAllText(path));
            var feats = (JArray?)root["features"] ?? throw new InvalidDataException("Not a FeatureCollection");

            var rings = new List<List<(double x, double y)>>();

            foreach (var f in feats)
            {
                var geom = f?["geometry"] as JObject;
                if (geom is null) continue;
                var type = (string?)geom["type"] ?? "";
                var coords = geom["coordinates"];

                switch (type)
                {
                    case "Polygon":
                    {
                        // polygon -> [ [outer], [hole1], [hole2], ... ]
                        if (coords is JArray poly && poly.Count > 0)
                        {
                            var outer = poly[0] as JArray;
                            if (outer != null)
                            {
                                var ring = new List<(double x, double y)>();
                                foreach (var p in outer)
                                {
                                    double x = p[0]!.Value<double>();
                                    double y = p[1]!.Value<double>();
                                    ring.Add((x, y));
                                }
                                if (ring.Count > 2) rings.Add(ring);
                            }
                        }
                        break;
                    }
                    case "MultiPolygon":
                    {
                        // multipolygon -> [ [ [outer], holes... ], [ [outer], ... ], ... ]
                        foreach (var poly in (JArray)coords!)
                        {
                            if (poly is JArray polyArr && polyArr.Count > 0)
                            {
                                var outer = polyArr[0] as JArray;
                                if (outer != null)
                                {
                                    var ring = new List<(double x, double y)>();
                                    foreach (var p in outer)
                                    {
                                        double x = p[0]!.Value<double>();
                                        double y = p[1]!.Value<double>();
                                        ring.Add((x, y));
                                    }
                                    if (ring.Count > 2) rings.Add(ring);
                                }
                            }
                        }
                        break;
                    }
                }
            }

            return rings;
        }
    }
}
