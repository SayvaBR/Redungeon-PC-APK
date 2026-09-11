using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class VampireChar : PlayerEntity
{
	private bool reviving;

	private int reviveTimer;

	private const int reviveDuration = 100;

	private Animation batAnim;

	private Animation spinAnim;

	private int flightTimer;

	private int flightDuration;

	private Light light;

	private BagOf<SoundName> flaps;

	private int sinceLastSound = 60;

	public bool FlightActive { get; private set; }

	[Preserve]
	public VampireChar(int x, int y)
		: base(x, y)
	{
		FlightActive = false;
		batAnim = new Animation(0.3f);
		batAnim.Add("fly", "kazhan_bat_", "1234");
		batAnim.Play("fly");
		normalAnimSpeed = 0.1f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "kazhan_n_", "1234");
		animation.Add("e", "kazhan_e_", "1234");
		animation.Add("w", "kazhan_w_", "1234");
		animation.Add("s", "kazhan_s_", "1234");
		animation.Add("spin", new List<SpriteName>
		{
			SpriteName.kazhan_n_1,
			SpriteName.kazhan_e_1,
			SpriteName.kazhan_s_1,
			SpriteName.kazhan_w_1
		});
		spinAnim = new Animation(0.6f);
		spinAnim.AddAndPlay("spin", new List<SpriteName>
		{
			SpriteName.kazhan_n_1,
			SpriteName.kazhan_e_1,
			SpriteName.kazhan_s_1,
			SpriteName.kazhan_w_1
		});
		PosShift = new Vector2(-3f, -7f);
		base.core.ParticleManager.AddEmitter(inWorld: true, base.Center, 8f, 2f).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Position -= base.WorldCenter;
			if (reviving || FlightActive)
			{
				p.Velocity.X = -1f;
			}
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position += p.Offset * 0.04f;
				p.Position = p.Position.Shift(0f, -0.01f * p.Offset.X * p.Offset.X);
				p.Dead = p.Age > 35;
			})
			.OnDraw(delegate(Particle p)
			{
				if (!(p.Velocity.X < 0f))
				{
					base.core.Renderer[(p.Offset.Y > 0f) ? (base.Z + 3) : base.Z].DrawSpriteW(_(SpriteName.glow_big), p.Position.Shift(-4f + Component._sin((float)(base.worldTicks + p.Age) / 20f), 3f + Component._cos((float)(base.worldTicks + p.Age) / 20f)) + base.WorldCenter + base.dAnim, scale: new Vector2(0.3f - 0.3f * (float)p.Age / 35f), tint: default(Color).FromRgb(14045110) * ((float)p.Age / 35f), rotation: (float)p.Age * 0.03f, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
				}
			})
			.Start(2, 4);
		flightDuration = 60 * ((base.core.ProfileData.CurrentCharLevel < 5) ? 2 : 5);
		flaps = new BagOf<SoundName>().Put(SoundName.kazhan_flap_1).Put(SoundName.kazhan_flap_2).Put(SoundName.kazhan_flap_3);
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16722612) * 0.7f, 6f, 0f, this);
		light.FollowRate = 0.2f;
		light.ChangeRate = 0.05f;
		light.TargetIntencity = 0f;
		light.TargetRadius = 0f;
		base.Load();
	}

	public override void TryTriggerAbility()
	{
		if (!Dead && !FlightActive && !reviving && !(Abilities.SkillCharge[Skill.Flight] < 1f))
		{
			BatFlying(activate: true);
			base.TryTriggerAbility();
		}
	}

	private void BatFlying(bool activate)
	{
		if (activate)
		{
			if (StopFall())
			{
				batAnim.Speed = 0.2f;
				SetFlying(value: true);
				FlightActive = true;
				flightTimer = flightDuration;
				FlightIgnoresObstacles = false;
				base.ZappedSprite = SpriteName.zapped_bat;
				light.TargetIntencity = 1f;
				light.TargetRadius = 6f;
				DeactivateSpellEffects();
				SendMessage(new PlaySoundMessage(SoundName.kazhan_turn));
			}
		}
		else
		{
			BSlide.SlowLanding = true;
			batAnim.Speed = 0.3f;
			Abilities.SkillCharge[Skill.Flight] = 0f;
			SetFlying(value: false);
			FlightActive = false;
			BSlide.SlowLanding = false;
			FacingDirection = new Vector2(0f, 1f);
			prevDirection = FacingDirection.Clone();
			base.ZappedSprite = SpriteName.zapped_knight;
			light.TargetIntencity = 0f;
			light.TargetRadius = 0f;
		}
	}

	public override bool Paralized()
	{
		if (!reviving)
		{
			return base.Paralized();
		}
		return true;
	}

	private void StartReviveEffect()
	{
		Abilities.SkillLevel[Skill.TurnIntoBat]--;
		Vector2 vector = base.core.CurrentPlayState.LevelGenerator.NextSafePoint(base.WorldCoordinates);
		int num = (int)(vector.X - base.WorldCoordinates.X);
		int num2 = (int)(vector.Y - base.WorldCoordinates.Y);
		SuspendedStartFlying(num, num2, 0.07f, ignoreObstacles: true);
		FacingDirection = new Vector2(0f, 1f);
	}

	public override void Update()
	{
		if (reviveTimer > 0)
		{
			batAnim.Update();
			spinAnim.Update();
			reviveTimer--;
			if (reviveTimer == 60)
			{
				StartReviveEffect();
			}
			if (reviveTimer == 0)
			{
				reviving = false;
				AnnounceAbilityStatus(Abilities.SkillDesc[Skill.TurnIntoBat], Abilities.SkillLevel[Skill.TurnIntoBat]);
			}
		}
		if (flightTimer > 0)
		{
			batAnim.Update();
			spinAnim.Update();
			flightTimer--;
			if (flightTimer == 0)
			{
				BatFlying(activate: false);
			}
			float num = 1f;
			if (flightTimer <= 40 || flightTimer >= flightDuration - 40)
			{
				num = flightTimer;
				if (num > 40f)
				{
					num = flightDuration - flightTimer;
				}
				num = (float)Tween.BackEaseOut(num, 0.0, 1.0, 40.0);
			}
			base.playState.Camera.ZoomBox.Set("bat", 1f - 0.2f * num, inWorld: true);
			if (flightTimer == 20)
			{
				SendMessage(new PlaySoundMessage(SoundName.kazhan_turn_back));
			}
		}
		if (FlightActive || reviving)
		{
			sinceLastSound++;
			if (batAnim.GetCurrentFrameNumber() == 0 && sinceLastSound > 10)
			{
				SendMessage(new PlaySoundMessage(SoundName.kazhan_flap_1));
				sinceLastSound = 0;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (reviving)
		{
			float num = Component._M(Component._m(Component._sin((float)reviveTimer * (float)Math.PI / 100f) * 2f, 1f), 0f);
			base.core.Renderer["fg", -2, false].FillScreen(Color.Black * num * 0.6f);
			Vector2 v = base.WorldCenter.Shift(Component._cos((float)base.worldTicks * 0.1f) * 10f * num, Component._sin((float)base.worldTicks * 0.1f) * 10f * num) + base.dAnim;
			if (reviveTimer < 20 || reviveTimer > 80)
			{
				int num2 = ((reviveTimer < 20) ? reviveTimer : (100 - reviveTimer));
				num2 = 4 * num2 / 20 + 1;
				base.core.Renderer[base.Z].DrawSpriteW(_("kazhan_morph_" + num2), v.Shift(0f, -8f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			else
			{
				base.core.Renderer["fg", -1, false].DrawSpriteW(batAnim.GetCurrentFrame(), v.Shift(0f, -8f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		else if (FlightActive)
		{
			float num3 = 1f;
			if (flightTimer <= 20 || flightTimer >= flightDuration - 20)
			{
				num3 = flightTimer;
				if (num3 > 20f)
				{
					num3 = flightDuration - flightTimer;
				}
				num3 /= 20f;
			}
			Sprite sprite;
			if (flightTimer < 20 || flightTimer > flightDuration - 20)
			{
				int num4 = ((flightTimer < 20) ? flightTimer : (flightDuration - flightTimer));
				num4 = 4 * num4 / 20 + 1;
				sprite = _("kazhan_morph_" + num4);
				base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, -8f) + base.dAnim, Color.White * burnOpacity, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			else
			{
				LastSpritePos = base.WorldCenter.Shift(-10f, -14f) + base.dAnim + PosShift;
				sprite = batAnim.GetCurrentFrame();
				base.core.Renderer[base.Burning ? "default" : "fg", base.Burning ? base.Z : (-1), false].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, -8f) + base.dAnim, Color.White * burnOpacity, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldCenter.Shift(-0f, 5f) + base.dAnim, Color.Black * 0.2f * burnOpacity, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical, SpriteOrigin.Center);
			if (!base.core.CurrentPlayState.Paused && !base.core.TakingScreenshot)
			{
				float num5 = 32f * num3;
				float num6 = num5 * (float)(flightTimer - 40) / (float)(flightDuration - 40);
				RectangleF rectangleF = new RectangleF(base.WorldCenter.X + base.dAnim.X - num5 / 2f, base.WorldCenter.Y + base.dAnim.Y + 20f * (1f - num3), num5, 3f);
				rectangleF = rectangleF.Grow(-1f, -1f, 1f, 1f);
				base.core.Renderer["fg", -10, false].DrawRectangleW(rectangleF, Color.Black * num3);
				rectangleF = rectangleF.Grow(1f, 1f, -1f, -1f);
				base.core.Renderer["fg", -10, false].DrawRectangleW(rectangleF, default(Color).FromRgb(4076369) * num3);
				rectangleF.Width = num6;
				rectangleF.X += (num5 - num6) / 2f;
				base.core.Renderer["fg", -10, false].DrawRectangleW(rectangleF, default(Color).FromRgb(12020102) * num3);
			}
			DrawBurningFlame();
		}
		else
		{
			base.Draw();
		}
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		if (reviving)
		{
			return true;
		}
		if (FlightActive)
		{
			if (injuryType != InjuryType.Saw && injuryType != InjuryType.Slime && injuryType != InjuryType.Sword)
			{
				return injuryType == InjuryType.Zap;
			}
			return true;
		}
		return base.TryResist(injuryType, offender);
	}

	protected override bool TryResistFall()
	{
		bool flag = false;
		if (Abilities.SkillLevel[Skill.TurnIntoBat] > 0)
		{
			flag = true;
			reviving = true;
			reviveTimer = 100;
			DeactivateSpellEffects();
		}
		if (!flag)
		{
			return base.TryResistFall();
		}
		return true;
	}

	public override bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		if (!FlightActive)
		{
			return base.TryResistSpell(spellType, offender);
		}
		return true;
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.kazhan_death));
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 3f).OnSpawn(delegate
		{
		}).OnUpdate(delegate(Particle p)
		{
			p.Position = p.Position.Shift(p.Offset.X * 0.3f + Component._sin((float)(base.worldTicks + p.Age) / 20f), p.Offset.Y * 0.05f - 2.1f + Component._cos((float)(base.worldTicks + p.Age) / 20f));
			p.Dead = p.Age > 100;
		})
			.OnDraw(delegate(Particle p)
			{
				int num = ((int)((float)p.Age * 0.3f)).Mod(4);
				Renderer renderer = base.core.Renderer["fg", base.Z + 1, false];
				int name;
				switch (num)
				{
				default:
					name = 639;
					break;
				case 1:
				case 3:
					name = 638;
					break;
				case 0:
					name = 637;
					break;
				}
				renderer.DrawSpriteW(_((SpriteName)name), p.Position, null, new Vector2(0.6f + (float)p.Age / 50f), p.Offset.X * (float)p.Age * 0.01f, SpriteFlip.None, SpriteOrigin.Center);
				if (p.Age < 20)
				{
					base.core.Renderer["fg", base.Z + 1, false].DrawSpriteW(_(SpriteName.glow_big), (base.WorldCenter + p.Position) / 2f, Color.DeepPink * ((float)p.Age / 20f), new Vector2(1f - 1f * (float)p.Age / 20f), 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
			})
			.Emit(10, 5, once: true, 2);
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.kazhan_shot;
	}
}
