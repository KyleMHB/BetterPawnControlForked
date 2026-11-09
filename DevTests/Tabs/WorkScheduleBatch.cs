using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorldDevTests;
using Verse;

namespace BetterPawnControlForked.DevTests;

// Expected behaviour (README "Features"; TESTING.md "Required in-game 1.6 matrix" and the Schedule checklist): each Work preset
// remembers every colonist's work priorities, and each Schedule preset their allowed area and 24-hour timetable; selecting a
// preset restores what was saved for it. The emergency toggle applies the presets chosen for emergency mode together, and
// turning it off restores the presets that were active before. The selections and emergency state survive save and reload.
// Mechanical setup: the tabs' flow is called directly (the active-policy button saves the current state, the chosen menu
// option loads the preset); emergency presets are set as the Manage Policies menu does, and the toggle method the button and
// keybind call is invoked. No tab, button, menu or keybind is clicked.
public class WorkScheduleBatch : Batch
{
    private const string WorkPreset = "BPC test work B", SchedulePreset = "BPC test schedule B";
    private const string AreaA = "BPC test area A", AreaB = "BPC test area B";
    private static readonly string[] SecondWorkCandidates = { "Cleaning", "PlantCutting", "Firefighter", "Crafting" };

    public override string Id => "bpc-work-schedule";
    public override int SegmentCount => 2;
    public override int MapSize => 75;
    public override IReadOnlyList<PlannedCase> Plan { get; } = new[]
    {
        "bpc.work-switch", "bpc.schedule-switch", "bpc.emergency-on", "bpc.emergency-reload", "bpc.emergency-off",
    }.Select(id => new PlannedCase(id, Levels.Mechanical)).ToArray();

    private static BpcTab work, schedule;
    private static BpcTab Work => work ??= new BpcTab("WorkManager", typeof(WorkLink));
    private static BpcTab Schedule => schedule ??= new BpcTab("ScheduleManager", typeof(ScheduleLink));

    // Work state: priorities of Hauling and one second work type every test pawn can do.
    private record WorkState(int Hauling, int Second) { public override string ToString() => $"Hauling {Hauling}/second {Second}"; }
    private static readonly WorkState WorkA = new(1, 3), WorkB = new(4, 1);

    private static WorkState ReadWork(Pawn p, WorkTypeDef second) => new(p.workSettings.GetPriority(WorkTypeDefOf.Hauling), p.workSettings.GetPriority(second));
    private static void ApplyWork(Pawn p, WorkTypeDef second, WorkState s)
    {
        p.workSettings.SetPriority(WorkTypeDefOf.Hauling, s.Hauling);
        p.workSettings.SetPriority(second, s.Second);
    }

    // Schedule state: allowed area plus the timetable; B sleeps for hours 0-7 and works otherwise, A is "anything" all day.
    private static TimeAssignmentDef Hour(bool b, int hour) => !b ? TimeAssignmentDefOf.Anything : hour < 8 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Work;
    private static void ApplySchedule(Pawn p, Area area, bool b)
    {
        p.playerSettings.AreaRestrictionInPawnCurrentMap = area;
        for (int h = 0; h < 24; h++) p.timetable.SetAssignment(h, Hour(b, h));
    }
    private static string ReadSchedule(Pawn p) =>
        $"{p.playerSettings.AreaRestrictionInPawnCurrentMap?.Label ?? "unrestricted"}/{string.Join("", p.timetable.times.Select(t => t.defName[0]))}";
    private static string ScheduleText(string area, bool b) => $"{area}/{string.Join("", Enumerable.Range(0, 24).Select(h => Hour(b, h).defName[0]))}";

    private static string Describe(IEnumerable<Pawn> pawns, System.Func<Pawn, string> read) => string.Join("; ", pawns.Select(p => $"{p.LabelShort} {read(p)}"));

