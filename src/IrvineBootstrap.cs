// src/IrvineBootstrap.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using CityTimelineMod.Importers;   // GeoJson
using CityTimelineMod.Util;        // BBox, Epsg2230Transformer, V2
using CityTimelineMod.Placement;   // WaterPlacer

namespace CityTimelineMod
{
    internal static class IrvineBootstrap
    {
        // ---------- file paths (adjust if your repo lives elsewhere) ----------
        private const string LINES_PATH =
            @"C:\Projects\CitySkylines2\Irvine\cs2-realcity\dist\irvine-ca\networks\water_lines_2230_simplified.geojson";
        private const string AREAS_PATH =
            @"C:\Projects\CitySkylines2\Irvine\cs2-realcity\dist\irvine-ca\networks\water_areas_2230_simplified.geojson";
        // ----------------------------------------------------------------------

        // ---------- tweakables ----------
        private const KeyCode TOGGLE_KEY = KeyCode.F8;      // show/hide the preview overlay
        private const float HEIGHT_OFFSET_METERS = 800f;    // float lines above terrain
        private const float LINE_WIDTH_METERS = 18f;        // line thickness
        private static readonly Color LINE_COLOR = new Color(0f, 0.9f, 1f, 0.92f);
        private const int MAX_LINES = 20;                   // how many line parts to draw
        private const int MAX_POINTS = 1000;                // cap points per line
        private const float METERS_PER_FOOT = 0.3048f;      // EPSG:2230 (US survey ft) → meters
        // --------------------------------

        private static GameObject s_previewParent;
        private static PreviewToggle s_toggle;

        // Mod.cs calls this
        internal static void RunOnce() => Run();

        internal static void Run()
        {
            try
            {
                if (!File.Exists(LINES_PATH) || !File.Exists(AREAS_PATH))
                {
                    Log.Error($"IrvineBootstrap: expected files not found.\n  Lines: {LINES_PATH}\n  Areas: {AREAS_PATH}");
                    return;
                }

                // 1) Log sizes
                var linesLen = new FileInfo(LINES_PATH).Length;
                var areasLen = new FileInfo(AREAS_PATH).Length;
                Log.Info($"[Paths] Lines: {LINES_PATH}  ({linesLen:N0} bytes)");
                Log.Info($"[Paths] Areas: {AREAS_PATH}  ({areasLen:N0} bytes)");

                // 2) Feature counts (quick sanity on the raw files)
                int nLinesFeatures = GeoJson.CountFeatures(LINES_PATH);
                int nAreaFeatures  = GeoJson.CountFeatures(AREAS_PATH);
                Log.Info($"Loaded OK. Water lines features: {nLinesFeatures}, water areas features: {nAreaFeatures}");

                // 3) Parse raw feet coords
                var rawLines = GeoJson.ReadLineParts(LINES_PATH) ?? new List<List<(double x, double y)>>();
                var rawAreas = GeoJson.ReadAreaOuterRings(AREAS_PATH) ?? new List<List<(double x, double y)>>();

                int lineParts = rawLines.Count;
                int areaRings = rawAreas.Count;
                int linePoints = CountPoints(rawLines);
                int areaPoints = CountPoints(rawAreas);

                Log.Info($"Parsed {lineParts} line parts ({linePoints:N0} pts).");
                Log.Info($"Parsed {areaRings} area rings ({areaPoints:N0} pts).");

                // If both are empty, bail early with a helpful message
                if (linePoints == 0 && areaPoints == 0)
                {
                    Log.Error(
                        "[IrvineBootstrap] No coordinates parsed. " +
                        "Check that the files are in EPSG:2230 feet and that the parser helpers " +
                        "match your GeoJSON structure (LineString/MultiLineString and Polygon/MultiPolygon).");
                    return;
                }

                // 4) BBox + transformer (feet → meters/world)
                var bbox = BBox.FromParts(
                    linePoints > 0 ? rawLines : null,
                    areaPoints > 0 ? rawAreas : null);

                Log.Info($"[EPSG2230 bbox] Min=({bbox.min.x:F2},{bbox.min.y:F2})  Max=({bbox.max.x:F2},{bbox.max.y:F2})  Size=({bbox.SizeX:F2} ft × {bbox.SizeY:F2} ft)");

                var tf = new Epsg2230Transformer(bbox.min.x, bbox.min.y, METERS_PER_FOOT);
                Log.Info($"[Transform] meters/foot={METERS_PER_FOOT}  (assume 1 world unit ≈ 1 meter)");

                // 5) Build preview polylines in world units
                var samples = WaterPlacer.BuildTransformedSamples(rawLines, tf, MAX_LINES, MAX_POINTS);

                // 6) Draw overlay lines with LineRenderers
                CreateOrReplacePreview(samples);

                Log.Info($"[DebugDraw] Placed {samples.Count} LineRenderer polylines in scene.");
            }
            catch (Exception ex)
            {
                Log.Error("IrvineBootstrap failed: " + ex);
            }
        }

        private static int CountPoints(List<List<(double x, double y)>> groups)
        {
            if (groups == null) return 0;
            int total = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                var part = groups[i];
                if (part != null) total += part.Count;
            }
            return total;
        }

        private static void CreateOrReplacePreview(List<List<V2>> polylines)
        {
            if (s_previewParent != null)
                UnityEngine.Object.Destroy(s_previewParent);

            s_previewParent = new GameObject("CityTimelineMod_Preview");
            s_toggle = s_previewParent.AddComponent<PreviewToggle>();
            s_toggle.ToggleKey = TOGGLE_KEY;

            // Try a basic always-on-top Unlit material; fall back to default if lookup fails.
            var shader =
                Shader.Find("Hidden/Internal-Colored") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default"); // final fallback

            if (shader == null)
                throw new ArgumentNullException(nameof(shader), "Could not locate a simple line shader.");

            var mat = new Material(shader);
            mat.color = LINE_COLOR;
            mat.renderQueue = 5000; // overlay on top
            try { mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always); } catch { }

            foreach (var line in polylines)
            {
                if (line.Count < 2) continue;

                var go = new GameObject("Polyline");
                go.transform.SetParent(s_previewParent.transform, false);

                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = mat;
                lr.alignment = LineAlignment.View;
                lr.textureMode = LineTextureMode.Stretch;
                lr.numCornerVertices = 4;
                lr.positionCount = line.Count;
                lr.useWorldSpace = true;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.generateLightingData = false;
                lr.widthMultiplier = LINE_WIDTH_METERS;

                var tmp = new Vector3[line.Count];
                for (int i = 0; i < line.Count; i++)
                    tmp[i] = new Vector3((float)line[i].x, HEIGHT_OFFSET_METERS, (float)line[i].y);

                lr.SetPositions(tmp);
            }
        }

        private sealed class PreviewToggle : MonoBehaviour
        {
            internal KeyCode ToggleKey = KeyCode.F8;

            private void Update()
            {
                if (UnityEngine.Input.GetKeyDown(ToggleKey))
                {
                    bool next = !gameObject.activeSelf;
                    gameObject.SetActive(next);
                    Log.Info($"[PreviewToggle] {(next ? "Shown" : "Hidden")} (key {ToggleKey})");
                }
            }
        }
    }
}
