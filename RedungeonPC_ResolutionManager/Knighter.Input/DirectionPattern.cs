using System;
using System.Collections.Generic;

namespace Knighter.Input;

/// <summary>A directional challenge advances once per input edge; mistakes reset only this pattern.</summary>
public sealed class DirectionPattern
{
	private readonly int[] steps;
	public IReadOnlyList<int> Steps => Array.AsReadOnly(steps);
	public int Progress { get; private set; }
	public bool Complete => Progress == steps.Length;
	public DirectionPattern(int[] steps)
	{
		if (steps == null || steps.Length < 2 || steps.Length > 6) throw new ArgumentException("A pattern needs 2 to 6 arrows.", nameof(steps));
		this.steps = (int[])steps.Clone();
		foreach (int step in this.steps) if (step < 0 || step > 3) throw new ArgumentOutOfRangeException(nameof(steps));
	}
	public bool Submit(int direction)
	{
		if (Complete) return false;
		if (steps[Progress] != direction) { Progress = 0; return false; }
		Progress++;
		return true;
	}
}