    public override IEnumerable<Wait> Segment(int index, BatchContext ctx)
    {
        if (index == 1)
        {
            foreach (var w in Reloaded(ctx)) yield return w;
            yield break;
        }

        Kit.RemoveStartingColonists(ctx);
        var area = Kit.Area(ctx, 12, 6);
        var pawns = new List<Pawn>
        {
            Kit.Worker(ctx, area.Min + new IntVec3(2, 0, 2), WorkTypeDefOf.Hauling),
            Kit.Worker(ctx, area.Min + new IntVec3(8, 0, 2), WorkTypeDefOf.Hauling),
        };
        var second = SecondWorkCandidates.Select(n => DefDatabase<WorkTypeDef>.GetNamedSilentFail(n))
            .FirstOrDefault(d => d != null && pawns.All(p => !p.WorkTypeIsDisabled(d)));
        if (!Kit.Require(ctx, "bpc.work-switch", (second != null, "both colonists can do one of " + string.Join(", ", SecondWorkCandidates))))
        {
            Kit.SkipRemaining(ctx, this, "no shared second work type", CoverageReasons.SetupFailed);
            yield break;
        }
        Find.PlaySettings.useWorkPriorities = true;
        ctx.Setup($"Developer enabled manual work priorities; second work type {second.defName}");

        // Work: preset 0 keeps state A, preset B keeps state B.
        var work0 = Work.Find(0);
        var workB = Work.Add(WorkPreset);
        foreach (var p in pawns) ApplyWork(p, second, WorkA);
        Work.Select(pawns, workB);
        foreach (var p in pawns) ApplyWork(p, second, WorkB);
        ctx.Setup($"Developer added Work preset '{WorkPreset}' (id {BpcTab.Id(workB)}), set both colonists to {WorkA} under preset 0 and {WorkB} under '{WorkPreset}'");
        Work.Select(pawns, work0);
        bool backA = pawns.All(p => ReadWork(p, second) == WorkA) && BpcTab.Id(Work.Active) == 0;
        string afterBack = Describe(pawns, p => ReadWork(p, second).ToString());
        Work.Select(pawns, workB);
        bool againB = pawns.All(p => ReadWork(p, second) == WorkB) && BpcTab.Id(Work.Active) == BpcTab.Id(workB);
        ctx.Begin("bpc.work-switch").Expect("afterPreset0", $"{WorkA} for both").Expect("afterPresetB", $"{WorkB} for both").Subject(pawns.ToArray())
            .Observe("afterPreset0", afterBack).Observe("afterPresetB", Describe(pawns, p => ReadWork(p, second).ToString()))
            .Observe("activePolicy", Work.Active.label)
            .Check(backA && againB, "selecting a Work preset did not restore its saved priorities or become active");
        Work.Select(pawns, work0);

        // Schedule: preset 0 keeps area A and an all-"anything" day, preset B keeps area B and a sleep/work day.
        ctx.Map.areaManager.TryMakeNewAllowed(out Area_Allowed areaA);
        areaA.RenamableLabel = AreaA;
        ctx.Map.areaManager.TryMakeNewAllowed(out Area_Allowed areaB);
        areaB.RenamableLabel = AreaB;
        foreach (var cell in area) { areaA[cell] = true; areaB[cell] = true; }
        var schedule0 = Schedule.Find(0);
        var scheduleB = Schedule.Add(SchedulePreset);
        foreach (var p in pawns) ApplySchedule(p, areaA, false);
        Schedule.Select(pawns, scheduleB);
        foreach (var p in pawns) ApplySchedule(p, areaB, true);
        ctx.Setup($"Developer made allowed areas '{AreaA}' and '{AreaB}' over the test area, added Schedule preset '{SchedulePreset}' " +
            $"(id {BpcTab.Id(scheduleB)}), and set both colonists to {ScheduleText(AreaA, false)} under preset 0 and {ScheduleText(AreaB, true)} under it");
        Schedule.Select(pawns, schedule0);
        string expectA = ScheduleText(AreaA, false), expectB = ScheduleText(AreaB, true);
        bool sBackA = pawns.All(p => ReadSchedule(p) == expectA) && BpcTab.Id(Schedule.Active) == 0;
        string sAfterBack = Describe(pawns, ReadSchedule);
        Schedule.Select(pawns, scheduleB);
        bool sAgainB = pawns.All(p => ReadSchedule(p) == expectB) && BpcTab.Id(Schedule.Active) == BpcTab.Id(scheduleB);
        ctx.Begin("bpc.schedule-switch").Expect("afterPreset0", expectA).Expect("afterPresetB", expectB).Subject(pawns.ToArray())
            .Observe("afterPreset0", sAfterBack).Observe("afterPresetB", Describe(pawns, ReadSchedule)).Observe("activePolicy", Schedule.Active.label)
            .Check(sBackA && sAgainB, "selecting a Schedule preset did not restore its saved area and timetable or become active");
        Schedule.Select(pawns, schedule0);

        // Emergency: level 1 uses preset B for Work and Schedule; turning it on applies both.
        BpcEmergency.PolicyFor(1, "work"); // creates the emergency levels if the save has none yet
        BpcEmergency.SetPolicy(1, "work", workB);
        BpcEmergency.SetPolicy(1, "restrict", scheduleB);
        bool ready = Kit.Require(ctx, "bpc.emergency-on",
            (!BpcEmergency.OnAlert, "emergency mode is off"),
            (BpcTab.Id(Work.Active) == 0 && BpcTab.Id(Schedule.Active) == 0, "preset 0 is active in Work and Schedule"),
            (pawns.All(p => ReadWork(p, second) == WorkA && ReadSchedule(p) == expectA), "both colonists are in state A"),
            (BpcTab.Id(BpcEmergency.PolicyFor(1, "work")) == BpcTab.Id(workB) && BpcTab.Id(BpcEmergency.PolicyFor(1, "restrict")) == BpcTab.Id(scheduleB),
                "emergency level 1 uses preset B for Work and Schedule"));
        ctx.Setup($"Developer set emergency presets: Work '{WorkPreset}', Schedule '{SchedulePreset}'");
        if (!ready)
        {
            Kit.SkipRemaining(ctx, this, "emergency mode was not set up", CoverageReasons.SetupFailed);
            yield break;
        }
        BpcEmergency.TurnOn();
        bool onOk = BpcEmergency.OnAlert && BpcTab.Id(Work.Active) == BpcTab.Id(workB) && BpcTab.Id(Schedule.Active) == BpcTab.Id(scheduleB)
            && pawns.All(p => ReadWork(p, second) == WorkB && ReadSchedule(p) == expectB);
        ctx.Begin("bpc.emergency-on").Expect("onAlert", true).Expect("activeWork", WorkPreset).Expect("activeSchedule", SchedulePreset)
            .Expect("pawnState", $"{WorkB} {expectB} for both").Subject(pawns.ToArray())
            .Observe("onAlert", BpcEmergency.OnAlert).Observe("activeWork", Work.Active.label).Observe("activeSchedule", Schedule.Active.label)
            .Observe("pawnState", Describe(pawns, p => $"{ReadWork(p, second)} {ReadSchedule(p)}"))
            .Check(onOk, "turning emergency mode on did not apply the emergency Work and Schedule presets");

        ctx.Remember("pawns", string.Join(",", pawns.Select(p => p.ThingID)));
        ctx.Remember("second", second.defName);
        ctx.Remember("workB", BpcTab.Id(workB).ToString());
        ctx.Remember("scheduleB", BpcTab.Id(scheduleB).ToString());
        yield return Wait.Reload();
    }

