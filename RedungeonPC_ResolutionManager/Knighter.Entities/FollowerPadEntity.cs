using Knighter.Graphics;
using Knighter.Helpers;

namespace Knighter.Entities;

public class FollowerPadEntity : Entity
{
	public bool Taken;

	public bool Active;

	public bool IsChestBase;

	private bool lastActive;

	private int anim = -1;

	private int dur = 20;

	public FollowerPadEntity(int x, int y, bool chestBase)
		: base(x, y, 1f, 1f)
	{
		Active = true;
		lastActive = true;
		Taken = false;
		IsChestBase = chestBase;
	}

	public override void Update()
	{
		if (lastActive != Active)
		{
			lastActive = Active;
			anim = ((!Active) ? dur : 0);
		}
		if (anim >= 0)
		{
			anim += (Active ? 1 : (-1));
			if (anim == 0 || anim == dur)
			{
				anim = -1;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (IsChestBase)
		{
			if (!Taken)
			{
				base.core.Renderer["bg", (int)base.WorldPosition.Y + 16, false].DrawSpriteW(_(SpriteName.chest_follower_base), base.WorldPosition.Shift(7f, 13f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			}
		}
		else
		{
			base.core.Renderer["bg", (int)base.WorldPosition.Y + 16, true].DrawSpriteW(_(SpriteName.follower_pad), base.WorldCenter.Shift(-0.5f, -2f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (Active || anim >= 0)
			{
				int num = 3;
				if (anim >= 0)
				{
					num = anim / (dur / 2) + 1;
				}
				base.core.Renderer[base.Z + 16].DrawSpriteW(_("follower_pad_teeth_front_" + num), base.WorldCenter.Shift(-0.5f, -2f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer[base.Z].DrawSpriteW(_("follower_pad_teeth_back_" + num), base.WorldCenter.Shift(-0.5f, -2f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}
}
