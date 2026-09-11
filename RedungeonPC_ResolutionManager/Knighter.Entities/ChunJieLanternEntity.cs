using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ChunJieLanternEntity : Entity
{
	private Animation anim;

	private int offset;

	private float scale;

	private float rate;

	public ChunJieLanternEntity(float x, float y)
		: base(x + 0.5f, y + 0.5f, 0f, 0f)
	{
		anim = new Animation().Add("hover", "chunjie_paper_lantern_", "1232");
		anim.Play("hover");
		anim.SkipToRandomFrame();
		offset = Component._rnd(0, 150);
		rate = Component._rnd(0.8f, 1.2f);
		scale = Component._rnd(0.7f, 0.9f);
	}

	public override void Load()
	{
		base.Load();
	}

	public override void Update()
	{
		anim.Update();
		base.Update();
	}

	public override void Draw()
	{
		Vector2 v = base.WorldPosition - (base.core.CurrentPlayState.Camera.Position - base.WorldPosition) * 0.3f * (scale + 0.05f) * (scale + 0.05f);
		base.R[base.Z + 100].DrawSpriteW(anim.GetCurrentFrame(), v.Shift(0f, -10f + Component._cos((float)(base.Age + offset + 150) * 0.03f) * 4f), Color.White, rotation: Component._sin((float)(base.Age + offset) * 0.02f) * 0.2f * rate, scale: Vector2.One * scale, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		base.Draw();
	}
}
