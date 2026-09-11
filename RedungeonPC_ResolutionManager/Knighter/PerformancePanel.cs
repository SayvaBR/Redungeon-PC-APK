#if DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Knighter.Graphics;

namespace Knighter;

public class PerformancePanel : IDevPanel
{
	public string Title => "Performance";

	public Color TitleColor => new Color(50, 150, 180);

	private int frameCount;

	private float fpsTimer;

	private float currentFps;

	public void Update(GameTime gameTime)
	{
		frameCount++;
		fpsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (fpsTimer >= 1f)
		{
			currentFps = frameCount / fpsTimer;
			frameCount = 0;
			fpsTimer = 0f;
		}
	}

	public void CollectLines(Renderer renderer, List<string> lines)
	{
		Core core = Core.Instance;
		lines.Add($"FPS: {currentFps:0}  avg: {core.FrameCounter.AverageFramesPerSecond:0.0}");
		lines.Add($"Res: {renderer.BufferWidth}x{renderer.BufferHeight}  Screen: {renderer.ScreenWidth}x{renderer.ScreenHeight}");
		lines.Add($"PixelScale: {Settings.PixelScale:0.00}");

		long bytes = GC.GetTotalMemory(forceFullCollection: false);
		lines.Add($"Memory: {bytes / (1024f * 1024f):0.0} MB");

		if (DevTools.DetailedPerformance)
		{
			lines.Add($"Update: {DevTools.LastUpdateMs:0.00}ms  Draw: {DevTools.LastDrawMs:0.00}ms");
			lines.Add($"Ticks: {core.Ticks}  TicksInState: {core.TicksInState}");
		}
	}
}
#endif
