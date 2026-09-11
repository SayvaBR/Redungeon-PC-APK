using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.States;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class MedusaChar : PlayerEntity
{
	public bool Dodging;

	public bool Dodged = true;

	private bool snakeForm;

	private int reviveT = -1;

	private int reviveD = 100;

	private int deathT;

	private int deathD = 50;

	private ParticleEmitter snakes;

	private int r = 3;

	private Entity target;

	private int newTargetT;

	private int newTargetD = 30;

	private int castingT;

	private int castingD = 60;

	private bool targetLock;

	private Vector2 lastTargetPos;

	private int dexterity;

	private int dodgedCounter;

	public static BagOf<SoundName> RaySounds;

	private bool reviving => reviveT > 0;

	private bool dying => deathT > 0;

	public InjuryType DodgeInjury { get; private set; }

	[Preserve]
	public MedusaChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.095f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "medusa_n_", "1234");
		animation.Add("e", "medusa_e_", "1234");
		animation.Add("w", "medusa_w_", "1234");
		animation.Add("s", "medusa_s_", "1234");
		animation.Add("spin", "medusa_fall_", "1111122222");
		AnimateUTurns = false;
		PosShift = new Vector2(-2.5f, -9f);
		ShadowShift = new Vector2(0f, 3f);
		base.ZappedSprite = SpriteName.zapped_medusa;
		RaySounds = new BagOf<SoundName>().Put(SoundName.medusa_ray_1).Put(SoundName.medusa_ray_2).Put(SoundName.medusa_ray_3)
			.Put(SoundName.medusa_ray_4);
	}

	public override void InitStepSounds()
	{
		StepSounds.Put(SoundName.medusa_step_1);
		StepSounds.Put(SoundName.medusa_step_2);
		StepSounds.Put(SoundName.medusa_step_3);
		StepSounds.Put(SoundName.medusa_step_4);
	}

	public override void Load()
	{
		int num = Abilities.SkillLevel[Skill.Petrification];
		castingD = ((num == 1) ? 80 : 40);
		newTargetD = ((num == 1) ? 50 : 20);
		r = ((num == 1) ? 3 : 4);
		dexterity = Abilities.SkillLevel[Skill.SerpentsDexterity];
		snakes = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Aux.X = Component._rnd(0f, (float)Math.PI * 2f);
			p.Aux.Y = 0f;
			p.Aux.Z = 16 + Component._rnd(-3, 3);
			p.Aux.W = Component._rnd(80, 130);
			p.Offset.X = Component._rnd(0.9f, 1.1f);
			p.Offset.Y = Component._rnd(0.9f, 1.1f);
		})
			.OnUpdate(delegate(Particle p)
			{
				float num2 = 1f;
				MedusaChar medusaChar = p.Parent.HostEntity as MedusaChar;
				if (medusaChar != null && !medusaChar.Dead)
				{
					if (medusaChar.reviveT >= 0)
					{
						num2 = Component._M((float)medusaChar.reviveT / (float)medusaChar.reviveD, 0.1f);
						if (medusaChar.reviveT <= 2)
						{
							p.Dead = true;
						}
					}
				}
				else
				{
					p.Dead = p.Position.LengthSquared() > 40000f;
				}
				p.Velocity = p.Position.Clone();
				p.Aux.X += (float)Math.PI * 2f / p.Aux.W;
				p.Aux.Y += (num2 * p.Aux.Z - p.Aux.Y) * 0.1f;
				if (medusaChar != null && !medusaChar.Dead)
				{
					p.Position = new Vector2(Component._cos(p.Aux.X * p.Offset.X), Component._sin(p.Aux.X * p.Offset.Y)) * (p.Aux.Y + Component._sin((float)p.Age * num2 * p.Aux.Z * 0.01f) * (p.Aux.Z / 20f));
				}
				else
				{
					p.Position *= 1.15f;
				}
				p.Velocity = p.Position - p.Velocity;
			})
			.OnDraw(delegate(Particle p)
			{
				MedusaChar medusaChar = p.Parent.HostEntity as MedusaChar;
				base.core.Renderer["fg", 1, false].DrawSpriteW(_("medusa_snake_" + ((int)((float)p.Age * 0.2f) % 4 + 1)), p.Parent.Position.Shift(0f, -10f) + p.Position + (medusaChar?.dAnim ?? Vector2.Zero), null, rotation: 0f - (float)Math.Atan2(p.Velocity.X, p.Velocity.Y) + (float)Math.PI, scale: Vector2.One * (0.8f + 0.1f * Component._sin((float)p.Age * p.Aux.Z * 0.005f)), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			})
			.Max(10);
		base.Load();
	}

	public override void InitAbilities(Abilities abilities)
	{
		base.InitAbilities(abilities);
	}

	public override bool SpawnFallFragments()
	{
		return true;
	}

	public override void CollideWith(Entity other)
	{
		base.CollideWith(other);
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		if (snakeForm || reviving || dying)
		{
			return true;
		}
		if (dexterity > 0)
		{
			DodgeInjury = injuryType;
			Dodge();
			return true;
		}
		return base.TryResist(injuryType, offender);
	}

	protected override bool TryResistFall()
	{
		if (snakeForm || reviving || dying)
		{
			return true;
		}
		if (dexterity > 0)
		{
			DodgeInjury = InjuryType.Fall;
			Dodge();
			return true;
		}
		return base.TryResistFall();
	}

	public override bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		if (snakeForm || reviving || dying)
		{
			return true;
		}
		return base.TryResistSpell(spellType, offender);
	}

	private void Dodge()
	{
		DeactivateSpellEffects();
		if (!Dodging)
		{
			if (base.HoldingWeb != null)
			{
				base.HoldingWeb.ReleasePlayer();
			}
			snakes.Start(5, 5);
			Dodging = true;
			Dodged = false;
			snakeForm = true;
			FlightTarget = null;
			base.playState.SloMoFactor = 0.5f;
			base.playState.SloMoAffectsPlayer = false;
			base.playState.SloMo = true;
			SendMessage(new PushStateMessage(new DodgeState(this, dodgedCounter)));
		}
	}

	public void DodgeAftermath()
	{
		base.playState.SloMo = false;
		base.playState.PreventDeathScreenshots = false;
		Dodging = false;
		if (Dodged)
		{
			Vector2 vector = base.core.CurrentPlayState.LevelGenerator.NextSafePoint(base.WorldCoordinates);
			int num = (int)(vector.X - base.WorldCoordinates.X);
			int num2 = (int)(vector.Y - base.WorldCoordinates.Y);
			SuspendedStartFlying(num, num2, 0.03f, ignoreObstacles: true, changeCourse: true, reviveD);
			FacingDirection = new Vector2(0f, 1f);
			reviveT = reviveD;
			dodgedCounter++;
		}
		else
		{
			deathT = deathD;
		}
	}

	protected override void OnReachTarget()
	{
		if (snakeForm && !Dodging)
		{
			snakeForm = false;
			snakes.Pause();
			Dodging = false;
			reviveT = -1;
		}
		base.OnReachTarget();
	}

	public override void TryTriggerAbility()
	{
		if (!base.Falling)
		{
			base.TryTriggerAbility();
		}
	}

	public override bool Paralized()
	{
		if (!snakeForm)
		{
			return base.Paralized();
		}
		return true;
	}

	public override void Update()
	{
		if (reviveT >= 0)
		{
			reviveT -= 2;
		}
		if (deathT > 0)
		{
			deathT--;
			if (deathT == 0)
			{
				snakes.Pause();
				Die(DodgeInjury, 60);
			}
		}
		CheckTarget();
		if (target == null)
		{
			if (newTargetT > 0)
			{
				newTargetT--;
			}
			if (newTargetT == 0)
			{
				FindTarget();
			}
		}
		else
		{
			if (castingT > 0)
			{
				castingT--;
			}
			targetLock = castingT < 18;
			if (castingT == 1)
			{
				lastTargetPos = target.WorldCenter;
			}
			if (castingT == 0)
			{
				if (target is SerpentEntity serpentEntity)
				{
					serpentEntity.Head().Break(this);
				}
				else
				{
					SendMessage(new SpawnEntityMessage(new PetrifiedEntity(target, lastTargetPos), target.CurrentPlatform));
				}
				newTargetT = newTargetD;
				target = null;
				targetLock = false;
			}
			if (castingT == castingD - 1)
			{
				SendMessage(new PlayWorldSoundMessage(RaySounds.DrawDifferent(), base.WorldCenter));
			}
			if (castingT == 17)
			{
				SendMessage(new PlayWorldSoundMessage(PetrifiedEntity.PetrificationSounds.DrawDifferent(), target.WorldCenter));
			}
		}
		base.Update();
	}

	private void FindTarget()
	{
		if (base.playState.Started)
		{
			List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, r).FindAll((Entity e) => !e.Unloaded && (e is SlimeEntity || e is BatEntity || (e is FollowerEntity && (e as FollowerEntity).Awake && !(e as FollowerEntity).Important) || e is WispEntity || (e is SerpentEntity && !(e as SerpentEntity).IsChineseDragon)));
			if (list.Count > 0)
			{
				target = list[Component._rnd(0, list.Count - 1)];
				castingT = castingD;
			}
		}
	}

	private bool TargetWithinReach(Entity t)
	{
		return (t.WorldCenterCoordinates - base.WorldCenterCoordinates).LengthSquared() <= ((float)r + 0.5f) * ((float)r + 0.5f);
	}

	private void CheckTarget()
	{
		if (target == null)
		{
			return;
		}
		bool flag = snakeForm;
		flag |= target.IsBroken || target.Unloaded;
		flag |= target is FollowerEntity && !(target as FollowerEntity).Awake;
		bool flag2 = !targetLock && !TargetWithinReach(target);
		if (!(flag || flag2))
		{
			return;
		}
		if (!flag && target is SerpentEntity serpentEntity)
		{
			for (SerpentEntity serpentEntity2 = serpentEntity.Head(); serpentEntity2 != null; serpentEntity2 = serpentEntity2.Next)
			{
				if (TargetWithinReach(serpentEntity2))
				{
					target = serpentEntity2;
					return;
				}
			}
		}
		target = null;
		newTargetT = newTargetD;
		targetLock = false;
	}

	protected override void UpdateAbilities()
	{
		base.UpdateAbilities();
	}

	public override void UpdateInSloMo()
	{
		base.UpdateInSloMo();
	}

	public override void Draw()
	{
		if (snakeForm)
		{
			if (reviving && reviveT < 40)
			{
				Sprite sprite = _("medusa_collect_" + (int)Component._M(1f, 3 - (int)((float)reviveT / 13.333333f)));
				base.core.Renderer["fg", -4, false].DrawSpriteW(sprite, base.WorldPosition + base.dAnim + PosShift);
			}
			return;
		}
		if (target != null)
		{
			Vector2 v = target.WorldCenter - base.WorldCenter - base.dAnim;
			Vector2 vector = v.Clone();
			vector.Normalize();
			int num = 15;
			vector *= v.Length() / (float)num;
			float num2 = ((castingT < 10) ? ((float)castingT / 10f) : ((castingT > castingD - 10) ? ((float)(castingD - castingT) / 10f) : 1f));
			for (int i = 0; i < num; i++)
			{
				Vector2 v2 = base.WorldCenter + base.dAnim + vector * (i + 1);
				base.core.Renderer[(int)Math.Round(v2.Y) + 1].DrawSpriteW(_(SpriteName.medusa_spiral), v2.Shift(0f, -3f), Color.White * (0.4f + 0.15f * Component._sin((float)base.worldTicks * 0.2f - (float)(i * 20))), rotation: (float)base.worldTicks * 0.02f + (float)(i * 3), scale: new Vector2(0.8f + 0.2f * Component._sin((float)base.worldTicks * 0.2f - (float)(i * 20))) * num2, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			}
			if (targetLock)
			{
				int num3 = 7 - (castingT / 3 + 1);
				base.core.Renderer[(int)Math.Round(target.WorldCoordinates.Y) + 10].DrawSpriteW(_("petri_" + num3), target.WorldCenter, null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		base.Draw();
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		if (!bolt)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.medusa_death, base.WorldCenter));
			base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 10f).AttachTo(this).OnSpawn(delegate(Particle p)
			{
				p.Offset.Normalize();
				p.Aux.X = Component._rnd(0.7f, 1.4f) * 1.5f;
				p.Aux.Y = Component._rnd(1, 4);
			})
				.OnUpdate(delegate(Particle p)
				{
					p.Velocity = p.Position.Clone();
					p.Position += p.Offset * p.Aux.X;
					p.Velocity = p.Position - p.Velocity;
				})
				.OnDraw(delegate(Particle p)
				{
					float num = Component._m((float)p.Age / 30f, 0.5f) + 0.5f;
					base.core.Renderer["fg", -5, false].DrawSpriteW(_("medusa_snake_" + ((int)((float)p.Age * 0.2f + p.Aux.Y) % 4 + 1)), p.Position, null, rotation: 0f - (float)Math.Atan2(p.Velocity.X, p.Velocity.Y) + (float)Math.PI, scale: Vector2.One * num, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
				})
				.Burst(15);
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.medusa_shot;
	}
}
