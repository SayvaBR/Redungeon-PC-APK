using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class VesnaBeamEntity : Entity
{
	private int frame = 1;

	private bool bigger;

	public VesnaBeamEntity(float x, float y, bool bigger = false)
		: base(x, y, 1f, 1f)
	{
		this.bigger = bigger;
	}

	public override void Update()
	{
		int num = (bigger ? 40 : 30);
		frame = ((base.Age < 2 || base.Age > num - 2) ? 1 : ((base.Age < 4 || base.Age > num - 4) ? 2 : 3));
		if (base.Age >= num)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = (bigger ? (1.3f + 0.05f * Component._sin(0.3f * (float)base.Age)) : 1f);
		Sprite sprite = _("vesna_beam_" + frame);
		base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, 10f), null, Vector2.One * num, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		if (frame == 3)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.vesna_beam), base.WorldCenter.Shift(0f, 10f - 13.5f * num), null, new Vector2(num, base.core.Renderer.ScreenHeight), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		base.Draw();
	}
}
