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
    # also copy bundle to the installer root with expected name
    Copy-Item -Path "$OutputDir\Bundle.exe" -Destination "$here\MachShipScreensaverBundle.exe" -Force

# Copy MSI to installer root so CI upload step can find it at installer\Product.msi
if (Test-Path "$OutputDir\Product.msi") {
    Copy-Item -Path "$OutputDir\Product.msi" -Destination "$here\Product.msi" -Force
}
}

Pop-Location
