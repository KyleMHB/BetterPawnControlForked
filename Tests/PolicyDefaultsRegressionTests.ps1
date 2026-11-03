param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path $root '.scratch\policy-defaults-regression'
[void](New-Item -ItemType Directory -Path $scratch -Force)

# Extract the actual property/method declarations, including their full bodies.
function Get-Declaration([string]$Source, [string]$Pattern) {
    $match = [regex]::Match($Source, $Pattern)
    if (!$match.Success) { throw "Production declaration missing: $Pattern" }
    $start = $match.Index
    $open = $Source.IndexOf('{', $start)
    $depth = 0
    for ($i = $open; $i -lt $Source.Length; $i++) {
        if ($Source[$i] -eq '{') { $depth++ }
        if ($Source[$i] -eq '}') {
            $depth--
            if ($depth -eq 0) { return $Source.Substring($start, $i - $start + 1) }
        }
    }
    throw "Unbalanced production declaration: $Pattern"
}
$assignSource = Get-Content (Join-Path $root 'Source\Managers\AssignManager.cs') -Raw
$managerSource = Get-Content (Join-Path $root 'Source\Managers\Manager.cs') -Raw
$storageSource = Get-Content (Join-Path $root 'Source\Core\DataStorage.cs') -Raw
$assignNames = @('_defaultOutfit', 'DefaultOutfit', '_defaultDrugPolicy', 'DefaultDrugPolicy',
    '_defaultPrisonerFoodPolicy', 'DefaultPrisonerFoodPolicy', '_defaultSlaveOutfit', 'DefaultSlaveOutfit',
    '_defaultSlaveFoodPolicy', 'DefaultSlaveFoodPolicy', '_defaultSlaveDrugPolicy', 'DefaultSlaveDrugPolicy',
    '_defaultSlaveReadingPolicy', 'DefaultSlaveReadingPolicy')
$assignDeclarations = ($assignNames | ForEach-Object { Get-Declaration $assignSource "internal static \w+ $_\s*\{" }) -join "`n"
$managerDeclarations = (@('_defaultFoodPolicy', 'DefaultFoodPolicy', '_defaultReadingPolicy', 'DefaultReadingPolicy') |
    ForEach-Object { Get-Declaration $managerSource "internal static \w+ $_\s*\{" }) -join "`n"
$capture = Get-Declaration $storageSource 'private void CapturePolicyDefaults\(\)'
$restore = Get-Declaration $storageSource 'private void RestorePolicyDefaults\(\)'

