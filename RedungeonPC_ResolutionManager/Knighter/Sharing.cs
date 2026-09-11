namespace Knighter;

public class Sharing : Component
{
	public void RateUs()
	{
		base.core.SystemCalls.OpenUrl("https://play.google.com/store/apps/details?id=com.nitrome.redungeon");
	}

	public void SendFeedback()
	{
		string versionString = base.core.SystemCalls.GetVersionString(withBuildNumber: false);
		string text = string.Format("{0}%20feedback%20(version%20{1})", "Redungeon", versionString);
		if (Settings.Os == Settings.OS.Android)
		{
			text = text.Replace("(", "[").Replace(")", "]");
		}
		string url = string.Format("mailto:{0}?subject={1}", "feedback@eneminds.com", text);
		base.core.SystemCalls.OpenUrl(url);
	}

	public void GoToWebPage()
	{
		base.core.SystemCalls.OpenUrl("https://www.eneminds.com");
	}

	public void GoToNitromePage()
	{
		base.core.SystemCalls.OpenUrl("https://play.google.com/store/apps/developer?id=Nitrome");
	}
}
