using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class GolemChar : PlayerEntity
{
	private Animation burnAnim;

	private Animation effectAnim;

	private Animation glowAnim;

	private Animation deflectAnim;

	private bool grillOn;

	private bool grillBurn;

	private GrillEntity grill;

	private bool shieldUnlocked;

	private int maxFireballs;

	private int fireballs;

	private int fireballsToAdd;

	private Light light;

	private bool shotReady;

	private bool shield;

	private int castT = -1;

	private int castD = 50;

	private bool casting => castT >= 0;

	[Preserve]
	public GolemChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.095f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "rik_n_", "1234");
		animation.Add("e", "rik_e_", "1234");
		animation.Add("w", "rik_w_", "1234");
		animation.Add("s", "rik_s_", "1234");
		animation.Add("spin", "rik_fall_", "1111122222");
		burnAnim = new Animation();
		burnAnim.Add("burn", "rik_burn_", "1213");
		burnAnim.Add("ignite", "rik_ignite_", "123");
		burnAnim.Add("fade", "rik_burn_fade_", "123");
		burnAnim.Play("ignite");
		burnAnim.Pause();
		effectAnim = new Animation();
		effectAnim.Add("effect", "rik_effect_", "1234567");
		effectAnim.Loop = false;
		effectAnim.Play("effect");
		effectAnim.Pause();
		deflectAnim = new Animation();
		deflectAnim.Add("deflect", "rik_deflect_", "12345678");
		deflectAnim.Loop = false;
		deflectAnim.Play("deflect");
		deflectAnim.Pause();
		glowAnim = new Animation(0.3f);
		glowAnim.Add("glow", "rik_glow_big_", "12345");
		glowAnim.Play("glow");
		AnimateUTurns = false;
		PosShift = new Vector2(-2.5f, -8.5f);
		ShadowShift = new Vector2(0f, 3f);
		base.ZappedSprite = SpriteName.zapped_rik;
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16759608), 2f, 0.2f, this);
		light.Radius = 3f;
		light.FollowRate = 0.8f;
		light.ChangeRate = 0.05f;
		light.Active = true;
	}

	public override void InitStepSounds()
	{
		StepSounds.Put(SoundName.rik_step_1);
		StepSounds.Put(SoundName.rik_step_2);
		StepSounds.Put(SoundName.rik_step_3);
		StepSounds.Put(SoundName.rik_step_4);
	}

	public override void Load()
	{
		shieldUnlocked = base.core.ProfileData.CurrentCharLevel > 1;
		maxFireballs = 10;
		fireballs = ((Abilities.SkillLevel[Skill.BetterFireShield] > 0) ? maxFireballs : 0);
		shotReady = false;
		shield = false;
		base.Load();
	}

	public override void InitAbilities(Abilities abilities)
	{
		base.InitAbilities(abilities);
	}

	public override void CollideWith(Entity other)
	{
		if (other is GrillEntity)
		{
			grill = (GrillEntity)other;
		}
		base.CollideWith(other);
	}

	public override void UnCollideWith(Entity other)
	{
		if (other == grill)
		{
			grill = null;
		}
		base.UnCollideWith(other);
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		bool flag = base.TryResist(injuryType, offender) || injuryType == InjuryType.Flame;
		if (injuryType == InjuryType.Flame && offender is WispEntity)
		{
			fireballsToAdd++;
		}
		if (((!flag && shield) || injuryType == InjuryType.Crushed) && (injuryType == InjuryType.Sword || injuryType == InjuryType.Spikes || injuryType == InjuryType.Bolt || injuryType == InjuryType.Saw || injuryType == InjuryType.Bat || injuryType == InjuryType.Slime || injuryType == InjuryType.Axe || injuryType == InjuryType.Follower || injuryType == InjuryType.Crushed))
		{
			flag = true;
			if (injuryType != InjuryType.Crushed)
			{
				deflectAnim.Reset();
				deflectAnim.Play();
				fireballs = 0;
				SendMessage(new PlayWorldSoundMessage(SoundName.rik_shield_break, base.WorldCenter));
			}
			if (injuryType == InjuryType.Bat || injuryType == InjuryType.Slime || injuryType == InjuryType.Follower)
			{
				SendMessage(new SpawnEntityMessage(new EffectEntity(offender.WorldCoordinates, "flame_death_", "1234567"), null));
				base.core.ParticleManager.AddSmoke(offender.WorldCoordinates, offender.Z);
			}
			base.playState.Camera.Shake("shield");
			switch (injuryType)
			{
			case InjuryType.Bolt:
				(offender as BoltEntity).HitObstacle();
				break;
			case InjuryType.Sword:
				(offender as RotobladeEntity).Break(this);
				break;
			case InjuryType.Spikes:
				(offender as SpikesEntity).Break(this);
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
			case InjuryType.Crushed:
				offender.Break(this);
				(base.playState.TileMap[base.WorldTile.Coordinates + LastMovementDir]?.Entities.Find((Entity e) => e is PistonEntity && !e.IsBroken))?.Break(this);
				break;
			}
		}
		return flag;
	}

	public override bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		if (spellType == SpellType.Ice)
		{
			return true;
		}
		return base.TryResistSpell(spellType, offender);
	}

	public override void TryTriggerAbility()
	{
		if (!base.Falling && fireballs >= maxFireballs)
		{
			fireballs = 0;
			castT = 0;
			FacingDirection = new Vector2(0f, -1f);
			SendMessage(new PlayWorldSoundMessage(SoundName.rik_shoot, base.WorldCenter));
			base.SpellEffects[SpellType.Ice].Deactivate();
			base.TryTriggerAbility();
		}
	}

	public override bool Paralized()
	{
		if (castT >= 20)
		{
			castT = -1;
		}
		if (!base.Paralized())
		{
			if (casting)
			{
				return castT < 20;
			}
			return false;
		}
		return true;
	}

	public override void Update()
	{
		if (castT >= 0)
		{
			castT++;
			if (castT == 10)
			{
				SendMessage(new SpawnEntityMessage(new GolemMissile(base.WorldCenterCoordinates.X, base.WorldCenterCoordinates.Y - 0.3f), null));
			}
			if (castT >= castD)
			{
				castT = -1;
			}
		}
		if (!effectAnim.Paused)
		{
			effectAnim.Update();
		}
		if (!burnAnim.Paused)
		{
			burnAnim.Update();
		}
		if (!deflectAnim.Paused)
		{
			deflectAnim.Update();
		}
		glowAnim.Update();
		grillOn = grill != null && grill.Active;
		light.TargetIntencity = (shield ? (1f + 0.2f * Component._sin((float)base.Age * 0.02f)) : (grillOn ? 0f : 0.5f));
		light.Position = base.WorldCenter + base.dAnim;
		if (grillOn && !grillBurn)
		{
			grillBurn = true;
			burnAnim.Play("ignite");
			burnAnim.Reset();
			burnAnim.Loop = false;
		}
		if (!grillOn && grillBurn)
		{
			burnAnim.Play("fade");
			burnAnim.Reset();
			burnAnim.Loop = false;
			grillBurn = false;
		}
		if (burnAnim.JustStopped && burnAnim.CurrentSequence == "ignite")
		{
			burnAnim.Play("burn");
			burnAnim.Reset();
			burnAnim.Loop = true;
		}
		base.Update();
	}

	public override void Draw()
	{
		DrawCustomHUD();
		if (!casting)
		{
			base.Draw();
		}
		else
		{
			int num = castT / 5 + 1;
			if (num > 4)
			{
				num = 4;
			}
			base.core.Renderer[base.Z].DrawSpriteW(_("rik_cast_" + num), base.WorldPosition.Shift(-11f, -21f) + (BSlide.Sliding ? base.dAnim : Vector2.Zero));
		}
		if (!effectAnim.Paused)
		{
			base.core.Renderer[base.Z + 1].DrawSpriteW(effectAnim.GetCurrentFrame(), LastSpritePos.Shift(-3f, -9f));
		}
		if (!burnAnim.Paused)
		{
			base.core.Renderer[base.Z + 1].DrawSpriteW(burnAnim.GetCurrentFrame(), LastSpritePos.Shift(-3f, -9f));
		}
		if (shield && !base.Falling)
		{
			base.core.Renderer[base.Z + 1].DrawSpriteW(glowAnim.GetCurrentFrame(), LastSpritePos.Shift(-3f, -9f));
		}
		if (!deflectAnim.Paused)
		{
			base.core.Renderer[base.Z + 1].DrawSpriteW(deflectAnim.GetCurrentFrame(), LastSpritePos.Shift(-3f, -9f));
		}
	}

	private void DrawCustomHUD()
	{
		if (base.core.TakingScreenshot || !base.playState.Started || base.core.ProfileData.CurrentCharLevel < 2)
		{
			return;
		}
		float num = (float)Tween.CircEaseOut(base.core.CurrentPlayState.Trans, 60.0, -60.0, base.core.CurrentPlayState.TransDuration);
		if (base.core.OptionsData.LeftHandedMode)
		{
			num *= -1f;
		}
		Vector2 v = base.playState.PlayerControl.SkillButtonCenter().Shift(num, -30f);
		default(Color).FromRgb(16777215);
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				int num2 = j * 5 + i + 1;
				Vector2 position = v.Shift(-19 - 6 * j, -25 - 6 * i) / Settings.GuiScale;
				bool flag = fireballs >= num2;
				base.core.Renderer["fg", -5000, false].DrawSpriteS(_(SpriteName.circle_4), position, flag ? Color.White : (Color.White * 0.3f), Vector2.One / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
				if (flag)
				{
					base.core.Renderer["fg", -5001, false].DrawSpriteS(_(SpriteName.glow), position, default(Color).FromRgb(16759608) * 0.8f, new Vector2(1.1f) / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
			}
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlayWorldSoundMessage(SoundName.rik_death, base.WorldCenter));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_rock_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_rock_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_rock_3), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_rock_4), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_head), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_arm), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_arm), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_foot), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rik_foot), null));
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rik_rock_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rik_rock_3), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rik_rock_4), null));
		return true;
	}

	protected override void UpdateAbilities()
	{
		if (base.playState.Started)
		{
			List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 0.8f).FindAll((Entity e) => e is FireballEntity && !((FireballEntity)e).IsBroken && ((FireballEntity)e).Type == BallType.Fire);
			foreach (Entity item in list)
			{
				(item as FireballEntity).Break(this);
				if (fireballs < maxFireballs && shieldUnlocked)
				{
					fireballs++;
				}
				_inc(Stat.RikFireballsCollected);
			}
			if ((list.Count > 0 && effectAnim.Paused) || fireballsToAdd > 0)
			{
				if (fireballsToAdd > 0)
				{
					fireballs += fireballsToAdd;
					if (fireballs > maxFireballs)
					{
						fireballs = maxFireballs;
					}
					fireballsToAdd = 0;
				}
				effectAnim.Reset();
				effectAnim.Play("effect");
				light.Intencity = 1f;
				if (fireballs < maxFireballs)
				{
					SendMessage(new PlayWorldSoundMessage(SoundName.rik_consume_fire, base.WorldCenter));
				}
			}
		}
		shotReady = fireballs >= maxFireballs;
		bool flag = shield;
		shield = shotReady && shieldUnlocked;
		if (shield && !flag)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.rik_shield_charged, base.WorldCenter));
		}
		if (Abilities != null && Abilities.SkillLevel[Skill.Blaze] > 0)
		{
			Abilities.SkillCharge[Skill.Blaze] = Component._m((float)fireballs / (float)maxFireballs, 1f);
		}
		base.UpdateAbilities();
	}

	public override void ResetAbilities(bool refill = false)
	{
		if (refill)
		{
			fireballs = maxFireballs;
		}
		base.ResetAbilities(refill);
	}
}
