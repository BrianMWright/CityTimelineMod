// src/Importers/GeoJson.cs
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CityTimelineMod.Importers
{
    internal static class GeoJson
    {
        internal static int CountFeatures(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var json = File.ReadAllText(path);

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Invalid JSON", ex);
            }

            // Require a FeatureCollection with an array "features"
            var typeToken = root["type"];
            var type = typeToken != null ? typeToken.ToString() : null;

            if (type == null || !string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Not a FeatureCollection");

            var features = root["features"] as JArray;
            if (features == null)
                throw new InvalidDataException("'features' is missing or not an array");

            return features.Count;
        }
    }
}
