using System;

namespace Knighter.Gameplay;

/// <summary>A run-local combo: five pickups per tier, three seconds to keep it alive.</summary>
public sealed class CoinChain
{
	public const int WindowTicks = 180;
	public const int MaxMultiplier = 5;
	public int Pickups { get; private set; }
	public int Remaining { get; private set; }
	public int PulseTicks { get; private set; }
	public int Multiplier => Math.Min(MaxMultiplier, 1 + Pickups / 5);
	public float Fill => Remaining / (float)WindowTicks;
	public int Collect(int value)
	{
		if (value <= 0) return 0;
		Pickups = Math.Min(25, Pickups + 1);
		Remaining = WindowTicks;
		PulseTicks = 12;
		return checked(value * Multiplier);
	}
	public void Update()
	{
		if (PulseTicks > 0) PulseTicks--;
		if (Remaining > 0 && --Remaining == 0) Pickups = 0;
	}
	public void Reset() { Pickups = Remaining = PulseTicks = 0; }
}
