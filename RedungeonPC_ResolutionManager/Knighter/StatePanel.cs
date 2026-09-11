#if DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.States;

namespace Knighter;

public class StatePanel : IDevPanel
{
	public string Title => "State";

	public Color TitleColor => new Color(150, 80, 190);

	public void Update(GameTime gameTime)
	{
	}

	public void CollectLines(Renderer renderer, List<string> lines)
	{
		Core core = Core.Instance;
		PlayState playState = core.CurrentPlayState;

		lines.Add($"State: {core.GetCurrentState().GetType().Name}");

		if (playState == null)
		{
			return;
		}
		lines.Add($"Entities: {playState.EntityManager.EntityCount}  Particles: {core.ParticleManager.ActiveParticleCount}");
		lines.Add($"Lights: {playState.LightManager.Lights.Count}  Shaders: {(Settings.UseCustomShaders ? "ON" : "OFF")}");

		if (DevTools.ShowCameraOverlay)
		{
			Camera cam = playState.Camera;
			lines.Add($"Camera: {cam.Position.X:0.0}, {cam.Position.Y:0.0}  Zoom: {cam.Zoom:0.00}");
		}
	}
}
#endif
