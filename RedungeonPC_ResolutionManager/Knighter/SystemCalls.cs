using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Desktop replacement for Android's SystemCalls.cs. OpenUrl uses the OS
/// shell instead of an Android Intent; leaderboards/achievements/sharing
/// are no-ops (no Google Play Games on desktop). GetDeviceUniqueId persists
/// a generated GUID via Storage so it stays stable across runs.
/// </summary>
public class SystemCalls : Component, ISystemCalls
{
	// original Android SystemCalls.cs also left this event unwired (empty add/remove),
	// so this is not a behavior regression.
	public event EventHandler InternetStatusChanged
	{
		add { }
		remove { }
	}

	public void OpenUrl(string url)
	{
		// Offline desktop edition: external promotion is disabled.
	}

	public bool IsInternetAvailable()
	{
		try
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}
		catch
		{
			return true;
		}
	}

	public void ShowLeaderboards()
	{
	}

	public void ShowAchievments()
	{
	}

	public void ShowSharingMenu(string text, Screenshot screenshot)
	{
	}

	public string GetVersionString(bool withBuildNumber = true)
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
		return withBuildNumber ? version.ToString() : $"{version.Major}.{version.Minor}";
	}

	public string GetDeviceName()
	{
		return Environment.MachineName;
	}

	public string GetDeviceUniqueId()
	{
		const string key = "device_unique_id";
		if (!core.Storage.FieldExist(key))
		{
			core.Storage.SetField(key, Guid.NewGuid().ToString());
			core.Storage.Save();
		}
		return core.Storage.GetField(key);
	}

	// Not part of ISystemCalls - called directly by MenuState.OnBackButtonPressed().
	// No-op for now; wire up to the game window if you want Escape to minimize.
	public void MinimizeGame()
	{
	}
}
