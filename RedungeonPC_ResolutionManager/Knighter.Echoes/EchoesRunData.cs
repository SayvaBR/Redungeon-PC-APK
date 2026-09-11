using System.Collections.Generic;

namespace Knighter.Echoes;

/// <summary>JSON-friendly run state. Permanent profile progression belongs to the separate Echoes save.</summary>
public sealed class EchoesRunData
{
    public int Version { get; set; } = 1;
    public int Seed { get; set; }
    public int Region { get; set; }
    public int Hearts { get; set; } = 3;
    public int MaxHearts { get; set; } = 3;
    public int Gold { get; set; }
    public int Shards { get; set; }
    public bool HasAncientEcho { get; set; }
    public bool HasBossSeal { get; set; }
    public int BossObjectivesCompleted { get; set; }
    public bool Victory { get; set; }
    public List<string> VisitedNodes { get; set; } = new List<string>();
    public List<string> ClaimedNodes { get; set; } = new List<string>();

    public static EchoesRunData NewRun(int seed) => new EchoesRunData { Seed = seed };

    /// <summary>Preserve build/currency/health between regions, reset only local progression.</summary>
    public bool AdvanceRegion()
    {
        if (BossObjectivesCompleted < 3) return false;
        if (Region >= 2) { Victory = true; return true; }
        Region++;
        HasBossSeal = false;
        BossObjectivesCompleted = 0;
        VisitedNodes.Clear();
        ClaimedNodes.Clear();
        return true;
    }
}
