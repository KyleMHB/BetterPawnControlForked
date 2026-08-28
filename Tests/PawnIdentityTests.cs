using System;
using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class PawnIdentityTests
    {
        [Fact]
        public void RejectsNullAndBrokenIdentities()
        {
            var pawn = new TestPawn("Pawn_1");

            Assert.False(PawnIdentity.Same<TestPawn>(null, pawn, item => item.Id));
            Assert.False(PawnIdentity.Same(pawn, new TestPawn(null), item => item.Id));
            Assert.False(PawnIdentity.Same(pawn, new TestPawn("Pawn_1"), item => throw new InvalidOperationException()));
        }

        [Fact]
        public void UsesReferenceIdentityBeforeStableLoadId()
        {
            var pawn = new TestPawn("Pawn_1");
            var keyCalls = 0;

            Assert.True(PawnIdentity.Same(pawn, pawn, item => { keyCalls++; return item.Id; }));
            Assert.Equal(0, keyCalls);
            Assert.True(PawnIdentity.Same(pawn, new TestPawn("Pawn_1"), item => item.Id));
        }

        private sealed class TestPawn
        {
            internal TestPawn(string id) { Id = id; }
            internal string Id { get; }
        }
    }
}
