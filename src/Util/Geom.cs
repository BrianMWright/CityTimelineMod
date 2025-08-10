// src/Util/Geom.cs
using System;
using System.Collections.Generic;

namespace CityTimelineMod.Util
{
    /// <summary>Lightweight 2D vector used for projected/world coords.</summary>
    internal readonly struct V2
    {
        public readonly double x;
        public readonly double y;

        public V2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString() => $"({x:F2}, {y:F2})";
    }

    /// <summary>
    /// Axis-aligned bounding box (feet in EPSG:2230 for our importer).
    /// </summary>
    internal sealed class BBox
    {
        public readonly V2 min;
        public readonly V2 max;

        public double SizeX => max.x - min.x;
        public double SizeY => max.y - min.y;

        public BBox(V2 min, V2 max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Build a bounding box from line parts and area outer rings
        /// (both are lists of polylines made of (x,y) doubles in EPSG:2230 feet).
        /// </summary>
        public static BBox FromParts(
            List<List<(double x, double y)>> lineParts,
            List<List<(double x, double y)>> areaRings)
        {
            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            // Scan each group safely (no lambdas/local functions; no tuple deconstruction).
            ScanGroups(lineParts, ref minX, ref minY, ref maxX, ref maxY);
            ScanGroups(areaRings, ref minX, ref minY, ref maxX, ref maxY);

            if (double.IsInfinity(minX) || double.IsInfinity(minY) ||
                double.IsInfinity(maxX) || double.IsInfinity(maxY))
            {
                throw new ArgumentException("BBox.FromParts: no coordinates provided.");
            }

            return new BBox(new V2(minX, minY), new V2(maxX, maxY));
        }

        /// <summary>
        /// Back-compat overload: build a bbox from a single list of parts.
        /// </summary>
        public static BBox FromParts(List<List<(double x, double y)>> parts)
        {
            return FromParts(parts, (List<List<(double x, double y)>>)null);
        }

        private static void ScanGroups(
            List<List<(double x, double y)>> groups,
            ref double minX, ref double minY,
            ref double maxX, ref double maxY)
        {
            if (groups == null) return;

            for (int i = 0; i < groups.Count; i++)
            {
                var part = groups[i];
                if (part == null) continue;

                for (int j = 0; j < part.Count; j++)
                {
                    var pt = part[j];
                    double x = pt.x;
                    double y = pt.y;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }
    }
}
