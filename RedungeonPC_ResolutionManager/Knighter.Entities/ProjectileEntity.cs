using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ProjectileEntity : Entity
{
	public enum ProjectileType
	{
		Laser,
		Bullet
	}

	private bool dead;

	private Vector2 dir;

	private Sprite sprite;

	private Light light;

	private int killReward;

	private PlayerEntity player;

	public ProjectileType Type { get; private set; }

	public ProjectileEntity(float x, float y, Vector2 dir, ProjectileType type)
		: base(x - 0.1f, y - 0.1f, 0.2f, 0.2f)
	{
		Type = type;
		this.dir = dir;
		SetFlying(value: true);
		switch (Type)
		{
		case ProjectileType.Laser:
			sprite = _(dir.X.IsZero() ? SpriteName.panicbot_laser_v : SpriteName.panicbot_laser_h);
			break;
		case ProjectileType.Bullet:
			sprite = _(SpriteName.bragg_bullet);
			break;
		}
		light = base.core.CurrentPlayState.LightManager.AddLight((Type == ProjectileType.Laser) ? Color.Red : Color.Gold, 2f, 0.7f, this);
		light.Radius = 4f;
		light.FollowRate = 1f;
	}

	public ProjectileEntity SetKillReward(int reward, PlayerEntity player)
	{
		killReward = reward;
		this.player = player;
		return this;
	}

	public override bool CanEnterNullTiles()
	{
		return true;
	}

	public override void Unload()
	{
		light.Die();
		base.Unload();
	}

	public override void Update()
	{
		x += dir.X * 0.5f;
		y += dir.Y * 0.5f;
		if (Math.Abs(base.WorldCoordinates.X - base.core.CurrentPlayState.Player.WorldCoordinates.X) > 6f || Math.Abs(base.WorldCoordinates.Y - base.core.CurrentPlayState.Player.WorldCoordinates.Y) > 10f)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		else
		{
			UpdateTiles();
		}
	}

	public void HitObstacle(Entity target)
	{
		SendMessage(new RemoveEntityMessage(this));
		dead = true;
		if (light != null)
		{
			light.Follow(null);
			light.Intencity = 1.5f;
			light.Die();
		}
		Vector2 worldCenter = base.WorldCenter;
		switch (Type)
		{
		case ProjectileType.Laser:
			base.core.ParticleManager.AddEmitter(inWorld: true, worldCenter, 4f).OnSpawn(delegate
			{
			}).OnUpdate(delegate(Particle p)
			{
				p.Dead = p.Age > 50;
				p.Position.Y -= 0.45f;
			})
				.OnDraw(delegate(Particle p)
				{
					float num = (float)p.Age / 50f;
					float num2 = 2f * (1f - num);
					float num3 = Component._sin((float)Math.PI / 50f) * 20f * (1f - num);
					base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.glow_big), p.Position, Color.Red * num, Vector2.One * (1f - num) * 0.5f, 0f, SpriteFlip.None, SpriteOrigin.Center);
					base.core.Renderer[base.Z].DrawRectangleW(p.Position.Shift((0f - num2) * 0.5f, (0f - num3) * 0.5f), num2, num3, Color.Lerp(Color.Red, Color.White, (1f - num) * (1f - num)));
				})
				.Emit(2, 3, once: true, 2);
			break;
		case ProjectileType.Bullet:
			base.core.ParticleManager.AddEmitter(inWorld: true, worldCenter).OnSpawn(delegate
			{
			}).OnUpdate(delegate(Particle p)
			{
				p.Dead = p.Age > 2;
			})
				.OnDraw(delegate(Particle p)
				{
					base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.bragg_bullet_hit), p.Position, null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				})
				.Emit(1);
			break;
		}
	}

	public override void CollideWith(Entity other)
	{
		if (dead || other is GrillEntity || other is SpikesEntity)
		{
			return;
		}
		bool flag = false;
		if (!(other is PlayerEntity) && !other.IsBroken && !(other is FireballEntity))
		{
			other.Break(this);
			flag = other.IsBroken;
			if (killReward > 0 && (other is BatEntity || other is SlimeEntity || other is FollowerEntity || other is WispEntity || (other is SerpentEntity && !(other as SerpentEntity).IsChineseDragon && (other as SerpentEntity).Part == SerpentEntity.SerpentPart.Head)))
			{
				player.CollectCoins(killReward, other, Color.Orange);
			}
		}
		if (flag || (base.core.CurrentPlayState.Player != null && !other.IsPassableFor(base.core.CurrentPlayState.Player)))
		{
			HitObstacle(other);
		}
		base.CollideWith(other);
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z + ((!(dir.Y < 0f)) ? 1 : 0)].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, -7f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}
}
