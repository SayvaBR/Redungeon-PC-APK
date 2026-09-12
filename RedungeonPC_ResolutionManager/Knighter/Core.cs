using System;
using System.Collections.Generic;
using System.Linq;
using Knighter;
using Knighter.Artifacts;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Localization;
using Knighter.Messages;
#if DEBUG
using Knighter.QA;
#endif
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter;

public sealed partial class Core : Component
{
	private static Core instance;

	private static bool initalized;

	public GameTime GameTime;

	public GraphicsDevice GraphicsDevice;

	public MobileGame Game;

	public FrameCounter FrameCounter;

	private readonly Stack<State> states;

	private bool startingFromShop;

	private Screenshot nextScreenshot;

	public GameOverActionType LastShownAction;

	public bool JustWatchedAd;

	private float coinsY = -15f;

	private float targetCoinsY = -15f;

	private float displayCoins;

	private float coinPitch;

	public int AttemptsFromStart;

	private ParticleEmitter dustEmitter;

	public bool UpdateOnlyTopState;

	private List<Pair<string, int>> debugMessages;

	private Dictionary<string, string> debugWatches;

	private Dictionary<string, int> debugWatchTtls;

	private List<string> debugWatchesToRemove;

	private Dictionary<string, float> liveTuners;

	private Dictionary<string, float> liveTuneSteps;

	private Dictionary<string, RectangleF> liveTuneMinus;

	private Dictionary<string, RectangleF> liveTunePlus;

	public static Core Instance => instance;

	public SystemCalls SystemCalls { get; private set; }

	public Scores Scores { get; private set; }

	public Storage Storage { get; private set; }

	public SpriteManager SpriteManager { get; private set; }

	public ParticleManager ParticleManager { get; private set; }

	public Renderer Renderer { get; private set; }

	/// <summary>
	/// Fonte única de verdade para resolução/escala/viewport/DestRect. Game1 chama
	/// Recalculate() sempre que o backbuffer muda (resize, F11/Alt+Enter, borderless,
	/// ou futuramente a troca de resolução no menu de opções).
	/// </summary>
	public ResolutionManager ResolutionManager { get; private set; }

	/// <summary>Fonte única de verdade para ações de teclado e gamepad.</summary>
	public InputManager Input { get; private set; }

	public ContentManager Content { get; private set; }

	public AudioManager AudioManager { get; private set; }

	public MessageManager MessageManager { get; private set; }

	public TimerManager TimerManager { get; private set; }

	public Store Store { get; private set; }

	public AdsManager AdsManager { get; private set; }

	public Sharing Sharing { get; private set; }

	public LevelModules LevelModules { get; private set; }

	public Achievements Achievments { get; private set; }

	public DebugSpawner DebugSpawner { get; private set; }

	public CrossPromotion CrossPromotion { get; private set; }

	public Analytics Analytics { get; private set; }

	public ArtifactManager ArtifactManager { get; private set; }

	public Cloud Cloud { get; private set; }

	public MissionManager MissionManager { get; private set; }

	public LocaleManager LocaleManager { get; private set; }

	public ProfileData ProfileData { get; set; }

	public CharDescription CurrentCharDesc => CharDescription.Get[ProfileData.Character];

	public OptionsData OptionsData { get; private set; }

	public float DeviceTilt { get; private set; }

	public int Ticks { get; private set; }

	public int TicksInState { get; private set; }

	public bool TakingScreenshot { get; private set; }

	public Screenshot GameplayScreenshot { get; private set; }

	public Screenshot AuxScreenshot { get; private set; }

	public PlayState CurrentPlayState { get; private set; }

	public TouchCollection TouchState { get; private set; }

	public Holiday Holiday { get; private set; }

	private Core(MobileGame game, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content, int bufferWidth, int bufferHeight)
	{
		InitDebugTools();
		GraphicsDevice = graphicsDevice;
		SystemCalls = new SystemCalls();
		Scores = new Scores();
#if DEBUG
		Storage = new Storage(QaSession.StoragePath);
#else
		Storage = new Storage();
#endif
		MessageManager = new MessageManager();
		TimerManager = new TimerManager();
		SpriteManager = new SpriteManager();
		ParticleManager = new ParticleManager();
		AudioManager = new AudioManager();
		ResolutionManager = new ResolutionManager();
		ResolutionManager.Recalculate(bufferWidth, bufferHeight);
		#if DEBUG
		Input = new InputManager(QaSession.CreateInputSource());
		#else
		Input = new InputManager();
		#endif
		Renderer = new Renderer(spriteBatch, ResolutionManager);
		ResolutionManager.Changed += OnResolutionChanged;
		Store = new Store();
		AdsManager = new AdsManager();
		Sharing = new Sharing();
		LevelModules = new LevelModules();
		Achievments = new Achievements();
		DebugSpawner = new DebugSpawner();
		CrossPromotion = new CrossPromotion();
		Analytics = new Analytics();
		ArtifactManager = new ArtifactManager();
		Cloud = new Cloud();
		MissionManager = new MissionManager();
		LocaleManager = new LocaleManager();
		OptionsData = new OptionsData();
		ProfileData = new ProfileData();
		Content = content;
		states = new Stack<State>();
		FrameCounter = new FrameCounter();
		Game = game;
		DebugMessage($"screen size: {bufferWidth}x{bufferHeight}");
	}

