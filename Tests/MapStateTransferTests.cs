using System.Collections.Generic;
using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class MapStateTransferTests
    {
        [Fact]
        public void ReplacesDestinationDefaultWithSourceSelection()
        {
            var plan = MapStateTransfer.Plan(new List<int> { 41, 99 }, 41, 99);

            Assert.True(plan.ShouldTransfer);
            Assert.Equal(0, plan.SourceSelectionIndex);
            Assert.Equal(new[] { 1 }, plan.DestinationSelectionIndexes);
        }

        [Fact]
        public void DoesNotTransferUnknownOrSameMap()
        {
            Assert.False(MapStateTransfer.Plan(new List<int> { 1, 2 }, 3, 4).ShouldTransfer);
            Assert.False(MapStateTransfer.Plan(new List<int> { 1, 2 }, 1, 1).ShouldTransfer);
            Assert.False(MapStateTransfer.Plan(new List<int> { 1, 2 }, -1, 4).ShouldTransfer);
        }
    }
}
