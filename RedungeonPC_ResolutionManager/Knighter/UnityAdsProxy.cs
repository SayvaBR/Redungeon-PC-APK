namespace Knighter;

/// <summary>
/// Desktop replacement for Android's UnityAdsProxy.cs (and the
/// UnityAdsListener.cs it used, now deleted - not needed with no-op ads).
/// </summary>
public class UnityAdsProxy : IAdProxy
{
	public AdsManager.OnHideDelegate OnHide { get; set; }
	public AdsManager.OnShowDelegate OnShow { get; set; }
	public AdsManager.OnVideoCompletedDelegate OnVideoCompleted { get; set; }

	public void Initialize()
	{
	}

	public void Show()
	{
	}

	public bool CanShow()
	{
		return false;
	}
}
