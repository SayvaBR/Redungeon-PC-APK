namespace Knighter;

public interface IAdProxy
{
	AdsManager.OnHideDelegate OnHide { get; set; }

	AdsManager.OnShowDelegate OnShow { get; set; }

	AdsManager.OnVideoCompletedDelegate OnVideoCompleted { get; set; }

	void Initialize();

	void Show();

	bool CanShow();
}
