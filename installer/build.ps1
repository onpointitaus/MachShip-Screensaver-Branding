param(
    [string]$OutputDir = "publish",
    [string]$ProjectExe = "MachShip.Screensaver.exe"
)

Set-StrictMode -Version Latest

$here = Split-Path -Parent $MyInvocation.MyCommand.Definition
Push-Location $here

if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }

# Assume WiX is on PATH (CI will install it). Build Product.wxs
& candle.exe -dOutputPath=$OutputDir Product.wxs -o "$OutputDir\Product.wixobj"
& light.exe -out "$OutputDir\Product.msi" "$OutputDir\Product.wixobj"

# Build bundle if Bundle.wxs present
if (Test-Path Bundle.wxs) {
    & candle.exe -dOutputPath=$OutputDir Bundle.wxs -o "$OutputDir\Bundle.wixobj"
    & light.exe -out "$OutputDir\Bundle.exe" "$OutputDir\Bundle.wixobj"
}

Pop-Location
