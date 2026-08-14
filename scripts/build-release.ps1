param(
    [string]$Version = '0.1.0',
    [string]$DependencyCache
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$config = Get-Content -LiteralPath (Join-Path $root 'release-config.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$config.version -ne $Version) { throw 'Version mismatch.' }
if ([string]::IsNullOrWhiteSpace($DependencyCache)) {
    $DependencyCache = Join-Path $env:LOCALAPPDATA 'UFO50-CHS\cache'
}
New-Item -ItemType Directory -Force -Path $DependencyCache | Out-Null

function Get-ReleaseDependency {
    param(
        [Parameter(Mandatory = $true)][string]$Uri,
        [Parameter(Mandatory = $true)][string]$Destination,
        [Parameter(Mandatory = $true)][string]$ExpectedSha256
    )
    $expected = $ExpectedSha256.ToUpperInvariant()
    if (Test-Path -LiteralPath $Destination) {
        $existing = (Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash.ToUpperInvariant()
        if ($existing -eq $expected) { return $Destination }
        Remove-Item -LiteralPath $Destination -Force
    }
    $download = $Destination + '.download'
    if (Test-Path -LiteralPath $download) { Remove-Item -LiteralPath $download -Force }
    Invoke-WebRequest -UseBasicParsing -Uri $Uri -OutFile $download
    $actual = (Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash.ToUpperInvariant()
    if ($actual -ne $expected) {
        Remove-Item -LiteralPath $download -Force
        throw "Dependency hash mismatch. Expected $expected, got $actual."
    }
    Move-Item -LiteralPath $download -Destination $Destination
    return $Destination
}

$zpixFile = Get-ReleaseDependency -Uri $config.zpix.url -Destination (Join-Path $DependencyCache "zpix-$($config.zpix.version).ttf") -ExpectedSha256 $config.zpix.sha256
$utmtFile = Get-ReleaseDependency -Uri $config.utmt.url -Destination (Join-Path $DependencyCache "UTMT-CLI-$($config.utmt.version)-Windows.zip") -ExpectedSha256 $config.utmt.sha256
$utmtSource = Get-ReleaseDependency -Uri $config.utmt.sourceUrl -Destination (Join-Path $DependencyCache "UndertaleModTool-$($config.utmt.version)-source.zip") -ExpectedSha256 $config.utmt.sourceSha256

& (Join-Path $PSScriptRoot 'validate-repository.ps1')

$dist = Join-Path $root 'dist'
$stage = Join-Path $dist ("stage-" + [guid]::NewGuid().ToString('N'))
$name = "UFO50-CHS-v$Version-test"
$package = Join-Path $stage $name
$zip = Join-Path $dist ($name + '.zip')
New-Item -ItemType Directory -Force -Path $package | Out-Null

try {
    foreach ($item in @('Install-UFO50-CHS.cmd','Uninstall-UFO50-CHS.cmd','README.txt','THIRD-PARTY-NOTICES.md','release-config.json','installer','payload')) {
        Copy-Item -LiteralPath (Join-Path $root $item) -Destination $package -Recurse
    }

    $zpixRoot = Join-Path $package 'third-party\zpix'
    $utmtRoot = Join-Path $package 'third-party\UndertaleModTool'
    New-Item -ItemType Directory -Force -Path $zpixRoot,$utmtRoot | Out-Null
    Copy-Item -LiteralPath $zpixFile -Destination (Join-Path $zpixRoot 'zpix.ttf')
    Copy-Item -LiteralPath (Join-Path $root 'docs\third-party\ZPIX-README.md') -Destination (Join-Path $zpixRoot 'README-and-license.md')
    Copy-Item -LiteralPath $utmtFile -Destination (Join-Path $utmtRoot "UTMT_CLI_v$($config.utmt.version)-Windows.zip")
    Copy-Item -LiteralPath $utmtSource -Destination (Join-Path $utmtRoot "UndertaleModTool-$($config.utmt.version)-source.zip")
    Copy-Item -LiteralPath (Join-Path $root 'docs\third-party\UTMT-GPL-3.0.txt') -Destination (Join-Path $utmtRoot 'LICENSE-GPL-3.0.txt')

    $entries = foreach ($file in Get-ChildItem -LiteralPath $package -Recurse -File | Sort-Object FullName) {
        $relative = $file.FullName.Substring($package.Length + 1).Replace('\', '/')
        [ordered]@{
            path = $relative
            sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToUpperInvariant()
            size = $file.Length
        }
    }
    [ordered]@{
        version = $Version
        generatedAt = (Get-Date).ToUniversalTime().ToString('o')
        files = @($entries)
    } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $package 'payload-manifest.json') -Encoding UTF8

    $forbidden = @(Get-ChildItem -LiteralPath $package -Recurse -File | Where-Object {
        $_.Extension -eq '.win' -or
        $_.Name -ieq 'ufo50.exe' -or
        $_.FullName -match '(?i)(\\|/)(ENGLISH|reference|all-code|private|\.git)(\\|/)'
    })
    if ($forbidden.Count) { throw "Release package contains forbidden files: $($forbidden.FullName -join ', ')" }
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