	public static void Initialize(MobileGame game, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content, int bufferWidth, int bufferHeight)
	{
		if (!initalized)
		{
			instance = new Core(game, graphicsDevice, spriteBatch, content, bufferWidth, bufferHeight);
			initalized = true;
		}
	}

	private void CreateScreenshotTargets()
	{
		GameplayScreenshot?.Texture?.Dispose();
		AuxScreenshot?.Texture?.Dispose();
		GameplayScreenshot = new Screenshot(Renderer.CreateTexture(Renderer.DisplayWidth, Renderer.DisplayHeight));
		AuxScreenshot = new Screenshot(Renderer.CreateTexture(Renderer.DisplayWidth, Renderer.DisplayHeight));
		if (SpriteManager != null)
		{
			SpriteManager.AddOrReplaceTexture("screenshot", GameplayScreenshot.Texture);
		}
	}

	/// <summary>
	/// Único ponto que reage a uma mudança de resolução (chamado via o evento
	/// ResolutionManager.Changed). Recria tudo que depende do tamanho do
	/// backbuffer, para que resize/fullscreen/borderless nunca deixem a
	/// renderização num estado inconsistente.
	/// </summary>
	private void OnResolutionChanged()
	{
		Renderer?.OnResolutionChanged();
		if (initalized && GameplayScreenshot != null)
		{
			CreateScreenshotTargets();
		}
	}

	private void ClearStates()
	{
		while (states.Count > 0)
		{
			states.Pop().Unload();
		}
		CurrentPlayState = null;
		states.Clear();
	}

#if DEBUG
	private bool verifyProgressResetForQA;
	internal void VerifyUiFixture()
	{
		string fixture = Environment.GetEnvironmentVariable("REDUNGEON_QA_SCREEN");
		if (fixture?.StartsWith("lykos-") == true) { VerifyLykosQa(); return; }
		bool ok = fixture switch {
			"startup" => GetCurrentState() is MenuState,
			"darkness-release" => !CurrentPlayState.Player.SpellEffects[SpellType.Darkness].Active && !Renderer.PostEffectManager.EnabledEffects.Contains(PostEffectType.Spotlight),
			"pause-resume" or "pause-options" => GetCurrentState() is PlayState,
			"pause-exit" or "title" => GetCurrentState() is MenuState,
			"reset-flow-confirm" => GetCurrentState() is MenuState && ProfileData.Coins == 0 && OptionsData.MusicVolume == .35f && ProfileData.GetNumberOfUnlocks() == 0,
			"frost-release" => !CurrentPlayState.Player.SpellEffects[SpellType.Ice].Active && CurrentPlayState.Player.FrostScreen == 0 && CurrentPlayState.Player.FrostVisual > 0,
			"wisp-glow-on" => OptionsData.EnhancedWispGlow,
			"wisp-glow-off" => !OptionsData.EnhancedWispGlow,
			"reset-flow-cancel" => GetCurrentState() is SettingsMenuState && ProfileData.Coins == 1234,
			_ => true };
		if (!ok) throw new InvalidOperationException("UI fixture failed: " + fixture + " state=" + GetCurrentState().GetType().Name);
	}

