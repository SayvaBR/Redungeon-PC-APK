using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class GolemMissile : Entity
{
	private ParticleEmitter emitter;

	private Light light;

	private bool loaded;

	public GolemMissile(float x, float y)
		: base(x - 0.2f, y - 0.5f, 0.4f, 0.95f)
	{
		SetFlying(value: true);
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16759608), 3f, 1.2f, this);
		light.Active = true;
	}

	public override void Load()
	{
		emitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldPosition, 1f).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Velocity = SciHelper.GetRandomVectorInCircle(0.05f).Shift(0f, 0f - Component._rnd(0.08f, 1.1f));
			p.Position = p.Position.Shift(0f, -8f);
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position += p.Velocity;
				p.Dead = p.Age >= 40;
			})
			.OnDraw(delegate(Particle p)
			{
				int num = (int)(7f * (float)(40 - p.Age) / 40f + 1f);
				base.core.Renderer[(int)p.Position.Y + 10 - 2].DrawSpriteW(_("circle_" + num), p.Position, default(Color).FromRgb(16763709), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				float num2 = 1f - (float)p.Age / 40f;
				base.core.Renderer[(int)p.Position.Y + 10 - 3].DrawSpriteW(_(SpriteName.glow), p.Position, default(Color).FromRgb(16763709) * 0.8f * num2, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				if (num >= 3)
				{
					base.core.Renderer[(int)p.Position.Y + 10 - 1].DrawSpriteW(_("circle_" + (num - 2)), p.Position.Shift(-0.5f, -0.5f), default(Color).FromRgb(16776960), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
			})
			.Start(4, 3);
		loaded = true;
		base.Load();
	}

	public override bool CanEnterNullTiles()
	{
		return true;
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z + 20].DrawSpriteW(_(SpriteName.circle_8), base.WorldCenter.Shift(0f, -7f), default(Color).FromRgb(16763709), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z + 20].DrawSpriteW(_(SpriteName.circle_7), base.WorldCenter.Shift(0f, -8f), Color.White, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.glow), base.WorldCenter.Shift(0f, -8f), Color.Red * 0.8f, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (IsBroken)
		{
			return;
		}
		bool flag = true;
		if (other.IsBroken || other is BoltEntity || other is FragmentEntity || other is PlayerEntity || other is EffectEntity || other is FireballEntity || other is GolemMissile || other is ItemEntity || other is PlatformEntity || other is GrillEntity || (other is SpikesEntity && !(other as SpikesEntity).Active) || other is WebEntity || other is PusherEntity || other is FollowerPadEntity || other is SawRailEntity)
		{
			flag = false;
		}
		if (flag)
		{
			other.Break(this);
			base.core.TimerManager.CreateTimer(20, 1, 1, delegate
			{
				if (base.core.CurrentPlayState?.Player != null)
				{
					foreach (Entity item in base.core.CurrentPlayState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 2f))
					{
						item.Break(this);
					}
				}
			});
			base.core.TimerManager.CreateTimer(40, 1, 1, delegate
			{
				if (base.core.CurrentPlayState?.Player != null)
				{
					foreach (Entity item2 in base.core.CurrentPlayState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 3.5f))
					{
						item2.Break(this);
					}
				}
			});
			IsBroken = true;
			SendMessage(new RemoveEntityMessage(this));
			base.core.CurrentPlayState.Camera.Shake("rik-explosion", 3f, 60);
			SendMessage(new PlayWorldSoundMessage(SoundName.rik_explosion, base.WorldCenter));
			SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates.Shift(-0.06f, -1.8f), "rik_boom_", "123456789ab").SetLayer("fg", -1000, lit: false), null));
			for (int num = 0; num < 8; num++)
			{
				float num2 = (float)num * (float)Math.PI * 2f / 8f;
				float num3 = 1.3f;
				Vector2 vector = new Vector2(Component._cos(num2) * num3, Component._sin(num2) * num3);
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates.Shift(-0.06f, -1.8f) + vector, "rik_boom_", "123456543").SetLayer("fg", -1000 + (int)(vector.Y * 16f), lit: false), null), 20 + Component._rnd(-1, 1));
			}
			for (int num4 = 0; num4 < 16; num4++)
			{
				float num5 = (float)num4 * (float)Math.PI * 2f / 16f;
				float num6 = 2.2f;
				Vector2 vector2 = new Vector2(Component._cos(num5) * num6, Component._sin(num5) * num6);
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates.Shift(-0.06f, -1.8f) + vector2, "rik_boom_", "1234543").SetLayer("fg", -1000 + (int)(vector2.Y * 16f), lit: false), null), 40 + Component._rnd(-1, 1));
			}
			Light obj = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16759608), 7f, 1.1f, this);
			obj.ChangeRate = 0.01f;
			obj.Active = true;
		}
		base.CollideWith(other);
	}

	public override void Update()
	{
		y -= 0.2f;
		TryMoveToCoordinates(base.CurrentMap, base.Coordinates);
		UpdateTiles();
		if (base.Age > 120 || IsBroken)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		base.Update();
	}
}
