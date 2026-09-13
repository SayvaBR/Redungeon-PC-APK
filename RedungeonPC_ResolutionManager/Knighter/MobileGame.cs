using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#if DEBUG
using Knighter.QA;
#endif

namespace Knighter;

public enum WindowMode
{
	Windowed,
	Fullscreen,
	Borderless
}

public sealed class MobileGame : Game
{
	private const int DefaultWindowedWidth = 1280;

	private const int DefaultWindowedHeight = 720;

	private GraphicsDeviceManager graphics;

	private SpriteBatch spriteBatch;

	private WindowMode windowMode = WindowMode.Windowed;

	// Guarda o tamanho de janela conhecido para restaurar ao sair de fullscreen/borderless.
	private int windowedWidth = DefaultWindowedWidth;

	private int windowedHeight = DefaultWindowedHeight;

	private bool suppressClientSizeChanged;

	private bool clientResizePending;

	private int pendingClientWidth;

	private int pendingClientHeight;

	private bool altEnterWasDown;

	private bool f11WasDown;
	private MouseCursor handCursor;
#if DEBUG
	private int visualFrame;
	private string VisualCapturePath => QaSession.IsActive ? System.Environment.GetEnvironmentVariable("REDUNGEON_QA_VISUAL") : null;
#endif

	public MobileGame()
	{
		graphics = new GraphicsDeviceManager(this);
		base.Content.RootDirectory = "Content";

		// Port desktop: sem restrição de orientação (isso só existia no mobile).
		Window.AllowUserResizing = true;
		Window.ClientSizeChanged += OnClientSizeChanged;

		windowedWidth = DefaultWindowedWidth;
		windowedHeight = DefaultWindowedHeight;
#if DEBUG
		if (System.Environment.GetEnvironmentVariable("REDUNGEON_QA_PORTRAIT") == "1") { windowedWidth = 540; windowedHeight = 960; }
#endif
		graphics.IsFullScreen = false;
		graphics.PreferredBackBufferWidth = windowedWidth;
		graphics.PreferredBackBufferHeight = windowedHeight;
		graphics.HardwareModeSwitch = false; // evita perder o contexto gráfico ao alternar fullscreen
	}

