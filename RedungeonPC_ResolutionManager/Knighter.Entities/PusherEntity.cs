using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PusherEntity : Entity
{
	private Sprite sprite;

	private Sprite spriteGlow1;

	private Sprite spriteGlow2;

	private Sprite area;

	private float rotation;

	private ParticleEmitter emitter;

	private int glow;

	private Light light;

	public int Dx { get; private set; }

	public int Dy { get; private set; }

	public PusherEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		Init(x, y, desc["dx"], desc["dy"], desc.Flipped);
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(8584967), 1.5f, 0.7f, this);
		light.ChangeRate = 0.05f;
		light.FollowRate = 1f;
		light.TargetIntencity = 0f;
		light.Intencity = 0f;
	}

	private void Init(int x, int y, int dx, int dy, bool flipped)
	{
		Dx = (flipped ? (-dx) : dx);
		Dy = dy;
		sprite = _(SpriteName.pusher);
		area = _(SpriteName.glow_big);
		spriteGlow1 = _(SpriteName.pusher_glow_1);
		spriteGlow2 = _(SpriteName.pusher_glow_2);
		rotation = (float)Math.Atan2(Dx, -Dy);
		emitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter).AttachTo(this, local: true).OnSpawn(delegate
		{
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Velocity = base.WorldCenter + p.Offset + new Vector2(Dx, Dy) * 16f;
				p.Position += new Vector2(Component._cos(rotation - (float)Math.PI / 2f), Component._sin(rotation - (float)Math.PI / 2f)) * 0.5f;
				p.Dead = (p.Velocity - p.Position - base.Origin * 16f).Length() < 5f;
			})
			.OnDraw(delegate(Particle p)
			{
				float num = Component._m((p.Velocity - p.Position - base.Origin * 16f).LengthSquared() / 50f, 1f);
				float value = Component._M(0.6f * (p.Velocity - p.Position - base.Origin * 16f).LengthSquared() / (p.Velocity - base.WorldCenter).LengthSquared(), 0.4f);
				base.core.Renderer[base.Z + 10].DrawSpriteW(spriteGlow2, p.Position + base.Origin * 16f, Color.White * (0.3f + Component._sin((float)p.Age * 0.2f) * 0.1f) * num, new Vector2(value), rotation, SpriteFlip.None, SpriteOrigin.Center);
			});
		emitter.Start(10);
		while (emitter.DeadCount == 0 && emitter.Age < 180)
		{
			emitter.Update();
		}
	}

	public override void Update()
	{
		if (glow > 0)
		{
			glow--;
		}
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer["bg", true].DrawSpriteW(sprite, base.WorldCenter, Color.White, null, rotation, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg"].DrawSpriteW(spriteGlow1, base.WorldCenter, Color.White * Component._sin((float)base.worldTicks * 0.3f), null, rotation, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg"].DrawSpriteW(spriteGlow2, base.WorldCenter, Color.White * Component._sin((float)base.worldTicks * 0.3f - 1.5f), null, rotation, SpriteFlip.None, SpriteOrigin.Center);
		float num = 1f + Component._sin((float)base.worldTicks * 0.2f);
		base.core.Renderer["bg", base.Z + 300, false].DrawSpriteW(area, base.WorldCenter + new Vector2(Dx, Dy) * 16f + base.dAnim, default(Color).FromRgb(11918937) * (0.1f + 0.01f * num), Vector2.One * (num * 0.04f + 0.5f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity { Burning: false } playerEntity && playerEntity.SuspendedStartFlying(Dx, Dy))
		{
			bool flag = Math.Abs(Dx) > Math.Abs(Dy);
			playerEntity.FacingDirection = new Vector2(flag ? Math.Sign(Dx) : 0, (!flag) ? Math.Sign(Dy) : 0);
			glow = 30;
			light.Intencity = 2f;
			SendMessage(new PlayWorldSoundMessage(SoundName.pusher, base.WorldCenter));
			_inc(Stat.JumpersUsed);
		}
		if (other is BoxEntity { Flying: false } boxEntity)
		{
			boxEntity.SuspendedStartFlying(Dx, Dy, 0.2f);
		}
		base.CollideWith(other);
	}
}
