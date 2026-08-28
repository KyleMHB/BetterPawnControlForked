using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class PolicyIdResolverTests
    {
        [Fact]
        public void MissingPolicyFallsBackToZero()
        {
            Assert.Equal(0, PolicyIdResolver.Resolve(7, new[] { 0, 2, 5 }));
            Assert.Equal(5, PolicyIdResolver.Resolve(5, new[] { 0, 2, 5 }));
        }
    }
}
