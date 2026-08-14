param([string]$GamePath)

. (Join-Path $PSScriptRoot 'Common.ps1')

$packageRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$gameRoot = Resolve-UFO50GamePath -ExplicitPath $GamePath -PackageRoot $packageRoot
if (Get-Process -Name 'ufo50' -ErrorAction SilentlyContinue) {
    throw 'UFO 50 is running. Close the game normally before uninstalling.'
}

$markerPath = Join-Path $gameRoot 'ufo50-chs-install.json'
if (-not (Test-Path -LiteralPath $markerPath)) {
    throw 'No UFO50-CHS installation marker was found. A safe restore location cannot be determined.'
}
$marker = Get-Content -LiteralPath $markerPath -Raw -Encoding UTF8 | ConvertFrom-Json
$backupRoot = [string]$marker.backupRoot
$payloadTextNames = @($marker.payloadTextNames | ForEach-Object { [string]$_ })
if (-not (Test-Path -LiteralPath (Join-Path $backupRoot 'data.win'))) {
    throw "The backup is incomplete. Refusing to uninstall: $backupRoot"
}

Restore-UFO50Backup -GameRoot $gameRoot -BackupRoot $backupRoot -PayloadTextNames $payloadTextNames
Remove-Item -LiteralPath $markerPath -Force

Write-Host ''
Write-Host 'UFO50-CHS was uninstalled and the original files were restored.' -ForegroundColor Green
Write-Host "The backup was retained at: $backupRoot"
