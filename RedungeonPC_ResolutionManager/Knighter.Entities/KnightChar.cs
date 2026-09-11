using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class KnightChar : PlayerEntity
{
	private Sprite sword;

	private int attackTimer = -1;

	private const int attackDuration = 40;

	[Preserve]
	public KnightChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.095f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "knight_n", "1234");
		animation.Add("e", "knight_e", "1234");
		animation.Add("w", "knight_w", "1234");
		animation.Add("s", "knight_s", "1234");
		animation.Add("spin", "knight_fall_", "1111122222");
		AnimateUTurns = false;
		sword = _(SpriteName.knight_sword_big);
		PosShift = new Vector2(-1.5f, -7f);
		ShadowShift = new Vector2(0f, 3f);
	}

	public override void InitStepSounds()
	{
		StepSounds.Put(SoundName.knight_step_1);
		StepSounds.Put(SoundName.knight_step_2);
	}

	public override bool SpawnFallFragments()
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0.2f, -0.2f), SpriteName.knight_shield, -1, new Vector4(0.07f, 0f, 1.8f, 0.2f)), null), 2);
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(-0.2f, -0.2f), SpriteName.knight_sword, -1, new Vector4(-0.07f, 0f, 1.8f, -0.4f)), null), 2);
		return true;
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		bool flag = Abilities.SkillLevel[Skill.Shield] > 0 && (injuryType == InjuryType.Sword || injuryType == InjuryType.Spikes || injuryType == InjuryType.Bolt || injuryType == InjuryType.Saw || injuryType == InjuryType.Bat || injuryType == InjuryType.Slime || injuryType == InjuryType.Axe || injuryType == InjuryType.Follower || injuryType == InjuryType.Serpent);
		if (flag)
		{
			_inc(Stat.KnightDeflectedWithShield);
			base.playState.Camera.Shake("shield");
			Rumble.Pulse("shield", 0.2f, 0.35f, 6);
			Abilities.SkillLevel[Skill.Shield]--;
			SendMessage(new PlayWorldSoundMessage(SoundName.knight_shield, base.WorldCenter));
			switch (injuryType)
			{
			case InjuryType.Bolt:
				(offender as BoltEntity).HitObstacle();
				break;
			case InjuryType.Sword:
				(offender as RotobladeEntity).Break(this);
				break;
			case InjuryType.Spikes:
				offender.Break(this);
				break;
			case InjuryType.Saw:
				(offender as SawEntity).Break(this);
				break;
			case InjuryType.Bat:
				(offender as BatEntity).Break(this);
				break;
			case InjuryType.Slime:
				(offender as SlimeEntity).Break(this);
				break;
			case InjuryType.Axe:
				offender.Break(this);
				break;
			case InjuryType.Follower:
				offender.Break(this);
				break;
			case InjuryType.Serpent:
				offender.Break(this);
				break;
			}
			bool lastShield = Abilities.SkillLevel[Skill.Shield] == 0;
			base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter).OnUpdate(delegate(Particle p)
			{
				p.Dead = p.Age == 30;
			}).OnDraw(delegate(Particle p)
			{
				float num = (30f - (float)p.Age) / 30f;
				float num2 = ((p.Age < 15) ? 0f : ((float)(p.Age - 15) / 15f));
				float num3 = Component._sin(num * (float)Math.PI);
				if (lastShield)
				{
					base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.shield_glow_piece_1), p.Position.Shift(-5f * num2, -3f - 7f * num2), Color.Lerp(Color.White, default(Color).FromRgb(15902269), 1f - num3 * num3 * num3) * num3 * 0.7f, Vector2.One * 1.2f, 0f, SpriteFlip.None, SpriteOrigin.Center);
					base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.shield_glow_piece_2), p.Position.Shift(-5f * num2, -3f + 7f * num2), Color.Lerp(Color.White, default(Color).FromRgb(15902269), 1f - num3 * num3 * num3) * num3 * 0.7f, Vector2.One * 1.2f, 0f, SpriteFlip.None, SpriteOrigin.Center);
					base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.shield_glow_piece_3), p.Position.Shift(7f * num2, -3f), Color.Lerp(Color.White, default(Color).FromRgb(15902269), 1f - num3 * num3 * num3) * num3 * 0.7f, Vector2.One * 1.2f, 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
				else
				{
					base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.shield_glow), p.Position.Shift(19f - 40f * num, -3f), Color.Lerp(Color.White, default(Color).FromRgb(15902269), 1f - num3 * num3 * num3) * num3 * 0.7f, new Vector2(num3, 1f) * 1.2f, 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
				base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.glow_big), p.Position.Shift(0f, -3f), Color.Lerp(Color.White, default(Color).FromRgb(15902269), 1f - num3 * num3 * num3) * num3 * 0.7f, new Vector2(num3, 1f) * 1.2f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
				.Emit(1);
			AnnounceAbilityStatus(Abilities.SkillDesc[Skill.Shield], Abilities.SkillLevel[Skill.Shield]);
		}
		return base.TryResist(injuryType, offender) || flag;
	}

	public override void TryTriggerAbility()
	{
		if (!base.Falling)
		{
			Abilities.SkillCharge[Skill.Thrust] = 0f;
			attackTimer = 0;
			base.playState.Camera.Shake("thrust");
			Rumble.Pulse("thrust", 0.15f, 0.5f, 6);
			if (base.HoldingWeb != null)
			{
				base.HoldingWeb.ReleasePlayer();
			}
			base.SpellEffects[SpellType.Ice].Deactivate();
			SendMessage(new PlayWorldSoundMessage(SoundName.gylbard_sword, base.WorldPosition));
			base.TryTriggerAbility();
		}
	}

	public override bool Paralized()
	{
		if (!base.Paralized())
		{
			return attackTimer >= 0;
		}
		return true;
	}

	public override void Update()
	{
		if (attackTimer >= 0 && attackTimer < 40)
		{
			attackTimer++;
			if (attackTimer == 1)
			{
				base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter).OnUpdate(delegate(Particle p)
				{
					p.Position = base.WorldCenter;
					p.Dead = p.Age == 30;
				}).OnDraw(delegate(Particle p)
				{
					float num2 = (30f - (float)p.Age) / 30f;
					base.core.Renderer["fg", -2, false].DrawSpriteW(_(SpriteName.knight_sword_big), p.Position.Shift(1f, -9f), Color.White * num2 * 2f, new Vector2(0.5f + (1f - num2) * 1.6f), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
				})
					.Emit(1);
			}
			if (attackTimer < 10)
			{
				List<Entity> list = base.core.CurrentPlayState.EntityManager.FindEntities((Entity e) => !e.IsBroken && (e is RotobladeEntity || e is SpikesEntity || e is CrossbowEntity || e is SawEntity || e is PistonEntity || e is PistonCoreEntity || e is BatEntity || e is SlimeEntity || e is PotEntity || e is ZapperEntity || e is ObstacleEntity || e is ChestEntity || e is StatueEntity || e is StatueEntity.StatueHitbox || e is FollowerEntity || e is FirewallEntity || e is CannonEntity || e is WispEntity || (e is SerpentEntity && !(e as SerpentEntity).IsChineseDragon)) && SwordReaches(e));
				int num = 0;
				foreach (Entity item in list)
				{
					if (item is ChestEntity)
					{
						item.Break(this);
						continue;
					}
					item.Break(this);
					if (item.IsBroken && !item.GrantsEnemyCoins)
					{
						ItemEntity itemEntity = new ItemEntity(item.WorldCenterCoordinates.X - 0.5f, item.WorldCenterCoordinates.Y - 0.5f, ItemEntity.ValueToType(base.playState.LevelGenerator.AvgCoinValue()));
						itemEntity.SetTarget(this, 40 + num * 15);
						SendMessage(new SpawnEntityMessage(itemEntity, null));
						num++;
					}
				}
			}
			if (attackTimer == 40)
			{
				attackTimer = -1;
				FacingDirection = new Vector2(0f, -1f);
			}
		}
		base.Update();
	}

	private bool SwordReaches(Entity entity)
	{
		for (int distance = 1; distance <= 5; distance++)
		{
			var tile = levelMap[WorldCoordinates.Shift(0f, -distance)];
			if (tile != null && (entity.OccupiedTiles.Contains(tile) || entity.OccupiedWorldTiles.Contains(tile))) return true;
		}
		return false;
	}

	public override void Draw()
	{
		if (attackTimer >= 0)
		{
			float num = Component._m(Component._sin((float)attackTimer * (float)Math.PI / 40f) * 2f, 1f);
			base.core.Renderer["fg", -2, false].FillScreen(Color.Black * num * 0.22f);
			Sprite sprite = _(SpriteName.knight_n6);
			Vector2 vector = base.WorldPosition.Shift(0f, -7f);
			base.core.Renderer["fg", -1, false].DrawSpriteW(sprite, vector + PosShift);
			base.core.Renderer["fg", -2, false].DrawSpriteW(sprite, vector.Shift(0f, 12f) + PosShift + base.dAnim, Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
			base.core.Renderer["fg", -2, false].DrawSpriteW(sword, vector.Shift(9f, 3f) + PosShift, null, new Vector2(0.8f, 1.25f), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		else
		{
			base.Draw();
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlayWorldSoundMessage(SoundName.spikes_break, base.WorldPosition));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.knight_helmet), null));
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.knight_shield), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.knight_sword), null));
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.knight_shield), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.knight_sword), null));
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.knight_shot;
	}
}
