using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.States;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public abstract class PlayerEntity : Entity, IPushableEntity
{
	public Vector2 FacingDirection;

	protected Vector2 prevDirection;

	protected Animation animation;

	protected Animation animationWalk;

	protected bool customAnimation;

	protected float normalAnimSpeed;

	protected bool AnimateUTurns = true;

	protected bool AnimPaused;

	protected bool Lit;

	protected int uTurn;

	protected Sprite uTurnSprite;

	protected int fallAnim;

	private bool animateFall;

	protected Tile tileBeforeJump;

	protected int lastJumpTick;

	protected float dropAnim;

	private bool shopDrop;

	public Vector2 PosShift;

	public Vector2 ShadowShift;

	public bool Dead;

	public Vector2 LastSpriteShift;

	public Vector2 LastSpritePos;

	public float LastSpriteAlpha;

	public int LastZ;

	public string LastLayer = "default";

	public Color LastTint;

	private ParticleEmitter poisonEmitter;

	private ParticleEmitter frostEmitter;

	private bool wasPoisoned;

	private bool wasFrosted;
	private int frostTail;
	private int poisonTail;
	public float FrostScreen { get; private set; }
	public float FrostVisual => SpellEffects[SpellType.Ice].Active ? 1f : frostTail / 300f;
	public float PoisonVisual => SpellEffects[SpellType.Poison].Active ? 1f : poisonTail / 300f;

	protected int hurtTtl;

	public Abilities Abilities;

	protected Light MainLight;

	private bool spawnedFragments;

	public BagOf<SoundName> StepSounds;

	private int burnT = -1;

	private int burnD = 30;

	private Animation burnAnim;

	private bool madeBurningShot;

	protected float burnOpacity = 1f;

	public bool Falling { get; private set; }

	protected PlayState playState => base.core.CurrentPlayState;

	public WebEntity HoldingWeb { get; private set; }

	public bool TrappedInWeb => HoldingWeb != null;

	public SpriteName ZappedSprite { get; protected set; }

	public bool Burning => burnT >= 0;

	public Dictionary<SpellType, SpellEffect> SpellEffects => playState.SpellEffects;

	public Animation Animation => animation;

	protected PlayerEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
		FacingDirection = new Vector2(0f, 1f);
		Falling = false;
		padding = 0.2f;
		gridAligned = true;
		PosShift = Vector2.Zero;
		ShadowShift = Vector2.Zero;
		ZappedSprite = SpriteName.zapped_knight;
		burnAnim = new Animation(0.2f, loop: false);
		burnAnim.Add("burn", "flame_death_", "1234567");
		burnAnim.Play("burn");
		InitSlideBehavior();
		BSlide.SlowLanding = true;
	}

	public override bool SlidingDisabled()
	{
		return Dead;
	}

	public override void Load()
	{
		MainLight = playState.LightManager.AddLight(Color.Khaki, 10f, 0.8f, this);
		MainLight.ChangeRate = 0.05f;
		MainLight.FollowRate = 0.1f;
		MainLight.Position = base.WorldCenter;
		MainLight.Intencity = 0f;
		StepSounds = new BagOf<SoundName>();
		InitStepSounds();
		poisonEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldPosition).AttachTo(this)
			.OnSpawn(delegate(Particle p)
			{
				float num2 = Component._rnd(0f, (float)Math.PI * 2f);
				float num3 = Component._rnd(6, 8);
				p.Offset = base.WorldCenter;
				p.Velocity = p.Position + new Vector2(Component._cos(num2), Component._sin(num2)) * num3;
				p.Position += new Vector2(Component._cos(num2), Component._sin(num2)) * 3f;
				p.Offset += new Vector2(Component._cos(num2), Component._sin(num2)) * 3f;
			})
			.OnUpdate(delegate(Particle p)
			{
				p.Offset = base.WorldCenter;
				p.Position += (p.Velocity - p.Position) * ((p.Age < 5) ? 0.15f : 0.015f);
				p.Dead = p.Age >= 50;
				if (p.Age >= 5)
				{
					p.Velocity = p.Offset;
				}
			})
			.OnDraw(delegate(Particle p)
			{
				int num = (int)Component._M((int)(6f * (1f - (float)p.Age / 50f)), 1f);
				base.core.Renderer[base.Z - 2].DrawSpriteW(_("wisp_particle_bubble_" + num), p.Position.Shift(0f, -7f), Color.White * PoisonVisual, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Max(10);
		frostEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldPosition).AttachTo(this)
			.OnSpawn(delegate(Particle p)
			{
				float num4 = Component._rnd(0f, (float)Math.PI * 2f);
				float num5 = Component._rnd(4, 7);
				p.Offset = base.WorldCenter;
				p.Velocity = new Vector2(Component._cos(num4) * num5 * 0.3f, 0f - num5);
				p.Position += new Vector2(Component._cos(num4), Component._sin(num4)) * 4f;
			})
			.OnUpdate(delegate(Particle p)
			{
				p.Velocity += new Vector2(Component._sin((float)p.Age * 0.15f) * 0.03f, 0.02f);
				p.Position += p.Velocity * 0.05f;
				p.Dead = p.Age >= 60;
			})
			.OnDraw(delegate(Particle p)
			{
				int num6 = (int)Component._M((int)(6f * (1f - (float)p.Age / 60f)), 1f);
				base.core.Renderer[base.Z - 2].DrawSpriteW(_("wisp_particle_snow_" + num6), p.Position, Color.White * (1f - (float)p.Age / 60f) * FrostVisual, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Max(10);
		base.Load();
	}

	public virtual SpriteName ShotSprite(int dir)
	{
		return SpriteName.pixel;
	}

	public virtual void InitStepSounds()
	{
		StepSounds.Put(SoundName.step_1);
		StepSounds.Put(SoundName.step_2);
	}

	public virtual void InitAbilities(Abilities abilities)
	{
		if (abilities == null)
		{
			ResetAbilities();
		}
		else
		{
			Abilities = abilities;
		}
	}

	public virtual void ResetAbilities(bool refill = false)
	{
		Abilities = base.core.CurrentCharDesc.Levels[base.core.ProfileData.CurrentCharLevel - 1].Abilities.Clone();
	}

	public virtual void TryTriggerAbility()
	{
	}

	public virtual void OnDoubleTap()
	{
	}

	public virtual void OnSwipe()
	{
	}

	public virtual void UpdateInSloMo()
	{
	}

	public virtual bool Paralized()
	{
		if (!base.TeleportPending && (!base.Flying || !FlightTarget.HasValue) && !shopDrop && !Burning)
		{
			return SpellEffects[SpellType.Ice].Active;
		}
		return true;
	}

	public override bool CanTeleport()
	{
		if (!Dead && !base.Flying)
		{
			return !Falling;
		}
		return false;
	}

	public override int TeleportDelay()
	{
		return 20;
	}

	public virtual bool WebCapture(WebEntity web)
	{
		HoldingWeb = web;
		return true;
	}

	public void WebRelease()
	{
		HoldingWeb = null;
	}

	public virtual void Jump(Vector2 direction)
	{
		if (SpellEffects[SpellType.Confusion].Active)
		{
			direction *= -1f;
		}
		if (SpellEffects[SpellType.Ice].Active)
		{
			((IceEffect)SpellEffects[SpellType.Ice]).Push(direction);
			playState.Camera.Shake("ice push", 1f, 6);
			Rumble.Pulse("ice push", 0.2f, 0.1f, 6);
		}
		else
		{
			if (Paralized())
			{
				return;
			}
			if (TrappedInWeb)
			{
				HoldingWeb.Pull(direction);
				playState.Camera.Shake("web pull", 1f, 6);
			Rumble.Pulse("web pull", 0.15f, 0.15f, 6);
				return;
			}
			DungeonTile dungeonTile = base.Tile as DungeonTile;
			if (dungeonTile != null && dungeonTile.Type != TileType.Ice)
			{
				dungeonTile = null;
			}
			if (BSlide.Sliding)
			{
				return;
			}
			prevDirection = FacingDirection.Clone();
			FacingDirection = direction;
			if (base.Tile != null)
			{
				tileBeforeJump = base.Tile;
				lastJumpTick = base.worldTicks;
				Vector2 coordinates = base.CenterCoordinates.Clone();
				PlatformEntity currentPlatform = CurrentPlatform;
				bool num = TryMoveToCoordinates(base.CurrentMap, base.Tile.Coordinates + direction);
				if (!num)
				{
					dxTween.Start(direction.X * 0.2f, 0f, 13);
					dyTween.Start(direction.Y * 0.2f, 0f, 13);
				}
				if (num && !base.Flying && !Dead && dungeonTile != null)
				{
					dungeonTile.IceTrailN |= direction.Y < 0f;
					dungeonTile.IceTrailE |= direction.X > 0f;
					dungeonTile.IceTrailW |= direction.X < 0f;
					dungeonTile.IceTrailS |= direction.Y > 0f;
				}
				if (num && base.core.ProfileData.Character != Character.Creep && !base.Flying)
				{
					PlayStepSound();
					SendMessage(new SpawnEntityMessage(new EffectEntity(coordinates, "dust_", "1234"), currentPlatform));
				}
				if (num && !base.Flying)
				{
					_inc(Stat.MetersWalked);
				}
				if (this is VampireChar { FlightActive: not false })
				{
					_inc(Stat.VampireMetersFlownAsBat);
				}
			}
		}
	}

	public virtual void PlayStepSound()
	{
		SendMessage(new PlayWorldSoundMessage(StepSounds.DrawDifferent(), base.WorldCenter));
	}

	public override void OnEnterTile(Tile tile)
	{
		playState.TrackDistance(base.WorldCoordinates.Y);
		if (!Dead)
		{
			playState.TrackSpeed();
		}
		if (tile.Type == TileType.Pit && CurrentPlatform == null && !base.Flying && !Dead)
		{
			Fall();
		}
		base.OnEnterTile(tile);
	}

	public override void Update()
	{
		base.Update();
		bool poisoned = SpellEffects[SpellType.Poison].Active;
		bool frosted = SpellEffects[SpellType.Ice].Active;
		FrostScreen = MathHelper.Clamp(FrostScreen + (frosted ? 1f / 65f : -1f / 32f), 0f, 1f);
		if (poisoned) poisonTail = 300; else if (poisonTail > 0) poisonTail--;
		if (frosted) frostTail = 300; else if (frostTail > 0) frostTail--;
		bool poisonParticles = !Dead && (poisoned || poisonTail > 0);
		bool frostParticles = !Dead && (frosted || frostTail > 0);
		if (poisonParticles && !wasPoisoned) poisonEmitter?.Start(4);
		else if (!poisonParticles && wasPoisoned) poisonEmitter?.Pause();
		if (frostParticles && !wasFrosted) frostEmitter?.Start(5);
		else if (!frostParticles && wasFrosted) frostEmitter?.Pause();
		wasPoisoned = poisonParticles;
		wasFrosted = frostParticles;
		if (Dead && !Burning)
		{
			return;
		}
		if (Burning)
		{
			burnAnim.Update();
			if (burnAnim.GetCurrentFrameNumber() == 2 && !madeBurningShot)
			{
				playState.MakeGameplayScreenshot(0, evenIfDead: true);
				madeBurningShot = true;
			}
			burnOpacity = ((burnT < 15) ? 1f : Component._M(1f - (float)(burnT - 15) / 15f, 0f));
			if (burnT == 1)
			{
				Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16759608), 7f);
				light.Follow(this);
				light.FollowRate = 1f;
				light.ChangeRate = 0.003f;
				light.Position = base.WorldCenter;
				light.Die();
				base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z, 5, this);
			}
			burnT++;
			if (burnT >= burnD)
			{
				SendMessage(new RemoveEntityMessage(this), 1);
				playState.Camera.Follow(null);
				return;
			}
		}
		UpdateAbilities();
		if (shopDrop)
		{
			if (dropAnim > 0.1f)
			{
				dropAnim -= 2.5f;
			}
			else
			{
				dropAnim = 0f;
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates, "dust_", "1234"), CurrentPlatform));
				shopDrop = false;
			}
		}
		if (Falling)
		{
			if (!customAnimation && base.core.CurrentCharDesc.FallAnimation)
			{
				animation.Speed = 0.6f;
				animation.Play("spin");
			}
			if (fallAnim < 40)
			{
				fallAnim++;
				if (fallAnim == 1)
				{
					TrySpawnFallFragments();
				}
			}
			else
			{
				Die(InjuryType.Fall);
			}
		}
		else if (!customAnimation)
		{
			if (uTurn > 0)
			{
				uTurn--;
			}
			if (!BSlide.Sliding && AnimateUTurns && FacingDirection.IsEqualTo(-prevDirection))
			{
				uTurn = 20;
				animation.Play((FacingDirection.X < 0f) ? "n" : ((FacingDirection.X > 0f) ? "s" : ((FacingDirection.Y < 0f) ? "e" : "w")));
				uTurnSprite = animation.GetCurrentFrame();
				prevDirection = FacingDirection.Clone();
			}
			if (BSlide.Sliding)
			{
				prevDirection = FacingDirection.Clone();
			}
			animation.Speed = normalAnimSpeed;
			string name = FacingDirection.DirectionId();
			animation.Play(name);
		}
		if (!TrappedInWeb && !BSlide.Sliding && !Burning && !AnimPaused && !SpellEffects[SpellType.Ice].Active)
		{
			animation.Update();
		}
		if (animationWalk != null)
		{
			animationWalk.Update();
		}
		if (hurtTtl > 0)
		{
			hurtTtl--;
			if (hurtTtl == 0)
			{
				EnterTile(base.Tile);
			}
		}
		base.Update();
	}

	protected override bool OnDoTeleport()
	{
		return base.OnDoTeleport();
	}

	public override void Draw()
	{
		float num = ((fallAnim != 0) ? (animateFall ? (((fallAnim - 10) * (fallAnim - 10) - 100) / 8) : (fallAnim * 2)) : 0);
		if (base.Flying && FlightTarget.HasValue)
		{
			float num2 = 0.5f - (base.WorldCoordinates - FlightStart).Length() / (FlightTarget.Value - FlightStart).Length();
			num -= 12f - 48f * num2 * num2;
		}
		Sprite sprite = animation.GetCurrentFrame();
		if (dxTween.Running || dyTween.Running)
		{
			if (!BSlide.Sliding)
			{
				num -= 2f;
			}
			if (animationWalk != null && animationWalk.CurrentSequence == animation.CurrentSequence)
			{
				sprite = animationWalk.GetFrame(((int)base.WorldCoordinates.Y).Mod(2));
			}
			if (uTurn > 0)
			{
				sprite = uTurnSprite;
			}
		}
		else
		{
			uTurn = 0;
		}
		bool lit = Lit;
		Renderer renderer = ((Falling && num > 0f) ? base.core.Renderer["bg", base.Z + 1, lit] : base.core.Renderer[base.Z + 1, lit]);
		Vector2 vector = (TrappedInWeb ? (HoldingWeb.PullVector * 5f) : Vector2.Zero);
		LastSpriteShift = base.dAnim.Shift(0f, num - dropAnim) + vector;
		LastSpritePos = base.WorldPosition + LastSpriteShift + PosShift;
		LastLayer = ((Falling && num > 0f) ? "bg" : "default");
		LastZ = base.Z + 1;
		float num3 = 1f - ((float)fallAnim - 20f) / 20f;
		LastTint = ((fallAnim == 0) ? Color.White : Color.Lerp(Color.Black, Color.White, num3 * num3 * num3)) * burnOpacity;
		float pulse = 0.82f + 0.18f * MathF.Sin(Age * 0.2f);
		LastTint = Color.Lerp(LastTint, new Color(70, 220, 90) * burnOpacity, PoisonVisual * 0.65f * pulse);
		LastTint = Color.Lerp(LastTint, new Color(95, 180, 255) * burnOpacity, FrostVisual * 0.65f * pulse);
		LastSpriteAlpha = burnOpacity * ((fallAnim == 0) ? 1f : (num3 * num3 * num3));
		if ((0u | ((Falling && !base.core.CurrentCharDesc.FallAnimation) ? 1u : 0u)) == 0)
		{
			renderer.DrawSpriteW(sprite, LastSpritePos, LastTint);
			if (num <= 0f)
			{
				renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldPosition.Shift(0f, 12f) + PosShift + ShadowShift + base.dAnim - new Vector2(0f, num - dropAnim * 0.5f), Color.Black * 0.2f * burnOpacity, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
			}
		}
		DrawBurningFlame();
		base.Draw();
	}

	protected void DrawBurningFlame()
	{
		if (Burning && !burnAnim.Paused)
		{
			base.core.Renderer[base.Z + 2].DrawSpriteW(burnAnim.GetCurrentFrame(), LastSpritePos.Shift(-4f, -18f) - PosShift);
		}
	}

	public virtual bool InteractsWithWorld()
	{
		return true;
	}

	public virtual bool TryResist(InjuryType injuryType, Entity offender = null)
	{
		return false;
	}

	public virtual bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		return false;
	}

	public void ApplySpell(SpellType spellType, Entity offenter = null)
	{
		if (!TryResistSpell(spellType))
		{
			SpellEffects[spellType].Activate();
		}
	}

	public void DrawSpellEffects()
	{
		foreach (KeyValuePair<SpellType, SpellEffect> spellEffect in SpellEffects)
		{
			spellEffect.Value?.Draw();
		}
	}

	public void AnnounceAbilityStatus(AbilityDesc ability, int remaining, int delay = 0)
	{
		base.core.CurrentPlayState.Hud.ShowAlert(__(ability.Name), string.Format(__(SId.SKILL_x_left), remaining), remaining switch
		{
			1 => default(Color).FromRgb(15902269), 
			0 => Color.Red, 
			_ => default(Color).FromRgb(8439569), 
		}, 60, ability.HudItemIcon);
	}

	public void Hurt(InjuryType injuryType, Entity offender = null)
	{
		if (Dead)
		{
			return;
		}
		switch (injuryType)
		{
		case InjuryType.Bat:
			if (InteractsWithWorld())
			{
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.WorldCoordinates, "hit_claws_", "123", screenEffect: true), null));
				SendMessage(new PlayWorldSoundMessage(SoundName.bat_attack, base.WorldCenter));
			}
			break;
		}
		if (TryResist(injuryType, offender))
		{
			return;
		}
		playState.Camera.Shake("hit");
		Rumble.Pulse("hit", 0.6f, 0.25f, 10);
		if (StopFall())
		{
			if (injuryType == InjuryType.Bolt && base.core.CurrentCharDesc.CrossbowAnimation)
			{
				((BoltEntity)offender).Victim = this;
				TrySpawnLeftovers(base.WorldCenterCoordinates, bolt: true);
			}
			if (injuryType == InjuryType.Timeout)
			{
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.WorldCoordinates, "hit_claws_", "123", screenEffect: true), null));
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.WorldCoordinates, "hit_claws_", "123", screenEffect: true, mirrored: true), null), 20);
				SendMessage(new SpawnEntityMessage(new EffectEntity(base.WorldCoordinates, "hit_claws_", "123", screenEffect: true), null), 30);
			}
			Die(injuryType);
		}
	}

	public void Crash(Entity offender)
	{
		Hurt(InjuryType.Crushed, offender);
		playState.MakeGameplayScreenshot(15, evenIfDead: true);
	}

	public void Die(InjuryType causeOfDeath, int customDelay = -1)
	{
		if (Dead)
		{
			return;
		}
		bool flag = customDelay >= 0;
		if (this is VesnaChar { LightDuration: >0 })
		{
			base.core.Achievments.Unlock(Achievement.VesnaDieWhileCastingSunrise);
		}
		if (causeOfDeath != InjuryType.Fall && Falling && !StopFall())
		{
			return;
		}
		Dead = true;
		OnDying(causeOfDeath);
		playState.Session.CauseOfDeath = causeOfDeath;
		playState.Session.MaxPlayerY = Math.Min(playState.Session.MaxPlayerY, base.Tile.Y + (int)base.Tile.Map.Y);
		if (!flag && !Falling && (!base.core.CurrentCharDesc.CrossbowAnimation || causeOfDeath != InjuryType.Bolt) && causeOfDeath != InjuryType.Slime && causeOfDeath != InjuryType.Zap && causeOfDeath != InjuryType.Flame && causeOfDeath != InjuryType.DeadBattery)
		{
			TrySpawnFragments();
		}
		if (flag || (causeOfDeath != InjuryType.Flame && causeOfDeath != InjuryType.DeadBattery))
		{
			SendMessage(new RemoveEntityMessage(this), 1);
			playState.Camera.Follow(null);
		}
		playState.ContinuePending = true;
		base.core.TimerManager.CreateTimer(flag ? customDelay : (causeOfDeath switch
		{
			InjuryType.DeadBattery => 90, 
			InjuryType.Fall => 20, 
			_ => 60, 
		}), 1, 1, delegate
		{
			playState.TransitionOut(CoreEvent.OfferToContiune);
		});
		if (TrappedInWeb)
		{
			_inc(Stat.DiedInSpiderWeb);
		}
		if (SpellEffects[SpellType.Confusion].Active)
		{
			_inc(Stat.DiedConfused);
		}
		if (SpellEffects[SpellType.Ice].Active)
		{
			_inc(Stat.DiedFrozen);
		}
		if (SpellEffects[SpellType.Darkness].Active)
		{
			_inc(Stat.DiedInMist);
		}
		if (SpellEffects[SpellType.Poison].Active)
		{
			_inc(Stat.DiedPoisoned);
		}
		DeactivateSpellEffects();
		if (Achievements.CauseOfDeathStat.ContainsKey(causeOfDeath))
		{
			_inc(Achievements.CauseOfDeathStat[causeOfDeath]);
		}
		if (flag)
		{
			CustomDeathSoundAndScreenshot();
		}
		else
		{
			switch (causeOfDeath)
			{
			case InjuryType.Bolt:
				SendMessage(new PlayWorldSoundMessage(SoundName.crossbow_hit, base.WorldPosition));
				playState.MakeGameplayScreenshot(base.core.CurrentCharDesc.CrossbowAnimation ? 4 : 15, evenIfDead: true);
				break;
			case InjuryType.Zap:
				SendMessage(new PlayWorldSoundMessage(SoundName.zap, base.WorldPosition));
				playState.Camera.Shake("hit", 4f, 14);
				base.core.Renderer.Flash(new Color(220, 240, 255), 0.85f);
				break;
			case InjuryType.Bat:
				playState.MakeGameplayScreenshot(25, evenIfDead: true);
				break;
			case InjuryType.Slime:
				playState.MakeGameplayScreenshot(20, evenIfDead: true);
				break;
			case InjuryType.Spikes:
				playState.MakeGameplayScreenshot(20, evenIfDead: true);
				break;
			case InjuryType.Sword:
				playState.MakeGameplayScreenshot(20, evenIfDead: true);
				break;
			case InjuryType.Saw:
				playState.MakeGameplayScreenshot(20, evenIfDead: true);
				break;
			case InjuryType.Timeout:
				playState.MakeGameplayScreenshot(5, evenIfDead: true);
				SendMessage(new PlaySoundMessage(SoundName.critters_kill, 0.6f));
				break;
			case InjuryType.Axe:
				playState.MakeGameplayScreenshot(10, evenIfDead: true);
				break;
			case InjuryType.Follower:
				playState.MakeGameplayScreenshot(15, evenIfDead: true);
				break;
			case InjuryType.Flame:
				burnT = 0;
				SendMessage(new PlaySoundMessage(SoundName.burning));
				base.core.Renderer.Flash(new Color(255, 120, 30), 0.7f);
				break;
			case InjuryType.DeadBattery:
				playState.MakeGameplayScreenshot(0, evenIfDead: true);
				break;
			case InjuryType.Serpent:
				playState.MakeGameplayScreenshot(15, evenIfDead: true);
				break;
			}
		}
		if (HoldingWeb != null)
		{
			HoldingWeb.ReleasePlayer();
		}
		string moduleIdWithPlayer = base.core.CurrentPlayState.LevelGenerator.GetModuleIdWithPlayer();
		if (moduleIdWithPlayer != string.Empty)
		{
			Event(AnalyticsCategory.Run, "death-module", moduleIdWithPlayer);
		}
	}

	protected virtual void OnDying(InjuryType causeOfDeath) { }

	protected void DeactivateSpellEffects()
	{
		foreach (KeyValuePair<SpellType, SpellEffect> spellEffect in SpellEffects)
		{
			spellEffect.Value?.Deactivate();
		}
	}

	protected virtual void CustomDeathSoundAndScreenshot()
	{
		playState.MakeGameplayScreenshot(15, evenIfDead: true);
	}

	public Sprite GetCurrentSprite()
	{
		return animation.GetCurrentFrame();
	}

	public void TrySpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		if (!spawnedFragments)
		{
			spawnedFragments = SpawnLeftovers(pos, bolt);
		}
	}

	public void TrySpawnFragments(bool bolt = false)
	{
		if (!spawnedFragments)
		{
			spawnedFragments = SpawnFragments(bolt);
		}
	}

	public void TrySpawnFallFragments()
	{
		if (!spawnedFragments)
		{
			spawnedFragments = SpawnFallFragments();
		}
	}

	public virtual bool SpawnFragments(bool bolt = false)
	{
		return false;
	}

	public virtual bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		return false;
	}

	public virtual bool SpawnFallFragments()
	{
		return false;
	}

	protected virtual bool TryResistFall()
	{
		return false;
	}

	public void Fall()
	{
		if (TryResistFall() || Dead)
		{
			return;
		}
		foreach (KeyValuePair<SpellType, SpellEffect> spellEffect in SpellEffects)
		{
			spellEffect.Value?.Deactivate();
		}
		Falling = true;
		animateFall = !justStoppedFlight;
		playState.MakeGameplayScreenshot(animateFall ? 15 : 5, evenIfDead: true);
		SendMessage(new PlayWorldSoundMessage(SoundName.fall, base.WorldCenter), 10);
	}

	public bool StopFall()
	{
		if (fallAnim < 20 && !spawnedFragments)
		{
			Falling = false;
			fallAnim = 0;
			return true;
		}
		return false;
	}

	protected virtual int CoinMultiplier()
	{
		return 1;
	}

	public virtual void CollectCoins(int number, Entity source, Color hintColor)
	{
		if (number <= 0) return;
		if (base.core.ProfileData.CoinDoublerEnabled)
		{
			number *= 2;
		}
		number *= CoinMultiplier();
		int oldMultiplier = playState.Session.CoinChain.Multiplier;
		number = playState.Session.CoinChain.Collect(number);
		if (playState.Session.CoinChain.Multiplier > oldMultiplier)
		{
			Rumble.Pulse("coin-chain", 0.12f, 0.22f, 5);
			hintColor = Color.Gold;
		}
		playState.Session.CollectedCoins += number;
		base.core.ProfileData.Coins += number;
		string text = string.Concat(number);
		if (source != null)
		{
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(source.CenterCoordinates, "+" + text, hintColor), source.CurrentPlatform));
		}
		else
		{
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, "+^" + text, hintColor, 1.5f), CurrentPlatform));
		}
		_inc(Stat.CoinsCollected, number);
	}

	public virtual Sprite CurrentSprite()
	{
		return animation.GetCurrentFrame();
	}

	public void ShopDrop()
	{
		dropAnim = 70f;
		shopDrop = true;
	}

	protected virtual void UpdateAbilities()
	{
		foreach (Skill value in Enum.GetValues(typeof(Skill)))
		{
			int num = Abilities.SkillLevel[value];
			if (!num.Equals(0))
			{
				float num2 = 1f / (float)(num * 60);
				Abilities.SkillCharge[value] = Component._m(1f, Abilities.SkillCharge[value] + num2);
			}
		}
	}
}
