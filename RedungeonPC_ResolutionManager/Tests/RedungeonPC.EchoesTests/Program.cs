using System.Diagnostics;
using System.Text.Json;
using Knighter.Echoes;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

int count = args.Length > 0 ? int.Parse(args[0]) : 10_000;
var clock = Stopwatch.StartNew();
int plans = 0;
for (int seed = 0; seed < count; seed++)
{
    for (int region = 0; region < 3; region++)
    {
        EchoesRegionPlan plan = EchoesRegionPlan.Generate(seed, region);
        var errors = EchoesPlanValidator.Validate(plan);
        Check(errors.Count == 0, $"Seed {seed}, region {region}: {string.Join("; ", errors)}");
        if (seed < 10)
        {
            var repeat = EchoesRegionPlan.Generate(seed, region);
            Check(plan.FloorTiles.OrderBy(p => p.Y).ThenBy(p => p.X).SequenceEqual(repeat.FloorTiles.OrderBy(p => p.Y).ThenBy(p => p.X)), "Seed is not deterministic.");
        }
        plans++;
    }
}
foreach (int seed in new[] { int.MinValue, -1, int.MaxValue })
    for (int region = 0; region < 3; region++)
        Check(EchoesPlanValidator.Validate(EchoesRegionPlan.Generate(seed, region)).Count == 0, "Extreme seed failed.");

var run = EchoesRunData.NewRun(12345);
Check(run.Hearts == 3 && run.Region == 0, "Run must begin with three hearts in Verdant Ruins.");
Check(!run.AdvanceRegion(), "Unfinished boss must not advance region.");
run.Gold = 17; run.Shards = 4; run.Hearts = 2; run.HasAncientEcho = true;
for (int region = 0; region < 3; region++)
{
    run.HasBossSeal = true; run.BossObjectivesCompleted = 3;
    run.VisitedNodes.Add("entry"); run.ClaimedNodes.Add("treasure");
    Check(run.AdvanceRegion(), "Finished boss must advance region.");
    Check(run.Gold == 17 && run.Shards == 4 && run.Hearts == 2 && run.HasAncientEcho, "Region transition lost run resources.");
    if (region < 2) Check(run.Region == region + 1 && run.VisitedNodes.Count == 0 && !run.HasBossSeal, "Local progression did not reset.");
}
Check(run.Victory && run.Region == 2, "Final region must end in victory.");
var restored = JsonSerializer.Deserialize<EchoesRunData>(JsonSerializer.Serialize(run));
Check(restored.Seed == 12345 && restored.Victory && restored.Gold == 17, "Run save round-trip failed.");
Console.WriteLine($"PASS: {plans:N0} region plans ({count:N0} seeds × 3 regions), extreme seeds, determinism, optional-gate closure, mandatory boss stages, sparse floor, route intersections, run progression and JSON round-trip. {clock.Elapsed.TotalSeconds:F1}s.");