	internal void OpenVisualSettings()
	{
		LocaleManager.SetCurrentLocale(Language.pt_PT);
		string fixture = Environment.GetEnvironmentVariable("REDUNGEON_QA_SCREEN");
		if (fixture == "splash") return;
		if (fixture == "startup") { ProfileData.LanguageSelectorPending = ProfileData.ControlsSelectorPending = false; return; }
		if (Environment.GetEnvironmentVariable("REDUNGEON_QA_PORTRAIT") == "1") { Game.SetWindowMode(WindowMode.Windowed); Game.SetWindowedResolution(540, 960); }
		if (fixture?.StartsWith("lykos-") == true) { OpenLykosQa(fixture); return; }
		if (fixture == "title") { ProfileData.LanguageSelectorPending = ProfileData.ControlsSelectorPending = false; ResetInternal(); return; }
		if (fixture == "pause-ps") OptionsData.PromptStyle = 1;
		if (fixture.StartsWith("pause")) { ProfileData.LanguageSelectorPending = ProfileData.ControlsSelectorPending = false; ResetInternal(true); return; }
		if (fixture?.StartsWith("lighting-") == true || fixture.StartsWith("wisp-glow-") || fixture.StartsWith("frost") || fixture == "poison" || fixture.StartsWith("darkness"))
		{
			ProfileData.ControlsSelectorPending = false;
			if (fixture.StartsWith("darkness") || fixture.StartsWith("lighting-")) ProfileData.LearnedSwipes = true;
			ResetInternal(true);
			var play = CurrentPlayState;
			int x = (int)play.Player.WorldCoordinates.X, y = (int)play.Player.WorldCoordinates.Y;
			for (int dy = -7; dy <= 3; dy++)
			for (int dx = -5; dx <= 5; dx++)
				if (play.TileMap[x + dx, y + dy] == null || play.TileMap[x + dx, y + dy].Type == Knighter.Gameplay.TileType.Pit)
					play.TileMap.AddTile(new Knighter.Tiles.DungeonTile(x + dx, y + dy, Knighter.Gameplay.TileType.Floor));
			SendMessage(new SpawnEntityMessage(new ObstacleEntity(x + 2, y - 2), null));
			play.LightManager.AddLight(Color.Orange, 4f, 0.8f).Position = new Vector2((x - 2) * 16f, (y - 2) * 16f);
			play.LightManager.AddLight(Color.LimeGreen, 3f, 0.7f).Position = new Vector2((x + 3) * 16f, (y - 3) * 16f);
			OptionsData.DynamicLighting = fixture != "lighting-off";
			OptionsData.LightingStrength = 0.5f;
			OptionsData.SoftShadows = fixture == "lighting-shadows";
			OptionsData.SmoothLighting = OptionsData.LightBloom = OptionsData.SoftShadows;
			if (fixture.StartsWith("wisp-glow-"))
			{
				OptionsData.DynamicLighting = OptionsData.LightBloom = false;
				OptionsData.LightingStrength = OptionsData.BloomStrength = 1f;
				OptionsData.EnhancedWispGlow = fixture == "wisp-glow-on";
				for (int i = -2; i <= 2; i++)
				{
					var json = new JsonObject();
					foreach (string key in new[] { "delay", "default-pause", "speed", "look-at-player", "fire", "zap", "snow", "poison", "confusion" }) json.Fields[key] = new JsonNumber(0);
					json.Fields["dark"] = new JsonNumber(1);
					json.Fields["path"] = new JsonText("");
					var desc = new TileDesc { ElementJson = json };
					SendMessage(new SpawnEntityMessage(new WispEntity(x + i, y - 3 - Math.Abs(i), desc), null));
				}
			}
			if (fixture.StartsWith("frost")) play.Player.ApplySpell(SpellType.Ice);
			if (fixture == "poison") play.Player.ApplySpell(SpellType.Poison);
			if (fixture.StartsWith("darkness")) play.Player.ApplySpell(SpellType.Darkness);
			return;
		}
		ClearStates();
		var settings = new SettingsMenuState();
		PushState(settings);
		string screen = Environment.GetEnvironmentVariable("REDUNGEON_QA_SCREEN");
		if (screen?.StartsWith("reset-flow") == true) { ProfileData.Coins = 1234; OptionsData.MusicVolume = .35f; settings.OpenCategoryForQA("progress"); }
		else if (screen == "statistics-scroll") settings.OpenCategoryForQA("statistics");
		else if (screen?.StartsWith("video-") == true) { OptionsData.UiScale = screen == "video-small" ? .85f : screen == "video-large" ? 1.15f : 1f; settings.OpenCategoryForQA("video"); }
		else settings.OpenCategoryForQA(screen);
		if (screen == "reset") PushState(new ResetProgressState());
		if (screen == "reset-test")
		{
			ProfileData.Coins = 1234;
			OptionsData.MusicVolume = 0.35f;
			ProfileData.SaveIntoStorage();
			verifyProgressResetForQA = true;
			PushState(new ResetProgressState { ConfirmForQA = true });
		}
	}

#endif
	private void PushState(State state)
	{
#if DEBUG
		EngineDiagnostics.Log($"PushState: {state.GetType().Name}");
#endif
		if (states.Count > 0)
		{
			GetCurrentState().OnLeaveBehind();
		}
		state.Load();
		state.TransitionIn();
		states.Push(state);
		TicksInState = 0;
	}

	private void PopState()
	{
		if (states.Count <= 0)
		{
			return;
		}
		State state = states.Pop();
		bool flag = !state.IsOverlay;
		state.Unload();
		if (states.Count > 0)
		{
			if (flag)
			{
				GetCurrentState().TransitionIn();
			}
			GetCurrentState().OnReturn();
			TicksInState = 0;
		}
	}

	public State GetCurrentState()
	{
		return states.Peek();
	}

