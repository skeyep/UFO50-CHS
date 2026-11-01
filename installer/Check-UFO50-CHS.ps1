param([string]$GamePath)

. (Join-Path $PSScriptRoot 'Common.ps1')
$packageRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$gameRoot = Resolve-UFO50GamePath -ExplicitPath $GamePath -PackageRoot $packageRoot
$markerPath = Join-Path $gameRoot 'ufo50-chs-install.json'
if (-not (Test-Path -LiteralPath $markerPath)) { throw 'No UFO50-CHS installation was found.' }
$marker = Get-Content -LiteralPath $markerPath -Raw -Encoding UTF8 | ConvertFrom-Json
$checks = @(
    @{ Path = (Join-Path $gameRoot 'data.win'); Expected = [string]$marker.installedDataWinSha256 },
    @{ Path = (Join-Path $gameRoot 'fonts\UFO50-CHS.ttf'); Expected = [string]$marker.installedFontSha256 }
)
foreach ($check in $checks) {
    if (-not (Test-Path -LiteralPath $check.Path) -or (Get-NormalizedHash -Path $check.Path) -ne $check.Expected.ToUpperInvariant()) {
        throw "Installed file does not match the installation record: $($check.Path)"
    }
}
Write-Host "UFO50-CHS version: $($marker.version)"
Write-Host "Game folder: $gameRoot"
Write-Host 'Game data and font match the installation record.'
