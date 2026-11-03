$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

function Get-ProductionMember([string]$File, [string]$Signature) {
    $source = Get-Content -LiteralPath (Join-Path $root $File) -Raw
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
    return $source.Substring($start, $end - $start)
}

# Run the actual manager body, availability properties, and serialized settings
# against doubles. No Unity, Harmony, WVC jobs, or save-game execution is claimed.
$code = @'
using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
namespace Verse {
 public class ModSettings { public virtual void ExposeData() {} }
 public static class Scribe_Values {
  public static bool? Saved;
  public static void Look(ref bool value, string key, bool defaultValue, bool forceSave) {
   if (key == "DisableMechWorkModesWithWVC") value = Saved ?? defaultValue;
  }
  public static void Look(ref float? value, string key, float? defaultValue, bool forceSave) {}
 }
 public class ModContentPack { public string PackageId; }
 public static class LoadedModManager { public static List<ModContentPack> RunningModsListForReading = new(); }
 public class Area {}
 public class PlayerSettings { public Area AreaRestrictionInPawnCurrentMap; }
 public class Pawn { public PlayerSettings playerSettings = new(); public Pawn overseer; public MechanitorControlGroup group;
  public Pawn GetOverseer() => overseer; public MechanitorControlGroup GetMechControlGroup() => group; }
 public static class Find { public static Map CurrentMap = new(); }
 public class Map { public int uniqueID = 7; }
}
namespace RimWorld {
 public class MechWorkModeDef {}
 public class Tracker { public List<MechanitorControlGroup> controlGroups = new(); }
 public class MechanitorControlGroup {
  public int Index; public Tracker Tracker; public MechWorkModeDef WorkMode; public int Writes;
  public void Assign(Pawn p) { p.group = this; }
  public void SetWorkMode(MechWorkModeDef mode) { if(mode == null) throw new Exception("Null mode write"); WorkMode = mode; Writes++; }
 }
}
namespace BetterPawnControlForked {
 SETTINGS
 static class BetterPawnControlForkedMod { public static BetterPawnControlForkedSettings Settings = new(); }
 static class Widget_ModsAvailable {
  WVC_CONSTANT
  public static BetterPawnControlForkedSettings Settings => BetterPawnControlForkedMod.Settings;
  WVC_AVAILABLE
  WVC_DISABLED
 }
 public class Policy { public int id; }
 public class MechLink { public int mapId, zone, controlGroupIndex; public Pawn mech; public MechWorkModeDef workmode; public Area area; }
 static class PawnCompatibility { public static bool SamePawn(Pawn a, Pawn b) => ReferenceEquals(a,b); }
 class MechManager {
  public static Policy active;
  public static void SetActivePolicy(Policy p){active=p;}
  MECH_LOAD
 }
 class Program {
  static void Require(bool condition, string message) { if(!condition) throw new Exception(message); }
  static void Apply(bool wvc, bool disabled, bool missingMode, bool changeGroup) {
   LoadedModManager.RunningModsListForReading.Clear();
   if(wvc) LoadedModManager.RunningModsListForReading.Add(new(){PackageId="WVC.SERGKART.BIOTECH.MOREMECHANOIDSWORKMODES"});
   BetterPawnControlForkedMod.Settings = new(){disableMechWorkModesWithWVC=disabled};
   var saved = missingMode ? null : new MechWorkModeDef(); var live = new MechWorkModeDef(); var destinationMode = new MechWorkModeDef();
   var tracker = new Tracker();
   var original = new MechanitorControlGroup{Index=1,Tracker=tracker,WorkMode=live};
   var destination = new MechanitorControlGroup{Index=2,Tracker=tracker,WorkMode=destinationMode};
   tracker.controlGroups.AddRange(new[]{original,destination});
   var pawn = new Pawn{overseer=new Pawn(),group=original}; var area = new Area();
   var link = new MechLink{mapId=7,zone=3,mech=pawn,controlGroupIndex=changeGroup?2:1,workmode=saved,area=area};
   var otherMap = new MechLink{mapId=8,zone=3,mech=pawn,controlGroupIndex=1,workmode=new(),area=new()};
   var otherPolicy = new MechLink{mapId=7,zone=4,mech=pawn,controlGroupIndex=1,workmode=new(),area=new()};
   var policy = new Policy{id=3};
   MechManager.LoadState(new(){otherMap,otherPolicy,link},new(){pawn},policy);
   bool restore = !missingMode && !(wvc && disabled);
   var group = changeGroup ? destination : original;
   Require(pawn.group == group, "Control group was not restored");
   Require(group.Writes == (restore?1:0), "Unexpected number of direct mode writes");
   Require(group.WorkMode == (restore?saved:(changeGroup?destinationMode:live)), "Incorrect work mode");
   Require(pawn.playerSettings.AreaRestrictionInPawnCurrentMap == area, "Area was not restored");
   Require(MechManager.active == policy, "Selected policy was not activated");
   Require(link.workmode == saved, "Stored mode reference was changed");
  }
  static void Main() {
   var settings = new BetterPawnControlForkedSettings();
   Require(settings.disableMechWorkModesWithWVC, "New setting must default to enabled");
   settings.disableMechWorkModesWithWVC=false; Scribe_Values.Saved=null; settings.ExposeData();
   Require(settings.disableMechWorkModesWithWVC, "Old settings without the key must enable WVC guard");
   Scribe_Values.Saved=false; settings.ExposeData();
   Require(!settings.disableMechWorkModesWithWVC, "Serialized user opt-in was lost");
   foreach(bool wvc in new[]{false,true}) foreach(bool disabled in new[]{false,true}) foreach(bool missing in new[]{false,true}) foreach(bool group in new[]{false,true}) Apply(wvc,disabled,missing,group);
   LoadedModManager.RunningModsListForReading.Clear();
   LoadedModManager.RunningModsListForReading.Add(new(){PackageId="unrelated.wvc"});
   Require(!Widget_ModsAvailable.WVCWorkModesAvailable, "Unrelated package detected as WVC");
   LoadedModManager.RunningModsListForReading.Add(new(){PackageId="wvc.sergkart.biotech.MoreMechanoidsWorkModes"});
   BetterPawnControlForkedMod.Settings=null;
   Require(Widget_ModsAvailable.DisableBPCMechWorkModes, "Unavailable settings must keep safe WVC default");
   Console.WriteLine("PASS: 16 production mech load scenarios, case-insensitive WVC detection, settings defaults and persisted opt-in.");
  }
 }
}
'@
$settings = Get-Content (Join-Path $root 'Source/Core/BetterPawnControlForkedSettings.cs') -Raw
$settings = $settings.Substring($settings.IndexOf('public class'))
$settings = $settings.Substring(0, $settings.LastIndexOf('}'))
$availability = Get-Content (Join-Path $root 'Source/Helpers/Widget_ModsAvailable.cs') -Raw
$constant = [regex]::Match($availability, 'private const string WVC_WORK_MODES_PACKAGE_ID = "[^"]+";').Value
if (-not $constant) { throw 'Missing production WVC package constant' }
$code = $code.Replace('SETTINGS', $settings)
$code = $code.Replace('WVC_CONSTANT', $constant)
$code = $code.Replace('WVC_AVAILABLE', (Get-ProductionMember 'Source/Helpers/Widget_ModsAvailable.cs' 'public static bool WVCWorkModesAvailable'))
$code = $code.Replace('WVC_DISABLED', (Get-ProductionMember 'Source/Helpers/Widget_ModsAvailable.cs' 'public static bool DisableBPCMechWorkModes'))
$code = $code.Replace('MECH_LOAD', (Get-ProductionMember 'Source/Managers/MechManager.cs' 'internal static void LoadState(List<MechLink>'))
$folder = Join-Path $root '.scratch/mech-work-mode-regressions'
New-Item -ItemType Directory -Path $folder -Force | Out-Null
Set-Content -LiteralPath (Join-Path $folder 'Program.cs') -Value $code -Encoding utf8
Set-Content -LiteralPath (Join-Path $folder 'Regression.csproj') -Value '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>' -Encoding utf8
& dotnet run --project (Join-Path $folder 'Regression.csproj') --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Mech work mode regressions failed' }
