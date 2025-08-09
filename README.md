# CityTimelineMod

**Bringing real-world geography and history into Cities: Skylines II.**  

CityTimelineMod is a Cities: Skylines II mod that can load, process, and visualize real-world geographic data inside the game. The initial focus is importing the **San Diego Creek network** from a GeoJSON file, laying the groundwork for future features like timeline-based city development and historical overlays.  

---

## ✨ Features (Current)
- ✅ Loads successfully in Cities: Skylines II (tested August 2025)
- ✅ Ready for integration with GeoJSON parsing
- ✅ Structured mod project with automated deployment to the CS2 mods folder

---

## 🚀 Planned
- Import and render the San Diego Creek network in-game
- Add support for multiple geographic datasets
- Timeline-based playback of city growth and events
- Integration with custom UI elements for data toggling

---

## 📂 Project Structure
```
CityTimelineMod/
├── src/
│   └── Importers/
│       └── GeoJson.cs    # GeoJSON feature counting logic
├── Properties/
├── CityTimelineMod.csproj
└── README.md
```

---

## 🛠 Development

### Requirements
- **.NET SDK** (version per CS2 modding requirements)
- **Cities: Skylines II** installed (Steam or other supported platform)
- Access to `CSII_TOOLPATH` modding tools

---

### Build & Deploy
```powershell
# From project root
dotnet build
dotnet publish

# Deploy DLL to CS2 Mods folder
Copy-Item .\bin\Debug\net48\CityTimelineMod.dll "$Env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\CityTimelineMod\" -Force
```

---

### Enabling the Mod
1. Launch Cities: Skylines II
2. Go to **Settings → Mods**
3. Enable **CityTimelineMod**

---

## ⚙️ Setup for Developers
If you are cloning this repository for the first time:

1. Ensure you have the **.NET SDK** and **CS2 mod tools** installed
2. Set the environment variable for the tool path:
   ```powershell
   setx CSII_TOOLPATH "C:\Path\To\CS2ModTools"
   ```
3. Build the mod:
   ```powershell
   dotnet build
   ```
4. Deploy the DLL to your CS2 Mods folder:
   ```powershell
   Copy-Item .\bin\Debug\net48\CityTimelineMod.dll "$Env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\CityTimelineMod\" -Force
   ```
5. Enable it in-game via **Settings → Mods**

---

## 📜 License
MIT License — feel free to use, modify, and share.
