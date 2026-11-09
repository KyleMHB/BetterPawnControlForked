using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorldDevTests;
using Verse;

namespace BetterPawnControlForked.DevTests;

// Expected behaviour (README; TESTING.md "Required in-game 1.6 matrix"): each Assign policy remembers every colonist's apparel
// and food policy, hostility response and medical care; selecting a policy restores what was saved for it; the policies and
// the active selection survive save and reload; deleting the active policy falls back to policy 0.
// Mechanical setup: the Assign tab's flow is called directly (the active-policy button saves the current state, the chosen menu
// option loads the policy). The tab, button and float menu are not clicked.
public class AssignBatch : Batch
{
    private const string HarmonyId = "KyleMHB.BetterPawnControlForked";
    private const string OutfitA = "BPC test outfit A", OutfitB = "BPC test outfit B", FoodA = "BPC test food A", FoodB = "BPC test food B";
    private const string PolicyB = "BPC test policy B";

    public override string Id => "bpc-assign";
    public override int SegmentCount => 2;
    public override int MapSize => 75;
    public override IReadOnlyList<PlannedCase> Plan { get; } = new[]
    {
        "bpc.loaded", "bpc.switch-new", "bpc.switch-back", "bpc.switch-again", "bpc.reload", "bpc.reload-switch", "bpc.delete-fallback",
    }.Select(id => new PlannedCase(id, Levels.Mechanical)).ToArray();

    private record Snapshot(string Outfit, string Food, HostilityResponseMode Hostility, MedicalCareCategory Care)
    {
        public override string ToString() => $"{Outfit}/{Food}/{Hostility}/{Care}";
    }

    private static Snapshot Read(Pawn p) => new(p.outfits.CurrentApparelPolicy?.label, p.foodRestriction.CurrentFoodPolicy?.label,
        p.playerSettings.hostilityResponse, p.playerSettings.medCare);

    private static readonly Snapshot StateA = new(OutfitA, FoodA, HostilityResponseMode.Attack, MedicalCareCategory.NoMeds);
    private static readonly Snapshot StateB = new(OutfitB, FoodB, HostilityResponseMode.Flee, MedicalCareCategory.Best);

    private static void Apply(Pawn p, Snapshot s)
    {
        p.outfits.CurrentApparelPolicy = Current.Game.outfitDatabase.AllOutfits.First(o => o.label == s.Outfit);
        p.foodRestriction.CurrentFoodPolicy = Current.Game.foodRestrictionDatabase.AllFoodRestrictions.First(f => f.label == s.Food);
        p.playerSettings.hostilityResponse = s.Hostility;
        p.playerSettings.medCare = s.Care;
    }

    private static string Describe(IEnumerable<Pawn> pawns) => string.Join("; ", pawns.Select(p => $"{p.LabelShort} {Read(p)}"));

    private static void ExpectState(BatchContext ctx, string id, List<Pawn> pawns, Snapshot expected, Policy expectedActive, string reason)
    {
        bool match = pawns.All(p => Read(p) == expected);
        var active = Bpc.Active;
        bool activeOk = Bpc.Id(active) == Bpc.Id(expectedActive);
        ctx.Begin(id).Expect("pawnState", expected.ToString()).Expect("activePolicy", expectedActive.label).Subject(pawns.ToArray())
            .Observe("pawnState", Describe(pawns)).Observe("activePolicy", active.label)
            .Check(match && activeOk, reason);
    }

