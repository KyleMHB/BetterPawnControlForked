using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class MigrationReportTests
    {
        [Fact]
        public void SummaryIsDeterministicAndComplete()
        {
            var report = new MigrationReport
            {
                SourceVersion = 0,
                ImportedPolicies = 12,
                ImportedLinks = 34,
                Repairs = 5,
                SkippedRecords = 2,
                WarningCount = 1
            };

            Assert.Equal("source=0, policies=12, links=34, repairs=5, skipped=2, warnings=1", report.ToString());
        }
    }
}
