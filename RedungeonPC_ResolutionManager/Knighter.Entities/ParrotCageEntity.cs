using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ParrotCageEntity : Entity
{
	private Animation parrot;

	public ParrotCageEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
		parrot = new Animation();
		parrot.Add("fly", "parrot_front_", "1234");
		parrot.Play("fly");
	}

	public override void Update()
	{
		parrot.Update();
		base.Update();
	}

	public override void InteractWith(Entity other)
	{
		if (other is PlayerEntity { Dead: false } && !IsBroken)
		{
			Break(other);
		}
		base.InteractWith(other);
	}

	public override bool IsPassableFor(Entity other)
	{
		if (!IsBroken)
		{
			return other is FragmentEntity;
		}
		return true;
	}

	public override void Break(Entity offender)
	{
		if (!IsBroken)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.piston_break, base.WorldPosition));
			IsBroken = true;
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_top), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_bar_1), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_bar_1), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_bar_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_bar_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_bar_3), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_cage_bar_3), null));
			(base.core.CurrentPlayState.Player as BraggChar)?.SpawnParrot(this);
			base.Break(offender);
		}
	}

	public override void Draw()
	{
		if (!IsBroken)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.bragg_cage_base), base.WorldPosition.Shift(7.5f, 12f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			base.core.Renderer[base.Z].DrawSpriteW(parrot.GetCurrentFrame(), base.WorldPosition.Shift(2f, -7f));
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.bragg_cage_bars), base.WorldPosition.Shift(7.5f, 12f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(_(SpriteName.bragg_cage_base), base.WorldPosition.Shift(7.5f, 12f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical, SpriteOrigin.TopCenter);
		}
		else
		{
			base.core.Renderer["bg", (int)base.WorldPosition.Y + 16, false].DrawSpriteW(_(SpriteName.bragg_cage_base_broken), base.WorldPosition.Shift(7.5f, 12f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		base.Draw();
	}
}
