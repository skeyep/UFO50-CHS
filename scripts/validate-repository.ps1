$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path

$required = @(
    'Install-UFO50-CHS.cmd', 'Uninstall-UFO50-CHS.cmd', 'README.md', 'README.txt',
    'THIRD-PARTY-NOTICES.md', 'release-config.json', 'payload-manifest.json',
    'docs\third-party\ZPIX-README.md', 'docs\third-party\UTMT-GPL-3.0.txt',
    'installer\Common.ps1', 'installer\Install-UFO50-CHS.ps1', 'installer\Uninstall-UFO50-CHS.ps1',
    'payload\patch-font.csx',
    'source\builders\audit-game51.mjs', 'source\builders\audit-layout.py',
    'source\builders\build-game51-review.mjs', 'source\translations\game-51-human-zh.json'
)
foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $relative))) { throw "Missing required file: $relative" }
}

$payloadRoot = Join-Path $root 'payload\ext\JAPANESE'
$payload = @(Get-ChildItem -LiteralPath $payloadRoot -File)
if ($payload.Count -ne 52) { throw "Expected 52 localized text files, found $($payload.Count)." }
foreach ($name in @('0_Text.json', 'm_Text.json')) {
    if (-not (Test-Path -LiteralPath (Join-Path $payloadRoot $name))) { throw "Missing payload file: $name" }
}

$forbidden = @(Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object {
    $_.FullName -notmatch '(?i)(\\|/)(?:\.git|dist)(\\|/)' -and (
        $_.Extension -in @('.win', '.ttf', '.otf', '.exe') -or
        $_.FullName -match '(?i)(\\|/)(ENGLISH|reference|all-code|private)(\\|/)'
    )
})
if ($forbidden.Count) { throw "Forbidden public files: $($forbidden.FullName -join ', ')" }

$tokens = $null
foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.ps1') {
    $errors = $null
    [void][Management.Automation.Language.Parser]::ParseFile($file.FullName, [ref]$tokens, [ref]$errors)
    if ($errors.Count) { throw "PowerShell parse error in $($file.FullName): $($errors[0].Message)" }
}

foreach ($file in Get-ChildItem -LiteralPath (Join-Path $root 'source\translations') -File -Filter '*.json') {
    $null = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
}
$translationSources = @(Get-ChildItem -LiteralPath (Join-Path $root 'source\translations') -File -Filter '*.json')
if ($translationSources.Count -ne 53) { throw "Expected 53 translation sources, found $($translationSources.Count)." }
$game51Source = Get-Content -LiteralPath (Join-Path $root 'source\translations\game-51-human-zh.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if (@($game51Source.PSObject.Properties).Count -ne 547) { throw 'Unexpected game 51 translation count.' }
$metaPayload = Get-Content -LiteralPath (Join-Path $payloadRoot 'm_Text.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$game51PayloadCount = @($metaPayload.PSObject.Properties | Where-Object Name -like 'game_51_*').Count
if ($game51PayloadCount -ne 1601) { throw "Unexpected game 51 payload count: $game51PayloadCount." }
foreach ($protocol in @{
    game_internal_name_51 = 'MIAS'; game_cheat_51_0 = 'GREGMILK'; game_cheat_51_1 = 'EXECMIAS';
    game_cheat_51_2 = 'HIDEHOLE'; game_cheat_51_3 = 'SOAPSTOP'; game_cheat_51_4 = '19880229'
}.GetEnumerator()) {
    if ([string]$metaPayload.($protocol.Key) -ne [string]$protocol.Value) {
        throw "Game 51 protocol mismatch: $($protocol.Key)"
    }
}
$config = Get-Content -LiteralPath (Join-Path $root 'release-config.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$config.version -ne '0.2.0') { throw 'Unexpected release version.' }

$manifest = Get-Content -LiteralPath (Join-Path $root 'payload-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($entry in $manifest.files) {
    $relative = [string]$entry.path
    if ($relative.Contains('..')) { throw "Invalid manifest path: $relative" }
    $path = Join-Path $root ($relative -replace '/', '\')
    if (-not (Test-Path -LiteralPath $path)) { throw "Manifest file missing: $relative" }
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToUpperInvariant()
    if ($hash -ne ([string]$entry.sha256).ToUpperInvariant()) { throw "Manifest hash mismatch: $relative" }
}

$privatePatterns = '(?i)C:\\Users\\|D:\\SteamLibrary\\|skeyep@qq\.com'
foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object { $_.Extension -in @('.md','.txt','.ps1','.cmd','.json','.mjs','.csx','.yml') }) {
    if ($file.FullName -match '(?i)(\\|/)(?:\.git|dist)(\\|/)') { continue }
    if (Select-String -LiteralPath $file.FullName -Pattern $privatePatterns -Quiet) {
        throw "Local path or private identifier found in public file: $($file.FullName)"
    }
}

Write-Host "Repository validation passed: $($payload.Count) payload files, $($translationSources.Count) translation sources."
