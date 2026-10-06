param(
    [switch]$SkipCompiledChecks,
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '..\v1.6\Assemblies\BetterPawnControlForked.dll'),
    [string]$ManagedPath = 'E:\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot

function Get-ProductionMember([string]$File, [string]$Signature) {
    $source = Get-Content -Raw -LiteralPath (Join-Path $root $File)
    $start = $source.IndexOf($Signature, [StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Missing production member: $Signature" }
    $brace = $source.IndexOf('{', $start)
    $depth = 1
    $end = $brace + 1
    while ($depth -gt 0) {
        if ($source[$end] -eq '{') { $depth++ }
        if ($source[$end] -eq '}') { $depth-- }
        $end++
    }
    $source.Substring($start, $end - $start)
}

# Execute production opening, closing, selection and capture/application bodies
# with game doubles. This does not exercise Unity, Harmony or Scribe.
$code = @'
using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
namespace Verse {
 public class Window {}
 public class Area {}
 public class PlayerSettings { public Area AreaRestrictionInPawnCurrentMap; }
 public class Timetable { public List<TimeAssignmentDef> times = new(); }
 public class Jobs { public object curJob = new(); public int Interruptions;
  public bool IsCurrentJobPlayerInterruptible() => true;
  public void EndCurrentJob(JobCondition condition) { Interruptions++; }
 }
 public class Pawn { public PlayerSettings playerSettings = new(); public Timetable timetable = new();
  public Jobs jobs = new(); public bool Downed, InMentalState, Drafted;
 }
 public class Map { public int uniqueID = 7; }
 public static class Find { public static Map CurrentMap = new(); public static WindowStack WindowStack = new(); }
 public class WindowStack { public FloatMenu Menu; public void Add(FloatMenu menu) { Menu = menu; } }
 public static class Log { public static void Message(object message) { throw new Exception(message.ToString()); } }
 public static class ListExtensions { public static bool NullOrEmpty<T>(this List<T> items) => items == null || items.Count == 0; }
}
namespace Verse.AI { public enum JobCondition { InterruptForced } }
namespace RimWorld {
 public class MainTabWindow_PawnTable : Window {}
 public class MainTabWindow_Assign : MainTabWindow_PawnTable {}
 public class MainTabWindow_Schedule : MainTabWindow_PawnTable {}
 public class MainTabWindow_Work : MainTabWindow_PawnTable {}
 public class MainTabWindow_Animals : MainTabWindow_PawnTable {}
 public class MainTabWindow_Mechs : MainTabWindow_PawnTable {}
 public class TimeAssignmentDef {}
 public enum MenuOptionPriority { Default }
 public class FloatMenu { public List<FloatMenuOption> Options; public FloatMenu(List<FloatMenuOption> options) { Options = options; } }
 public class FloatMenuOption { public Action action;
  public FloatMenuOption(string label, Action action, MenuOptionPriority priority, object a, object b, float c, object d) { this.action = action; }
 }
}
namespace BetterPawnControlForked {
 __SESSION__
 public class Policy { public int id; public string label; }
 public class ScheduleLink { public int mapId, zone; public Pawn colonist; public Area area; public List<TimeAssignmentDef> schedule;
  public ScheduleLink(int zone, Pawn pawn, Area area, List<TimeAssignmentDef> times, int mapId) {
   this.zone = zone; colonist = pawn; this.area = area; schedule = times == null ? null : new(times); this.mapId = mapId;
  }
  public void RepairSchedule() { if (schedule.Count != 24) throw new Exception("Expected a complete timetable."); }
 }
 public static class PawnCompatibility {
  public static bool SupportsSchedule(Pawn p) => p.playerSettings != null;
  public static bool SupportsAssign(Pawn p) => true;
  public static bool SupportsWork(Pawn p) => true;
  public static bool SupportsWeapons(Pawn p) => true;
  public static bool SamePawn(Pawn a, Pawn b) => ReferenceEquals(a, b);
 }
 public class Options { public bool automaticPawnsInterrupt = true; }
 public static class BetterPawnControlForkedMod { public static Options Settings = new(); }
 public class OtherManager {
  public static bool showPaste; public static List<object> links = new();
  public static List<Pawn> Colonists() => new(); public static List<Pawn> Robots() => new();
  public static List<Pawn> Animals() => new(); public static List<Pawn> Mechs() => new();
  public static Policy GetActivePolicy() => new();
  public static void LoadState(List<object> links, List<Pawn> pawns, Policy policy) {}
  public static void SaveCurrentState(List<Pawn> pawns) {} public static void LinksCleanUp() {}
 }
 public class AssignManager : OtherManager {} public class WorkManager : OtherManager {}
 public class AnimalManager : OtherManager {} public class MechManager : OtherManager {}
 public class WeaponsManager : OtherManager {} public class RobotManager : OtherManager {}
 public static class Widget_WorkTab { public static void ClearCache() {} }
 public static class Widget_ModsAvailable {
  public const string WORKTAB_MAINTAB = "work", NUMBERS_MAINTAB = "numbers", ANIMALTAB_MAINTAB = "animals",
   NUMBERS_DEFNAME = "numbersAnimals", WEAPONSTAB_MAINTAB = "weapons", AIROBOTX2_MAINTAB = "robots";
  public static bool DisableBPCOnWorkTab, WTBAvailable, MiscRobotsAvailable;
 }
 public class ScheduleManager {
  public static bool showPaste; public static List<ScheduleLink> links = new(), clipboard = new();
  public static List<Pawn> roster = new(); public static List<Policy> policies = new(); public static Policy active;
  public static List<Pawn> Colonists() => roster; public static Policy GetActivePolicy() => active;
  public static void SetActivePolicy(Policy policy) { active = policy; } public static void LinksCleanUp() {}
  __LOAD__
  __SAVE__
  __COPY__
  __PASTE__
 }
 public class Opening { __OPEN__ public static void Run() { Postfix(new MainTabWindow_Schedule()); } }
 public class Closing { __CLOSE__ public static void Run() { Postfix(new MainTabWindow_Schedule()); } }
 public class Selection { __SELECT__ public static void Run(int id) {
  OpenSchedulePolicySelectMenu(ScheduleManager.links, ScheduleManager.roster);
  Find.WindowStack.Menu.Options[id].action();
 } }
 public static class ScheduleBehavior {
  static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
  static void Expect(Pawn pawn, Area area, TimeAssignmentDef hour, string scenario) {
   Assert(ReferenceEquals(pawn.playerSettings.AreaRestrictionInPawnCurrentMap, area), scenario + ": area overwritten.");
   Assert(pawn.timetable.times.Count == 24 && pawn.timetable.times.All(t => ReferenceEquals(t, hour)), scenario + ": timetable overwritten.");
  }
  public static void Run() {
   var areaA = new Area(); var areaB = new Area(); var areaC = new Area();
   var sleep = new TimeAssignmentDef(); var work = new TimeAssignmentDef(); var recreation = new TimeAssignmentDef();
   var pawn = new Pawn(); var first = new Policy { id = 0, label = "First" }; var second = new Policy { id = 1, label = "Second" };
   ScheduleManager.active = first; ScheduleManager.policies.AddRange(new[] { first, second }); ScheduleManager.roster.Add(pawn);
   var saved = new ScheduleLink(0, pawn, areaA, Enumerable.Repeat(sleep, 24).ToList(), 7);
   ScheduleManager.links.Add(saved);
   ScheduleManager.links.Add(new ScheduleLink(1, pawn, areaC, Enumerable.Repeat(recreation, 24).ToList(), 7));
   pawn.playerSettings.AreaRestrictionInPawnCurrentMap = areaB;
   pawn.timetable.times.AddRange(Enumerable.Repeat(work, 24)); ScheduleManager.showPaste = true;
   Opening.Run();
   Expect(pawn, areaB, work, "Opening Schedule after external edits");
   Assert(pawn.jobs.Interruptions == 0, "Opening interrupted a live job.");
   Assert(PawnTableSession.ActiveWindowType == typeof(MainTabWindow_Schedule), "Opening lost the table session.");
   Assert(!ScheduleManager.showPaste, "Opening did not reset paste UI.");
   Closing.Run();
   Assert(PawnTableSession.ActiveWindowType == null, "Closing left a stale session.");
   Assert(ReferenceEquals(saved.area, areaB) && saved.schedule.All(t => ReferenceEquals(t, work)), "Closing failed to capture live state.");
   Selection.Run(1); Expect(pawn, areaC, recreation, "Explicit selection");
   Selection.Run(0); Expect(pawn, areaB, work, "Restoring captured state after close");
   Opening.Run();
   pawn.playerSettings.AreaRestrictionInPawnCurrentMap = areaA;
   pawn.timetable.times = Enumerable.Repeat(sleep, 24).ToList();
   // The policy button captures before showing the menu; compiled checks below
   // verify that the real button remains wired to this production capture body.
   ScheduleManager.SaveCurrentState(ScheduleManager.roster);
   Selection.Run(1); Selection.Run(0); Expect(pawn, areaA, sleep, "Restoring outgoing preset capture");
   ScheduleManager.clipboard.Add(new ScheduleLink(1, pawn, areaC, Enumerable.Repeat(recreation, 24).ToList(), 7));
   ScheduleManager.PasteToActivePolicy(); Expect(pawn, areaC, recreation, "Explicit paste");
   Assert(pawn.jobs.Interruptions > 0, "Explicit application lost job interruption.");
   Closing.Run();
  }
 }
}
'@
$members = @{
    '__OPEN__' = Get-ProductionMember 'Source/Patches/MainTabWindow_PawnTable_PostOpen.cs' 'static void Postfix('
    '__CLOSE__' = Get-ProductionMember 'Source/Patches/Window_PreClose.cs' 'static void Postfix('
    '__LOAD__' = Get-ProductionMember 'Source/Managers/ScheduleManager.cs' 'internal static void LoadState(List<ScheduleLink>'
    '__SAVE__' = Get-ProductionMember 'Source/Managers/ScheduleManager.cs' 'internal static void SaveCurrentState('
    '__COPY__' = Get-ProductionMember 'Source/Managers/ScheduleManager.cs' 'internal static bool CopySchedule('
    '__PASTE__' = Get-ProductionMember 'Source/Managers/ScheduleManager.cs' 'internal static void PasteToActivePolicy('
    '__SELECT__' = Get-ProductionMember 'Source/Patches/PawnTable_PawnTableOnGUI.cs' 'private static void OpenSchedulePolicySelectMenu('
    '__SESSION__' = Get-ProductionMember 'Source/Helpers/PawnTableSession.cs' 'internal static class PawnTableSession'
}
foreach ($entry in $members.GetEnumerator()) { $code = $code.Replace($entry.Key, $entry.Value) }
Add-Type -TypeDefinition $code
[BetterPawnControlForked.ScheduleBehavior]::Run()
Write-Output 'PASS: Schedule opening preserves external areas and timetables, session, paste UI and live jobs.'
Write-Output 'PASS: production closing, outgoing capture, selection round trips and paste retain Schedule state.'
if ($SkipCompiledChecks) { return }

# The existing compiled Assign check also loads the game dependencies and DLL.
. (Join-Path $PSScriptRoot 'AssignTabOpenRegressionTests.ps1') -AssemblyPath $AssemblyPath -ManagedPath $ManagedPath
$opcodes = @{}
foreach ($field in [Reflection.Emit.OpCodes].GetFields([Reflection.BindingFlags]'Public,Static')) {
    $opcode = $field.GetValue($null)
    $opcodes[[int]$opcode.Value -band 0xffff] = $opcode
}
function Get-ScheduleOperations([Reflection.MethodBase]$Method) {
    $il = $Method.GetMethodBody().GetILAsByteArray()
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
function Get-ScheduleRoute([string]$TypeName, [string]$MethodName) {
    $type = $assembly.GetType($TypeName, $true)
    $methods = @($type.GetMethods([Reflection.BindingFlags]'Static,Instance,Public,NonPublic') | Where-Object Name -eq $MethodName)
    foreach ($nested in $type.GetNestedTypes([Reflection.BindingFlags]'Public,NonPublic')) {
        $methods += @($nested.GetMethods([Reflection.BindingFlags]'Static,Instance,Public,NonPublic') | Where-Object { $_.Name.StartsWith("<$MethodName>") })
    }
    if ($methods.Count -eq 0) { throw "Missing production method $TypeName.$MethodName." }
    foreach ($method in $methods) { Get-ScheduleOperations $method }
}
function Assert-ScheduleRoute([string]$TypeName, [string]$MethodName, [string]$Expected) {
    if (@(Get-ScheduleRoute $TypeName $MethodName) -notcontains $Expected) { throw "$TypeName.$MethodName lost $Expected." }
}
$manager = 'BetterPawnControlForked.ScheduleManager'
$ui = 'BetterPawnControlForked.Patches.PawnTable_PawnTableOnGUI'
if (@(Get-ScheduleRoute 'BetterPawnControlForked.MainTabWindow_PawnTable_OnPostOpen' 'Postfix') -contains "$manager.LoadState") {
    throw 'Compiled Schedule opening still applies stale saved state.'
}
Assert-ScheduleRoute $ui 'OpenSchedulePolicySelectMenu' "$manager.LoadState"
Assert-ScheduleRoute $ui 'DrawBPCButtons_ScheduleTab' "$manager.SaveCurrentState"
Assert-ScheduleRoute $ui 'DrawSheduleBPCButtons' "$manager.LoadState"
Assert-ScheduleRoute 'BetterPawnControlForked.Patches.Window_PreClose' 'Postfix' "$manager.SaveCurrentState"
Assert-ScheduleRoute $manager 'PasteToActivePolicy' "$manager.LoadState"
Assert-ScheduleRoute 'BetterPawnControlForked.AlertManager' 'LoadState' "$manager.LoadState"
Assert-ScheduleRoute 'BetterPawnControlForked.CaravanArrivalAction_Enter_Arrived' 'Postfix' "$manager.LoadState"
if (-not (@(Get-ScheduleRoute $manager 'DeletePolicy') | Where-Object { $_ -match '(stsfld:.*\.DirtyPolicy|\.set_DirtyPolicy)$' })) {
    throw 'Schedule deletion lost dirty-policy fallback.'
}
Write-Output 'PASS: compiled Schedule selection, capture, close, paste, emergency, deletion fallback and arrival routes remain connected.'
