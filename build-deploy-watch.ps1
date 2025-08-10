# build-deploy-watch.ps1
param (
    [string]$Config = "Debug"
)

$modName = "CityTimelineMod"
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$buildDll = Join-Path $projectDir "bin\$Config\net48\$modName.dll"
$deployDir = Join-Path $env:LOCALAPPDATA "..\LocalLow\Colossal Order\Cities Skylines II\Mods\$modName"
$playerLog = Join-Path $env:LOCALAPPDATA "..\LocalLow\Colossal Order\Cities Skylines II\Player.log"

Write-Host "=== Building $modName in $Config mode ===" -ForegroundColor Cyan
dotnet build -c $Config
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-Not (Test-Path $buildDll)) {
    Write-Host "ERROR: Built DLL not found: $buildDll" -ForegroundColor Red
    exit 1
}

Write-Host "=== Deploying DLL to: $deployDir ===" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $deployDir | Out-Null
Copy-Item $buildDll $deployDir -Force

Write-Host "=== Clearing old Player.log ===" -ForegroundColor Cyan
if (Test-Path $playerLog) {
    Clear-Content $playerLog
}

Write-Host "=== Watching Player.log ===" -ForegroundColor Cyan
Get-Content $playerLog -Wait -Tail 0
