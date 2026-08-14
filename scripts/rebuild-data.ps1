param(
    [Parameter(Mandatory = $true)][string]$BaselineDataWin,
    [Parameter(Mandatory = $true)][string]$UtmtExe,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$config = Get-Content -LiteralPath (Join-Path $root 'release-config.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$baseline = (Resolve-Path -LiteralPath $BaselineDataWin).Path
$utmt = (Resolve-Path -LiteralPath $UtmtExe).Path
$patch = Join-Path $root 'payload\patch-font.csx'
$output = [System.IO.Path]::GetFullPath($OutputPath)

$baselineHash = (Get-FileHash -LiteralPath $baseline -Algorithm SHA256).Hash.ToUpperInvariant()
if ($baselineHash -ne ([string]$config.supportedDataWinSha256).ToUpperInvariant()) {
    throw "Original data.win hash mismatch. Expected $($config.supportedDataWinSha256), got $baselineHash."
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $output) | Out-Null
$previous = $env:UFO50_CHS_FONT_SIZE_PX
try {
    $env:UFO50_CHS_FONT_SIZE_PX = '8'
    & $utmt load $baseline -s $patch -o $output
    if ($LASTEXITCODE -ne 0) { throw "UndertaleModTool exited with code $LASTEXITCODE." }
}
finally {
    $env:UFO50_CHS_FONT_SIZE_PX = $previous
}

$outputHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash.ToUpperInvariant()
if ($outputHash -ne ([string]$config.patchedDataWinSha256).ToUpperInvariant()) {
    throw "Patched data.win hash mismatch. Expected $($config.patchedDataWinSha256), got $outputHash."
}
Write-Host "Verified patched data.win: $outputHash"
