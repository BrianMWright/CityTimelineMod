// src/Placement/WaterPlacer.cs
using System;
using System.Collections.Generic;
using CityTimelineMod.Util;

namespace CityTimelineMod.Placement
{
    internal static class WaterPlacer
    {
        /// <summary>
        /// Convert raw EPSG:2230 feet coordinates into world-unit V2s for preview/placement.
        /// </summary>
        internal static List<List<V2>> BuildTransformedSamples(
            List<List<(double x, double y)>> parts,
            Epsg2230Transformer tf,
            int maxPolylines = 5,
            int maxPointsPerLine = 500)
        {
            var result = new List<List<V2>>();
            int linesAdded = 0;

            foreach (var part in parts)
            {
                var polyline = new List<V2>();
                int count = 0;

                foreach (var pt in part)
                {
                    polyline.Add(tf.ToWorld(new V2(pt.x, pt.y)));
                    count++;
                    if (count >= maxPointsPerLine) break;
                }

                if (polyline.Count > 1)
                {
                    result.Add(polyline);
                    linesAdded++;
                    if (linesAdded >= maxPolylines) break;
                }
            }

            return result;
        }
    }
}
