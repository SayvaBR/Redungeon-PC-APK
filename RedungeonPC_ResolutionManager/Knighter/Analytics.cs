using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Desktop replacement for Android's Analytics.cs (Firebase/Flurry). No-op:
/// nothing is tracked. Safe to leave this way indefinitely.
/// </summary>
public class Analytics : Component, IAnalytics
{
	public void Initialize()
	{
	}

	public void TrackScreen(string screenName)
	{
	}

	public void TrackEvent(AnalyticsCategory category, string action, string label)
	{
	}

	public void TrackEvent(AnalyticsCategory category, string action, string label, int value)
	{
	}

	public void TrackException(string message, bool isFatal)
	{
	}

	public void Dispatch()
	{
	}
}