$program = @'
using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using BetterPawnControlForked;
using BetterPawnControlForked.CoreLogic;
using BetterPawnControlForked.Patches;
namespace HarmonyLib {
 [AttributeUsage(AttributeTargets.Class)] public class HarmonyPatch : Attribute {
  public HarmonyPatch(Type type, string method) {} }
}
namespace Verse {
 public static class Current { public static Game Game; }
 public class Game {
  public OutfitDatabase outfitDatabase = new OutfitDatabase();
  public DrugPolicyDatabase drugPolicyDatabase = new DrugPolicyDatabase();
  public FoodRestrictionDatabase foodRestrictionDatabase = new FoodRestrictionDatabase();
  public ReadingPolicyDatabase readingPolicyDatabase = new ReadingPolicyDatabase();
 }
}
namespace RimWorld {
 public class ApparelPolicy {} public class DrugPolicy {} public class FoodPolicy {} public class ReadingPolicy {} public class DrugPolicyDef {}
 public class Database<T> where T : class, new() {
  public List<T> Policies = new List<T> { new T(), new T() };
  public T Default() { if (Policies.Count == 0) Policies.Add(new T()); return Policies[0]; }
  // Mirrors the decompiled installed 1.6 database swap; does not invoke Harmony.
  public void SetDefault(T policy) { int index=Policies.IndexOf(policy); T previous=Policies[0]; Policies[0]=policy; Policies[index]=previous; }
 }
 public class OutfitDatabase : Database<ApparelPolicy> { public List<ApparelPolicy> AllOutfits => Policies; public ApparelPolicy DefaultOutfit() => Default(); }
 public class DrugPolicyDatabase : Database<DrugPolicy> { public List<DrugPolicy> AllPolicies => Policies; public DrugPolicy DefaultDrugPolicy() => Default(); public void MakePolicyDefault(DrugPolicyDef def) {} }
 public class FoodRestrictionDatabase : Database<FoodPolicy> { public List<FoodPolicy> AllFoodRestrictions => Policies; public FoodPolicy DefaultFoodRestriction() => Default(); }
 public class ReadingPolicyDatabase : Database<ReadingPolicy> { public List<ReadingPolicy> AllReadingPolicies => Policies; public ReadingPolicy DefaultReadingPolicy() => Default(); }
}
namespace BetterPawnControlForked {
 class AssignLink {} class AnimalLink {}
 class FeatureState { public FoodPolicy defaultFoodPolicy; public ReadingPolicy defaultReadingPolicy; }
 class BpcState {
  public FeatureState assign = new FeatureState(), animal = new FeatureState();
  public ApparelPolicy defaultOutfit, defaultSlaveOutfit;
  public DrugPolicy defaultDrugPolicy, defaultSlaveDrugPolicy;
  public FoodPolicy defaultSlaveFoodPolicy, defaultPrisonerFoodPolicy;
  public ReadingPolicy defaultSlaveReadingPolicy;
 }
 class DataStorage {
  public BpcState state = new BpcState();
  public static DataStorage Instance = new DataStorage();
  public static BpcState State => Instance.state;
  __CAPTURE__
  __RESTORE__
  public void Capture() => CapturePolicyDefaults();
  public void Restore() => RestorePolicyDefaults();
 }
 class Manager<T> {
  static FeatureState Feature => typeof(T) == typeof(AssignLink) ? DataStorage.State.assign : DataStorage.State.animal;
  __MANAGER__
 }
 class AssignManager : Manager<AssignLink> { __ASSIGN__ }
}
class Program {
 static int checks;
 static void Same(object expected, object actual, string label) { if (!ReferenceEquals(expected,actual)) throw new Exception(label); checks++; }
 static void Reset() { Current.Game = new Game(); DataStorage.Instance = new DataStorage(); }
 static void Main() {
  Reset(); var g=Current.Game;
  var outfitA=g.outfitDatabase.Policies[0]; var outfitB=g.outfitDatabase.Policies[1];
  AssignManager.DefaultOutfit=outfitB;
  Same(outfitB,g.outfitDatabase.DefaultOutfit(),"BPC apparel setter -> vanilla");
  Same(outfitB,AssignManager._defaultOutfit,"BPC apparel save reference");
  g.outfitDatabase.SetDefault(outfitA); OutfitDatabase_SetDefault.Postfix(g.outfitDatabase);
  Same(outfitA,AssignManager._defaultOutfit,"vanilla apparel postfix -> BPC backing");
  Same(outfitA,AssignManager.DefaultOutfit,"vanilla apparel -> BPC getter");
  var drugA=g.drugPolicyDatabase.Policies[0]; var drugB=g.drugPolicyDatabase.Policies[1];
  AssignManager.DefaultDrugPolicy=drugB; Same(drugB,g.drugPolicyDatabase.DefaultDrugPolicy(),"BPC drug setter");
  g.drugPolicyDatabase.SetDefault(drugA); DrugPolicyDatabase_SetDefault.Postfix(g.drugPolicyDatabase);
  Same(drugA,AssignManager._defaultDrugPolicy,"vanilla drug postfix");
  g.drugPolicyDatabase.SetDefault(drugB); DrugPolicyDatabase_MakePolicyDefault.Postfix(g.drugPolicyDatabase);
  Same(drugB,AssignManager.DefaultDrugPolicy,"scenario drug default capture");
  var foodA=g.foodRestrictionDatabase.Policies[0]; var foodB=g.foodRestrictionDatabase.Policies[1];
  AssignManager.DefaultFoodPolicy=foodB; Same(foodB,g.foodRestrictionDatabase.DefaultFoodRestriction(),"BPC colonist food setter");
  g.foodRestrictionDatabase.SetDefault(foodA); FoodRestrictionDatabase_SetDefault.Postfix(g.foodRestrictionDatabase);
  Same(foodA,AssignManager._defaultFoodPolicy,"vanilla food postfix");
  Same(foodA,AssignManager.DefaultFoodPolicy,"vanilla food getter");
  AnimalManagerDefault(foodB);
  var readA=g.readingPolicyDatabase.Policies[0]; var readB=g.readingPolicyDatabase.Policies[1];
  AssignManager.DefaultReadingPolicy=readB; Same(readB,g.readingPolicyDatabase.DefaultReadingPolicy(),"BPC reading setter");
  g.readingPolicyDatabase.SetDefault(readA); ReadingPolicyDatabase_SetDefault.Postfix(g.readingPolicyDatabase);
  Same(readA,AssignManager._defaultReadingPolicy,"vanilla reading postfix");
  Same(readA,AssignManager.DefaultReadingPolicy,"vanilla reading getter");
  Manager<AnimalLink>.DefaultReadingPolicy=readB;
  Same(readA,g.readingPolicyDatabase.DefaultReadingPolicy(),"separate feature reading setter");
  Same(readB,Manager<AnimalLink>.DefaultReadingPolicy,"separate feature reading getter");
  AssignManager.DefaultSlaveOutfit=outfitB; AssignManager.DefaultSlaveFoodPolicy=foodB;
  AssignManager.DefaultPrisonerFoodPolicy=foodB; AssignManager.DefaultSlaveDrugPolicy=drugA;
  AssignManager.DefaultSlaveReadingPolicy=readB;
  Same(outfitA,AssignManager.DefaultOutfit,"slave apparel leaves colonist default");
  Same(outfitB,AssignManager.DefaultSlaveOutfit,"separate slave apparel");
  Same(foodB,AssignManager.DefaultSlaveFoodPolicy,"separate slave food");
  Same(foodB,AssignManager.DefaultPrisonerFoodPolicy,"separate prisoner food");
  Same(drugA,AssignManager.DefaultSlaveDrugPolicy,"separate slave drug");
  Same(readB,AssignManager.DefaultSlaveReadingPolicy,"separate slave reading");
  // Mutations of detached databases must not overwrite the current world's saved state.
  OutfitDatabase_SetDefault.Postfix(new OutfitDatabase()); DrugPolicyDatabase_SetDefault.Postfix(new DrugPolicyDatabase());
  FoodRestrictionDatabase_SetDefault.Postfix(new FoodRestrictionDatabase()); ReadingPolicyDatabase_SetDefault.Postfix(new ReadingPolicyDatabase());
  Same(outfitA,AssignManager._defaultOutfit,"detached apparel DB"); Same(drugB,AssignManager._defaultDrugPolicy,"detached drug DB");
  Same(foodA,AssignManager._defaultFoodPolicy,"detached food DB"); Same(readA,AssignManager._defaultReadingPolicy,"detached reading DB");
  // Loading legacy BPC references restores all four defaults to the vanilla databases.
  DataStorage.State.defaultOutfit=outfitB; DataStorage.State.defaultDrugPolicy=drugA;
  DataStorage.State.assign.defaultFoodPolicy=foodB; DataStorage.State.assign.defaultReadingPolicy=readB;
  DataStorage.Instance.Restore();
  Same(outfitB,g.outfitDatabase.DefaultOutfit(),"legacy apparel restore"); Same(drugA,g.drugPolicyDatabase.DefaultDrugPolicy(),"legacy drug restore");
  Same(foodB,g.foodRestrictionDatabase.DefaultFoodRestriction(),"legacy food restore"); Same(readB,g.readingPolicyDatabase.DefaultReadingPolicy(),"legacy reading restore");
  // Saving captures changes even if an external mod changes list order without SetDefault.
  g.outfitDatabase.SetDefault(outfitA); g.drugPolicyDatabase.SetDefault(drugB);
  g.foodRestrictionDatabase.SetDefault(foodA); g.readingPolicyDatabase.SetDefault(readA);
  DataStorage.Instance.Capture();
  Same(outfitA,DataStorage.State.defaultOutfit,"save apparel capture"); Same(drugB,DataStorage.State.defaultDrugPolicy,"save drug capture");
  Same(foodA,DataStorage.State.assign.defaultFoodPolicy,"save food capture"); Same(readA,DataStorage.State.assign.defaultReadingPolicy,"save reading capture");
  // Browsing only reads the default; it must never restore an old cached reference.
  AssignManager._defaultOutfit=outfitB; Same(outfitA,AssignManager.DefaultOutfit,"browse uses authoritative apparel");
  Same(outfitA,g.outfitDatabase.DefaultOutfit(),"browse does not set default");
  g.outfitDatabase.Policies.Remove(outfitA);
  Same(outfitB,AssignManager.DefaultOutfit,"deleted colonist default fallback");
  g.foodRestrictionDatabase.Policies.Remove(foodB);
  Same(foodA,AssignManager.DefaultSlaveFoodPolicy,"deleted slave food fallback");
  Same(foodA,AssignManager.DefaultPrisonerFoodPolicy,"deleted prisoner food fallback");
  Same(foodA,Manager<AnimalLink>.DefaultFoodPolicy,"deleted animal food fallback");
  AssignManager.DefaultOutfit=new ApparelPolicy(); Same(outfitB,g.outfitDatabase.DefaultOutfit(),"invalid default rejected");
  DataStorage.State.defaultOutfit=new ApparelPolicy(); DataStorage.Instance.Restore();
  Same(outfitB,DataStorage.State.defaultOutfit,"missing saved reference repaired");
  // A second colony has independent world-owned default state.
  Reset(); Same(Current.Game.outfitDatabase.DefaultOutfit(),AssignManager.DefaultOutfit,"new colony default");
  Console.WriteLine($"PASS: {checks} production policy-default synchronization and scope checks.");
 }
 static void AnimalManagerDefault(FoodPolicy policy) {
  var colonist=Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
  Manager<AnimalLink>.DefaultFoodPolicy=policy;
  Same(colonist,Current.Game.foodRestrictionDatabase.DefaultFoodRestriction(),"animal food setter leaves colonist DB");
  Same(policy,Manager<AnimalLink>.DefaultFoodPolicy,"animal food getter retains separate selection");
 }
}
'@
$program = $program.Replace('__CAPTURE__', $capture).Replace('__RESTORE__', $restore).
    Replace('__MANAGER__', $managerDeclarations).Replace('__ASSIGN__', $assignDeclarations)
Set-Content -LiteralPath (Join-Path $scratch 'Program.cs') -Value $program -Encoding utf8
$helperPath = [System.Security.SecurityElement]::Escape((Join-Path $root 'Source\CoreLogic\PolicyDefaults.cs'))
$patchPath = [System.Security.SecurityElement]::Escape((Join-Path $root 'Source\Patches\PolicyDatabase_Defaults.cs'))
$project = @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include="Program.cs"/><Compile Include="$helperPath"/><Compile Include="$patchPath"/></ItemGroup></Project>
"@
$projectPath = Join-Path $scratch 'PolicyDefaultsRegression.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& dotnet run --project $projectPath --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Policy default production-wiring regression failed.' }
