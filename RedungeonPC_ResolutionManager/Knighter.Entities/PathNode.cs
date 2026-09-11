using System;

namespace Knighter.Entities;

public class PathNode
{
	public int Dx;

	public int Dy;

	public int TicksPerTile;

	public int StopTime;

	public int Distance => Math.Max(Math.Abs(Dx), Math.Abs(Dy));
}
