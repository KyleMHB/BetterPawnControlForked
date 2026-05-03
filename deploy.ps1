$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "..\_Shared\RimWorldModTools.ps1"
. $toolPath

Invoke-RimWorldModDeploy `
    -ModName "BetterPawnControlForked" `
    -SourceRoot $PSScriptRoot `
    -BuildPath (Join-Path $PSScriptRoot "Source\BetterPawnControlForked.csproj") `
    -Configuration "Release" `
    -DotNetHome (Join-Path $PSScriptRoot ".dotnet") `
    -Folders @("About", "Common", "v1.6") `
    -Files @("LoadFolders.xml") `
    -RemoveFilePatterns @("*.pdb")
