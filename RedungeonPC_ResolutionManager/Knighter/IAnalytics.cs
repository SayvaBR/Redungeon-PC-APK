namespace Knighter;

public interface IAnalytics
{
	void Initialize();

	void TrackScreen(string screenName);

	void TrackEvent(AnalyticsCategory category, string action, string label);

	void TrackEvent(AnalyticsCategory category, string action, string label, int value);

	void TrackException(string message, bool isFatal);
}
