param([string]$GamePath)

. (Join-Path $PSScriptRoot 'Common.ps1')
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$packageRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$configPath = Join-Path $packageRoot 'release-config.json'
if (-not (Test-Path -LiteralPath $configPath)) { throw 'The package is missing release-config.json.' }
$config = Get-Content -LiteralPath $configPath -Raw -Encoding UTF8 | ConvertFrom-Json
$null = Assert-PayloadManifest -PackageRoot $packageRoot
$gameRoot = Resolve-UFO50GamePath -ExplicitPath $GamePath -PackageRoot $packageRoot

if (Get-Process -Name 'ufo50' -ErrorAction SilentlyContinue) {
    throw 'UFO 50 is running. Close the game normally before installing.'
}

$dataWin = Join-Path $gameRoot 'data.win'
$exePath = Join-Path $gameRoot 'ufo50.exe'
if (-not (Test-Path -LiteralPath $exePath) -or -not (Test-Path -LiteralPath $dataWin)) {
    throw "The selected directory is not a complete UFO 50 installation: $gameRoot"
}

$markerPath = Join-Path $gameRoot 'ufo50-chs-install.json'
if (Test-Path -LiteralPath $markerPath) {
    throw 'UFO50-CHS is already installed. Run Uninstall-UFO50-CHS.cmd before installing another version.'
}

$currentDataHash = Get-NormalizedHash -Path $dataWin
$supportedHash = ([string]$config.supportedDataWinSha256).ToUpperInvariant()
if ($currentDataHash -ne $supportedHash) {
    throw "Unsupported data.win version.`nCurrent:  $currentDataHash`nRequired: $supportedHash`nVerify the game files in Steam, then retry."
}

$payloadJapanese = Join-Path $packageRoot 'payload\ext\JAPANESE'
$patchScript = Join-Path $packageRoot 'payload\patch-font.csx'
if (-not (Test-Path -LiteralPath $payloadJapanese) -or -not (Test-Path -LiteralPath $patchScript)) {
    throw 'The package is missing the localization payload or data.win patch script.'
}
$payloadFiles = @(Get-ChildItem -LiteralPath $payloadJapanese -File | Sort-Object Name)
if ($payloadFiles.Count -ne 52) { throw "Unexpected localization file count: $($payloadFiles.Count). Expected 52." }

$cacheRoot = Join-Path $env:LOCALAPPDATA 'UFO50-CHS\cache'
New-Item -ItemType Directory -Force -Path $cacheRoot | Out-Null
$zpixPath = Join-Path $cacheRoot "zpix-$($config.zpix.version).ttf"
$utmtZip = Join-Path $cacheRoot "UTMT-CLI-$($config.utmt.version)-Windows.zip"
$null = Get-VerifiedDownload -Uri $config.zpix.url -Destination $zpixPath -ExpectedSha256 $config.zpix.sha256
$null = Get-VerifiedDownload -Uri $config.utmt.url -Destination $utmtZip -ExpectedSha256 $config.utmt.sha256

$utmtRoot = Join-Path $cacheRoot "UTMT-CLI-$($config.utmt.version)"
$utmtExe = Join-Path $utmtRoot 'UndertaleModCli.exe'
if (-not (Test-Path -LiteralPath $utmtExe)) {
    New-Item -ItemType Directory -Force -Path $utmtRoot | Out-Null
    Expand-Archive -LiteralPath $utmtZip -DestinationPath $utmtRoot -Force
    $found = Get-ChildItem -LiteralPath $utmtRoot -Recurse -File -Filter 'UndertaleModCli.exe' | Select-Object -First 1
    if ($null -eq $found) { throw 'UndertaleModCli.exe was not found after extraction.' }
    $utmtExe = $found.FullName
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupRoot = Join-Path $gameRoot "chs-backup\ufo50-chs-$($config.version)-$stamp"
$backupJapanese = Join-Path $backupRoot 'ext\JAPANESE'
New-Item -ItemType Directory -Force -Path $backupJapanese | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $backupRoot 'fonts') | Out-Null
Copy-Item -LiteralPath $dataWin -Destination (Join-Path $backupRoot 'data.win')

