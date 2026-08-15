param([string]$Version = '0.1.1')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$paths = @(
    'Install-UFO50-CHS.cmd',
    'Uninstall-UFO50-CHS.cmd',
    'README.txt',
    'THIRD-PARTY-NOTICES.md',
    'release-config.json'
)
$paths += Get-ChildItem -LiteralPath (Join-Path $root 'installer') -Recurse -File |
    ForEach-Object { $_.FullName.Substring($root.Length + 1) }
$paths += Get-ChildItem -LiteralPath (Join-Path $root 'payload') -Recurse -File |
    ForEach-Object { $_.FullName.Substring($root.Length + 1) }

$entries = foreach ($relative in $paths | Sort-Object) {
    $fullPath = Join-Path $root $relative
    [ordered]@{
        path = $relative.Replace('\', '/')
        sha256 = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToUpperInvariant()
        size = (Get-Item -LiteralPath $fullPath).Length
    }
}
$manifestJson = [ordered]@{
    version = $Version
    generatedAt = (Get-Date).ToUniversalTime().ToString('o')
    files = @($entries)
} | ConvertTo-Json -Depth 5
$manifestJson = $manifestJson.Replace("`r`n", "`n") + "`n"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $root 'payload-manifest.json'), $manifestJson, $utf8NoBom)

Write-Host "Updated payload-manifest.json with $($entries.Count) files."
