using System;
using Knighter.Entities;

namespace Knighter;

public class AdsManager : Component
{
	public delegate void OnShowDelegate();

	public delegate void OnHideDelegate();

	public delegate void OnVideoCompletedDelegate(string rewardItemKey, bool skipped);

	private readonly UnityAdsProxy unityAdsProxy;

	private readonly AdMobProxy adMobProxy;

	private bool completed;

	private bool skipped;

	private Action<WatchAddStatus> onAdsHide;

	public readonly AdsConfig AdsConfig;

	public AdsManager()
	{
		unityAdsProxy = new UnityAdsProxy();
		adMobProxy = new AdMobProxy();
		AdsConfig = new AdsConfig();
	}

	public void Initialiaze()
	{
		if (!base.core.ProfileData.AdsRemoved)
		{
			AdsConfig.Initialize();
			unityAdsProxy.Initialize();
			unityAdsProxy.OnShow = OnShow;
			unityAdsProxy.OnHide = OnHide;
			unityAdsProxy.OnVideoCompleted = OnVideoCompleted;
			adMobProxy.Initialize();
			adMobProxy.OnShow = OnShow;
			adMobProxy.OnHide = OnHide;
			adMobProxy.OnVideoCompleted = OnVideoCompleted;
		}
	}

	public bool CanShowAdMob()
	{
		try
		{
			return adMobProxy.CanShow();
		}
		catch (Exception ex)
		{
			Exception("AdsManager.CanShowAdMob " + ex.Message, isFatal: false);
			return false;
		}
	}

	public bool CanShowUnityAds()
	{
		try
		{
			return unityAdsProxy.CanShow();
		}
		catch (Exception ex)
		{
			Exception("AdsManager.CanShowUnityAds " + ex.Message, isFatal: false);
			return false;
		}
	}

	public void ShowAdMob(Action<WatchAddStatus> onAdsHide)
	{
		try
		{
			this.onAdsHide = onAdsHide;
			adMobProxy.Show();
		}
		catch (Exception ex)
		{
			Exception("AdsManager.ShowAdMob: " + ex.Message, isFatal: false);
			OnVideoCompleted(string.Empty, skipped: true);
			OnHide();
		}
	}

	public void ShowUnityAds(Action<WatchAddStatus> onAdsHide)
	{
		try
		{
			this.onAdsHide = onAdsHide;
			unityAdsProxy.Show();
		}
		catch (Exception ex)
		{
			Exception("AdsManager.ShowUnityAds: " + ex.Message, isFatal: false);
			OnVideoCompleted(string.Empty, skipped: true);
			OnHide();
		}
	}

	public int GetProgressPercent()
	{
		int num = 0;
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			CharDescription charDescription = CharDescription.Get[value];
			if (!base.core.ProfileData.Characters[value].Unlocked)
			{
				continue;
			}
			num += charDescription.UnlockPrice;
			for (int i = 1; i < base.core.ProfileData.Characters[value].Level; i++)
			{
				if (charDescription.Levels[i] != null)
				{
					num += charDescription.Levels[i].Price;
				}
			}
		}
		return num * 100 / CharDescription.GetOverallPrice();
	}

	public int GetOptimalWatchAdReward()
	{
		int progressPercent = GetProgressPercent();
		if (progressPercent >= 5)
		{
			if (progressPercent >= 15)
			{
				return 150;
			}
			return 100;
		}
		return 50;
	}

	private void OnVideoCompleted(string rewardItemKey, bool skipped)
	{
		completed = true;
		this.skipped = skipped;
	}

	private void OnHide()
	{
		base.core.AudioManager.MusicVolumeBox.Remove("ad");
		WatchAddStatus obj = ((!completed) ? WatchAddStatus.Ignored : (skipped ? WatchAddStatus.Skipped : WatchAddStatus.Watched));
		if (onAdsHide != null)
		{
			onAdsHide(obj);
			onAdsHide = null;
		}
	}

	private void OnShow()
	{
		base.core.AudioManager.MusicVolumeBox.SetFixed("ad", 0f, inWorld: false, 0.1f);
		skipped = false;
		completed = false;
	}
}