	public override void Load()
	{
		Subscribe(MessageType.Screenshot);
		Subscribe(MessageType.CoreEvent);
		Subscribe(MessageType.PushState);
		Subscribe(MessageType.PopState);
		ProfileData.LoadFromStorage();
		_inc(Stat.Sessions);
		AdsManager.Initialiaze();
		Store.Initialize();
		Store.RequestProducts();
		Scores.Authenticate();
		displayCoins = ProfileData.Coins;
		SpriteManager.Load();
		AudioManager.Load();
		Renderer.Load();
		LevelModules.Load();
		LoadOptions();
		ApplyOptions(loading: true);
		Achievments.Load();
		CrossPromotion.Initialize();
		Analytics.Initialize();
		LocaleManager.Load();
		PushState(new SplashState());
		CreateScreenshotTargets();
		SystemCalls.InternetStatusChanged += OnInternetAvailabilityChange;
		// GooglePlayHelper sign-in wiring removido no port desktop
		// GooglePlayHelper sign-in wiring removido no port desktop
		// GooglePlayHelper sign-in wiring removido no port desktop
Holiday = Holiday.None;
		DateTime now = DateTime.Now;
		if ((now.Month == 12 && now.Day > 5) || (now.Month == 1 && now.Day < 15))
		{
			Holiday = Holiday.Xmas;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>
		{
			{ 2018, 47 },
			{ 2019, 36 },
			{ 2020, 25 },
			{ 2021, 43 },
			{ 2022, 32 },
			{ 2023, 22 },
			{ 2024, 41 },
			{ 2025, 29 },
			{ 2026, 48 },
			{ 2027, 37 },
			{ 2028, 26 }
		};
		int num = 32;
		if (dictionary.ContainsKey(now.Year))
		{
			num = dictionary[now.Year];
		}
		if (now.DayOfYear >= num && now.DayOfYear <= num + 25)
		{
			Holiday = Holiday.ChunJie;
		}
	}

	public override void Unload()
	{
		SystemCalls.InternetStatusChanged -= OnInternetAvailabilityChange;
		if (dustEmitter != null)
		{
			dustEmitter.Stop();
		}
		ClearStates();
		SaveOptions();
		LevelModules.Unload();
		SpriteManager.Unload();
		AudioManager.Unload();
		Renderer.Unload();
		ProfileData.SaveIntoStorage();
		Unsubscribe(MessageType.CoreEvent);
		Unsubscribe(MessageType.PushState);
		Unsubscribe(MessageType.PopState);
		Unsubscribe(MessageType.Screenshot);
	}

	public override void Update()
	{
		// A confirmation can originate inside State.Update; mutate the stack
		// only at a frame boundary, never while its enumerator is active.
		if (resetProgressPending) CommitProgressReset();
#if DEBUG
		DevTools.BeginUpdate();
		EngineDiagnostics.Update(GameTime);
		if (DevTools.FullPause)
		{
			DevTools.EndUpdate();
			return;
		}
#endif
		Input.Update();
		if (dustEmitter != null)
		{
			Vector2 vector = ((CurrentPlayState != null) ? CurrentPlayState.Camera.Position : Vector2.Zero);
			float num = ((CurrentPlayState != null) ? CurrentPlayState.Camera.Zoom : 1f);
			dustEmitter.Position = vector + base.core.Renderer.ScreenCenter / num - new Vector2(0f, 50f);
		}
		coinsY += (targetCoinsY - coinsY) * 0.1f;
		if (Math.Abs((float)ProfileData.Coins - displayCoins) > 10f)
		{
			displayCoins += ((float)ProfileData.Coins - displayCoins) * 0.1f;
		}
		else
		{
			displayCoins = ProfileData.Coins;
		}
		coinPitch -= 1f / 120f;
		if (coinPitch <= 0f)
		{
			coinPitch = 0f;
		}
		UpdateDebugMessages();
		int ticksInState;
		foreach (State state in states)
		{
			if (UpdateOnlyTopState && states.Peek() != state)
			{
				continue;
			}
			if (state.TransT > 0)
			{
				state.TransT--;
				if (state.TransT == 0)
				{
					if (state.Transition == State.TransType.Out)
					{
						state.OnOutTransitionDone();
					}
					if (state.Transition != State.TransType.Out)
					{
						state.Transition = State.TransType.None;
					}
				}
			}
			if (state.Transition != State.TransType.None)
			{
				state.UpdateTransition();
			}
			state.Update();
			ticksInState = TicksInState + 1;
			TicksInState = ticksInState;
		}
		TouchState = MouseTouchAdapter.GetState();
#if DEBUG
		if (QaSession.IsActive && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDUNGEON_QA_VISUAL"))) TouchState = default;
#endif
		State currentState = GetCurrentState();
		if (currentState.Transition == State.TransType.None)
		{
			if (Input.WasPressed(GameAction.Pause))
			{
				currentState.OnStartButtonPressed();
			}
			if (Input.WasPressed(GameAction.Cancel) && currentState.Transition == State.TransType.None)
			{
				currentState.OnBackButtonPressed();
			}
		}
		HandleDebugInput();
		GetCurrentState().HandleInput();
		Achievments.Update();
		DebugSpawner.Update();
		ParticleManager.Update();
		MessageManager.Update();
		TimerManager.Update();
		AudioManager.Update();
		Renderer.Update();
		Cloud.Update();
		DebugWatch("fps", FrameCounter.AverageFramesPerSecond.ToString("N1"));
		ticksInState = Ticks + 1;
		Ticks = ticksInState;
#if DEBUG
		QaSession.ObserveState(Ticks, GetCurrentState()?.GetType().Name);
		DevTools.EndUpdate();
#endif
	}

	private void DrawAll()
	{
		foreach (State state in states)
		{
			state.Draw();
			if (state.IsOpaque)
			{
				break;
			}
		}
		ParticleManager.Draw();
		if (GetCurrentState().ShowCoins)
		{
			targetCoinsY = 3 + base.topSafeArea;
		}
		else
		{
			targetCoinsY = -15f;
		}
		if (!TakingScreenshot && coinsY > -19f)
		{
			string text = __(SId.MISC_total) + " ^" + (int)Math.Ceiling(displayCoins);
			base.core.Renderer["fg", 10000, false].DrawTextS(text, new Vector2(base.core.Renderer.ScreenWidth / 2 - 1, coinsY), TextProfile.OrangeBoldText.Alter(null, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Center, width: base.core.Renderer.ScreenWidth, decoration: TextDecoration.Contour, secondColor: Color.Black, height: null, font: null, scale: 0.75f));
		}
		DrawDebugMessages();
		DrawLiveTuners();
	}

	public override void Draw()
	{
		if (TakingScreenshot)
		{
			if (nextScreenshot != null)
			{
				TakeScreenshot(nextScreenshot);
				nextScreenshot = null;
			}
			TakingScreenshot = false;
		}
		DrawAll();
#if DEBUG
		DevTools.DrawCameraOverlay(this);
		EngineDiagnostics.Draw(this);
		DevTools.BeginDraw();
#endif
		Renderer.Draw();
#if DEBUG
		DevTools.EndDraw();
#endif
		float deltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;
		FrameCounter.Update(deltaTime);
	}

	private void Reset(bool start = false, SessionData session = null)
	{
		ResetInternal(start, session);
	}

	private bool resetProgressPending;

	public void ResetLocalProgress() => resetProgressPending = true;

	private void CommitProgressReset()
	{
		resetProgressPending = false;
		Storage.BackupProgress();
		ClearStates();
		ProfileData = new ProfileData { Locale = LocaleManager.CurrentLocale, LanguageSelectorPending = false, ControlsSelectorPending = false };
		ProfileData.SaveIntoStorage();
		startingFromShop = false;
		Input.Reset();
		MouseTouchAdapter.Reset();
		ResetInternal();
#if DEBUG
		if (verifyProgressResetForQA)
		{
			verifyProgressResetForQA = false;
			if (ProfileData.Coins != 0 || OptionsData.MusicVolume != 0.35f || ProfileData.GetNumberOfUnlocks() != 0 || GetCurrentState() is not MenuState)
				throw new InvalidOperationException("Progress reset contract failed");
		}
#endif
	}

	private void ResetInternal(bool start = false, SessionData session = null)
	{
		bool num = states.Count > 0 && GetCurrentState() is LogoState;
		ClearStates();
		CurrentPlayState = new PlayState(startingFromShop);
		PushState(CurrentPlayState);
		startingFromShop = false;
		if (!start)
		{
			PushState(new MenuState(session));
		}
		else
		{
			CurrentPlayState.StartPlaying();
		}
		if (num)
		{
			AfterInitialReset();
		}
	}

	public override void OnMessage(Message message, object sender)
	{
		switch (message.Type)
		{
		case MessageType.CoreEvent:
			ProcessCoreEvent((message as CoreEventMessage).CoreEvent);
			break;
		case MessageType.PushState:
		{
			PushStateMessage pushStateMessage = message as PushStateMessage;
			PushState(pushStateMessage.State);
			break;
		}
		case MessageType.PopState:
			PopState();
			break;
		case MessageType.Screenshot:
		{
			ScreenshotMessage screenshotMessage = message as ScreenshotMessage;
			if (screenshotMessage.When == WhenToTakeScreenshot.WhileUpdating)
			{
				TakingScreenshot = true;
				TakeScreenshot(screenshotMessage.Screenshot);
				TakingScreenshot = false;
			}
			else
			{
				nextScreenshot = screenshotMessage.Screenshot;
				TakingScreenshot = true;
			}
			break;
		}
		}
		base.OnMessage(message, sender);
	}

	private void ProcessCoreEvent(CoreEvent coreEvent)
	{
		switch (coreEvent)
		{
		case CoreEvent.ShowArtifacts:
			PushState(new ArtifactsState());
			break;
		case CoreEvent.Promo:
			break;
		case CoreEvent.ShowEnemindsLogo:
			ClearStates();
			PushState(new LogoState());
			break;
		case CoreEvent.ShowNitromeLogo:
			ClearStates();
			PushState(new NitromeState());
			break;
		case CoreEvent.StartGame:
			while (GetCurrentState() != CurrentPlayState)
			{
				PopState();
			}
			CurrentPlayState.StartPlaying();
			break;
		case CoreEvent.GameOver:
		{
			base.core.Analytics.Dispatch();
			if (GetCurrentState() is ContinueState)
			{
				PopState();
			}
			SessionData session = CurrentPlayState.Session;
			PushState(new MenuState(session));
			ProfileData.LastDistance = session.Distance;
			ProfileData.BestDistance = Math.Max(ProfileData.LastDistance, ProfileData.BestDistance);
			ProfileData.SaveIntoStorage();
			Scores.ReportBestScore(gold: false);
			if (session.Revives == 0)
			{
				Scores.ReportBestScore(gold: true);
			}
			AttemptsFromStart++;
			Event(AnalyticsCategory.Run, "passed-distance", SciHelper.GetVerboseRange(session.Distance, 20), session.Distance);
			Event(AnalyticsCategory.Run, "collected-coins", SciHelper.GetVerboseRange(session.CollectedCoins, 10), session.CollectedCoins);
			Event(AnalyticsCategory.Run, "cause-of-death", session.CauseOfDeath.ToString());
			Event(AnalyticsCategory.Run, base.core.ProfileData.AdsRemoved ? "premium-revives" : "revives", session.Revives.ToString(), session.Revives);
			Event(AnalyticsCategory.Run, "chosen-character", ProfileData.Character.ToString());
			Event(AnalyticsCategory.Run, "time-seconds", SciHelper.GetVerboseRange(session.Ticks / 60, 10), session.Ticks / 60);
			Cloud.Sync();
			break;
		}
		case CoreEvent.ResetGame:
			Reset();
			break;
		case CoreEvent.Shop:
			PushState(new ShopState());
			break;
		case CoreEvent.ResetAndStartGame:
			ClearStates();
			CurrentPlayState = new PlayState(startingFromShop);
			startingFromShop = false;
			PushState(CurrentPlayState);
			CurrentPlayState.StartPlaying();
			break;
		case CoreEvent.ShowPause:
			if (GetCurrentState() is PlayState)
			{
				PushState(new PauseState());
			}
			break;
		case CoreEvent.HidePause:
			PopState();
			CurrentPlayState.Unpause();
			break;
		case CoreEvent.OfferToContiune:
		{
			bool adsEnabled = CurrentPlayState.Session.Revives == 0;
			int revivePrice = ((CurrentPlayState.Session.Revives == 0) ? 100 : (500 * CurrentPlayState.Session.Revives));
			PushState(new ContinueState(revivePrice, adsEnabled));
			break;
		}
		case CoreEvent.Continue:
			SendMessage(new PopStateMessage(), 1);
			CurrentPlayState.Continue();
			break;
		case CoreEvent.ShowOptions:
			PushState(new SettingsMenuState());
			break;
		case CoreEvent.HideOptions:
			PopState();
			break;
		case CoreEvent.ShowGetCoins:
			// Local-only economy: no purchase/rewarded-video storefront.
			SendMessage(new PlaySoundMessage(SoundName.button_up));
			break;
		case CoreEvent.HideGetCoins:
			PopState();
			break;
		case CoreEvent.PopState:
			PopState();
			break;
		case CoreEvent.Wait:
			PushState(new WaitState());
			break;
		case CoreEvent.StopWait:
			if (GetCurrentState() is WaitState waitState)
			{
				waitState.TransitionOut(CoreEvent.PopState);
			}
			break;
		}
	}

	public void Pause(bool enteringBackground)
	{
		CurrentPlayState.Pause(enteringBackground: true);
		SendMessage(new CoreEventMessage(CoreEvent.ShowPause));
	}

	public void NextRunFromShop()
	{
		startingFromShop = true;
	}

	public void OnEnteringBackground()
	{
		Knighter.Gameplay.Rumble.Stop();
		base.core.ProfileData.SaveIntoStorage();
		if (CurrentPlayState == GetCurrentState() && !CurrentPlayState.Player.Dead)
		{
			SendMessage(new ScreenshotMessage(WhenToTakeScreenshot.WhileUpdating, GameplayScreenshot));
			Pause(enteringBackground: true);
		}
	}

	private void TakeScreenshot(Screenshot screenshot)
	{
		DrawAll();
		base.core.Renderer.DrawToTexture(screenshot.Texture);
	}

	private void OnInternetAvailabilityChange(object sender, EventArgs e)
	{
		if (SystemCalls.IsInternetAvailable() && !base.core.Store.AllProductsAvailable())
		{
			base.core.Store.RequestProducts();
		}
	}

	private ParticleEmitter CreateDustEmitter()
	{
		Sprite speck = _(SpriteName.pixel);
		return base.core.ParticleManager.AddEmitter(inWorld: false, Vector2.Zero, base.core.Renderer.ScreenWidth / 2).OnSpawn(delegate(Particle p)
		{
			p.Aux.X = SciHelper.GetRandom(0f, 1f);
			p.Aux.Y = SciHelper.GetRandom(-0.1f, 0.1f);
			p.Aux.Z = SciHelper.GetRandom(75f, 125f);
			switch (Holiday)
			{
			case Holiday.Xmas:
				p.Offset.X = Component._rnd(1, 11);
				break;
			case Holiday.ChunJie:
				p.Offset.X = Component._rnd(1, 12);
				if (SciHelper.ChanceRoll(0.3f))
				{
					p.Dead = true;
				}
				break;
			}
			p.Velocity = SciHelper.GetRandomVectorInCircle(1f) * 0.2f;
		}).OnUpdate(delegate(Particle p)
		{
			if (!p.Dead)
			{
				p.Dead = p.Age > 500;
				p.Position += p.Velocity;
				p.Aux.W += p.Aux.Y;
			}
		})
			.OnDraw(delegate(Particle p)
			{
				Vector2 position = p.Position;
				Vector2 obj = ((CurrentPlayState != null) ? CurrentPlayState.Camera.Position : Vector2.Zero);
				float num = ((CurrentPlayState != null) ? CurrentPlayState.Camera.Zoom : 1f);
				Vector2 vector = (obj - p.Position + base.core.Renderer.ScreenCenter / num) * p.Aux.X;
				Color color = Color.AliceBlue * 0.3f * Component._sin((float)p.Age * (float)Math.PI / 500f);
				Sprite sprite = speck;
				Vector2 value = Vector2.One * p.Aux.X * 4f;
				SpriteOrigin spriteOrigin = SpriteOrigin.TopLeft;
				switch (Holiday)
				{
				case Holiday.Xmas:
					sprite = _("snowflake_" + (int)p.Offset.X);
					value = Vector2.One;
					spriteOrigin = SpriteOrigin.Center;
					color = Color.White * Component._sin((float)p.Age * (float)Math.PI / 500f);
					break;
				case Holiday.ChunJie:
					sprite = _(SpriteName.pixel);
					spriteOrigin = SpriteOrigin.Center;
					value = new Vector2(2f * (0.5f + 0.5f * Component._sin((float)p.Age * 0.05f)), 5f) * (0.7f + 0.05f * p.Offset.X % 4f);
					color = ((p.Offset.X <= 3f) ? Color.Gold : ((p.Offset.X <= 6f) ? Color.Crimson : ((p.Offset.X <= 9f) ? Color.CornflowerBlue : Color.LawnGreen)));
					break;
				}
				Renderer renderer = base.core.Renderer[500 + (int)(1000f * p.Aux.X), true];
				Sprite sprite2 = sprite;
				Vector2 position2 = position - vector;
				Color? tint = ((p.Age % (int)p.Aux.Z < 10) ? color : (color * 0.5f));
				Vector2? scale = value;
				SpriteOrigin origin = spriteOrigin;
				renderer.DrawSpriteW(sprite2, position2, tint, scale, p.Aux.W, SpriteFlip.None, origin);
			});
	}

	public void PlayCoinSound(int delay = 0)
	{
		SendMessage(new PlaySoundMessage(SoundName.coin, 1f, coinPitch), delay);
		coinPitch += 0.07f;
		if (coinPitch >= 1f)
		{
			coinPitch = 1f;
		}
	}

	public void ApplyOptions(bool loading = false)
	{
		if (!OptionsData.PlayMusic)
		{
			AudioManager.MusicVolumeBox.SetFixed("mute", 0f, inWorld: false, loading ? 1f : 0.1f);
			if (loading)
			{
				AudioManager.Update();
			}
		}
		else
		{
			AudioManager.MusicVolumeBox.Remove("mute");
		}

		if (loading)
		{
			// Aplica o modo de janela/resolução salvo (padrão: Windowed 1280x720)
			// antes mesmo de qualquer state ser exibido — base para o futuro
			// menu de configurações de vídeo.
			Game.SetWindowedResolution(OptionsData.WindowedWidth, OptionsData.WindowedHeight);
			Game.SetWindowMode(OptionsData.WindowMode);
		}
		Game.ApplyVideoSettings(OptionsData.VSync, OptionsData.FpsLimit);
	}

	public void LoadOptions()
	{
		OptionsData.LoadFromStorage();
	}

	public void SaveOptions()
	{
		OptionsData.SaveIntoStorage();
	}

	private void AfterInitialReset()
	{
		dustEmitter = CreateDustEmitter();
		dustEmitter.Start(5);
		Event(AnalyticsCategory.Ux, "controls", OptionsData.SwipeControl ? "swipes" : "arrows");
		Event(AnalyticsCategory.Ux, "play-music", OptionsData.PlayMusic.ToString());
		Event(AnalyticsCategory.Ux, "play-sounds", OptionsData.PlaySounds.ToString());
		Event(AnalyticsCategory.Overall, "progress-percent", SciHelper.GetVerboseRange(AdsManager.GetProgressPercent(), 5), AdsManager.GetProgressPercent());
		base.core.Analytics.Dispatch();
	}

	private void InitDebugTools()
	{
		debugMessages = new List<Pair<string, int>>();
		debugWatches = new Dictionary<string, string>();
		debugWatchTtls = new Dictionary<string, int>();
		debugWatchesToRemove = new List<string>();
		liveTuners = new Dictionary<string, float>();
		liveTuneSteps = new Dictionary<string, float>();
		liveTuneMinus = new Dictionary<string, RectangleF>();
		liveTunePlus = new Dictionary<string, RectangleF>();
	}

	public void DebugMessage(string message)
	{
#if DEBUG
		EngineDiagnostics.Log(message);
#endif
		if (Settings.DrawDebugMessages)
		{
			debugMessages.Insert(0, new Pair<string, int>(message, 200));
		}
	}

	public void DebugMessage(bool condition, string message)
	{
		if (condition)
		{
			DebugMessage(message);
		}
	}

	public void DebugWatch(string watchName, string watchValue, int ttl = 10)
	{
		if (Settings.DrawDebugWatches)
		{
			debugWatches[watchName] = watchValue;
			debugWatchTtls[watchName] = ttl;
		}
	}

	public void DebugWatch(string watchName, int watchValue, int ttl = 10)
	{
		DebugWatch(watchName, watchValue.ToString(), ttl);
	}

	public void DebugWatch(object obj, string watchName, string watchValue, int ttl = 10)
	{
		if (Settings.DrawDebugWatches)
		{
			DebugWatch(obj.GetType().Name + " " + $"{obj.GetHashCode() % 1000,3:X}" + ((watchName != "") ? (" " + watchName) : ""), watchValue, ttl);
		}
	}

	public void RemoveDebugWatch(string watchName)
	{
		if (Settings.DrawDebugWatches)
		{
			debugWatches.Remove(watchName);
			debugWatchTtls.Remove(watchName);
		}
	}

	public float LiveTune(string name, float value, float step = 0.1f)
	{
		if (!Settings.DrawDebugMessages)
		{
			return value;
		}
		if (liveTuners.ContainsKey(name))
		{
			return liveTuners[name];
		}
		liveTuners.Add(name, value);
		liveTuneSteps.Add(name, step);
		return value;
	}

	public int LiveTune(string name, int value, int step = 1)
	{
		if (!Settings.DrawDebugMessages)
		{
			return value;
		}
		if (liveTuners.ContainsKey(name))
		{
			return (int)liveTuners[name];
		}
		liveTuners.Add(name, value);
		liveTuneSteps.Add(name, step);
		return value;
	}

	private void HandleDebugInput()
	{
		if (!Settings.DrawDebugMessages || liveTuners.Count == 0)
		{
			return;
		}
		foreach (TouchLocation item in TouchState)
		{
			if (item.State != TouchLocationState.Pressed)
			{
				continue;
			}
			foreach (KeyValuePair<string, RectangleF> liveTuneMinu in liveTuneMinus)
			{
				if (liveTuneMinu.Value.Contains(item.Position))
				{
					liveTuners[liveTuneMinu.Key] -= liveTuneSteps[liveTuneMinu.Key];
				}
				if (liveTunePlus[liveTuneMinu.Key].Contains(item.Position))
				{
					liveTuners[liveTuneMinu.Key] += liveTuneSteps[liveTuneMinu.Key];
				}
			}
		}
	}

	private void UpdateDebugMessages()
	{
		if (Settings.DrawDebugMessages)
		{
			foreach (Pair<string, int> debugMessage in debugMessages)
			{
				debugMessage.B--;
			}
			debugMessages.RemoveAll((Pair<string, int> m) => m.B == 0);
		}
		if (!Settings.DrawDebugWatches)
		{
			return;
		}
		foreach (string item in debugWatchTtls.Keys.ToList())
		{
			debugWatchTtls[item]--;
			if (debugWatchTtls[item] == 0)
			{
				debugWatchesToRemove.Add(item);
			}
		}
		foreach (string item2 in debugWatchesToRemove)
		{
			debugWatches.Remove(item2);
			debugWatchTtls.Remove(item2);
		}
		debugWatchesToRemove.Clear();
	}

	private void DrawDebugMessages()
	{
		float num = 17f;
		if (Settings.DrawDebugMessages)
		{
			foreach (Pair<string, int> debugMessage in debugMessages)
			{
				num += 5f;
				Renderer["fg", 10000, false].DrawSimpleTextS(debugMessage.A, new Vector2((float)Renderer.ScreenWidth - Renderer.SimpleTextWidth(debugMessage.A, 0.5f) - 3f, num), Color.White * 0.9f * ((float)debugMessage.B / 200f), 0.5f);
			}
		}
		if (!Settings.DrawDebugWatches)
		{
			return;
		}
		num = 17f;
		float num2 = 0f;
		foreach (KeyValuePair<string, string> debugWatch in debugWatches)
		{
			num += 5f;
			float num3 = Renderer["fg", 10000, false].DrawSimpleTextS(debugWatch.Key + ": ", new Vector2(2f, num), Color.Lime * 0.6f, 0.5f);
			float num4 = Renderer["fg", 10000, false].DrawSimpleTextS(debugWatch.Value, new Vector2(2f + num3, num), Color.White * 0.5f, 0.5f);
			num2 = Math.Max(num3 + num4, num2);
		}
		Renderer["fg", 9999, false].DrawRectangleS(new RectangleF(0f, 19f, num2 + 4f, 5 * debugWatches.Count + 4), Color.Black * 0.5f);
	}

	private void DrawLiveTuners()
	{
		if (!Settings.DrawDebugMessages || liveTuners.Count == 0)
		{
			return;
		}
		int num = Renderer.ScreenHeight - 100 - 16;
		int num2 = 37;
		float num3 = 0f;
		foreach (KeyValuePair<string, float> item in liveTuners.Reverse().ToList())
		{
			float val = Renderer["fg", 10000, false].DrawSimpleTextS(item.Key, new Vector2(num2, num), Color.Lime * 0.6f, 0.5f);
			float val2 = Renderer["fg", 10000, false].DrawSimpleTextS($"{item.Value:0.00}", new Vector2(num2, num + 6), Color.White * 0.5f, 0.5f);
			liveTuneMinus[item.Key] = new RectangleF(2f, num - 3, 16f, 16f);
			Renderer["fg", 10000, false].DrawRectangleS(liveTuneMinus[item.Key], Color.Red * 0.3f);
			liveTunePlus[item.Key] = new RectangleF(19f, num - 3, 16f, 16f);
			Renderer["fg", 10000, false].DrawRectangleS(liveTunePlus[item.Key], Color.Lime * 0.3f);
			num3 = Math.Max((float)num2 + Math.Max(val, val2) + 2f, num3);
			num -= 17;
		}
		Renderer["fg", 9999, false].DrawRectangleS(new RectangleF(0f, num + 16 + 1 - 5, num3 + 2f, 17 * liveTuners.Count + 5), Color.Black * 0.5f);
	}
}
