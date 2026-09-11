using Knighter.Entities;

namespace Knighter.States;

public class SessionData
{
	public readonly Knighter.Gameplay.CoinChain CoinChain = new();
	public int CollectedCoins;

	public int Distance;

	public int Ticks;

	public InjuryType CauseOfDeath;

	public int Revives;

	public int MaxPlayerY;
}
