using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Knighter.Echoes;

/// <summary>Headless structural checks; runtime hazard timing still needs playtesting.</summary>
public static class EchoesPlanValidator
{
    private static readonly Point[] Directions = { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };

    public static IReadOnlyList<string> Validate(EchoesRegionPlan plan)
    {
        var errors = new List<string>();
        var nodes = plan.Nodes.ToDictionary(n => n.Id);
        if (nodes.Count != plan.Nodes.Count) errors.Add("Duplicate node ID.");
        if (plan.Edges.Select(e => e.Id).Distinct().Count() != plan.Edges.Count) errors.Add("Duplicate edge ID.");
        if (plan.Bounds.Width > 80 || plan.Bounds.Height > 100) errors.Add("Region exceeds tile budget.");
        if (plan.FloorTiles.Count > plan.Bounds.Width * plan.Bounds.Height * 0.30) errors.Add("Region fills too much void.");
        if (plan.Nodes.Count(n => n.Kind == EchoesNodeKind.Start) != 1 || plan.Nodes.Count(n => n.Kind == EchoesNodeKind.Exit) != 1)
            errors.Add("Region requires exactly one entry and exit.");
        if (!plan.Nodes.Where(n => n.Kind == EchoesNodeKind.BossObjective).Select(n => n.ObjectiveIndex).OrderBy(i => i).SequenceEqual(new[] { 0, 1, 2 }))
            errors.Add("Boss requires three ordered objectives.");
        foreach (EchoesEdge edge in plan.Edges)
        {
            if (!nodes.ContainsKey(edge.FromId) || !nodes.ContainsKey(edge.ToId)) { errors.Add("Orphan edge: " + edge.Id); continue; }
            if (edge.Path.Count < 2 || edge.Path[0] != nodes[edge.FromId].Position || edge.Path[edge.Path.Count - 1] != nodes[edge.ToId].Position)
                errors.Add("Invalid route endpoints: " + edge.Id);
            for (int i = 1; i < edge.Path.Count; i++)
                if (Distance(edge.Path[i - 1], edge.Path[i]) != 1) errors.Add("Non-cardinal route step: " + edge.Id);
            if (edge.RequiresAncientEcho && nodes[edge.ToId].Kind != EchoesNodeKind.Secret)
                errors.Add("Ancient ability placed on a nonsecret connection.");
        }
        // A node is the only legal contact between separately authored routes.
        // Check adjacency too: overlapping floor is not the only way to create a bypass.
        for (int a = 0; a < plan.Edges.Count; a++)
        {
            EchoesEdge first = plan.Edges[a];
            var firstTiles = new HashSet<Point>(first.Tiles);
            for (int b = a + 1; b < plan.Edges.Count; b++)
            {
                EchoesEdge second = plan.Edges[b];
                var shared = new[] { first.FromId, first.ToId }.Intersect(new[] { second.FromId, second.ToId }).Select(id => nodes[id].Position).ToArray();
                foreach (Point p in second.Tiles)
                {
                    if (shared.Any(n => Distance(n, p) <= 3)) continue;
                    if (firstTiles.Contains(p) || Directions.Any(d => firstTiles.Contains(new Point(p.X + d.X, p.Y + d.Y))))
                    { errors.Add("Unintended route contact: " + first.Id + " / " + second.Id); break; }
                }
            }
        }
        var graphReachable = ReachNodes(plan, null);
        foreach (EchoesNode node in plan.Nodes.Where(n => n.Kind != EchoesNodeKind.Secret))
            if (!graphReachable.Contains(node.Id)) errors.Add("Required node unreachable without Ancient Echo: " + node.Id);
        int ordinaryEdges = plan.Edges.Count(e => !e.RequiresAncientEcho);
        if (ordinaryEdges < graphReachable.Count) errors.Add("Region has no ordinary exploration loop.");
        // Mandatory boss stages cannot be bypassed via the exploration loop.
        foreach (EchoesNode objective in plan.Nodes.Where(n => n.Kind == EchoesNodeKind.BossObjective || n.Kind == EchoesNodeKind.Seal))
            if (ReachNodes(plan, objective.Id).Contains(plan.Exit.Id)) errors.Add("Exit bypasses required objective: " + objective.Id);

        var floor = new HashSet<Point>(plan.FloorTiles);
        // Close the middle of every optional ancient bridge exactly as runtime should.
        foreach (EchoesEdge gate in plan.Edges.Where(e => e.RequiresAncientEcho))
            floor.Remove(gate.Path[gate.Path.Count / 2]);
        var tileReachable = Flood(floor, plan.Start.Position);
        foreach (EchoesNode node in plan.Nodes.Where(n => n.Kind != EchoesNodeKind.Secret))
            if (!tileReachable.Contains(node.Position)) errors.Add("Floor disconnected with ancient gate closed: " + node.Id);
        foreach (EchoesNode node in plan.Nodes.Where(n => n.Kind == EchoesNodeKind.Secret))
            if (tileReachable.Contains(node.Position)) errors.Add("Ancient gate can be walked around.");
        if (Flood(new HashSet<Point>(plan.FloorTiles), plan.Start.Position).Count != plan.FloorTiles.Count)
            errors.Add("Orphan floor tiles.");
        return errors;
    }

    private static int Distance(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    private static HashSet<string> ReachNodes(EchoesRegionPlan plan, string blocked)
    {
        var seen = new HashSet<string> { plan.Start.Id };
        var queue = new Queue<string>(); queue.Enqueue(plan.Start.Id);
        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            foreach (EchoesEdge edge in plan.Edges)
            {
                if (edge.RequiresAncientEcho || edge.FromId == blocked || edge.ToId == blocked) continue;
                string next = edge.FromId == current ? edge.ToId : edge.ToId == current ? edge.FromId : null;
                if (next != null && seen.Add(next)) queue.Enqueue(next);
            }
        }
        return seen;
    }

    private static HashSet<Point> Flood(HashSet<Point> floor, Point start)
    {
        var seen = new HashSet<Point>();
        var queue = new Queue<Point>();
        if (floor.Contains(start)) { seen.Add(start); queue.Enqueue(start); }
        while (queue.Count > 0)
        {
            Point current = queue.Dequeue();
            foreach (Point d in Directions)
            {
                var next = new Point(current.X + d.X, current.Y + d.Y);
                if (floor.Contains(next) && seen.Add(next)) queue.Enqueue(next);
            }
        }
        return seen;
    }
}
