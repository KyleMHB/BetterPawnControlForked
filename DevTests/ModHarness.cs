using System;
using System.Collections.Generic;
using RimWorldDevTests;

namespace BetterPawnControlForked.DevTests;

public sealed class ModHarness : HarnessSettings
{
    public override string DisplayName => "Better Pawn Control Forked";
    public override string ArgPrefix => "bpc";
    public override string PackageId => "kylemhb.betterpawncontrolforked";
    public override Type ProductionType => typeof(global::BetterPawnControlForked.BetterPawnControlForkedMod);
    public override string EvidenceFolder => "BetterPawnControlForkedTests";
    public override string[] RuntimeFolders => new[] { "About", "Common", "v1.6" };
    public override string[] RuntimeFiles => new[] { "LoadFolders.xml" };
    public override IEnumerable<Batch> CreateBatches() => new Batch[] { new AssignBatch(), new WorkScheduleBatch() };
    public override IEnumerable<string> RunAll => new[] { "bpc-assign", "bpc-work-schedule" };
}
