using System.Collections.Generic;
using System.Linq;

namespace BetterPawnControlForked.CoreLogic
{
    internal sealed class CorePolicyRecord
    {
        internal int Id { get; set; }
        internal string Label { get; set; }
    }

    internal sealed class CoreLinkRecord
    {
        internal int PolicyId { get; set; }
        internal int MapId { get; set; }
        internal string PawnKey { get; set; }
    }

    internal sealed class CoreMapSelectionRecord
    {
        internal int MapId { get; set; }
        internal int PolicyId { get; set; }
    }

    internal sealed class CoreFeatureState
    {
        internal List<CorePolicyRecord> Policies { get; set; }
        internal List<CoreLinkRecord> Links { get; set; }
        internal List<CoreMapSelectionRecord> ActiveSelections { get; set; }
        internal Dictionary<string, int> WorkTypes { get; set; }
        internal Dictionary<string, string> WorkGivers { get; set; }
    }

    internal static class SchemaMigration
    {
        internal static CoreFeatureState ToVersion2(CoreFeatureState state, MigrationReport report)
        {
            state = state ?? new CoreFeatureState();
            report = report ?? new MigrationReport();
            state.Policies = state.Policies ?? new List<CorePolicyRecord>();
            state.Links = state.Links ?? new List<CoreLinkRecord>();
            state.ActiveSelections = state.ActiveSelections ?? new List<CoreMapSelectionRecord>();
            state.WorkTypes = state.WorkTypes ?? new Dictionary<string, int>();
            state.WorkGivers = state.WorkGivers ?? new Dictionary<string, string>();

            int originalPolicyCount = state.Policies.Count;
            state.Policies = state.Policies
                .Where(policy => policy != null)
                .GroupBy(policy => policy.Id)
                .Select(group => group.First())
                .ToList();
            report.Repairs += originalPolicyCount - state.Policies.Count;

            if (state.Policies.All(policy => policy.Id != 0))
            {
                state.Policies.Insert(0, new CorePolicyRecord { Id = 0, Label = "Auto" });
                report.Repairs++;
            }

            var validPolicyIds = new HashSet<int>(state.Policies.Select(policy => policy.Id));
            int originalLinkCount = state.Links.Count;
            state.Links = state.Links
                .Where(link => link != null && !string.IsNullOrEmpty(link.PawnKey) && validPolicyIds.Contains(link.PolicyId))
                .ToList();
            int skippedLinks = originalLinkCount - state.Links.Count;
            report.Repairs += skippedLinks;
            report.SkippedRecords += skippedLinks;

            int originalSelectionCount = state.ActiveSelections.Count;
            state.ActiveSelections = state.ActiveSelections
                .Where(selection => selection != null)
                .GroupBy(selection => selection.MapId)
                .Select(group => group.First())
                .ToList();
            report.Repairs += originalSelectionCount - state.ActiveSelections.Count;

            foreach (var selection in state.ActiveSelections)
            {
                selection.PolicyId = PolicyIdResolver.Resolve(selection.PolicyId, validPolicyIds);
            }

            if (state.ActiveSelections.Count == 0)
            {
                state.ActiveSelections.Add(new CoreMapSelectionRecord { MapId = 0, PolicyId = 0 });
                report.Repairs++;
            }

            if (report.SourceVersion == 0)
            {
                report.ImportedPolicies += state.Policies.Count;
                report.ImportedLinks += state.Links.Count;
            }

            return state;
        }
    }
}
