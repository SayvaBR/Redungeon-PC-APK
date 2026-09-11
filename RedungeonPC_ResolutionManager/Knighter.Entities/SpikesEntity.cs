using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SpikesEntity : Entity
{
	private Animation animation;

	private Sprite brokenSprite;

	private int delay;

	private int timeOut;

	private int timeIn;

	private bool isStatic;

	private bool firstAnimation;

	private Vector2 brokenShift;

	public bool Active { get; private set; }

	public SpikesEntity(int x, int y, int delay, int timeOut, int timeIn)
		: base(x, y, 1f, 1f)
	{
		Init(x, y, delay, timeOut, timeIn);
	}

	public SpikesEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		Init(x, y, desc["delay"], desc["time-out"], desc["time-in"]);
	}

	private void Init(int x, int y, int delay, int timeOut, int timeIn)
	{
		animation = new Animation(0.3f, loop: false);
		animation.AddAndPlay("show", new List<SpriteName>
		{
			SpriteName.spikes_1,
			SpriteName.spikes_2,
			SpriteName.spikes_3,
			SpriteName.spikes_4,
			SpriteName.spikes_5,
			SpriteName.spikes_4
		});
		animation.Pause();
		brokenSprite = _(SpriteName.spikes_broken);
		brokenShift = new Vector2(-1f, -2f);
		IsBroken = false;
		this.delay = delay;
		this.timeOut = timeOut;
		this.timeIn = timeIn;
		isStatic = timeIn == 0 || timeOut == 0;
		if (isStatic)
		{
			Active = timeOut > 0;
			if (Active)
			{
				animation.Play();
			}
		}
		else
		{
			Active = false;
			firstAnimation = true;
		}
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z);
		SendMessage(new PlayWorldSoundMessage(SoundName.spikes_break, base.WorldPosition));
		Active = false;
		LeaveTile(base.Tile);
		_inc(Stat.SpikesBroken);
		base.Break(offender);
	}

	public override void Update()
	{
		if (!IsBroken)
		{
			if (!isStatic)
			{
				bool active = Active;
				int num = timeOut + timeIn;
				int num2 = (base.worldTicks - delay).Mod(num);
				Active = num2 < timeOut;
				if (active != Active && Active)
				{
					EnterTile(base.Tile);
				}
				if (!isStatic)
				{
					if (num2 == timeOut - 9)
					{
						animation.Reverse = true;
						animation.Play();
						SendMessage(new PlayWorldSoundMessage(SoundName.spikes_hide, base.WorldCenter));
						firstAnimation = false;
					}
					if (num2 == num - 7 || (firstAnimation && Active))
					{
						animation.Reverse = false;
						animation.Play();
						SendMessage(new PlayWorldSoundMessage(SoundName.spikes_show, base.WorldCenter));
						firstAnimation = false;
					}
				}
			}
			animation.Update();
		}
		base.Update();
	}

	public override void Draw()
	{
		if (!IsBroken)
		{
			base.core.Renderer[base.Z].DrawSpriteW(animation.GetCurrentFrame(), base.WorldPosition.Shift(1f, -5f));
		}
		else
		{
			base.core.Renderer[base.Z].DrawSpriteW(brokenSprite, base.WorldPosition + brokenShift);
		}
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (Active && other is PlayerEntity { Flying: false } playerEntity)
		{
			playerEntity.Hurt(InjuryType.Spikes, this);
		}
		base.CollideWith(other);
	}
}
