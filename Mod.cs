// Mod.cs
using System;
using Colossal.Logging;
using Game;                // UpdateSystem
using Game.Modding;        // IMod
using UnityEngine;

namespace CityTimelineMod
{
    public sealed class Mod : IMod
    {
        public static ILog Log = LogManager.GetLogger(nameof(CityTimelineMod)).SetShowsErrorsInUI(true);

        // Current CS2 mod interface signature
        public void OnLoad(UpdateSystem updateSystem)
        {
            Debug.Log("[CityTimelineMod] It loaded! (UnityEngine)");
            Log.Info("[CityTimelineMod] It loaded! (Colossal)");

            try { IrvineBootstrap.RunOnce(); }
            catch (Exception ex)
            {
                Debug.LogError("[CityTimelineMod] Bootstrap error: " + ex);
                Log.Error(ex);
            }
        }

        public void OnDispose()
        {
            Log.Info("[CityTimelineMod] Disposed.");
        }
    }
}
