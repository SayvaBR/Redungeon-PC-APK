using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PanicBotChar : PlayerEntity
{
	private Animation flasher;

	private Light redLight;

	private Light blueLight;

	private bool flasherActive = true;

	private int glitchT;

	private float batteryLife;

	private float shotCost;

	private float charge;

	private bool charging;

	private bool wasCharging;

	private bool batteryDead;

	private bool drawCharging;

	private int drawChargingT;

	private bool firstLife;

	private Light chargeLight;

	private Animation chargeAnimation;

	private int batteryDeadDelayT;

	private int batteryDeadDelayD = 40;

	private int batteryDeadT;

	private int batteryDeadD = 160;

	[Preserve]
	public PanicBotChar(int x, int y)
		: base(x, y)
	{
		firstLife = base.core.CurrentPlayState.Session.Revives == 0;
		normalAnimSpeed = 0.095f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "panicbot_n_", "1234");
		animation.Add("e", "panicbot_e_", "1234");
		animation.Add("w", "panicbot_w_", "1234");
		animation.Add("s", "panicbot_s_", "1234");
		animation.Add("spin", "panicbot_fall_", "1111122222");
		flasher = new Animation();
		flasher.Add("flash", "panicbot_light_", "12345678");
		flasher.Add("flash-red", "panicbot_light_", "5674");
		flasher.Add("red", "panicbot_light_", "7");
		flasher.Play("flash");
		PosShift = new Vector2(-2f, -9f);
		redLight = base.core.CurrentPlayState.LightManager.AddLight(Color.Red, 5f, 0.7f);
		blueLight = base.core.CurrentPlayState.LightManager.AddLight(Color.Blue, 5f, 0.7f);
		charge = (firstLife ? 0.5f : 1f);
		wasCharging = false;
		charging = false;
		drawCharging = false;
		batteryDead = false;
		chargeAnimation = new Animation(0.3f);
		chargeAnimation.Add("buzz", "panicbot_charge_", "1234");
		chargeAnimation.Play("buzz");
	}

	public override void Load()
	{
		if (firstLife)
		{
			ZapperEntity zapperEntity = new ZapperEntity((int)base.WorldCoordinates.X - 1, (int)base.WorldCoordinates.Y, "e", 1, 1, flipped: false);
			zapperEntity.IsPanicBotStation = true;
			SendMessage(new SpawnEntityMessage(zapperEntity, CurrentPlatform));
			zapperEntity = new ZapperEntity((int)base.WorldCoordinates.X + 1, (int)base.WorldCoordinates.Y, "-", 1, 1, flipped: false);
			zapperEntity.IsPanicBotStation = true;
			SendMessage(new SpawnEntityMessage(zapperEntity, CurrentPlatform));
		}
		chargeLight = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(6996223), 4f, 0.7f, this);
		chargeLight.FollowRate = 1f;
		chargeLight.ChangeRate = 1f;
		chargeLight.TargetIntencity = 0f;
		chargeLight.Active = true;
		batteryLife = Abilities.SkillLevel[Skill.Electronic];
		shotCost = Abilities.SkillLevel[Skill.PanicLaser];
		base.Load();
	}

	public override bool Paralized()
	{
		if (!batteryDead)
		{
			return base.Paralized();
		}
		return true;
	}

	public override void TryTriggerAbility()
	{
		if (!Dead && !base.Falling)
		{
			if (charge < shotCost / 100f)
			{
				SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_PANICLASER_alert_low_battery)), CurrentPlatform));
				return;
			}
			charge -= shotCost / 100f;
			SendMessage(new SpawnEntityMessage(new ProjectileEntity(base.WorldCenterCoordinates.X, base.WorldCenterCoordinates.Y, FacingDirection.Clone(), ProjectileEntity.ProjectileType.Laser), null));
			SendMessage(new PlayWorldSoundMessage(SoundName.panicbot_laser, base.WorldCenter));
			base.TryTriggerAbility();
		}
	}

	public override void Update()
	{
		float num = charge;
		if (Dead && base.playState.Session.CauseOfDeath == InjuryType.DeadBattery)
		{
			batteryDeadT++;
			if (batteryDeadT == batteryDeadD)
			{
				SendMessage(new RemoveEntityMessage(this));
				base.playState.Camera.Follow(null);
				return;
			}
		}
		if (!charging)
		{
			charge = Component._M(charge - 1f / (batteryLife * 60f), 0f);
			bool flag = batteryDead;
			batteryDead = charge < 0.005f;
			if (batteryDead && !flag)
			{
				batteryDeadDelayT = batteryDeadDelayD;
			}
			chargeLight.TargetIntencity = 0f;
		}
		chargeAnimation.Update();
		if (batteryDead)
		{
			if (batteryDeadDelayT == batteryDeadDelayD - 25)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.panicbot_dead_battery, base.WorldCenter));
			}
			batteryDeadDelayT--;
			if (batteryDeadDelayT == 0)
			{
				Die(InjuryType.DeadBattery);
			}
		}
		wasCharging = charging;
		charging = false;
		flasher.Update();
		float num2 = 0.1f;
		blueLight.Active = true;
		blueLight.TargetRadius = 5f;
		blueLight.TargetIntencity = 0.7f;
		redLight.TargetRadius = 5f;
		redLight.TargetIntencity = 0.7f;
		if (charge > 0.4f)
		{
			flasher.Play("flash");
			blueLight.Color = Color.Blue;
		}
		else
		{
			if (num > 0.4f)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.panicbot_battery_alarm, base.WorldCenter));
			}
			flasher.Play("flash-red");
			blueLight.Color = Color.Red;
		}
		redLight.Position = base.WorldCenter.Shift(30f * Component._cos((float)(-base.Age) * num2), 30f * Component._sin((float)(-base.Age) * num2));
		blueLight.Position = base.WorldCenter.Shift(-30f * Component._cos((float)(-base.Age) * num2), -30f * Component._sin((float)(-base.Age) * num2));
		if (charge < 0.2f)
		{
			blueLight.Active = false;
			redLight.Position = base.WorldCenter;
			redLight.TargetRadius = 10f;
			redLight.TargetIntencity = 1.2f;
			flasher.Play("red");
		}
		if (batteryDead)
		{
			AnimPaused = true;
			flasherActive = false;
			Lit = true;
			MainLight.TargetIntencity = 0f;
		}
		else
		{
			AnimPaused = false;
			Lit = false;
			flasherActive = true;
			MainLight.TargetIntencity = 0.8f;
		}
		if (glitchT > 0)
		{
			glitchT--;
			if (glitchT == 0)
			{
				redLight.Intencity = redLight.TargetIntencity;
				blueLight.Intencity = blueLight.TargetIntencity;
			}
		}
		else if (SciHelper.ChanceRoll(Component._M(0.1f - charge, 0f) / 0.1f / 5f))
		{
			glitchT = 2;
		}
		if (glitchT > 0 || base.Falling)
		{
			flasherActive = false;
		}
		if (!flasherActive)
		{
			redLight.Intencity = 0f;
			blueLight.Intencity = 0f;
			redLight.ChangeRate = 0f;
			blueLight.ChangeRate = 0f;
		}
		else
		{
			redLight.ChangeRate = 0.2f;
			blueLight.ChangeRate = 0.2f;
		}
		if (drawChargingT > 0)
		{
			drawChargingT--;
		}
		base.Update();
	}

	public override void Unload()
	{
		redLight.Dead = true;
		blueLight.Dead = true;
		base.Unload();
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		bool flag = false;
		if (injuryType == InjuryType.Zap)
		{
			flag = true;
			if (Dead)
			{
				return true;
			}
			if (offender is WispEntity)
			{
				charge += 0.3f;
				drawChargingT = 30;
			}
			charge = Component._m(charge + 0.01f, 1f);
			if (!wasCharging)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.panicbot_charging, base.WorldCenter));
			}
			charging = true;
			drawCharging = true;
			if (chargeLight != null)
			{
				chargeLight.TargetIntencity = Component._rnd(0.3f, 1f);
			}
			if (offender is ZapperEntity zapperEntity)
			{
				zapperEntity.Deplete();
			}
		}
		if (!flag)
		{
			return base.TryResist(injuryType, offender);
		}
		return true;
	}

	public override bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		if (!base.TryResistSpell(spellType, offender))
		{
			return spellType == SpellType.Poison;
		}
		return true;
	}

	public override void Draw()
	{
		if (Dead && batteryDeadT > 0)
		{
			if (batteryDeadT % 30 > 15)
			{
				base.core.Renderer["fg", -2000, false].DrawSpriteS(_(SpriteName.panicbot_dead_battery_icon), base.core.Renderer.ScreenCenter.Shift(0f, -30f), Color.Red, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			if (batteryDeadT >= batteryDeadD - 140)
			{
				base.core.Renderer["fg", -4000, false].FillScreen(Color.Black * (1f - Component._M(batteryDeadT - (batteryDeadD - 10), 0f) / 10f));
				float num = (float)batteryDeadT - (float)(batteryDeadD - 140);
				if (num < 60f)
				{
					float num2 = 1f;
					float num3 = (float)Tween.ExpoEaseOut(num - 20f, base.core.Renderer.ScreenWidth, -base.core.Renderer.ScreenWidth, 40.0);
					base.core.Renderer["fg", -4000, false].DrawRectangleS(new Vector2((float)base.core.Renderer.ScreenWidth * 0.5f - num3 / 2f - 1f, (float)base.core.Renderer.ScreenHeight * 0.5f - 3f * num2), num3 + 2f, 6f * num2, Color.White);
				}
				return;
			}
			float num4 = (float)batteryDeadT / (float)(batteryDeadD - 140);
			float num5 = (float)base.core.Renderer.ScreenHeight * 0.5f * num4;
			base.core.Renderer["fg", -4000, false].DrawRectangleS(new Vector2(-1f, -1f), base.core.Renderer.ScreenWidth + 2, num5 + 1f, Color.Black);
			base.core.Renderer["fg", -4000, false].DrawRectangleS(new Vector2(-1f, (float)base.core.Renderer.ScreenHeight - num5), base.core.Renderer.ScreenWidth + 2, num5 + 1f, Color.Black);
			base.core.Renderer["fg", -4000, false].DrawRectangleS(new Vector2(-1f, num5 - 3f), base.core.Renderer.ScreenWidth + 2, (float)base.core.Renderer.ScreenHeight - 2f * num5 + 6f, Color.White * num4);
		}
		if (batteryDead && base.core.TakingScreenshot)
		{
			base.core.Renderer["fg", -2000, false].DrawSpriteS(_(SpriteName.panicbot_dead_battery_icon), base.core.Renderer.ToScreen(base.WorldCenter).Shift(0f, -35f), Color.Red, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		base.Draw();
		if (drawCharging || drawChargingT > 0)
		{
			base.core.Renderer[LastLayer, base.Z - 1, false].DrawSpriteW(_(SpriteName.panicbot_charge_glow), LastSpritePos.Shift(-4f, -4f), Color.White * Component._rnd(0.3f, 1f));
			base.core.Renderer[LastLayer, base.Z + 1, false].DrawSpriteW(chargeAnimation.GetCurrentFrame(), LastSpritePos.Shift(-6f, -4f), Color.White);
		}
		Vector2 link = animation.GetCurrentFrame().Link;
		base.core.Renderer[LastLayer, base.Z + 2, Lit].DrawSpriteW(flasherActive ? flasher.GetCurrentFrame() : _(SpriteName.panicbot_light_0), LastSpritePos + link.Shift(-7f, -8f), LastTint);
		DrawCustomHUD();
		drawCharging = false;
	}

	private void DrawCustomHUD()
	{
		if (!base.core.TakingScreenshot && base.playState.Started)
		{
			float num = (float)Tween.CircEaseOut(base.core.CurrentPlayState.Trans, 60.0, -60.0, base.core.CurrentPlayState.TransDuration);
			if (base.core.OptionsData.LeftHandedMode)
			{
				num *= -1f;
			}
			Vector2 vector = base.playState.PlayerControl.SkillButtonCenter().Shift(num, -30f);
			Color color = default(Color).FromRgb(drawCharging ? 16777215 : ((charge > 0.4f) ? 9164031 : ((charge > 0.2f) ? 16762944 : 15532544)));
			base.core.Renderer["fg", -5000, false].DrawSpriteS(_(SpriteName.panicbot_charge_icon), vector, batteryDead ? default(Color).FromRgb(4603737) : color, null, 0.3f, SpriteFlip.None, SpriteOrigin.Center);
			for (int i = 1; i <= 10; i++)
			{
				base.core.Renderer["fg", -5000, false].DrawRectangleS(vector.Shift(-5f, -8 - 6 * i), 10f, 5f, ((float)(i * 10) < charge * 100f + 5f) ? color : default(Color).FromRgb(4603737));
			}
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlayWorldSoundMessage(SoundName.piston_break, base.WorldCenter));
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_arm), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_arm), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_leg), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_leg), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_part_1), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_part_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_bulb), null));
		}
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_head), null));
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new PlayWorldSoundMessage(SoundName.piston_break, base.WorldCenter));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_arm), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_arm), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_leg), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_leg), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_part_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_part_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.panicbot_bulb), null));
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.panicbot_shot;
	}

	public override void ResetAbilities(bool refill = false)
	{
		charge = 1f;
		base.ResetAbilities(refill);
	}

	protected override void UpdateAbilities()
	{
		Abilities.SkillCharge[Skill.PanicLaser] = Component._m(1f, charge / (shotCost / 100f));
		if (!base.playState.Started)
		{
			return;
		}
		List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 0.8f).FindAll((Entity e) => e is FireballEntity && !((FireballEntity)e).IsBroken && ((FireballEntity)e).Type == BallType.Zap);
		foreach (Entity item in list)
		{
			(item as FireballEntity).Break(this);
			if ((item as FireballEntity).Parent is CannonEntity cannonEntity)
			{
				cannonEntity.LoseCharge();
			}
			charge += 0.1f;
			drawChargingT = 30;
		}
		if (list.Count > 0)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.panicbot_charging, base.WorldCenter));
		}
		charge = Component._m(charge, 1f);
	}
}
