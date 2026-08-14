Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-NormalizedHash {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Add-PathCandidate {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Candidates,
        [string]$Path
    )
    if ([string]::IsNullOrWhiteSpace($Path)) { return }
    try {
        $resolved = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
        if (-not $Candidates.Contains($resolved)) { $Candidates.Add($resolved) }
    }
    catch { }
}

function Resolve-UFO50GamePath {
    param(
        [string]$ExplicitPath,
        [Parameter(Mandatory = $true)][string]$PackageRoot
    )

    $directCandidates = New-Object 'System.Collections.Generic.List[string]'
    Add-PathCandidate -Candidates $directCandidates -Path $ExplicitPath
    Add-PathCandidate -Candidates $directCandidates -Path $PackageRoot
    Add-PathCandidate -Candidates $directCandidates -Path (Split-Path -Parent $PackageRoot)

    $steamRoots = New-Object 'System.Collections.Generic.List[string]'
    foreach ($registryPath in @(
        'HKCU:\Software\Valve\Steam',
        'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam',
        'HKLM:\SOFTWARE\Valve\Steam'
    )) {
        try {
            $steam = Get-ItemProperty -LiteralPath $registryPath -ErrorAction Stop
            Add-PathCandidate -Candidates $steamRoots -Path $steam.SteamPath
            Add-PathCandidate -Candidates $steamRoots -Path $steam.InstallPath
        }
        catch { }
    }

    foreach ($steamRoot in @($steamRoots)) {
        Add-PathCandidate -Candidates $directCandidates -Path (Join-Path $steamRoot 'steamapps\common\UFO 50')
        $libraryFile = Join-Path $steamRoot 'steamapps\libraryfolders.vdf'
        if (-not (Test-Path -LiteralPath $libraryFile)) { continue }
        $content = Get-Content -LiteralPath $libraryFile -Raw -Encoding UTF8
        foreach ($match in [regex]::Matches($content, '"path"\s+"([^"]+)"')) {
            $libraryRoot = $match.Groups[1].Value -replace '\\\\', '\'
            Add-PathCandidate -Candidates $directCandidates -Path (Join-Path $libraryRoot 'steamapps\common\UFO 50')
        }
    }

    foreach ($candidate in @($directCandidates)) {
        if (Test-Path -LiteralPath (Join-Path $candidate 'ufo50.exe')) { return $candidate }
    }

    Add-Type -AssemblyName System.Windows.Forms
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = 'Select the UFO 50 game directory that contains ufo50.exe.'
    $dialog.ShowNewFolderButton = $false
    if ($dialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        $selected = (Resolve-Path -LiteralPath $dialog.SelectedPath).Path
        if (Test-Path -LiteralPath (Join-Path $selected 'ufo50.exe')) { return $selected }
    }
    throw 'UFO 50 was not found. Extract the package and retry, or select the directory that contains ufo50.exe.'
}

function Assert-PayloadManifest {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)

    $manifestPath = Join-Path $PackageRoot 'payload-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath)) { throw 'The package is missing payload-manifest.json.' }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($entry in $manifest.files) {
        $relative = [string]$entry.path
        if ([string]::IsNullOrWhiteSpace($relative) -or $relative.Contains('..')) {
            throw "The package manifest contains an invalid path: $relative"
        }
        $fullPath = Join-Path $PackageRoot ($relative -replace '/', '\')
        if (-not (Test-Path -LiteralPath $fullPath)) { throw "The package is missing a file: $relative" }
        $actual = Get-NormalizedHash -Path $fullPath
        $expected = ([string]$entry.sha256).ToUpperInvariant()
        if ($actual -ne $expected) { throw "Package file hash mismatch: $relative" }
    }
    return $manifest
}

function Restore-UFO50Backup {
    param(
        [Parameter(Mandatory = $true)][string]$GameRoot,
        [Parameter(Mandatory = $true)][string]$BackupRoot,
        [string[]]$PayloadTextNames
    )

    $resolvedGame = (Resolve-Path -LiteralPath $GameRoot).Path
    $resolvedBackup = (Resolve-Path -LiteralPath $BackupRoot).Path
    $expectedRoot = [System.IO.Path]::GetFullPath((Join-Path $resolvedGame 'chs-backup'))
    $expectedPrefix = $expectedRoot.TrimEnd('\') + '\'
    if (-not $resolvedBackup.StartsWith($expectedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to restore from outside the game backup directory: $resolvedBackup"
    }

    $backupRecordPath = Join-Path $resolvedBackup 'backup.json'
    if (-not (Test-Path -LiteralPath $backupRecordPath)) {
        throw "The backup record is missing: $backupRecordPath"
    }
    $backupRecord = Get-Content -LiteralPath $backupRecordPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $backupDataPath = Join-Path $resolvedBackup 'data.win'
    $expectedDataHash = ([string]$backupRecord.originalDataWinSha256).ToUpperInvariant()
    if ([string]::IsNullOrWhiteSpace($expectedDataHash) -or (Get-NormalizedHash -Path $backupDataPath) -ne $expectedDataHash) {
        throw 'The backup data.win failed integrity verification.'
    }

    Copy-Item -LiteralPath $backupDataPath -Destination (Join-Path $resolvedGame 'data.win') -Force

    $targetJapanese = Join-Path $resolvedGame 'ext\JAPANESE'
    $backupJapanese = Join-Path $resolvedBackup 'ext\JAPANESE'
    New-Item -ItemType Directory -Force -Path $targetJapanese | Out-Null
    foreach ($name in @($PayloadTextNames)) {
        $target = Join-Path $targetJapanese $name
        $backup = Join-Path $backupJapanese $name
        if (Test-Path -LiteralPath $backup) {
            Copy-Item -LiteralPath $backup -Destination $target -Force
        }
        elseif (Test-Path -LiteralPath $target) {
            Remove-Item -LiteralPath $target -Force
        }
    }

    $targetFont = Join-Path $resolvedGame 'fonts\UFO50-CHS.ttf'
    $backupFont = Join-Path $resolvedBackup 'fonts\UFO50-CHS.ttf'
    if (Test-Path -LiteralPath $backupFont) {
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $targetFont) | Out-Null
        Copy-Item -LiteralPath $backupFont -Destination $targetFont -Force
    }
    elseif (Test-Path -LiteralPath $targetFont) {
        Remove-Item -LiteralPath $targetFont -Force
    }

    if ((Get-NormalizedHash -Path (Join-Path $resolvedGame 'data.win')) -ne $expectedDataHash) {
        throw 'Restored data.win failed integrity verification.'
    }
}
