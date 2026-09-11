using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class BoltEntity : Entity
{
	public Entity Owner;

	private readonly int dir;

	private readonly Sprite sprite;

	private bool dead;

	public PlayerEntity Victim { get; set; }

	public BoltEntity(float x, float y, int dir, Entity owner)
		: base(x - 0.05f, y - 0.05f, 0.1f, 0.1f)
	{
		Owner = owner;
		dead = false;
		this.dir = dir;
		sprite = base.core.SpriteManager.GetSprite(SpriteName.crossbow_bolt);
		SetFlying(value: true);
	}

	public override bool CanEnterNullTiles()
	{
		return true;
	}

	public override bool CanTeleport()
	{
		return true;
	}

	public override void Update()
	{
		base.Update();
		x += (float)dir * 0.5f;
		if (Victim != null && !Victim.TryMoveToCoordinates(base.CurrentMap, base.CenterCoordinates.Shift(-0.5f + (float)dir, -0.5f)))
		{
			Victim.TrySpawnFragments(bolt: true);
			Victim = null;
		}
		if (Math.Abs(base.WorldCoordinates.X - base.core.CurrentPlayState.Player.WorldCoordinates.X) > 10f)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		else if (!TryMoveToCoordinates(base.CurrentMap, base.Coordinates))
		{
			HitObstacle();
		}
	}

	public void HitObstacle()
	{
		if (Victim != null)
		{
			Victim.TrySpawnFragments(bolt: true);
			Victim = null;
			SendMessage(new RemoveEntityMessage(Victim));
		}
		SendMessage(new RemoveEntityMessage(this));
		dead = true;
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter.Shift((float)dir * 3f, -6f), 1f).OnSpawn(delegate(Particle p)
		{
			p.Velocity = new Vector2((float)dir * Component._rnd(0.1f, 1f), Component._rnd(-0.15f, 0.15f));
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += p.Velocity;
			p.Velocity += new Vector2(0f, 0.02f);
			p.Dead = p.Age > 20;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z + 20].DrawDotW(p.Position.X, p.Position.Y, Color.Orange * (Component._m(20 - p.Age, 10f) / 10f), 0.5f);
			})
			.Burst(10);
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift((float)(-dir) * 0.5f, 0f), SpriteName.crossbow_bolt, 40, new Vector4((float)(-dir) * Component._rnd(0.05f, 0.1f), Component._rnd(-0.02f, 0.02f), Component._rnd(1f, 2f), (float)(-dir) * Component._rnd(0.05f, 0.3f))), null));
	}

	public override void CollideWith(Entity other)
	{
		if (!dead)
		{
			if (other is PlayerEntity playerEntity)
			{
				playerEntity.Hurt(InjuryType.Bolt, this);
			}
			base.CollideWith(other);
		}
	}

	public override void Draw()
	{
		if (Victim != null)
		{
			if (base.core.TakingScreenshot)
			{
				for (int i = 0; i < 5; i++)
				{
					base.core.Renderer[base.Z - 17].DrawSpriteW(_(Victim.ShotSprite(dir)), base.WorldPosition.Shift(5 * dir - dir * i * 4, -10f), Color.White * (0.3f * (float)(6 - i) / 5f), null, 0f, (dir <= 0) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
				}
			}
			base.core.Renderer[base.Z].DrawSpriteW(_(Victim.ShotSprite(dir)), base.WorldPosition.Shift(5 * dir, -10f), null, null, 0f, (dir <= 0) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
		}
		base.core.Renderer[base.Z - 16].DrawSpriteW(sprite, base.WorldPosition + new Vector2(0f, -5f), null, null, 0f, (dir >= 0) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, 3f), Color.Black * 0.2f, null, 0f, SpriteFlip.Vertical);
		base.Draw();
	}
}
