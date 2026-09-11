using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class VesnaChar : PlayerEntity
{
	private int maxLightDuration = 180;

	private bool waitingForDarkness;

	private bool waitIsOver;

	private Light ritualLight;

	public int LightDuration { get; private set; }

	[Preserve]
	public VesnaChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.1f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "vesna_n", "1234");
		animation.Add("e", "vesna_e", "1234");
		animation.Add("w", "vesna_w", "1234");
		animation.Add("s", "vesna_s", "1234");
		animation.Add("spin", "vesna_fall_", "1111122222");
		PosShift = new Vector2(-3f, -10f);
	}

	public override void Load()
	{
		ritualLight = base.playState.LightManager.AddLight(default(Color).FromRgb(16759296), 10f, 0f, this);
		ritualLight.FollowRate = 1f;
		ritualLight.ChangeRate = 0.05f;
		base.Load();
	}

	public override void TryTriggerAbility()
	{
		if (Dead || LightDuration > 0)
		{
			return;
		}
		if (Abilities.SkillCharge[Skill.ResistDarkness] < 1f)
		{
			base.playState.Hud.AbilitiesHud.skillPanels[Skill.ResistDarkness].Shake();
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_still_charging)), CurrentPlatform));
			return;
		}
		base.core.CurrentPlayState.Hud.ShowAlert("resist-darkness", __(Abilities.SkillDesc[Skill.ResistDarkness].Name), Color.Orange, 90, Abilities.SkillDesc[Skill.ResistDarkness].HudMainIcon);
		base.SpellEffects[SpellType.Ice].Deactivate();
		base.SpellEffects[SpellType.Darkness].Deactivate();
		Abilities.SkillCharge[Skill.ResistDarkness] = 0f;
		LightDuration = maxLightDuration;
		if (base.playState.Terminator > base.WorldPosition.Y + 45f)
		{
			base.playState.TerminatorTarget = base.WorldPosition.Y + 45f;
		}
		waitingForDarkness = true;
		waitIsOver = false;
		base.TryTriggerAbility();
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		if (!base.TryResist(injuryType, offender))
		{
			if (injuryType == InjuryType.Timeout)
			{
				return LightDuration > 0;
			}
			return false;
		}
		return true;
	}

	public override bool Paralized()
	{
		if (!base.Paralized())
		{
			return LightDuration > 0;
		}
		return true;
	}

	public override void Update()
	{
		if (base.Falling)
		{
			LightDuration = 0;
		}
		if (LightDuration > 0)
		{
			base.core.AudioManager.MusicVolumeBox.Set("sudden sunrise", 0.8f, inWorld: true, 0.05f, 0.02f);
			int num = maxLightDuration - LightDuration;
			if (num < 15)
			{
				base.playState.Camera.ZoomBox.Set("sudden sunrise", 0.8f, inWorld: true, 0.07f, 0.02f);
			}
			var _ = LightDuration;
			waitingForDarkness = base.playState.Terminator >= base.WorldPosition.Y + 40f && !waitIsOver;
			LightDuration--;
			if (LightDuration > 20)
			{
				base.playState.Camera.YOffsetBox.Set("sudden sunrise", (float)base.core.Renderer.ScreenHeight * 0.25f, inWorld: true, 0.04f, 0.06f);
			}
			if (waitingForDarkness)
			{
				LightDuration = (int)Component._M(LightDuration, maxLightDuration - 6);
			}
			else
			{
				if (!waitIsOver)
				{
					SendMessage(new PlaySoundMessage(SoundName.vesna_rutial_background));
				}
				waitIsOver = true;
				ritualLight.TargetIntencity = 1.1f;
				base.playState.TerminatorLightModifier = Component._m(0f, (float)(maxLightDuration - LightDuration) / 40f);
				if (num % 5 == 0)
				{
					int num2 = num / 5;
					Vector2 v = Vector2.Zero;
					switch (num2)
					{
					case 2:
						v = new Vector2(-2f, 0f);
						break;
					case 3:
						v = new Vector2(-1f, 1f);
						break;
					case 4:
						v = new Vector2(1f, 1f);
						break;
					case 5:
						v = new Vector2(2f, 0f);
						break;
					case 10:
						v = new Vector2(-3f, 1f);
						break;
					case 11:
						v = new Vector2(-2f, 2f);
						break;
					case 12:
						v = new Vector2(0f, 2.3f);
						break;
					case 13:
						v = new Vector2(2f, 2f);
						break;
					case 14:
						v = new Vector2(3f, 1f);
						break;
					}
					switch (num2)
					{
					case 3:
						base.playState.TerminatorTarget += 10f;
						SendMessage(new PlaySoundMessage(SoundName.vesna_rutial_part_1));
						base.playState.Camera.Shake("light beam 1", 4f, 15);
						break;
					case 11:
						base.playState.TerminatorTarget += 25f;
						SendMessage(new PlaySoundMessage(SoundName.vesna_rutial_part_2));
						base.playState.Camera.Shake("light beam 2", 4f, 15);
						break;
					}
					if (!v.IsEqualTo(Vector2.Zero))
					{
						SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X + v.X, base.WorldCoordinates.Y + v.Y), null));
					}
				}
				if (LightDuration == 80)
				{
					base.playState.TerminatorDontKeepUp = true;
					base.playState.TerminatorTarget += 220f;
					SendMessage(new PlaySoundMessage(SoundName.vesna_rutial_part_3));
					base.playState.Camera.Shake("light beam 3", 5f, 15);
					SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X - 4f, base.WorldCoordinates.Y + 4f, bigger: true), null));
					SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X - 2.8f, base.WorldCoordinates.Y + 5.4f, bigger: true), null));
					SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X - 1.1f, base.WorldCoordinates.Y + 6f, bigger: true), null));
					SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X + 1.1f, base.WorldCoordinates.Y + 6f, bigger: true), null));
					SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X + 2.8f, base.WorldCoordinates.Y + 5.4f, bigger: true), null));
					SendMessage(new SpawnEntityMessage(new VesnaBeamEntity(base.WorldCoordinates.X + 4f, base.WorldCoordinates.Y + 4f, bigger: true), null));
				}
			}
			if (LightDuration == 0)
			{
				FacingDirection = new Vector2(0f, 1f);
			}
		}
		else
		{
			ritualLight.TargetIntencity = 0f;
			base.playState.TerminatorLightModifier = 1f;
		}
		base.Update();
	}

	public override void Draw()
	{
		if (LightDuration > 0)
		{
			int num = maxLightDuration - LightDuration;
			int lightDuration = LightDuration;
			Sprite sprite = ((num >= 5) ? ((num < 10) ? _(SpriteName.vesna_s1) : ((num < 15) ? _(SpriteName.vesna_cast_1) : ((num < 20) ? _(SpriteName.vesna_cast_2) : ((lightDuration > 15) ? _(SpriteName.vesna_cast_3) : ((lightDuration > 10) ? _(SpriteName.vesna_cast_2) : _(SpriteName.vesna_cast_1)))))) : ((FacingDirection.Y < 0f && num < 20) ? _(SpriteName.vesna_e1) : _(SpriteName.vesna_s1)));
			if (num > 10)
			{
				int num2 = (maxLightDuration - 10) / 2;
				float num3 = (float)num - 10f;
				if (num3 > (float)num2)
				{
					num3 = (float)(num2 * 2) - num3;
				}
				num3 = Component._m(num3 / 10f, 1f);
				num3 *= 0.8f;
				base.core.Renderer[base.Z + 1].DrawRectangleW(new RectangleF(base.WorldCenter.X - 9.5f * num3, base.WorldCenter.Y + 3f - 300f, 19f * num3, 300f), default(Color).FromRgb(16430137));
				base.core.Renderer[base.Z + 1].DrawRectangleW(new RectangleF(base.WorldCenter.X - 3.5f * num3, base.WorldCenter.Y + 3f - 300f, 7f * num3, 300f), default(Color).FromRgb(16763709));
				base.core.Renderer[base.Z + 1].DrawRectangleW(new RectangleF(base.WorldCenter.X - 1.5f * num3, base.WorldCenter.Y + 3f - 300f, 3f * num3, 300f), Color.White);
				base.core.Renderer[base.Z + 1].DrawSpriteW(_(SpriteName.vesna_beam_base), base.WorldCenter.Shift(0f, 3f), null, new Vector2(num3, 1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			base.core.Renderer[base.Z + 1].DrawSpriteW(sprite, base.WorldPosition + PosShift);
			base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldPosition.Shift(0f, 12f) + PosShift + base.dAnim, Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
			LastSpritePos = base.WorldPosition + PosShift;
		}
		else
		{
			base.Draw();
		}
	}

	private void SpawnGrass()
	{
		SendMessage(new PlaySoundMessage(SoundName.vesna_death));
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 4f).OnSpawn(delegate(Particle p)
		{
			p.Velocity.X = (float)Math.Atan2(p.Offset.Y, p.Offset.X);
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += p.Offset * 0.35f;
			p.Dead = p.Age > 30;
		})
			.OnDraw(delegate(Particle p)
			{
				float num3 = (float)(30 - p.Age) / 30f;
				base.core.Renderer[base.Z + 5].DrawSpriteW(_(SpriteName.glow_big), p.Position, default(Color).FromRgb(8629553) * (1f - num3), new Vector2(1.5f * num3, num3 * (1f - num3)) * 0.5f, p.Velocity.X, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Emit(2, 5, once: true, 10);
		for (int num = -2; num <= 2; num++)
		{
			for (int num2 = -2; num2 <= 2; num2++)
			{
				if ((Math.Abs(num) != 2 || Math.Abs(num2) != 2) && base.CurrentMap[base.Coordinates.X + (float)num, base.Coordinates.Y + (float)num2] is DungeonTile dungeonTile)
				{
					dungeonTile.GrowGrass(10 + (Math.Abs(num) + Math.Abs(num2)) * 5);
				}
			}
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 4f).OnSpawn(delegate
		{
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += new Vector2((0f - p.Offset.X) / 70f, -3f + p.Offset.Y / 5f);
			p.Dead = p.Age > 70;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z + 5].DrawSpriteW(_(SpriteName.glow_big), p.Position, ((p.Offset.Y > 2f) ? default(Color).FromRgb(6070870) : ((p.Offset.Y > -1.8f) ? default(Color).FromRgb(13914170) : default(Color).FromRgb(15905130))) * ((float)(70 - p.Age) / 70f), new Vector2(0.08f, (float)(p.Age + 10) / 70f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Emit(1, 1, once: true, 30);
		SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates, "dust_", "1234"), CurrentPlatform));
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.vesna_flower, -1, new Vector4(0f, 0f, 2f, 0.5f)), null));
		}
		SpawnGrass();
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.vesna_flower), null));
		if (!bolt)
		{
			SpawnGrass();
		}
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.vesna_shot;
	}
}
