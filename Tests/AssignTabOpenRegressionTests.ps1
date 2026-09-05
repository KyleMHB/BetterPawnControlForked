param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '..\v1.6\Assemblies\BetterPawnControlForked.dll'),
    [string]$ManagedPath = 'E:\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Get-ChildItem -LiteralPath $ManagedPath -Filter '*.dll' | ForEach-Object {
    try {
        [void][Reflection.Assembly]::LoadFrom($_.FullName)
    }
    catch {
        # Only dependencies needed by the inspected types must load.
    }
}

$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))

function Get-CalledMethods([Reflection.MethodBase]$Method, [Reflection.Module]$Module) {
    $il = $Method.GetMethodBody().GetILAsByteArray()
    $calledMethods = @()
    for ($index = 0; $index -le $il.Length - 5; $index++) {
        if ($il[$index] -ne 0x28 -and $il[$index] -ne 0x6f) {
            continue
        }

        $token = [BitConverter]::ToInt32($il, $index + 1)
        try {
            $calledMethod = $Module.ResolveMethod($token)
            if ($null -ne $calledMethod) {
                $calledMethods += "$($calledMethod.DeclaringType.FullName).$($calledMethod.Name)"
            }
        }
        catch {
            # Operand bytes can resemble call opcodes; unresolved tokens are not calls.
        }
    }

    return $calledMethods
}

$patchType = $assembly.GetType('BetterPawnControlForked.MainTabWindow_PawnTable_OnPostOpen', $true)
$postfix = $patchType.GetMethod('Postfix', [Reflection.BindingFlags]'Static,NonPublic')

if ($null -eq $postfix) {
    throw 'Assign tab PostOpen patch does not expose its Postfix method.'
}

$calledMethods = Get-CalledMethods $postfix $assembly.ManifestModule

if ($calledMethods -contains 'BetterPawnControlForked.AssignManager.LoadState') {
    throw 'Opening Assign applies the saved policy and can overwrite changes made through other policy UIs.'
}

$uiPatchType = $assembly.GetType('BetterPawnControlForked.Patches.PawnTable_PawnTableOnGUI', $true)
$selectionMethod = $uiPatchType.GetMethod('OpenAssignPolicySelectMenu', [Reflection.BindingFlags]'Static,NonPublic')
if ($null -eq $selectionMethod) {
    throw 'The Assign policy selection UI does not expose its selection-menu method.'
}

$selectionMethods = @($selectionMethod)
$selectionCallbackName = '<OpenAssignPolicySelectMenu>b__0'
foreach ($nestedType in $uiPatchType.GetNestedTypes([Reflection.BindingFlags]'NonPublic')) {
    $selectionCallback = $nestedType.GetMethod($selectionCallbackName, [Reflection.BindingFlags]'Static,Instance,NonPublic')
    if ($null -ne $selectionCallback) {
        $selectionMethods += $selectionCallback
    }
}

$selectionCalls = @()
foreach ($method in $selectionMethods) {
    $selectionCalls += Get-CalledMethods $method $assembly.ManifestModule
}

if ($selectionCalls -notcontains 'BetterPawnControlForked.AssignManager.LoadState') {
    throw 'The Assign policy selection UI no longer applies a selected policy.'
}

Write-Output 'PASS: opening Assign does not apply a saved policy.'
Write-Output 'PASS: explicit Assign policy selection still applies saved state.'
