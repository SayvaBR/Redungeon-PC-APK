using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Desktop replacement for Android's Scores.cs (Google Play Games
/// leaderboards/achievements via GooglePlayHelper). No-op on desktop -
/// nothing to sign into. If a build error later reveals more members are
/// needed here, send me the error and I'll extend this.
/// </summary>
public class Scores : Component
{
	public void Authenticate()
	{
	}

	public void ForceReportAllUnlockedAchievements()
	{
	}

	public void ReportBestScore(bool gold)
	{
	}

	public void ReportAchievment(Achievement achievement)
	{
	}

	public void ReportAchievmentsProgress(List<Achievement> achievements)
	{
	}
}
