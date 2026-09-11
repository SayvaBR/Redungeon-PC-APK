using Knighter.Graphics;

namespace Knighter;

public static class Settings
{
	public enum OS
	{
		Unknown,
		iOS,
		Android
	}

	public enum TerminatorDebugMode
	{
		None,
		Disabled,
		StopBehindPlayer,
		ComeAndGo
	}

	public static OS Os = OS.Android;

	/// <summary>Indica a execução no cliente Android, sem inferir isso do idioma/OS legado.</summary>
	public static bool IsTouchDevice;

	/// <summary>
	/// Somente leitura: reflete Core.Instance.ResolutionManager.PixelScale, a
	/// única fonte de verdade para a escala de pixel art. Mantido aqui (em vez
	/// de removido) só para não quebrar código legado que ainda lê
	/// Settings.PixelScale para fins de exibição/depuração.
	/// </summary>
	public static float PixelScale => Core.Instance?.ResolutionManager.PixelScale ?? 1f;

	public static float GuiScale = 0.85f;

	public const int TileSize = 16;

	public static bool HighlightOccupiedTiles = false;

	public static bool DrawDebugShapes = false;

	public static bool DrawDebugMessages = false;

	public static bool DrawDebugWatches = false;

	public static bool ShowDebugButtons = true;

	public static bool SkipShopAnimations = false;

	public static bool SkipAds = false;

	public static bool ShowModuleGroups = false;

	// Corrigido: Removida a chamada a SystemCalls que pertencia ao Mobile
	public static bool UseCustomShaders = false;

	public static bool HideScreenshotOverlays = false;

	public static TerminatorDebugMode TerminatorMode = TerminatorDebugMode.Disabled;

	public const string NameOfGame = "Redungeon";

	public const string NameOfCompany = "Eneminds";

	public const string NameOfPublisher = "Nitrome";

	public const int OfferId = 1172;

	public const int OfferBuffer1 = 500;

	public const int OfferBuffer2 = 1000;

	public const int OfferBuffer3 = 50000;

	public const string FacebookUrl = "https://m.facebook.com/nitrome";

	public const string NitromeTwitterUrl = "https://mobile.twitter.com/nitrome";

	public const string WebUrl = "https://www.eneminds.com";

	public const string FeedbackEmail = "feedback@eneminds.com";

	public const string EnemindsTwitterUrl = "https://mobile.twitter.com/eneminds";

	public const string EnemindsFacebookUrl = "https://m.facebook.com/eneminds";

	public const string NitromeUrl = "https://play.google.com/store/apps/developer?id=Nitrome";

	public const string BundleId = "com.nitrome.redungeon";

	public const string UnityAdsId = "116215";

	public const string AdMobId = "ca-app-pub-0896659817499072/5912200267";

	public const string ShortDownloadLinks = "Google Play: goo.gl/FUb9zH";

	public const string GameInStoreUrl = "https://play.google.com/store/apps/details?id=com.nitrome.redungeon";

	public const string PlatformPanicInStoreUrl = "https://play.google.com/store/apps/details?id=com.nitrome.platformpanic";

	public const string AnalyticsId = "UA-77836307-1";

	public static SpriteName ShareIcon
	{
		get
		{
			if (Os != OS.iOS)
			{
				return SpriteName.icon_share_android;
			}
			return SpriteName.icon_share_ios;
		}
	}
}
