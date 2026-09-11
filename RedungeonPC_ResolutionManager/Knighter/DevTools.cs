#if DEBUG
using System;
using System.Diagnostics;
using System.IO;
using Knighter.Gameplay;
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Knighter;

/// <summary>
/// Atalhos e toggles de desenvolvimento do port. Nunca altera gameplay —
/// só liga/desliga visualizações de debug e ferramentas auxiliares.
/// Remoção: apagar este arquivo e as chamadas marcadas "DevTools" em Core/Game1/Renderer.
/// </summary>
public static class DevTools
{
	public static bool ShowLightMap { get; private set; }

	public static bool ShowRenderTargets { get; private set; }

	public static bool SuppressParticles { get; private set; }

	public static bool ShowCameraOverlay { get; private set; }

	public static bool DetailedPerformance { get; private set; }

	/// <summary>
	/// Congela o avanço do Terminator (a "Darkness"/grue que persegue o
	/// player e mata se ele ficar parado tempo demais) e desliga o gatilho
	/// de morte por Timeout enquanto ligado. Não mexe no resto do PlayState
	/// — só essas duas coisas em PlayState.Update().
	/// </summary>
	public static bool DisableTerminator { get; private set; }

	/// <summary>
	/// Pausa TOTAL da simulação (não é o menu de pausa do jogo) — congela
	/// entidades, física, partículas etc., mas o EngineDiagnostics/Sprite
	/// Inspector continuam funcionando normalmente. Pensado pra poder
	/// inspecionar sprites parados sem o jogo se mexer.
	/// </summary>
	public static bool FullPause { get; private set; }

	public static float LastUpdateMs { get; private set; }

	public static float LastDrawMs { get; private set; }

	private static long updateStartTicks;

	private static long drawStartTicks;

	private static bool screenshotRequested;

	private static readonly bool[] keyWasDown = new bool[256];

	public static void BeginUpdate()
	{
		updateStartTicks = Stopwatch.GetTimestamp();
	}

	public static void EndUpdate()
	{
		LastUpdateMs = ElapsedMs(updateStartTicks);
	}

	public static void BeginDraw()
	{
		drawStartTicks = Stopwatch.GetTimestamp();
	}

	public static void EndDraw()
	{
		LastDrawMs = ElapsedMs(drawStartTicks);
	}

	public static void HandleInput(Core core)
	{
		KeyboardState keyboard = Keyboard.GetState();
		ToggleKey(keyboard, Keys.F1, () => EngineDiagnostics.Toggle());
		ToggleKey(keyboard, Keys.F2, () => Settings.DrawDebugShapes = !Settings.DrawDebugShapes);
		ToggleKey(keyboard, Keys.F3, () => Settings.HighlightOccupiedTiles = !Settings.HighlightOccupiedTiles);
		ToggleKey(keyboard, Keys.F4, () => ShowLightMap = !ShowLightMap);
		ToggleKey(keyboard, Keys.F5, () => ShowRenderTargets = !ShowRenderTargets);
		ToggleKey(keyboard, Keys.F6, () => Settings.UseCustomShaders = !Settings.UseCustomShaders);
		ToggleKey(keyboard, Keys.F7, () => SuppressParticles = !SuppressParticles);
		ToggleKey(keyboard, Keys.F8, () => Settings.DrawDebugWatches = !Settings.DrawDebugWatches);
		ToggleKey(keyboard, Keys.F9, () => ShowCameraOverlay = !ShowCameraOverlay);
		ToggleKey(keyboard, Keys.F10, () => DetailedPerformance = !DetailedPerformance);
		ToggleKey(keyboard, Keys.U, () => core.Achievments.DebugTestToast());
		ToggleKey(keyboard, Keys.OemOpenBrackets, () => core.DebugSpawner.CycleSelection(-1));
		ToggleKey(keyboard, Keys.OemCloseBrackets, () => core.DebugSpawner.CycleSelection(1));
		ToggleKey(keyboard, Keys.Enter, () => core.DebugSpawner.SpawnSelected());
		ToggleKey(keyboard, Keys.T, () => DisableTerminator = !DisableTerminator);
		ToggleKey(keyboard, Keys.F12, () => screenshotRequested = true);
		ToggleKey(keyboard, Keys.P, () => FullPause = !FullPause);
		ToggleKey(keyboard, Keys.D0, () => InputDeviceTracker.ForceBrand(GamepadBrand.PlayStation));
		ToggleKey(keyboard, Keys.D9, () => InputDeviceTracker.ForceBrand(GamepadBrand.Xbox));
	}

	public static void TrySaveScreenshot(GraphicsDevice graphicsDevice)
	{
		if (!screenshotRequested)
		{
			return;
		}
		screenshotRequested = false;
		try
		{
			int width = graphicsDevice.PresentationParameters.BackBufferWidth;
			int height = graphicsDevice.PresentationParameters.BackBufferHeight;
			Texture2D capture = new Texture2D(graphicsDevice, width, height);
			Color[] pixels = new Color[width * height];
			graphicsDevice.GetBackBufferData(pixels);
			capture.SetData(pixels);
			string dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
			Directory.CreateDirectory(dir);
			string path = Path.Combine(dir, $"redungeon_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
			using FileStream stream = File.Create(path);
			capture.SaveAsPng(stream, width, height);
			capture.Dispose();
			EngineDiagnostics.Log($"Screenshot: {path}");
		}
		catch (Exception ex)
		{
			EngineDiagnostics.Log($"Screenshot falhou: {ex.Message}");
		}
	}

	public static void DrawCameraOverlay(Core core)
	{
		if (!ShowCameraOverlay || core.CurrentPlayState == null)
		{
			return;
		}
		Camera camera = core.CurrentPlayState.Camera;
		Vector2 center = core.Renderer.ScreenCenter;
		core.Renderer["fg", 19999, false].DrawDotS(center, Color.Yellow, 3f);
		core.Renderer["fg", 19999, false].DrawSimpleTextS(
			$"Cam {camera.Position.X:0.0},{camera.Position.Y:0.0} Z:{camera.Zoom:0.00}",
			center + new Vector2(6f, -10f),
			Color.Yellow * 0.9f,
			0.45f);
	}

	private static void ToggleKey(KeyboardState keyboard, Keys key, Action action)
	{
		int index = (int)key;
		bool down = keyboard.IsKeyDown(key);
		if (down && !keyWasDown[index])
		{
			action();
		}
		keyWasDown[index] = down;
	}

	private static float ElapsedMs(long startTicks)
	{
		return (float)(Stopwatch.GetTimestamp() - startTicks) * 1000f / Stopwatch.Frequency;
	}
}
#endif
