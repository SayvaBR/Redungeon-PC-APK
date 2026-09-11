using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Knighter.Echoes;

public enum EchoesNodeKind { Start, Junction, Merchant, Treasure, Refuge, Shrine, Seal, BossObjective, Exit, Secret }

public sealed class EchoesNode
{
    public string Id { get; internal set; }
    public EchoesNodeKind Kind { get; internal set; }
    public Point Position { get; internal set; }
    public int SectorIndex { get; internal set; }
    public int ObjectiveIndex { get; internal set; } = -1;
}

public sealed class EchoesEdge
{
    public string Id { get; internal set; }
    public string FromId { get; internal set; }
    public string ToId { get; internal set; }
    /// <summary>Every centerline step is one cardinal tile, including both endpoints.</summary>
    public IReadOnlyList<Point> Path { get; internal set; }
    public IReadOnlyCollection<Point> Tiles { get; internal set; }
    public bool RequiresAncientEcho { get; internal set; }
}

/// <summary>
/// Authored region skeleton with deterministic seed variation. Coordinates are tiles, with
/// positive Y toward the exit. Empty coordinates are void, never implicit arena floor.
/// Native routes do not rotate any legacy module: runtime adapters may populate straight runs.
/// </summary>
public sealed class EchoesRegionPlan
{
    public int Seed { get; private set; }
    public int Region { get; private set; }
    public string RegionName => new[] { "Verdant Ruins", "Iron Maw", "Red Core" }[Region];
    public string BossName => new[] { "The Root Keeper", "The Furnace Heart", "The Darkness" }[Region];
    public IReadOnlyList<EchoesNode> Nodes { get; private set; }
    public IReadOnlyList<EchoesEdge> Edges { get; private set; }
    public IReadOnlyCollection<Point> FloorTiles { get; private set; }
    public Rectangle Bounds { get; private set; }
    public EchoesNode Start => Nodes.First(n => n.Kind == EchoesNodeKind.Start);
    public EchoesNode Exit => Nodes.First(n => n.Kind == EchoesNodeKind.Exit);

