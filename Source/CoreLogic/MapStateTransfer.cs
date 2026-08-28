using System.Collections.Generic;

namespace BetterPawnControlForked.CoreLogic
{
    internal sealed class MapTransferPlan
    {
        internal bool ShouldTransfer { get; set; }
        internal int SourceSelectionIndex { get; set; } = -1;
        internal List<int> DestinationSelectionIndexes { get; } = new List<int>();
    }

    internal static class MapStateTransfer
    {
        internal static MapTransferPlan Plan(IReadOnlyList<int> activeMapIds, int sourceMapId, int destinationMapId)
        {
            var result = new MapTransferPlan();
            if (activeMapIds == null || sourceMapId < 0 || sourceMapId == destinationMapId)
            {
                return result;
            }

            for (var index = 0; index < activeMapIds.Count; index++)
            {
                if (activeMapIds[index] == sourceMapId && result.SourceSelectionIndex < 0)
                {
                    result.SourceSelectionIndex = index;
                }
                else if (activeMapIds[index] == destinationMapId)
                {
                    result.DestinationSelectionIndexes.Add(index);
                }
            }

            result.ShouldTransfer = result.SourceSelectionIndex >= 0;
            return result;
        }
    }
}
