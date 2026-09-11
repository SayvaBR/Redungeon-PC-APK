using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SawEntity : Entity
{
	private Animation animation;

	private int delay;

	private int direction;

	private int distance;

	private int addHalf;

	private int ticksPerTile1;

	private int ticksPerTile2;

	private int stopTime1;

	private int stopTime2;

	private int spawnX;

	private int shadowLength = 6;

	private List<float> shadowX;

	private float speed;

	public SawEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 0.5f)
	{
		Init(desc["delay"], desc["dir"], desc["distance"], desc["add-half"], desc["ticks-per-tile-1"], desc["ticks-per-tile-2"], desc["stop-time-1"], desc["stop-time-2"], desc.Flipped);
	}

	private void Init(int delay, int direction, int distance, int addHalf, int ticksPerTile1, int ticksPerTile2, int stopTime1, int stopTime2, bool flipped = false)
	{
		animation = new Animation(0.4f);
		List<SpriteName> frames = new List<SpriteName>
		{
			SpriteName.saw_1,
			SpriteName.saw_2,
			SpriteName.saw_3,
			SpriteName.saw_4
		};
		animation.AddAndPlay("spin", frames);
		this.delay = delay;
		this.direction = ((direction != 0) ? 1 : (-1));
		this.distance = distance;
		this.addHalf = addHalf;
		this.ticksPerTile1 = ticksPerTile1;
		this.ticksPerTile2 = ticksPerTile2;
		this.stopTime1 = stopTime1;
		this.stopTime2 = stopTime2;
		if (flipped)
		{
			this.direction *= -1;
		}
		spawnX = (int)base.WorldCoordinates.X;
		shadowX = new List<float>();
		for (int i = 0; i < shadowLength; i++)
		{
			shadowX.Add(x);
		}
	}

	private float PosFromTime(int time)
	{
		float num = (float)direction * ((float)distance + 0.5f * (float)addHalf);
		int num2 = stopTime1;
		if (time < num2)
		{
			return 0f;
		}
		time -= num2;
		num2 = distance * ticksPerTile1;
		if (time < num2)
		{
			return MathHelper.Lerp(0f, num, (float)time / (float)num2);
		}
		time -= num2;
		num2 = stopTime2;
		if (time < num2)
		{
			return num;
		}
		time -= num2;
		num2 = distance * ticksPerTile2;
		return MathHelper.Lerp(num, 0f, (float)time / (float)num2);
	}

	public override void Update()
	{
		animation.Update();
		int num = stopTime1 + stopTime2 + distance * ticksPerTile1 + distance * ticksPerTile2;
		int num2 = (base.worldTicks - delay).Mod(num);
		float num3 = PosFromTime(num2);
		if (num2 == stopTime1 || num2 == stopTime1 + distance * ticksPerTile1 + stopTime2)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.saw_move, base.WorldCenter, 0.6f));
		}
		if (!IsBroken)
		{
			speed = x;
		}
		x = (float)spawnX + num3;
		if (!IsBroken)
		{
			speed = x - speed;
		}
		shadowX.RemoveAt(0);
		shadowX.Add(x);
		UpdateTiles();
		base.Update();
	}

	public override void Draw()
	{
		if (IsBroken)
		{
			base.core.Renderer[base.Z - 1].DrawSpriteW(_(SpriteName.saw_broken), base.WorldPosition - new Vector2(2f, 2f));
			base.Draw();
			return;
		}
		Sprite currentFrame = animation.GetCurrentFrame();
		base.core.Renderer[base.Z + 1].DrawSpriteW(currentFrame, base.WorldPosition - new Vector2(2f, 2f));
		base.core.Renderer[base.Z + 1].DrawSpriteW(_(SpriteName.saw_axis), base.WorldPosition - new Vector2(2f, 2f));
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(currentFrame, base.WorldPosition.Shift(-2f, 7f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
		for (int i = 0; i < shadowLength; i++)
		{
			base.core.Renderer[base.Z + 1].DrawSpriteW(animation.GetCurrentFrame(), 16f * new Vector2(shadowX[i], y) - new Vector2(2f, 2f), Color.White * (0.2f * Math.Abs(shadowX[0] - x) / 2f));
		}
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z);
		SendMessage(new PlayWorldSoundMessage(SoundName.saw_break, base.WorldPosition));
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter).OnSpawn(delegate(Particle p)
		{
			p.Velocity.Y = -2f;
		}).OnUpdate(delegate(Particle p)
		{
			p.Dead = p.Age == 70;
			p.Position += new Vector2(((offender is KnightChar) ? (0f - speed) : speed) * 15f, p.Velocity.Y);
			p.Velocity.Y += 0.1f;
		})
			.OnDraw(delegate(Particle p)
			{
				float num = (50f - (float)p.Age) / 50f;
				base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.saw_full), p.Position, Color.White * num * num * num * num, null, (float)base.worldTicks * 0.2f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Emit(1);
		_inc(Stat.SawsBroken);
		base.Break(offender);
	}

	public override void CollideWith(Entity other)
	{
		if (!IsBroken)
		{
			if (other is PlayerEntity { Flying: false } playerEntity)
			{
				playerEntity.Hurt(InjuryType.Saw, this);
				SendMessage(new PlayWorldSoundMessage(SoundName.saw_cut, base.WorldCenter, 0.4f));
			}
			base.CollideWith(other);
		}
	}
}
