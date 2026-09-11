using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FireballEntity : Entity
{
	protected Animation anim;

	protected Light light;

	protected Entity target;

	protected int breakT;

	protected int breakD = 30;

	protected bool breakJump;

	protected Vector2 offset;

	protected float scale = 1f;

	protected int dDepth;

	private ParticleEmitter emitter;

	public int Elevation;

	public Entity Parent { get; protected set; }

	public BallType Type { get; private set; }

	public FireballEntity(Entity parent, float x, float y, BallType type)
		: base(x, y, 0.1f, 0.1f)
	{
		Parent = parent;
		Type = type;
		anim = new Animation();
		switch (Type)
		{
		case BallType.Fire:
			anim.Add("spin", "fireball_", "12345678");
			break;
		case BallType.Zap:
			anim.Add("spin", "zapball_", "123456");
			break;
		}
		anim.Play("spin");
		anim.SkipToRandomFrame();
		offset = Vector2.Zero;
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb((Type == BallType.Fire) ? 16759608 : 6996223), 1.2f, 0.6f, this);
		light.Offset = new Vector2(0f, 0f);
		light.Active = true;
		emitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldPosition).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			int num = Component._rnd(0, 100);
			p.Aux.X = ((num >= 60) ? 1 : 0);
			p.Aux.Z = base.Z;
			p.Aux.Y = Component._rnd(0, 6);
			float num2 = Component._rnd(0f, (float)Math.PI * 2f);
			if (p.Aux.X.IsEqualTo(0f))
			{
				int num3 = Component._rnd(4, 6);
				p.Velocity = p.Position + new Vector2((float)num3 * Component._cos(num2), (float)num3 * Component._sin(num2));
				p.Position += new Vector2(3f * Component._cos(num2), 3f * Component._sin(num2));
			}
			else if (p.Aux.X.IsEqualTo(1f))
			{
				int num4 = Component._rnd(4, 7);
				p.Velocity = p.Position + new Vector2((float)num4 * Component._cos(num2), (float)num4 * Component._sin(num2));
				p.Position += new Vector2(3f * Component._cos(num2), 3f * Component._sin(num2));
			}
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position += (p.Velocity - p.Position) * 0.05f;
				p.Dead = p.Age >= 15;
			})
			.OnDraw(delegate(Particle p)
			{
				int num = (int)(6f * (1f - (float)p.Age / 20f));
				num = (int)Component._M(num, 1f);
				if (Type == BallType.Zap)
				{
					num += (int)p.Aux.Y;
					num %= 6;
					num++;
				}
				base.core.Renderer[(int)p.Aux.Z - 2].DrawSpriteW(_(((Type == BallType.Fire) ? "circle_" : "zap_particle_") + num), p.Position.Shift(0f, -7f), ((Type == BallType.Fire) ? default(Color).FromRgb(16565559) : Color.White) * ((p.Aux.X > 0.5f) ? 0.5f : 1f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Start(80 + Component._rnd(-5, 5))
			.RandomDelay();
		base.Load();
	}

	public override void Update()
	{
		if (IsBroken && breakT < breakD)
		{
			breakT++;
		}
		if (IsBroken && breakT >= breakD)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		anim.Update();
		base.Update();
	}

	public override void Draw()
	{
		if (!IsBroken || target != null)
		{
			Vector2 vector = base.WorldCenter.Shift(0f, -3f + (float)Elevation) + offset;
			if (IsBroken && target != null)
			{
				float num = (float)breakT / (float)breakD;
				base.core.Renderer[base.Z + 30 + dDepth].DrawSpriteW(anim.GetCurrentFrame(), vector + ((target ?? this).WorldCenter - vector) * num, null, new Vector2(0.75f) * (1f - num) * scale, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			else
			{
				base.core.Renderer[base.Z + dDepth].DrawSpriteW(anim.GetCurrentFrame(), vector, null, new Vector2(0.75f * scale), 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			base.Draw();
		}
	}

	public override void Break(Entity offender)
	{
		if (!IsBroken)
		{
			IsBroken = true;
			if (offender is GolemChar || offender is PanicBotChar)
			{
				target = offender;
			}
			else
			{
				target = null;
			}
			emitter.Emit(3, 5, once: true, 2);
			light.TargetIntencity = 0f;
			base.Break(offender);
		}
	}

	public override void CollideWith(Entity other)
	{
		if (IsBroken)
		{
			return;
		}
		if (other is PlayerEntity { FlyingFreely: false, Dead: false } playerEntity)
		{
			playerEntity.Hurt((Type == BallType.Fire) ? InjuryType.Flame : InjuryType.Zap, this);
			if (playerEntity.Dead && Type == BallType.Zap)
			{
				SendMessage(new SpawnEntityMessage(new ZappedEffectEntity(playerEntity), null));
			}
		}
		base.CollideWith(other);
	}
}
