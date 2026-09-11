using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class NathanChar : PlayerEntity
{
	private int spinDuration;

	private int topSpinDuration;

	private const int r = 8;

	private Light aimLight;

	private NathansDroneEntity drone1;

	private NathansDroneEntity drone2;

	public bool TargetPractice => spinDuration > 0;

	[Preserve]
	public NathanChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.1f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "nathan_n_", "1234");
		animation.Add("e", "nathan_e_", "1234");
		animation.Add("w", "nathan_w_", "1234");
		animation.Add("s", "nathan_s_", "1234");
		animation.Add("spin", "nathan_fall_", "11112222");
		PosShift = new Vector2(-3f, -7f);
	}

	public override void InitAbilities(Abilities abilities)
	{
		base.InitAbilities(abilities);
		if (Abilities.SkillLevel[Skill.Drones] > 0)
		{
			drone1 = new NathansDroneEntity((int)Math.Round(base.WorldCoordinates.X), (int)Math.Round(base.WorldCoordinates.Y), this, -1);
			SendMessage(new SpawnEntityMessage(drone1, null));
			drone2 = new NathansDroneEntity((int)Math.Round(base.WorldCoordinates.X), (int)Math.Round(base.WorldCoordinates.Y), this, 1);
			SendMessage(new SpawnEntityMessage(drone2, null));
		}
		if (Abilities.SkillLevel[Skill.Drone] > 0)
		{
			SendMessage(new SpawnEntityMessage(new NathansDroneEntity((int)Math.Round(base.WorldCoordinates.X), (int)Math.Round(base.WorldCoordinates.Y), this, 0), null));
		}
	}

	public override bool SpawnFallFragments()
	{
		FragmentEntity entity = new FragmentEntity(base.WorldCenterCoordinates.Shift(-0.15f, -0.15f), SpriteName.nate_goggle_1, -1, new Vector4(-0.11f, -0.03f, 1.8f, 0.4f));
		SendMessage(new SpawnEntityMessage(entity, null));
		return true;
	}

	public override void TryTriggerAbility()
	{
		if (Dead || base.Falling)
		{
			return;
		}
		if (Abilities.SkillCharge[Skill.BreakTraps] < 1f)
		{
			base.playState.Hud.AbilitiesHud.skillPanels[Skill.BreakTraps].Shake();
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_still_charging)), CurrentPlatform));
			return;
		}
		List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 8f).FindAll((Entity e) => !e.IsBroken && (e is RotobladeEntity || e is SpikesEntity || e is CrossbowEntity || e is PistonEntity || e is SawEntity || e is ZapperEntity || e is FirewallEntity || e is GrillEntity || e is CannonEntity));
		if (list.Count > 0)
		{
			Abilities.SkillCharge[Skill.BreakTraps] = 0f;
			SendMessage(new PlaySoundMessage(SoundName.nate_warmup));
			DeactivateSpellEffects();
			int num = 0;
			int num2 = 0;
			foreach (Entity item in list)
			{
				SendMessage(new SpawnEntityMessage(new NathansCrosshairEntity((int)Math.Round(base.WorldCoordinates.X), (int)Math.Round(base.WorldCoordinates.Y), item, this, (num2 % 2 == 0) ? drone1 : drone2), null), num);
				num += 5;
				num2++;
			}
			spinDuration = num + 60;
			topSpinDuration = spinDuration;
			aimLight.TargetIntencity = 1f;
		}
		else
		{
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_TARGET_alert_no_targets)), CurrentPlatform));
		}
		base.TryTriggerAbility();
	}

	public override void Load()
	{
		aimLight = base.playState.LightManager.AddLight(default(Color).FromRgb(16711680), 10f, 0f, this);
		aimLight.FollowRate = 1f;
		aimLight.ChangeRate = 0.1f;
		base.Load();
	}

	public override void Update()
	{
		if (spinDuration > 0)
		{
			spinDuration--;
			if (spinDuration == 0)
			{
				base.core.CurrentPlayState.Hud.ShowAlert("break-traps", __(Abilities.SkillDesc[Skill.BreakTraps].Name), Color.DarkRed, 90, Abilities.SkillDesc[Skill.BreakTraps].HudMainIcon);
				aimLight.TargetIntencity = 0f;
				FacingDirection = new Vector2(0f, 1f);
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (spinDuration > 0)
		{
			int num = topSpinDuration - spinDuration;
			int num2 = spinDuration;
			Sprite sprite = ((num >= 5) ? ((num < 10) ? _(SpriteName.nathan_s_1) : ((num < 15) ? _(SpriteName.nathan_cast_1) : ((num < 20) ? _(SpriteName.nathan_cast_2) : ((num < 25) ? _(SpriteName.nathan_cast_3) : ((num2 > 20) ? _(SpriteName.nathan_cast_4) : ((num2 > 15) ? _(SpriteName.nathan_cast_3) : ((num2 > 10) ? _(SpriteName.nathan_cast_2) : _(SpriteName.nathan_cast_1)))))))) : ((FacingDirection.Y < 0f && num < 20) ? _(SpriteName.nathan_e_1) : _(SpriteName.nathan_s_1)));
			base.core.Renderer["fg", -2, false].FillScreen(Color.Black * num * 0.6f);
			base.core.Renderer["fg", 2, false].DrawSpriteW(sprite, base.WorldPosition + PosShift);
		}
		else
		{
			base.Draw();
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.nate_death));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_bow), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_arm), null));
		for (int i = 0; i < 2; i++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_gear_1), null));
		}
		for (int j = 0; j < 2; j++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_gear_2), null));
		}
		for (int k = 0; k < 3; k++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_shard), null));
		}
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_goggle_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_goggle_3), null));
		}
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 3f).OnSpawn(delegate(Particle p)
		{
			p.Velocity = SciHelper.GetRandomVectorInCircle(0.4f);
		}).OnUpdate(delegate(Particle p)
		{
			if (p.Age < 20)
			{
				p.Position += p.Velocity;
				p.Velocity += new Vector2(0f, 0.05f);
			}
			p.Dead = p.Age > 25;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer.DrawDotW(p.Position.X, p.Position.Y - 8f, default(Color).FromRgb(12194836) * ((float)(25 - p.Age) / 25f), 1f);
			})
			.Burst(20);
		return true;
	}

	public override bool Paralized()
	{
		if (!base.Paralized())
		{
			return spinDuration > 0;
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.nate_goggle_1), null));
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		if (dir <= 0)
		{
			return SpriteName.nathan_shot_1;
		}
		return SpriteName.nathan_shot;
	}
}
