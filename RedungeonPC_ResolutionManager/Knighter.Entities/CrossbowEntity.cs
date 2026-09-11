using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class CrossbowEntity : Entity
{
	private int delay;

	private int interval;

	private int dir;

	private Animation anim;

	private int loadingDuration;

	private Sprite boltSprite;

	private bool madeShot;

	private int ticksSinceSound = 60;

	public CrossbowEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		delay = desc["delay"];
		interval = desc["interval"];
		dir = ((desc["dir"] != 0) ? 1 : (-1));
		if (desc.Flipped)
		{
			dir *= -1;
		}
		anim = new Animation((interval > 20) ? 0.2f : 0.25f, loop: false);
		anim.AddAndPlay("load", new List<SpriteName>
		{
			SpriteName.crossbow_1,
			SpriteName.crossbow_2,
			SpriteName.crossbow_3
		});
		loadingDuration = anim.DurationOf("load");
		anim.Pause();
		anim.Add("shoot", new List<SpriteName>
		{
			SpriteName.crossbow_4,
			SpriteName.crossbow_5
		});
		boltSprite = base.core.SpriteManager.GetSprite(SpriteName.crossbow_bolt);
	}

	public override void Update()
	{
		if (IsBroken)
		{
			return;
		}
		int num = (base.worldTicks - delay).Mod(interval);
		ticksSinceSound++;
		if (num == interval - loadingDuration)
		{
			anim.Reset();
			anim.Play("load");
			if (ticksSinceSound > 10)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.crossbow_fire, base.WorldPosition));
				ticksSinceSound = 0;
			}
		}
		if (anim.JustStopped && anim.CurrentSequence == "shoot")
		{
			anim.Play("load");
			anim.Pause();
		}
		if (num == 0 && !madeShot)
		{
			anim.Reset();
			anim.Play("shoot");
			SendMessage(new SpawnEntityMessage(new BoltEntity(base.WorldCenterCoordinates.X + 0.5f * (float)dir, base.WorldCenterCoordinates.Y, dir, this), null));
			madeShot = true;
		}
		else
		{
			madeShot = false;
		}
		anim.Update();
		base.Update();
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z);
		SendMessage(new PlayWorldSoundMessage(SoundName.crossbow_break, base.WorldPosition));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.crossbow_fragment_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.crossbow_fragment_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.crossbow_fragment_2), null));
		_inc(Stat.CrossbowsBroken);
		base.Break(offender);
	}

	public override void Draw()
	{
		Sprite sprite;
		if (!IsBroken)
		{
			sprite = anim.GetCurrentFrame();
			base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldPosition + new Vector2(0f, -5f), null, null, 0f, (dir >= 0) ? SpriteFlip.Horizontal : SpriteFlip.None);
			if (anim.CurrentSequence == "load")
			{
				base.core.Renderer[base.Z].DrawSpriteW(boltSprite, base.WorldPosition + new Vector2((float)dir * (4f - 1.5f * (float)anim.GetCurrentFrameNumber()), 1f), null, null, 0f, (dir >= 0) ? SpriteFlip.Horizontal : SpriteFlip.None);
			}
		}
		else
		{
			sprite = _(SpriteName.crossbow_broken);
			base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldPosition + new Vector2(-2f, -5f), null, null, 0f, (dir >= 0) ? SpriteFlip.Horizontal : SpriteFlip.None);
		}
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldCenter.Shift((dir > 0) ? (-6) : (-8), -8f), Color.Black * 0.2f, null, 0f, (dir < 0) ? SpriteFlip.Vertical : (SpriteFlip.Horizontal | SpriteFlip.Vertical));
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		if (!(other is BoltEntity) || ((BoltEntity)other).Owner != this)
		{
			return other is FragmentEntity;
		}
		return true;
	}
}