    private IEnumerable<Wait> Reloaded(BatchContext ctx)
    {
        var ids = ctx.Recall("pawns").Split(',');
        var pawns = ctx.Map.mapPawns.AllPawnsSpawned.Where(p => ids.Contains(p.ThingID)).ToList();
        var second = DefDatabase<WorkTypeDef>.GetNamed(ctx.Recall("second"));
        int workB = int.Parse(ctx.Recall("workB")), scheduleB = int.Parse(ctx.Recall("scheduleB"));
        string expectA = ScheduleText(AreaA, false), expectB = ScheduleText(AreaB, true);

        bool reloadOk = pawns.Count == ids.Length && BpcEmergency.OnAlert && BpcTab.Id(Work.Active) == workB && BpcTab.Id(Schedule.Active) == scheduleB
            && pawns.All(p => ReadWork(p, second) == WorkB && ReadSchedule(p) == expectB)
            && BpcTab.Id(BpcEmergency.PolicyFor(1, "work")) == workB && BpcTab.Id(BpcEmergency.PolicyFor(1, "restrict")) == scheduleB;
        ctx.Begin("bpc.emergency-reload").Expect("pawnsFound", ids.Length).Expect("onAlert", true).Expect("activeWorkId", workB).Expect("activeScheduleId", scheduleB)
            .Expect("pawnState", $"{WorkB} {expectB} for both").Subject(pawns.ToArray())
            .Observe("pawnsFound", pawns.Count).Observe("onAlert", BpcEmergency.OnAlert)
            .Observe("activeWorkId", BpcTab.Id(Work.Active)).Observe("activeScheduleId", BpcTab.Id(Schedule.Active))
            .Observe("emergencyWorkId", BpcTab.Id(BpcEmergency.PolicyFor(1, "work"))).Observe("emergencyScheduleId", BpcTab.Id(BpcEmergency.PolicyFor(1, "restrict")))
            .Observe("pawnState", Describe(pawns, p => $"{ReadWork(p, second)} {ReadSchedule(p)}"))
            .Check(reloadOk, "emergency mode, its presets or the colonists' emergency state was lost across save and reload");
        if (pawns.Count != ids.Length)
        {
            Kit.SkipRemaining(ctx, this, "the colonists were not found after reload", CoverageReasons.SetupFailed);
            yield break;
        }

        // Turning emergency mode off restores the presets that were active before it, with their saved state.
        BpcEmergency.TurnOff();
        bool offOk = !BpcEmergency.OnAlert && BpcTab.Id(Work.Active) == 0 && BpcTab.Id(Schedule.Active) == 0
            && pawns.All(p => ReadWork(p, second) == WorkA && ReadSchedule(p) == expectA);
        ctx.Begin("bpc.emergency-off").Expect("onAlert", false).Expect("activeWorkId", 0).Expect("activeScheduleId", 0)
            .Expect("pawnState", $"{WorkA} {expectA} for both").Subject(pawns.ToArray())
            .Observe("onAlert", BpcEmergency.OnAlert).Observe("activeWorkId", BpcTab.Id(Work.Active)).Observe("activeScheduleId", BpcTab.Id(Schedule.Active))
            .Observe("pawnState", Describe(pawns, p => $"{ReadWork(p, second)} {ReadSchedule(p)}"))
            .Check(offOk, "turning emergency mode off did not restore preset 0 and its saved state in Work and Schedule");
    }
}
