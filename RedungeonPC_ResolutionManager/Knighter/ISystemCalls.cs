using System;

namespace Knighter;

public interface ISystemCalls
{
	event EventHandler InternetStatusChanged;

	void OpenUrl(string url);

	bool IsInternetAvailable();

	void ShowLeaderboards();

	void ShowAchievments();

	void ShowSharingMenu(string text, Screenshot screenshot);

	string GetVersionString(bool withBuildNumber = true);

	string GetDeviceName();

	string GetDeviceUniqueId();
}
