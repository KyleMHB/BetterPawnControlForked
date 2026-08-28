using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class SchemaMigrationFixtureTests
    {
        [Fact]
        public void OriginalFixturePreservesPoliciesLinksSelectionsAndUnknownDefs()
        {
            var report = new MigrationReport { SourceVersion = 0 };
            var migrated = SchemaMigration.ToVersion2(ReadFixture("original-v0.xml"), report);

            Assert.Equal(new[] { "Auto", "Raid" }, migrated.Policies.Select(policy => policy.Label));
            Assert.Equal(2, migrated.Links.Count);
            Assert.Equal(4, migrated.ActiveSelections.Single().PolicyId);
            Assert.Equal(3, migrated.WorkTypes["MissingOptionalWork"]);
            Assert.Equal("1,2,3", migrated.WorkGivers["MissingOptionalGiver"]);
            Assert.Equal(2, report.ImportedPolicies);
            Assert.Equal(2, report.ImportedLinks);
        }

        [Fact]
        public void Pre29FixtureMigratesWithoutDuplicatingPolicies()
        {
            var report = new MigrationReport { SourceVersion = 1 };
            var migrated = SchemaMigration.ToVersion2(ReadFixture("fork-v1.xml"), report);

            Assert.Equal(new[] { 0, 2 }, migrated.Policies.Select(policy => policy.Id));
            Assert.Single(migrated.Links);
            Assert.Equal(2, migrated.ActiveSelections.Single().PolicyId);
        }

        [Fact]
        public void CorruptFixtureProducesDeterministicRepairsAndFallbacks()
        {
            var report = new MigrationReport { SourceVersion = 0 };
            var migrated = SchemaMigration.ToVersion2(ReadFixture("corrupt-v0.xml"), report);

            Assert.Equal(new[] { 0, 2 }, migrated.Policies.Select(policy => policy.Id));
            Assert.Single(migrated.Links);
            Assert.Equal("Pawn_4", migrated.Links[0].PawnKey);
            Assert.Single(migrated.ActiveSelections);
            Assert.Equal(0, migrated.ActiveSelections[0].PolicyId);
            Assert.Equal(2, report.SkippedRecords);
            Assert.Equal(5, report.Repairs);
        }

        private static CoreFeatureState ReadFixture(string name)
        {
            var document = XDocument.Load(Path.Combine(System.AppContext.BaseDirectory, "Fixtures", name));
            var root = document.Root;
            return new CoreFeatureState
            {
                Policies = root.Element("policies")?.Elements("policy").Select(element => new CorePolicyRecord
                {
                    Id = (int)element.Attribute("id"),
                    Label = (string)element.Attribute("label")
                }).ToList(),
                Links = root.Element("links")?.Elements("link").Select(element => new CoreLinkRecord
                {
                    PolicyId = (int)element.Attribute("policyId"),
                    MapId = (int)element.Attribute("mapId"),
                    PawnKey = (string)element.Attribute("pawnKey")
                }).ToList(),
                ActiveSelections = root.Element("active")?.Elements("selection").Select(element => new CoreMapSelectionRecord
                {
                    MapId = (int)element.Attribute("mapId"),
                    PolicyId = (int)element.Attribute("policyId")
                }).ToList(),
                WorkTypes = root.Element("workTypes")?.Elements("work").ToDictionary(
                    element => (string)element.Attribute("defName"),
                    element => (int)element.Attribute("value")) ?? new Dictionary<string, int>(),
                WorkGivers = root.Element("workGivers")?.Elements("giver").ToDictionary(
                    element => (string)element.Attribute("defName"),
                    element => (string)element.Attribute("value")) ?? new Dictionary<string, string>()
            };
        }
    }
}
