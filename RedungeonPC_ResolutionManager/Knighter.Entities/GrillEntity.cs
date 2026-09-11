using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class GrillEntity : Entity
{
	private enum GrillState
	{
		Calm,
		Ignite,
		Burn
	}

	private Animation grillAnimation;

	private Animation smokeAnimation;

	private int delay;

	private int timeOut;

	private int timeIn;

	private bool isStatic;

	private bool firstAnimation;

	private Light light;

	private Animation flameAnimation;

	private List<Sprite> flameIgnition;

	private Sprite grillTile;

	private bool horizontal;

	private GrillState state;

	public bool Active { get; private set; }

	public GrillEntity(int x, int y, int delay, int timeOut, int timeIn)
		: base(x, y, 1f, 1f)
	{
		Init(x, y, delay, timeOut, timeIn);
	}

	public GrillEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		Init(x, y, desc["delay"], desc["time-out"], desc["time-in"]);
	}

	private void Init(int x, int y, int delay, int timeOut, int timeIn)
	{
		horizontal = (x + y) % 2 != 0;
		grillTile = _(horizontal ? SpriteName.grill_tile_h : SpriteName.grill_tile_v);
		grillAnimation = new Animation(0.3f, loop: false);
		grillAnimation.AddAndPlay("burn", new List<SpriteName>
		{
			SpriteName.pixel,
			SpriteName.grill_tile_warmup_1,
			SpriteName.grill_tile_warmup_2,
			SpriteName.grill_tile_warmup_3,
			SpriteName.grill_tile_burn_1,
			SpriteName.grill_tile_burn_2
		}).Pause();
		flameAnimation = new Animation();
		flameAnimation.Add("burn", "grill_burn_", "1234").Play("burn");
		flameIgnition = new List<Sprite>
		{
			_(SpriteName.grill_ignite_1),
			_(SpriteName.grill_ignite_2),
			_(SpriteName.grill_ignite_3)
		};
		smokeAnimation = new Animation(0.13f);
		smokeAnimation.Add("smoke", "grill_smoke_", "1234");
		smokeAnimation.Play("smoke");
		smokeAnimation.SkipToRandomFrame();
		this.delay = delay;
		this.timeOut = timeOut;
		this.timeIn = timeIn;
		isStatic = timeIn == 0 || timeOut == 0;
		if (isStatic)
		{
			Active = timeOut > 0;
			if (Active)
			{
				grillAnimation.Play();
				return;
			}
			Active = false;
			firstAnimation = true;
		}
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16759608), 1.5f, 1f, this);
		light.ChangeRate = 0.1f;
		light.Active = true;
		base.Load();
	}

	public override void Update()
	{
		if (!IsBroken)
		{
			grillAnimation.Update();
			flameAnimation.Update();
			int currentFrameNumber = grillAnimation.GetCurrentFrameNumber();
			if (currentFrameNumber == 0)
			{
				state = GrillState.Calm;
			}
			else if (currentFrameNumber <= 3)
			{
				state = GrillState.Ignite;
			}
			else
			{
				state = GrillState.Burn;
			}
			if (!isStatic)
			{
				bool active = Active;
				int num = timeOut + timeIn;
				int num2 = (base.worldTicks - delay).Mod(num);
				Active = state == GrillState.Burn;
				if (active != Active && Active)
				{
					EnterTile(base.Tile);
				}
				if (!isStatic)
				{
					if (num2 == timeOut)
					{
						grillAnimation.Reverse = true;
						grillAnimation.Play();
						firstAnimation = false;
					}
					if (num2 == num - 1 || (firstAnimation && Active))
					{
						grillAnimation.Reverse = false;
						grillAnimation.Play();
						SendMessage(new PlayWorldSoundMessage(SoundName.grill_lightup, base.WorldCenter));
						firstAnimation = false;
					}
				}
			}
			light.TargetIntencity = ((state == GrillState.Calm) ? 0f : 1f);
		}
		else
		{
			smokeAnimation.Update();
			light.TargetIntencity = 0f;
			Active = false;
		}
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer["bg", true].DrawSpriteW(grillTile, base.WorldPosition);
		if (!IsBroken)
		{
			int currentFrameNumber = grillAnimation.GetCurrentFrameNumber();
			if (state != GrillState.Calm)
			{
				base.core.Renderer[base.Z].DrawSpriteW(grillAnimation.GetCurrentFrame(), base.WorldCenter, null, null, horizontal ? ((float)Math.PI / 2f) : 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			if (state == GrillState.Ignite)
			{
				base.core.Renderer[base.Z].DrawSpriteW(flameIgnition[currentFrameNumber - 1], base.WorldPosition.Shift(1f, -5f));
			}
			else if (state == GrillState.Burn)
			{
				base.core.Renderer[base.Z].DrawSpriteW(flameAnimation.GetCurrentFrame(), base.WorldPosition.Shift(1f, -5f));
			}
		}
		else
		{
			base.core.Renderer[base.Z].DrawSpriteW(smokeAnimation.GetCurrentFrame(), base.WorldCenter, Color.White * 0.4f, new Vector2(1f, 0.8f + 0.2f * Component._sin(((float)base.Age + 20f * x + 20f * y) * 0.1f)), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z);
		SendMessage(new PlayWorldSoundMessage(SoundName.hiss, base.WorldPosition));
		Active = false;
		LeaveTile(base.Tile);
		base.Break(offender);
	}

	public override void CollideWith(Entity other)
	{
		if (!IsBroken)
		{
			if (Active && other is PlayerEntity { Flying: false } playerEntity)
			{
				playerEntity.Hurt(InjuryType.Flame, this);
			}
			base.CollideWith(other);
		}
	}
}
