namespace BetterPawnControlForked.CoreLogic
{
    internal sealed class MigrationReport
    {
        internal int SourceVersion { get; set; }
        internal int ImportedPolicies { get; set; }
        internal int ImportedLinks { get; set; }
        internal int Repairs { get; set; }
        internal int SkippedRecords { get; set; }
        internal int WarningCount { get; set; }

        public override string ToString()
        {
            return "source=" + SourceVersion
                + ", policies=" + ImportedPolicies
                + ", links=" + ImportedLinks
                + ", repairs=" + Repairs
                + ", skipped=" + SkippedRecords
                + ", warnings=" + WarningCount;
        }
    }
}
