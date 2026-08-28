using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using BetterPawnControlForked.CoreLogic;

namespace BetterPawnControlForked
{
    public class DataStorage : WorldComponent
    {
        internal const int CurrentDataVersion = 2;
        private static readonly BpcState StartupState = new BpcState();

        internal static DataStorage Current { get; private set; }
        internal BpcState state;
        private int dataVersion = CurrentDataVersion;
        private int loadedSourceVersion = CurrentDataVersion;

        public DataStorage(World world) : base(world)
        {
            state = new BpcState();
            Current = this;
        }

        internal static FeatureState<T> GetFeature<T>() where T : Link
        {
            return (Current?.state ?? StartupState).Feature<T>();
        }

        internal static BpcState State => Current?.state ?? StartupState;

        public override void ExposeData()
        {
            base.ExposeData();
            state = state ?? new BpcState();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                dataVersion = CurrentDataVersion;
            }

            Scribe_Values.Look(ref dataVersion, "dataVersion", 0, true);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                loadedSourceVersion = dataVersion;
            }

            Scribe_References.Look(ref state.defaultOutfit, "DefaultOutfit");
            Scribe_References.Look(ref state.assign.defaultFoodPolicy, "DefaultFoodPolicy");
            Scribe_References.Look(ref state.defaultDrugPolicy, "DefaultDrugPolicy");
            Scribe_References.Look(ref state.assign.defaultReadingPolicy, "DefaultReadingPolicy");
            Scribe_References.Look(ref state.defaultPrisonerFoodPolicy, "DefaultPrisonerFoodPolicy");
            Scribe_References.Look(ref state.defaultSlaveOutfit, "DefaultSlaveOutfit");
            Scribe_References.Look(ref state.defaultSlaveFoodPolicy, "DefaultSlaveFoodPolicy");
            Scribe_References.Look(ref state.defaultSlaveDrugPolicy, "DefaultSlaveDrugPolicy");
            Scribe_References.Look(ref state.defaultSlaveReadingPolicy, "DefaultSlaveReadingPolicy");
            Scribe_Values.Look(ref state.defaultLoadoutId, "DefaultWeaponsLoadout", 0, true);

            Scribe_Collections.Look(ref state.assign.policies, "AssignPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.assign.links, "AssignLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.assign.activePolicies, "AssignActivePolicies", LookMode.Deep);

            Scribe_Collections.Look(ref state.animal.policies, "AnimalPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.animal.links, "AnimalLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.animal.activePolicies, "AnimalActivePolicies", LookMode.Deep);

            Scribe_Collections.Look(ref state.schedule.policies, "RestrictPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.schedule.links, "ScheduleLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.schedule.activePolicies, "RestrictActivePolicies", LookMode.Deep);

            Scribe_Collections.Look(ref state.work.policies, "WorkPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.work.links, "WorkLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.work.activePolicies, "WorkActivePolicies", LookMode.Deep);

            Scribe_Collections.Look(ref state.mech.policies, "MechPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.mech.links, "MechLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.mech.activePolicies, "MechActivePolicies", LookMode.Deep);

            Scribe_Collections.Look(ref state.weapons.policies, "WeaponsPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.weapons.links, "WeaponsLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.weapons.activePolicies, "WeaponsActivePolicies", LookMode.Deep);

            Scribe_Collections.Look(ref state.robot.policies, "RobotPolicies", LookMode.Deep);
            Scribe_Collections.Look(ref state.robot.links, "RobotLinks", LookMode.Deep);
            Scribe_Collections.Look(ref state.robot.activePolicies, "RobotActivePolicies", LookMode.Deep);

            Scribe_Values.Look(ref state.alertLevel, "ActiveLevel", 0, true);
            Scribe_Collections.Look(ref state.alertLevels, "AlertLevelsList", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MigrateLoadedState();
            }
        }

        private void MigrateLoadedState()
        {
            var report = new MigrationReport { SourceVersion = loadedSourceVersion };
            state.EnsureInitialized();

            NormalizeFeature(state.assign, report);
            NormalizeFeature(state.animal, report);
            NormalizeFeature(state.schedule, report);
            NormalizeFeature(state.work, report);
            NormalizeFeature(state.mech, report);
            NormalizeFeature(state.weapons, report);
            NormalizeFeature(state.robot, report);

            foreach (var link in state.schedule.links.Where(link => link != null))
            {
                link.RepairSchedule();
            }

            RemoveInvalidPawnLinks(report);
            state.alertLevels = state.alertLevels ?? new List<AlertLevel>();
            dataVersion = CurrentDataVersion;

            if (loadedSourceVersion < CurrentDataVersion)
            {
                if (report.SkippedRecords > 0)
                {
                    report.WarningCount = 1;
                    Log.Warning("[BPC] Migrated save to schema 2: " + report);
                }
                else
                {
                    Log.Message("[BPC] Migrated save to schema 2: " + report);
                }
            }
        }

        private static void NormalizeFeature<T>(FeatureState<T> feature, MigrationReport report) where T : Link
        {
            var sourcePolicies = feature.policies;
            var sourceLinks = feature.links;
            var sourceSelections = feature.activePolicies;
            var normalized = SchemaMigration.ToVersion2(new CoreFeatureState
            {
                Policies = sourcePolicies?.Select(policy => policy == null ? null : new CorePolicyRecord { Id = policy.id, Label = policy.label }).ToList(),
                Links = sourceLinks?.Select(link => link == null ? null : new CoreLinkRecord { PolicyId = link.zone, MapId = link.mapId, PawnKey = "loaded" }).ToList(),
                ActiveSelections = sourceSelections?.Select(selection => selection == null ? null : new CoreMapSelectionRecord
                {
                    MapId = selection.mapId,
                    PolicyId = selection.activePolicy?.id ?? 0
                }).ToList()
            }, report);

            feature.policies = normalized.Policies.Select(record =>
                sourcePolicies?.FirstOrDefault(policy => policy != null && policy.id == record.Id)
                ?? new Policy(record.Id, record.Label == "Auto" ? "BPC.Auto".Translate().ToString() : record.Label)).ToList();

            var validPolicyIds = new HashSet<int>(normalized.Policies.Select(policy => policy.Id));
            feature.links = (sourceLinks ?? new List<T>())
                .Where(link => link != null && validPolicyIds.Contains(link.zone))
                .ToList();

            feature.activePolicies = normalized.ActiveSelections.Select(record =>
            {
                var selection = sourceSelections?.FirstOrDefault(item => item != null && item.mapId == record.MapId)
                    ?? new MapActivePolicy();
                selection.mapId = record.MapId;
                selection.activePolicy = feature.policies.First(policy => policy.id == record.PolicyId);
                return selection;
            }).ToList();
            feature.EnsureInitialized();
        }

        private void RemoveInvalidPawnLinks(MigrationReport report)
        {
            int removed = 0;
            removed += RemoveWhere(state.assign.links, link => link.colonist == null);
            removed += RemoveWhere(state.schedule.links, link => link.colonist == null);
            removed += RemoveWhere(state.work.links, link => link.colonist == null);
            removed += RemoveWhere(state.weapons.links, link => link.colonist == null);
            removed += RemoveWhere(state.animal.links, link => link.animal == null);
            removed += RemoveWhere(state.mech.links, link => link.mech == null);
            removed += RemoveWhere(state.robot.links, link => link.robot == null);
            report.Repairs += removed;
            report.SkippedRecords += removed;
        }

        private static int RemoveWhere<T>(List<T> items, System.Predicate<T> predicate)
        {
            var before = items.Count;
            items.RemoveAll(predicate);
            return before - items.Count;
        }
    }
}
