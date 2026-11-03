param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '..\v1.6\Assemblies\BetterPawnControlForked.dll'),
    [string]$ManagedPath = 'E:\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed',
    [switch]$SkipCompiledChecks
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Compile the actual opening patch and session helper against hostile manager
# doubles. Any attempt to enumerate pawns or apply saved state fails the test.
$doubles = @'
using System;
using System.Collections.Generic;
namespace HarmonyLib {
    public class HarmonyPatch : Attribute { public HarmonyPatch(Type type, string name) {} }
}
namespace RimWorld {
    public class MainTabWindow_PawnTable { public void PostOpen() {} }
    public class MainTabWindow_Assign : MainTabWindow_PawnTable {}
    public class MainTabWindow_Schedule : MainTabWindow_PawnTable {}
    public class MainTabWindow_Work : MainTabWindow_PawnTable {}
    public class MainTabWindow_Animals : MainTabWindow_PawnTable {}
    public class MainTabWindow_Mechs : MainTabWindow_PawnTable {}
}
namespace AnimalTab { public class MainTabWindow_Animals : RimWorld.MainTabWindow_PawnTable {} }
namespace Numbers {
    public class MainTabWindow_Numbers : RimWorld.MainTabWindow_PawnTable {}
    public class MainTabWindow_NumbersAnimals : RimWorld.MainTabWindow_PawnTable {}
}
namespace BetterPawnControlForked {
    public class Pawn { public int Group = 2; public string Mode = "live"; public string Area = "live"; }
    public class Policy {}
    public class OtherManager {
        public static bool showPaste;
        public static List<object> links = new List<object>();
        public static List<Pawn> Colonists() { return new List<Pawn>(); }
        public static List<Pawn> Robots() { return new List<Pawn>(); }
        public static Policy GetActivePolicy() { return new Policy(); }
        public static void LoadState(List<object> links, List<Pawn> pawns, Policy policy) {}
    }
    public class AssignManager : OtherManager {}
    public class ScheduleManager : OtherManager {}
    public class WorkManager : OtherManager {}
    public class WeaponsManager : OtherManager {}
    public class RobotManager : OtherManager {}
    public class AnimalManager {
        public static List<object> links = new List<object>();
        public static List<Pawn> Animals() { throw new Exception("Opening Animals enumerated live state."); }
        public static Policy GetActivePolicy() { return new Policy(); }
        public static void LoadState(List<object> links, List<Pawn> pawns, Policy policy) { throw new Exception("Opening Animals applied saved state."); }
    }
    public class MechManager {
        public static bool showPaste;
        public static List<object> links = new List<object>();
        public static List<Pawn> Mechs() { throw new Exception("Opening Mechs enumerated live state."); }
        public static Policy GetActivePolicy() { return new Policy(); }
        public static void LoadState(List<object> links, List<Pawn> pawns, Policy policy) { throw new Exception("Opening Mechs applied saved state."); }
    }
    public static class PawnCompatibility {
        public static bool SupportsSchedule(Pawn p) { return true; }
        public static bool SupportsWork(Pawn p) { return true; }
        public static bool SupportsWeapons(Pawn p) { return true; }
    }
    public static class Widget_ModsAvailable {
        public const string WORKTAB_MAINTAB = "WorkTab.MainTabWindow_WorkTab";
        public const string NUMBERS_MAINTAB = "Numbers.MainTabWindow_Numbers";
        public const string ANIMALTAB_MAINTAB = "AnimalTab.MainTabWindow_Animals";
        public const string NUMBERS_DEFNAME = "Numbers.MainTabWindow_NumbersAnimals";
        public const string WEAPONSTAB_MAINTAB = "WeaponsTabReborn.MainTabWindow_Weapons";
        public const string AIROBOTX2_MAINTAB = "AIRobot.X2_MainTabWindow_Robots";
        public static bool DisableBPCOnWorkTab, WTBAvailable, MiscRobotsAvailable;
    }
    public static class TabStateOpenBehavior {
        public static void Run() {
            var patch = typeof(MainTabWindow_PawnTable_OnPostOpen).GetMethod("Postfix", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            foreach (var window in new RimWorld.MainTabWindow_PawnTable[] {
                new RimWorld.MainTabWindow_Animals(), new AnimalTab.MainTabWindow_Animals(),
                new Numbers.MainTabWindow_Numbers(), new Numbers.MainTabWindow_NumbersAnimals(), new RimWorld.MainTabWindow_Mechs() }) {
                MechManager.showPaste = true;
                patch.Invoke(null, new object[] { window });
                if (PawnTableSession.ActiveWindowType != window.GetType()) throw new Exception("Opening lost the outgoing capture session.");
                if (window is RimWorld.MainTabWindow_Mechs && MechManager.showPaste) throw new Exception("Mech paste UI was not reset.");
                PawnTableSession.Close(window.GetType());
                if (PawnTableSession.ActiveWindowType != null) throw new Exception("Closing left a stale session.");
            }
        }
    }
}
'@
$root = Split-Path -Parent $PSScriptRoot
$openPatch = Get-Content -Raw -LiteralPath (Join-Path $root 'Source\Patches\MainTabWindow_PawnTable_PostOpen.cs')
$session = Get-Content -Raw -LiteralPath (Join-Path $root 'Source\Helpers\PawnTableSession.cs')
if ($PSVersionTable.PSVersion.Major -lt 7) {
    # Windows PowerShell's older C# compiler lacks nameof. Only the attribute's
    # compile-time name is normalized; the production method bodies are unchanged.
    $openPatch = $openPatch.Replace('nameof(MainTabWindow_PawnTable.PostOpen)', '"PostOpen"')
}
Add-Type -TypeDefinition ('using System; using System.Collections.Generic;' + [Environment]::NewLine + $openPatch + [Environment]::NewLine + $session.Replace('using System;', '') + [Environment]::NewLine + $doubles.Replace('using System;', '').Replace('using System.Collections.Generic;', ''))
[BetterPawnControlForked.TabStateOpenBehavior]::Run()
Write-Output 'PASS: production tab opening preserves external edits in Animals, Animal Tab, Numbers and Mechs; session and paste UI remain correct.'
if ($SkipCompiledChecks) { return }

# Inspect real compiled routes too: selection, outgoing capture, paste, emergency
# and deletion fallback must still reach the production managers.
Get-ChildItem -LiteralPath $ManagedPath -Filter '*.dll' | ForEach-Object {
    try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))