    public override IEnumerable<Wait> Segment(int index, BatchContext ctx)
    {
        if (index == 1)
        {
            foreach (var w in Reloaded(ctx)) yield return w;
            yield break;
        }

        ctx.Setup($"{ModsConfig.ActiveModsInLoadOrder.Count()} active mods");
        var storage = Find.World.GetComponent<DataStorage>();
        var patched = Harmony.GetAllPatchedMethods().Count(m => Harmony.GetPatchInfo(m)?.Owners.Contains(HarmonyId) == true);
        var policy0 = Bpc.Policies.FirstOrDefault(p => Bpc.Id(p) == 0);
        ctx.Begin("bpc.loaded").Expect("dataStorage", true).Expect("patchedMethods", "> 0").Expect("policy0Active", true)
            .Observe("dataStorage", storage != null).Observe("patchedMethods", patched).Observe("policies", Bpc.Policies.Count)
            .Observe("policy0Active", policy0 != null && Bpc.Id(Bpc.Active) == 0)
            .Check(storage != null && patched > 0 && policy0 != null && Bpc.Id(Bpc.Active) == 0,
                "the world component, the Harmony patches or the default Assign policy is missing");

        Kit.RemoveStartingColonists(ctx);
        var area = Kit.Area(ctx, 10, 6);
        var pawns = new List<Pawn>
        {
            Kit.Worker(ctx, area.Min + new IntVec3(2, 0, 2), WorkTypeDefOf.Hauling),
            Kit.Worker(ctx, area.Min + new IntVec3(6, 0, 2), WorkTypeDefOf.Hauling),
        };
        foreach (var label in new[] { OutfitA, OutfitB }) Current.Game.outfitDatabase.MakeNewOutfit().label = label;
        foreach (var label in new[] { FoodA, FoodB }) Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction().label = label;
        var policyB = new Policy(Bpc.Policies.Max(Bpc.Id) + 1, PolicyB);
        Bpc.Policies.Add(policyB);
        foreach (var p in pawns) Apply(p, StateA);
        ctx.Setup($"Developer made apparel policies '{OutfitA}', '{OutfitB}', food policies '{FoodA}', '{FoodB}' and Assign policy '{PolicyB}' (id {Bpc.Id(policyB)}); set both colonists to {StateA}");

        // Selecting a policy that has no saved state keeps the colonists' current settings.
        Bpc.SelectPolicy(pawns, policyB);
        ExpectState(ctx, "bpc.switch-new", pawns, StateA, policyB, "selecting a new Assign policy changed the colonists or did not become active");

        foreach (var p in pawns) Apply(p, StateB);
        ctx.Setup($"Developer set both colonists to {StateB} while '{PolicyB}' was active");

        Bpc.SelectPolicy(pawns, policy0);
        ExpectState(ctx, "bpc.switch-back", pawns, StateA, policy0, "selecting the default policy did not restore its saved settings");

        Bpc.SelectPolicy(pawns, policyB);
        ExpectState(ctx, "bpc.switch-again", pawns, StateB, policyB, "selecting policy B again did not restore its saved settings");

        ctx.Remember("pawns", string.Join(",", pawns.Select(p => p.ThingID)));
        ctx.Remember("policyB", Bpc.Id(policyB).ToString());
        yield return Wait.Reload();
    }

    private IEnumerable<Wait> Reloaded(BatchContext ctx)
    {
        var ids = ctx.Recall("pawns").Split(',');
        var pawns = ctx.Map.mapPawns.AllPawnsSpawned.Where(p => ids.Contains(p.ThingID)).ToList();
        int bId = int.Parse(ctx.Recall("policyB"));
        var policyB = Bpc.Policies.FirstOrDefault(p => Bpc.Id(p) == bId);
        var policy0 = Bpc.Policies.FirstOrDefault(p => Bpc.Id(p) == 0);
        int Count(int zone) => Bpc.Links.Count(l => Bpc.Zone(l) == zone && pawns.Any(p => p == Bpc.Colonist(l)));
        bool stateB = pawns.Count == ids.Length && pawns.All(p => Read(p) == StateB);
        ctx.Begin("bpc.reload").Expect("pawnsFound", ids.Length).Expect("policyB", PolicyB).Expect("activePolicyId", bId)
            .Expect("linksPolicy0", ids.Length).Expect("linksPolicyB", ids.Length).Expect("pawnState", StateB.ToString())
            .Subject(pawns.ToArray())
            .Observe("pawnsFound", pawns.Count).Observe("policyB", policyB?.label ?? "missing").Observe("activePolicyId", Bpc.Id(Bpc.Active))
            .Observe("linksPolicy0", Count(0)).Observe("linksPolicyB", Count(bId)).Observe("pawnState", Describe(pawns))
            .Check(pawns.Count == ids.Length && policyB != null && policy0 != null && Bpc.Id(Bpc.Active) == bId
                && Count(0) == ids.Length && Count(bId) == ids.Length && stateB,
                "an Assign policy, its saved colonist links or the active selection was lost across save and reload");
        if (policyB == null || policy0 == null || pawns.Count != ids.Length) yield break;

        // The saved references must resolve after loading: switching back restores policy 0's settings.
        Bpc.SelectPolicy(pawns, policy0);
        ExpectState(ctx, "bpc.reload-switch", pawns, StateA, policy0, "after reload, selecting the default policy did not restore its saved settings");

        // Deleting the active policy falls back to policy 0 and removes its links.
        Bpc.SelectPolicy(pawns, policyB);
        Bpc.DeletePolicy(policyB);
        bool removed = Bpc.Policies.All(p => Bpc.Id(p) != bId);
        int remaining = Bpc.Links.Count(l => Bpc.Zone(l) == bId);
        ctx.Begin("bpc.delete-fallback").Expect("activePolicyId", 0).Expect("policyRemoved", true).Expect("linksRemaining", 0)
            .Observe("activePolicyId", Bpc.Id(Bpc.Active)).Observe("policyRemoved", removed).Observe("linksRemaining", remaining)
            .Check(Bpc.Id(Bpc.Active) == 0 && removed && remaining == 0, "deleting the active policy did not fall back to policy 0 or left its links");
    }
}
