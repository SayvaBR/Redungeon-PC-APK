using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class WaitState : State
{
	private readonly Animation fire;

	public WaitState()
	{
		base.TransDuration = 10;
		IsOverlay = true;
		ShowCoins = false;
		fire = new Animation();
		fire.Add("burn", "menu_fire_", "1234");
		fire.Play("burn");
	}

	public override void Load()
	{
		Screen("wait");
		base.Load();
	}

	public override void Update()
	{
		fire.Update();
		base.Update();
	}

	public override void Draw()
	{
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 20000, false].FillScreen(Color.Black * 0.9f * num);
		base.core.Renderer["fg", 20001, false].DrawSpriteS(_(SpriteName.glow_huge), base.core.Renderer.ScreenCenter.Shift(0f, -10f), scale: new Vector2(0.75f), tint: Color.Orange * (0.4f + Component._sin((float)base.ticks * 0.05f) * 0.1f) * num, rotation: 0f, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		base.core.Renderer["fg", 20001, false].DrawSpriteS(fire.GetCurrentFrame(), base.core.Renderer.ScreenCenter.Shift(0f, -10f), null, new Vector2(0.75f * num), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", 20001, false].DrawTextS(__(SId.MESSAGES_wait_please), base.core.Renderer.ScreenCenter.Shift(0f, 30f), TextProfile.OrangeBoldText.Alter(null, null, boxAlignment: Alignment2D.BottomCenter, textAlignment: Alignment2D.Center, width: 200, height: 25, scale: num, decoration: TextDecoration.None));
		base.Draw();
	}
}
