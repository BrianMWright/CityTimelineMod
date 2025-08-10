// src/Mod.cs
using Game.Modding;
using Game;

namespace CityTimelineMod
{
    public sealed class Mod : IMod
    {
        public string Name => "CityTimelineMod";
        public string Description => "Realtime data → in-game placement scaffold (Irvine proof-of-concept)";

        public void OnLoad(UpdateSystem updateSystem)
        {
            // Kick off our one-time bootstrap (parses GeoJSON, transforms, draws debug lines)
            IrvineBootstrap.RunOnce();
        }

        public void OnDispose()
        {
            // No-op for now; add cleanup if we start registering systems or allocating resources
        }
    }
}
