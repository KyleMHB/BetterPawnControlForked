using System.Collections.Generic;
using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class MapStateTransferEdgeCaseTests
    {
        [Fact]
        public void RepeatedLandingRemovesEveryStaleDestinationSelection()
        {
            var plan = MapStateTransfer.Plan(new List<int> { 9, 12, 12, 44 }, 9, 12);

            Assert.True(plan.ShouldTransfer);
            Assert.Equal(new[] { 1, 2 }, plan.DestinationSelectionIndexes);
        }

        [Fact]
        public void MultiMapSourceSelectionIsLeftAloneUntilCallerApprovesTransition()
        {
            var plan = MapStateTransfer.Plan(new List<int> { 9, 44 }, 9, 12);

            Assert.True(plan.ShouldTransfer);
            Assert.Empty(plan.DestinationSelectionIndexes);
        }
    }
}
