// src/Importers/GeoJson.cs
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CityTimelineMod.Importers
{
    internal static class GeoJson
    {
        // Count features in a FeatureCollection (sanity check)
        internal static int CountFeatures(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var json = File.ReadAllText(path);
            JObject root;
            try { root = JObject.Parse(json); }
            catch (Exception ex) { throw new InvalidDataException("Invalid JSON", ex); }

            var typeToken = root["type"];
            var type = typeToken != null ? typeToken.ToString() : null;
            if (type == null || !string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Not a FeatureCollection");

            var features = root["features"] as JArray;
            if (features == null)
                throw new InvalidDataException("'features' is missing or not an array");

            return features.Count;
        }

        // Read all LineStrings & MultiLineStrings -> list of (x,y) in EPSG:2230 feet
        internal static List<List<(double x, double y)>> ReadLineStrings(string path)
        {
            var json = File.ReadAllText(path);
            var root = JObject.Parse(json);

            var lines = new List<List<(double, double)>>();

            var features = root["features"] as JArray ?? new JArray();
            foreach (var f in features)
            {
                var geom = f["geometry"] as JObject;
                if (geom == null) continue;

                var gtype = geom["type"] != null ? geom["type"].ToString() : null;
                if (string.Equals(gtype, "LineString", StringComparison.OrdinalIgnoreCase))
                {
                    var coords = (JArray)geom["coordinates"];
                    lines.Add(ParseLineCoords(coords));
                }
                else if (string.Equals(gtype, "MultiLineString", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = (JArray)geom["coordinates"];
                    foreach (var part in parts)
                        lines.Add(ParseLineCoords((JArray)part));
                }
            }
            return lines;
        }

        // Read Polygon/MultiPolygon outer rings -> list of (x,y) in EPSG:2230 feet
        internal static List<List<(double x, double y)>> ReadPolygonOuterRings(string path)
        {
            var json = File.ReadAllText(path);
            var root = JObject.Parse(json);

            var rings = new List<List<(double, double)>>();

            var features = root["features"] as JArray ?? new JArray();
            foreach (var f in features)
            {
                var geom = f["geometry"] as JObject;
                if (geom == null) continue;

                var gtype = geom["type"] != null ? geom["type"].ToString() : null;
                if (string.Equals(gtype, "Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    var polys = (JArray)geom["coordinates"]; // [ [outer], [hole1], ... ]
                    if (polys.Count > 0) rings.Add(ParseLineCoords((JArray)polys[0]));
                }
                else if (string.Equals(gtype, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
                {
                    var multi = (JArray)geom["coordinates"]; // [ [ [outer], [hole]... ], ... ]
                    foreach (var poly in multi)
                    {
                        var ringsArray = (JArray)poly;
                        if (ringsArray.Count > 0)
                            rings.Add(ParseLineCoords((JArray)ringsArray[0])); // outer only
                    }
                }
            }
            return rings;
        }

        // Helpers
        private static List<(double x, double y)> ParseLineCoords(JArray coords)
        {
            var list = new List<(double, double)>(coords.Count);
            foreach (var c in coords)
            {
                var arr = (JArray)c;
                double x = arr[0].Value<double>();
                double y = arr[1].Value<double>();
                list.Add((x, y));
            }
            return list;
        }
    }
}