$opcodes = @{}
foreach ($field in [Reflection.Emit.OpCodes].GetFields([Reflection.BindingFlags]'Public,Static')) {
    $opcode = $field.GetValue($null)
    $opcodes[[int]$opcode.Value -band 0xffff] = $opcode
}

function Get-MethodOperations([Reflection.MethodBase]$Method) {
    $body = $Method.GetMethodBody()
    if ($null -eq $body) { return }
    $il = $body.GetILAsByteArray()
    $index = 0
    while ($index -lt $il.Length) {
        $value = [int]$il[$index++]
        if ($value -eq 0xfe) { $value = 0xfe00 -bor [int]$il[$index++] }
        $op = $opcodes[$value]
        if ($null -eq $op) { throw "Invalid opcode in $($Method.Name)." }
        $size = switch ($op.OperandType.ToString()) {
            'InlineNone' { 0 }
            'ShortInlineBrTarget' { 1 }
            'ShortInlineI' { 1 }
            'ShortInlineVar' { 1 }
            'InlineVar' { 2 }
            'InlineI8' { 8 }
            'InlineR' { 8 }
            'InlineSwitch' { 4 + 4 * [BitConverter]::ToInt32($il, $index) }
            default { 4 }
        }
        if ($op.OperandType.ToString() -eq 'InlineMethod') {
            $called = $Method.Module.ResolveMethod([BitConverter]::ToInt32($il, $index))
            "$($called.DeclaringType.FullName).$($called.Name)"
        }
        if ($op.OperandType.ToString() -eq 'InlineField') {
            $field = $Method.Module.ResolveField([BitConverter]::ToInt32($il, $index))
            "$($op.Name):$($field.DeclaringType.FullName).$($field.Name)"
        }
        $index += $size
    }
}

function Get-Operations([string]$TypeName, [string]$MethodName) {
    $type = $assembly.GetType($TypeName, $true)
    $methods = @($type.GetMethods([Reflection.BindingFlags]'Static,Instance,Public,NonPublic') | Where-Object Name -eq $MethodName)
    foreach ($nested in $type.GetNestedTypes([Reflection.BindingFlags]'Public,NonPublic')) {
        $methods += @($nested.GetMethods([Reflection.BindingFlags]'Static,Instance,Public,NonPublic') | Where-Object { $_.Name.StartsWith("<$MethodName>") })
    }
    if ($methods.Count -eq 0) { throw "Missing production method $TypeName.$MethodName." }
    foreach ($method in $methods) { Get-MethodOperations $method }
}
function Assert-Calls([string]$TypeName, [string]$MethodName, [string]$Expected) {
    if (@(Get-Operations $TypeName $MethodName) -notcontains $Expected) { throw "$TypeName.$MethodName lost $Expected." }
}

$opening = @(Get-Operations 'BetterPawnControlForked.MainTabWindow_PawnTable_OnPostOpen' 'Postfix')
foreach ($manager in 'AnimalManager', 'MechManager') {
    $qualified = "BetterPawnControlForked.$manager"
    if ($opening -contains "$qualified.LoadState") { throw "Compiled tab opening still applies $manager state." }
    $tab = if ($manager -eq 'AnimalManager') { 'Animal' } else { 'Mech' }
    $ui = 'BetterPawnControlForked.Patches.PawnTable_PawnTableOnGUI'
    Assert-Calls $ui "Open${tab}PolicySelectMenu" "$qualified.LoadState"
    Assert-Calls $ui "DrawBPCButtons_${tab}Tab" "$qualified.SaveCurrentState"
    Assert-Calls $ui "Draw${tab}BPCButtons" "$qualified.LoadState"
    Assert-Calls 'BetterPawnControlForked.AlertManager' 'LoadState' "$qualified.LoadState"
    Assert-Calls 'BetterPawnControlForked.Patches.Window_PreClose' 'Postfix' "$qualified.SaveCurrentState"
    $delete = @(Get-Operations $qualified 'DeletePolicy')
    if ($delete -notcontains "stsfld:$qualified.DirtyPolicy" -and $delete -notcontains "$qualified.set_DirtyPolicy") {
        # DirtyPolicy belongs to the generic Manager base; its declaring type
        # includes the link type and may be represented as a property setter.
        if (-not ($delete | Where-Object { $_ -match '(stsfld:.*\.DirtyPolicy|\.set_DirtyPolicy)$' })) { throw "$manager deletion lost dirty fallback." }
    }
    Write-Output "PASS: $manager compiled selection, outgoing capture, emergency and deletion fallback remain connected."
}
Assert-Calls 'BetterPawnControlForked.MechManager' 'PasteToActivePolicy' 'BetterPawnControlForked.MechManager.LoadState'
Write-Output 'PASS: compiled Mech paste still applies the active preset.'