$targetJapanese = Join-Path $gameRoot 'ext\JAPANESE'
foreach ($payloadFile in $payloadFiles) {
    $existing = Join-Path $targetJapanese $payloadFile.Name
    if (Test-Path -LiteralPath $existing) {
        Copy-Item -LiteralPath $existing -Destination (Join-Path $backupJapanese $payloadFile.Name)
    }
}
$targetFont = Join-Path $gameRoot 'fonts\UFO50-CHS.ttf'
if (Test-Path -LiteralPath $targetFont) {
    Copy-Item -LiteralPath $targetFont -Destination (Join-Path $backupRoot 'fonts\UFO50-CHS.ttf')
}

$backupRecord = [ordered]@{
    version = [string]$config.version
    createdAt = (Get-Date).ToString('o')
    originalDataWinSha256 = $currentDataHash
    payloadTextNames = @($payloadFiles.Name)
}
$backupRecord | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $backupRoot 'backup.json') -Encoding UTF8

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("UFO50-CHS-" + [guid]::NewGuid().ToString('N'))
$patchedData = Join-Path $tempRoot 'data.win'
$installationStarted = $false
$previousFontSize = $env:UFO50_CHS_FONT_SIZE_PX
try {
    New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
    $env:UFO50_CHS_FONT_SIZE_PX = '8'
    Write-Host 'Building the localized data.win. This may take a moment...'
    & $utmtExe load $dataWin -s $patchScript -o $patchedData
    if ($LASTEXITCODE -ne 0) { throw "The data.win patch build failed with exit code $LASTEXITCODE." }

    $patchedHash = Get-NormalizedHash -Path $patchedData
    $expectedPatchedHash = ([string]$config.patchedDataWinSha256).ToUpperInvariant()
    if ($patchedHash -ne $expectedPatchedHash) {
        throw "Patched data.win hash mismatch. Expected $expectedPatchedHash, got $patchedHash."
    }

    $installationStarted = $true
    New-Item -ItemType Directory -Force -Path (Join-Path $gameRoot 'fonts') | Out-Null
    New-Item -ItemType Directory -Force -Path $targetJapanese | Out-Null
    Copy-Item -LiteralPath $patchedData -Destination $dataWin -Force
    Copy-Item -LiteralPath $zpixPath -Destination $targetFont -Force
    foreach ($payloadFile in $payloadFiles) {
        Copy-Item -LiteralPath $payloadFile.FullName -Destination (Join-Path $targetJapanese $payloadFile.Name) -Force
    }

    if ((Get-NormalizedHash -Path $dataWin) -ne $expectedPatchedHash) { throw 'Installed data.win failed integrity verification.' }
    if ((Get-NormalizedHash -Path $targetFont) -ne ([string]$config.zpix.sha256).ToUpperInvariant()) { throw 'Installed Zpix failed integrity verification.' }

    $marker = [ordered]@{
        version = [string]$config.version
        installedAt = (Get-Date).ToString('o')
        backupRoot = $backupRoot
        installedDataWinSha256 = $expectedPatchedHash
        installedFontSha256 = ([string]$config.zpix.sha256).ToUpperInvariant()
        payloadTextNames = @($payloadFiles.Name)
    }
    $marker | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $markerPath -Encoding UTF8
    Write-Host ''
    Write-Host "UFO50-CHS $($config.version) installed successfully." -ForegroundColor Green
    Write-Host "Game directory: $gameRoot"
    Write-Host "Original-file backup: $backupRoot"
    Write-Host 'Font: official unmodified Zpix. The game patch applies the 1px baseline adjustment.'
    Write-Host 'After launching the game, open Language and select Chinese (the former Japanese slot).'
    $localizationCredit = 'Simplified Chinese localization by Skeyep_' + [char]0x76EE + [char]0x76EE + '.'
    Write-Host $localizationCredit
    Write-Host 'For study and community exchange only. Commercial use is prohibited.'
}
catch {
    if ($installationStarted) {
        Write-Warning 'Installation failed after file replacement started. Restoring the original files...'
        Restore-UFO50Backup -GameRoot $gameRoot -BackupRoot $backupRoot -PayloadTextNames @($payloadFiles.Name)
    }
    throw
}
finally {
    $env:UFO50_CHS_FONT_SIZE_PX = $previousFontSize
    $resolvedTempBase = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
    $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
    if ($resolvedTemp.StartsWith($resolvedTempBase, [System.StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
        Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
    }
}
