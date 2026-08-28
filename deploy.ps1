param(
    [string]$Version = "2.9.0",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = [IO.Path]::GetFullPath($PSScriptRoot)
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts"))
$stageRoot = [IO.Path]::GetFullPath((Join-Path $artifactsRoot "staging\BetterPawnControlForked"))
$zipPath = [IO.Path]::GetFullPath((Join-Path $artifactsRoot ("BetterPawnControlForked-" + $Version + ".zip")))
$packagesRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot ".dotnet\.nuget\packages"))
$projectPath = Join-Path $repositoryRoot "Source\BetterPawnControlForked.csproj"
$solutionPath = Join-Path $repositoryRoot "Source\BetterPawnControlForked.sln"
$testProjectPath = Join-Path $repositoryRoot "Tests\BetterPawnControlForked.Core.Tests.csproj"
$dllPath = Join-Path $repositoryRoot "v1.6\Assemblies\BetterPawnControlForked.dll"

function Assert-UnderRepository([string]$Path, [string]$Parent) {
    $resolvedPath = [IO.Path]::GetFullPath($Path)
    $resolvedParent = [IO.Path]::GetFullPath($Parent).TrimEnd('\') + '\'
    if (-not $resolvedPath.StartsWith($resolvedParent, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Unsafe path outside expected root: $resolvedPath"
    }
}

Assert-UnderRepository $artifactsRoot $repositoryRoot
Assert-UnderRepository $stageRoot $artifactsRoot
Assert-UnderRepository $zipPath $artifactsRoot

New-Item -ItemType Directory -Force -Path $artifactsRoot, $packagesRoot | Out-Null
if (Test-Path -LiteralPath $stageRoot) {
    Remove-Item -Recurse -Force -LiteralPath $stageRoot
}
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -Force -LiteralPath $zipPath
}

dotnet restore $solutionPath --packages $packagesRoot /p:RestoreFallbackFolders=
if ($LASTEXITCODE -ne 0) { throw "Restore failed." }

dotnet build $projectPath -c $Configuration --no-restore /p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) { throw "Release build failed." }

dotnet test $testProjectPath -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw "Automated tests failed." }

Get-ChildItem -LiteralPath $repositoryRoot -Recurse -Filter *.xml |
    Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\artifacts\*" } |
    ForEach-Object {
        try {
            [xml](Get-Content -Raw -LiteralPath $_.FullName) | Out-Null
        }
        catch {
            throw "Invalid XML: $($_.FullName). $($_.Exception.Message)"
        }
    }

[xml]$project = Get-Content -Raw -LiteralPath $projectPath
$projectVersion = [string]$project.Project.PropertyGroup.Version
[xml]$versionXml = Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot "About\Version.xml")
$metadataVersion = [string]$versionXml.VersionData.overrideVersion
$dllVersion = [Reflection.AssemblyName]::GetAssemblyName($dllPath).Version.ToString(3)
if ($projectVersion -ne $Version -or $metadataVersion -ne $Version -or $dllVersion -ne $Version) {
    throw "Version mismatch. Requested=$Version Project=$projectVersion Metadata=$metadataVersion DLL=$dllVersion"
}

[xml]$english = Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot "Common\Languages\English\Keyed\BetterPawnControlForked.xml")
$duplicateKeys = $english.LanguageData.ChildNodes |
    Where-Object { $_.NodeType -eq [Xml.XmlNodeType]::Element } |
    Group-Object Name |
    Where-Object Count -gt 1
if ($duplicateKeys) {
    throw "Duplicate English translation keys: $($duplicateKeys.Name -join ', ')"
}

New-Item -ItemType Directory -Force -Path $stageRoot | Out-Null
foreach ($folder in @("About", "Common", "v1.6")) {
    Copy-Item -Recurse -Force -LiteralPath (Join-Path $repositoryRoot $folder) -Destination $stageRoot
}
Copy-Item -Force -LiteralPath (Join-Path $repositoryRoot "LoadFolders.xml") -Destination $stageRoot

Get-ChildItem -LiteralPath $stageRoot -Recurse -Filter *.pdb | Remove-Item -Force
$topLevel = Get-ChildItem -LiteralPath $stageRoot | Select-Object -ExpandProperty Name
$unexpectedTopLevel = $topLevel | Where-Object { $_ -notin @("About", "Common", "v1.6", "LoadFolders.xml") }
if ($unexpectedTopLevel) {
    throw "Unexpected package entries: $($unexpectedTopLevel -join ', ')"
}

$assemblies = @(Get-ChildItem -LiteralPath $stageRoot -Recurse -Filter *.dll)
if ($assemblies.Count -ne 1 -or $assemblies[0].FullName -notlike "*\v1.6\Assemblies\BetterPawnControlForked.dll") {
    throw "The package must contain only the v1.6 BetterPawnControlForked assembly."
}

$forbidden = Get-ChildItem -LiteralPath $stageRoot -Recurse -File |
    Where-Object { $_.Extension -in @(".cs", ".csproj", ".sln", ".pdb", ".user", ".cache") -or $_.Name -in @("project.assets.json") }
if ($forbidden) {
    throw "Source or build files entered the package: $($forbidden.FullName -join ', ')"
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$fixedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
$stream = [IO.File]::Open($zipPath, [IO.FileMode]::CreateNew)
try {
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $false)
    try {
        Get-ChildItem -LiteralPath $stageRoot -Recurse -File |
            Sort-Object { $_.FullName.Substring($stageRoot.Length) } |
            ForEach-Object {
                $relativePath = $_.FullName.Substring($stageRoot.Length).TrimStart('\').Replace('\', '/')
                $entry = $archive.CreateEntry($relativePath, [IO.Compression.CompressionLevel]::Optimal)
                $entry.LastWriteTime = $fixedTimestamp
                $input = [IO.File]::OpenRead($_.FullName)
                $output = $entry.Open()
                try {
                    $input.CopyTo($output)
                }
                finally {
                    $output.Dispose()
                    $input.Dispose()
                }
            }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $stream.Dispose()
}

$inspection = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $badEntries = $inspection.Entries | Where-Object {
        $_.FullName.StartsWith("/") -or $_.FullName.Contains("../") -or
        $_.FullName -match "\.(cs|csproj|sln|pdb|user|cache)$"
    }
    if ($badEntries) {
        throw "Invalid ZIP entries: $($badEntries.FullName -join ', ')"
    }
}
finally {
    $inspection.Dispose()
}

Remove-Item -Recurse -Force -LiteralPath ([IO.Path]::GetFullPath((Join-Path $artifactsRoot "staging")))
Write-Host "Created $zipPath"
