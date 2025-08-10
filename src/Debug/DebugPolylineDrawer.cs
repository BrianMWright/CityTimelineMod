// src/Debug/DebugPolylineDrawer.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using CityTimelineMod.Util;

namespace CityTimelineMod.DebugView
{
    /// <summary>
    /// Lightweight in-game line drawer using Unity LineRenderer.
    /// Creates a single parent GameObject and one child LineRenderer per polyline.
    /// </summary>
    internal sealed class DebugPolylineDrawer : MonoBehaviour
    {
        private static DebugPolylineDrawer _instance;
        private Material _lineMat;
        private int _lineCounter = 0;

        /// <summary>Ensure there is exactly one drawer in the scene and return it.</summary>
        internal static DebugPolylineDrawer EnsureInScene()
        {
            if (_instance != null) return _instance;

            var go = new GameObject("[CityTimelineMod] DebugPolylineDrawer");
            // Keep this across scene reloads; safe for editor-play; fine in CS2
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DebugPolylineDrawer>();
            return _instance;
        }

        private void Awake()
        {
            // Very basic unlit material that works in player builds
            _lineMat = new Material(Shader.Find("Sprites/Default"));
        }

        /// <summary>
        /// Add a world-space polyline. V2 inputs should already be in world units (meters).
        /// We place them on XZ plane with a fixed Y offset (heightAboveTerrain).
        /// </summary>
        internal void AddPolyline(IReadOnlyList<V2> worldMeters, float heightAboveTerrain = 8f, float width = 2.0f)
        {
            if (worldMeters == null || worldMeters.Count < 2) return;

            var child = new GameObject($"polyline_{_lineCounter++:0000}");
            child.transform.SetParent(transform, worldPositionStays: false);

            var lr = child.AddComponent<LineRenderer>();
            lr.material = _lineMat;
            lr.positionCount = worldMeters.Count;
            lr.widthMultiplier = width;
            lr.numCornerVertices = 2;
            lr.numCapVertices = 2;
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.TransformZ;
            lr.textureMode = LineTextureMode.Stretch;

            // Cycle a few debug colors
            var color = NextColor();
            lr.startColor = color;
            lr.endColor   = color;

            // Unity’s ground plane is XZ; use V2.x → X, V2.y → Z
            var tmp = new Vector3[worldMeters.Count];
            for (int i = 0; i < worldMeters.Count; i++)
                tmp[i] = new Vector3((float)worldMeters[i].x, heightAboveTerrain, (float)worldMeters[i].y);

            lr.SetPositions(tmp);
        }

        private static readonly Color[] _colors = new[]
        {
            new Color(0.10f, 0.65f, 0.95f, 0.9f), // cyan-ish
            new Color(0.15f, 0.85f, 0.40f, 0.9f), // green
            new Color(0.95f, 0.55f, 0.10f, 0.9f), // orange
            new Color(0.90f, 0.20f, 0.30f, 0.9f), // red
            new Color(0.60f, 0.45f, 0.95f, 0.9f), // purple
        };
        private int _ci = 0;
        private Color NextColor()
        {
            var c = _colors[_ci % _colors.Length];
            _ci++;
            return c;
        }
    }
}