	protected override void Initialize()
	{
		// Mouse → TouchPanel: o jogo inteiro lê core.TouchState (TouchPanel.GetState).
		TouchPanel.EnableMouseTouchPoint = true; // pode remover
		TouchPanel.WindowHandle = Window.Handle; // pode remover
		base.IsMouseVisible = true;
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
		Core.Initialize(this, graphics.GraphicsDevice, spriteBatch, base.Content, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
		Core.Instance.Load();
		var sprite = Core.Instance.SpriteManager.GetSprite(Knighter.Graphics.SpriteName.swipe_hand_1);
		var source = Core.Instance.SpriteManager.GetTexture(sprite.TextureName);
		var pixels = new Color[sprite.SrcWidth * sprite.SrcHeight];
		source.GetData(0, new Rectangle(sprite.X, sprite.Y, sprite.SrcWidth, sprite.SrcHeight), pixels, 0, pixels.Length);
		int w = sprite.SrcWidth * 2, h = sprite.SrcHeight * 2;
		var scaled = new Color[w * h];
		for (int y = 0; y < h; y++)
		for (int x = 0; x < w; x++) scaled[y * w + x] = pixels[(y / 2) * sprite.SrcWidth + x / 2];
		using var cursorTexture = new Texture2D(GraphicsDevice, w, h);
		cursorTexture.SetData(scaled);
		handCursor = MouseCursor.FromTexture2D(cursorTexture, 2, 2);
		Mouse.SetCursor(handCursor);
	}

	protected override void UnloadContent()
	{
		Knighter.Gameplay.Rumble.Stop();
		Core.Instance.Unload();
		Mouse.SetCursor(MouseCursor.Arrow);
		handCursor?.Dispose();
	}

	protected override void Update(GameTime gameTime)
	{
		if (base.IsActive
#if DEBUG
			|| QaSession.IsActive
#endif
		)
		{
			ApplyPendingClientResize();
			HandleWindowInput();
			TouchPanel.DisplayWidth = (int)Core.Instance.ResolutionManager.LogicalWidth;
			TouchPanel.DisplayHeight = (int)Core.Instance.ResolutionManager.LogicalHeight;

			Core.Instance.GameTime = gameTime;
			Core.Instance.Update();
			IsMouseVisible = InputDeviceTracker.UsingMouse && Core.Instance.GetCurrentState() is not Knighter.States.SplashState;
#if DEBUG
			if (!string.IsNullOrEmpty(VisualCapturePath) && ++visualFrame == 2)
				Core.Instance.OpenVisualSettings();
			if (!string.IsNullOrEmpty(VisualCapturePath)) Core.Instance.TickLykosQa(visualFrame);
			if (!string.IsNullOrEmpty(VisualCapturePath) && visualFrame == 24 && System.Environment.GetEnvironmentVariable("REDUNGEON_QA_SCREEN")?.StartsWith("pause") == true)
				Core.Instance.Pause(false);
			if (QaSession.ShouldExitAfterCurrentFrame)
			{
				Exit();
			}
#endif
			base.Update(gameTime);
		}
		else
		{
			MouseTouchAdapter.Reset();
			Knighter.Gameplay.Rumble.Stop();
			Core.Instance?.Input.Reset();
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		if (base.IsActive
#if DEBUG
			|| QaSession.IsActive
#endif
		)
		{
			base.GraphicsDevice.Clear(new Color(15, 15, 15));
			Core.Instance.Draw();
			base.Draw(gameTime);
#if DEBUG
			if (!string.IsNullOrEmpty(VisualCapturePath) && visualFrame == (int.TryParse(System.Environment.GetEnvironmentVariable("REDUNGEON_QA_CAPTURE_FRAME"), out int captureFrame) ? captureFrame : 180))
			{
				Core.Instance.VerifyUiFixture();
				int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
				int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
				var pixels = new Color[w * h];
				GraphicsDevice.GetBackBufferData(pixels);
				using var texture = new Texture2D(GraphicsDevice, w, h);
				texture.SetData(pixels);
				using var output = System.IO.File.Create(VisualCapturePath);
				texture.SaveAsPng(output, w, h);
				Exit();
			}
#endif
		}
	}

	public void OnEnteringBackground()
	{
		if (Core.Instance != null)
		{
			Core.Instance.OnEnteringBackground();
		}
	}

	private void HandleWindowInput()
	{
		KeyboardState keyboard = Keyboard.GetState();

		bool altDown = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
		bool altEnterDown = altDown && keyboard.IsKeyDown(Keys.Enter);
		if (altEnterDown && !altEnterWasDown)
		{
			ToggleFullscreen();
		}
		altEnterWasDown = altEnterDown;

		bool f11Down = keyboard.IsKeyDown(Keys.F11);
		if (f11Down && !f11WasDown)
		{
			ToggleFullscreen();
		}
		f11WasDown = f11Down;
	}

	private void ToggleFullscreen()
	{
		SetWindowMode(windowMode == WindowMode.Windowed ? WindowMode.Fullscreen : WindowMode.Windowed);
	}

	/// <summary>
	/// Único ponto que muda o modo de janela (chamado hoje por F11/Alt+Enter,
	/// e futuramente pelo menu de opções). Sempre termina recalculando a
	/// resolução via ResolutionManager, para que Renderer/Camera/HUD nunca
	/// fiquem com um backbuffer diferente do que eles acham que existe.
	/// </summary>
	public void SetWindowMode(WindowMode mode)
	{
		clientResizePending = false;
		if (mode == WindowMode.Windowed && windowMode != WindowMode.Windowed)
		{
			// Voltando para janela: restaura o último tamanho de janela conhecido.
			graphics.PreferredBackBufferWidth = windowedWidth;
			graphics.PreferredBackBufferHeight = windowedHeight;
		}

		windowMode = mode;
		suppressClientSizeChanged = true;
		switch (mode)
		{
		case WindowMode.Windowed:
			Window.IsBorderless = false;
			graphics.IsFullScreen = false;
			break;
		case WindowMode.Fullscreen:
			Window.IsBorderless = false;
			graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
			graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
			graphics.IsFullScreen = true;
			break;
		case WindowMode.Borderless:
			graphics.IsFullScreen = false;
			Window.IsBorderless = true;
			graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
			graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
			break;
		}
		graphics.ApplyChanges();
		suppressClientSizeChanged = false;
		ApplyCurrentResolution();
	}

	/// <summary>Chamado pelo menu de opções (futuro) ao trocar a resolução em modo janela.</summary>
	public void SetWindowedResolution(int width, int height)
	{
		clientResizePending = false;
		windowedWidth = width;
		windowedHeight = height;
		if (windowMode != WindowMode.Windowed)
		{
			return;
		}
		graphics.PreferredBackBufferWidth = width;
		graphics.PreferredBackBufferHeight = height;
		graphics.ApplyChanges();
		ApplyCurrentResolution();
	}

	private void OnClientSizeChanged(object sender, System.EventArgs e)
	{
		if (suppressClientSizeChanged || GraphicsDevice == null)
		{
			return;
		}
		if (Window.ClientBounds.Width <= 0 || Window.ClientBounds.Height <= 0)
		{
			return;
		}
		pendingClientWidth = Window.ClientBounds.Width;
		pendingClientHeight = Window.ClientBounds.Height;
		clientResizePending = true;
	}

	private void ApplyPendingClientResize()
	{
		if (!clientResizePending || windowMode != WindowMode.Windowed)
		{
			return;
		}

		clientResizePending = false;
		windowedWidth = pendingClientWidth;
		windowedHeight = pendingClientHeight;
		suppressClientSizeChanged = true;
		try
		{
			graphics.PreferredBackBufferWidth = pendingClientWidth;
			graphics.PreferredBackBufferHeight = pendingClientHeight;
			graphics.ApplyChanges();
		}
		finally
		{
			suppressClientSizeChanged = false;
		}
		ApplyCurrentResolution();
	}

	/// <summary>Chamado pelo Core ao carregar/aplicar as opções salvas (base para o futuro menu de configurações).</summary>
	public void ApplyVideoSettings(bool vsync, int fpsLimit)
	{
		graphics.SynchronizeWithVerticalRetrace = vsync;

		// A simulação (Update) fica sempre travada em 60 ticks/segundo.
		// Motivo: toda a lógica de gameplay (movimento, cooldowns, animação)
		// é escrita "por tick", não por tempo real (deltaTime) — então se o
		// Update() rodar mais rápido que 60/s, o jogo inteiro acelera junto
		// (era exatamente o bug reportado a 120 FPS). Até migrarmos essa
		// lógica para deltaTime de verdade, a taxa de simulação não pode
		// depender do limite de FPS escolhido pelo jogador.
		base.IsFixedTimeStep = true;
		base.TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / 60.0);

		graphics.ApplyChanges();
	}

	private void ApplyCurrentResolution()
	{
		if (Core.Instance == null)
		{
			return;
		}
		Core.Instance.ResolutionManager.Recalculate(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
	}
}
