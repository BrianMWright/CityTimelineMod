# deploy.ps1 — clean deploy for CityTimelineMod (net48)

$proj = Split-Path -Parent $MyInvocation.MyCommand.Path
$bin  = Join-Path $proj "bin\Debug\net48"

# 1) Build fresh
dotnet clean "$proj\CityTimelineMod.csproj" | Out-Null
dotnet build "$proj\CityTimelineMod.csproj" -c Debug | Out-Null

# 2) Prepare destination
$dst = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\CityTimelineMod"
Remove-Item -Recurse -Force $dst -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $dst | Out-Null

# 3) Copy ONLY the mod DLL
Copy-Item -Force (Join-Path $bin "CityTimelineMod.dll") $dst

# 4) Write a minimal manifest the loader can read
$manifest = @'
{
  "id": "com.brianwright.CityTimelineMod",
  "name": "CityTimelineMod",
  "description": "Irvine data + timeline tools",
  "version": "0.0.1",
  "type": "code",
  "assemblies": ["CityTimelineMod.dll"],
  "entryPoint": "CityTimelineMod.Mod"
}
'@
Set-Content -LiteralPath (Join-Path $dst "mod.json") -Value $manifest -Encoding UTF8

# 5) Clean up any stray temp files from previous deploys
Get-ChildItem $dst -Filter "ilpp.pid" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

# 6) Show what we deployed
Write-Host "`nDeployed to: $dst"
Get-ChildItem $dst | Select-Object Name,Length,LastWriteTime
