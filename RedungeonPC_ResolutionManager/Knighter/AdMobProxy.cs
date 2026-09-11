namespace Knighter;

/// <summary>
/// Desktop replacement for Android's AdMobProxy.cs. No ads on desktop:
/// CanShow() always false, so AdsManager gracefully skips any "watch ad"
/// path (it already has to handle ads failing to load on real devices too).
/// </summary>
public class AdMobProxy : IAdProxy
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