    public static EchoesRegionPlan Generate(int seed, int region)
    {
        if (region < 0 || region > 2) throw new ArgumentOutOfRangeException(nameof(region));
        // Stable arithmetic PRNG, independent of runtime-specific System.Random behavior.
        uint state = unchecked((uint)seed ^ ((uint)(region + 1) * 0x9E3779B9u));
        uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
        bool mirror = (Next() & 1) != 0;
        int upper = 18 + (int)(Next() % 3), middle = upper + 13, lower = upper + 26;
        Point P(int x, int y) => new Point(mirror ? 56 - x : x, y);
        var nodes = new List<EchoesNode>();
        EchoesNode Node(string id, EchoesNodeKind kind, int x, int y, int sector, int objective = -1)
        {
            var node = new EchoesNode { Id = id, Kind = kind, Position = P(x, y), SectorIndex = sector, ObjectiveIndex = objective };
            nodes.Add(node);
            return node;
        }
        Node("entry", EchoesNodeKind.Start, 28, 4, 0);
        Node("crossroads", EchoesNodeKind.Junction, 28, upper, 0);
        Node("merchant", EchoesNodeKind.Merchant, 8, upper, 0);
        Node("galleries", EchoesNodeKind.Junction, 18, middle, 1);
        Node("refuge", EchoesNodeKind.Refuge, 40, middle, 1);
        Node("treasure", EchoesNodeKind.Treasure, 52, middle, 1);
        Node("shrine", EchoesNodeKind.Shrine, 6, middle, 1);
        Node("secret", EchoesNodeKind.Secret, 6, middle + 10, 1);
        Node("reunion", EchoesNodeKind.Junction, 28, lower, 1);
        Node("seal", EchoesNodeKind.Seal, 28, lower + 10, 2);
        Node("boss-1", EchoesNodeKind.BossObjective, 28, lower + 22, 2, 0);
        Node("boss-2", EchoesNodeKind.BossObjective, 40, lower + 34, 2, 1);
        Node("boss-3", EchoesNodeKind.BossObjective, 28, lower + 46, 2, 2);
        Node("exit", EchoesNodeKind.Exit, 28, lower + 51, 2);
        var edges = new List<EchoesEdge>();
        void Edge(string from, string to, bool ancient = false, params Point[] bends)
        {
            var a = nodes.First(n => n.Id == from);
            var b = nodes.First(n => n.Id == to);
            var waypoints = new List<Point> { a.Position };
            waypoints.AddRange(bends.Select(p => P(p.X, p.Y)));
            waypoints.Add(b.Position);
            var path = new List<Point> { a.Position };
            foreach (Point target in waypoints.Skip(1))
            {
                Point current = path[path.Count - 1];
                if (current.X != target.X && current.Y != target.Y) throw new InvalidOperationException("Route bend must be cardinal.");
                while (current != target)
                {
                    current = new Point(current.X + Math.Sign(target.X - current.X), current.Y + Math.Sign(target.Y - current.Y));
                    path.Add(current);
                }
            }
            var tiles = new HashSet<Point>(path);
            // A repeating mix of one- and three-tile bridges. Keep ends narrow so
            // separate exits only meet in the deliberately authored node pockets.
            int phase = (int)(Next() % 7);
            for (int i = 3; i < path.Count - 3; i++)
            {
                if ((i + phase) % 7 < 2 || ancient) continue;
                Point prev = path[i - 1], p = path[i], next = path[i + 1];
                if (prev.X == p.X && next.X == p.X)
                { tiles.Add(new Point(p.X - 1, p.Y)); tiles.Add(new Point(p.X + 1, p.Y)); }
                else if (prev.Y == p.Y && next.Y == p.Y)
                { tiles.Add(new Point(p.X, p.Y - 1)); tiles.Add(new Point(p.X, p.Y + 1)); }
            }
            edges.Add(new EchoesEdge { Id = from + ":" + to, FromId = from, ToId = to, Path = path.ToArray(), Tiles = tiles, RequiresAncientEcho = ancient });
        }
        Edge("entry", "crossroads");
        Edge("crossroads", "merchant");
        Edge("crossroads", "galleries", false, new Point(28, upper + 5), new Point(18, upper + 5));
        Edge("crossroads", "refuge", false, new Point(40, upper));
        Edge("galleries", "shrine");
        Edge("shrine", "secret", true);
        Edge("refuge", "treasure");
        Edge("galleries", "reunion", false, new Point(18, lower));
        Edge("refuge", "reunion", false, new Point(40, lower));
        Edge("reunion", "seal");
        Edge("seal", "boss-1");
        Edge("boss-1", "boss-2", false, new Point(40, lower + 22));
        Edge("boss-2", "boss-3", false, new Point(40, lower + 46));
        Edge("boss-3", "exit");
        var floor = new HashSet<Point>(edges.SelectMany(e => e.Tiles));
        foreach (EchoesNode n in nodes)
            foreach (Point p in Pocket(n)) floor.Add(p);
        int minX = floor.Min(p => p.X), minY = floor.Min(p => p.Y);
        return new EchoesRegionPlan
        {
            Seed = seed, Region = region, Nodes = nodes.ToArray(), Edges = edges.ToArray(), FloorTiles = floor,
            Bounds = new Rectangle(minX, minY, floor.Max(p => p.X) - minX + 1, floor.Max(p => p.Y) - minY + 1)
        };
    }

    /// <summary>Cross-shaped pockets are at most five tiles across, with void corners.</summary>
    public static IEnumerable<Point> Pocket(EchoesNode node)
    {
        for (int y = -2; y <= 2; y++)
            for (int x = -2; x <= 2; x++)
                if (Math.Abs(x) + Math.Abs(y) <= 2)
                    yield return new Point(node.Position.X + x, node.Position.Y + y);
    }
}
