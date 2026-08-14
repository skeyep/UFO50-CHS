param([string]$Version = '0.1.0')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
& (Join-Path $PSScriptRoot 'validate-repository.ps1')
$config = Get-Content -LiteralPath (Join-Path $root 'release-config.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$config.version -ne $Version) { throw 'Version mismatch.' }

$dist = Join-Path $root 'dist'
$stage = Join-Path $dist ("stage-" + [guid]::NewGuid().ToString('N'))
$name = "UFO50-CHS-v$Version-test"
$package = Join-Path $stage $name
$zip = Join-Path $dist ($name + '.zip')
New-Item -ItemType Directory -Force -Path $package | Out-Null

try {
    foreach ($item in @('Install-UFO50-CHS.cmd','Uninstall-UFO50-CHS.cmd','README.txt','THIRD-PARTY-NOTICES.md','release-config.json','payload-manifest.json','installer','payload')) {
        Copy-Item -LiteralPath (Join-Path $root $item) -Destination $package -Recurse
    }
    $forbidden = @(Get-ChildItem -LiteralPath $package -Recurse -File | Where-Object { $_.Extension -in @('.win','.ttf','.otf','.exe') })
    if ($forbidden.Count) { throw 'Release package contains forbidden files.' }
    if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
    Compress-Archive -LiteralPath $package -DestinationPath $zip -CompressionLevel Optimal
    $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToUpperInvariant()
    "$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath ($zip + '.sha256') -Encoding ASCII
    Write-Host "Created $zip"
    Write-Host "SHA-256 $hash"
}
finally {
    if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
}
