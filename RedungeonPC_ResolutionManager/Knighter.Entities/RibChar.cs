using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class RibChar : PlayerEntity
{
	private bool reviving;

	private int reviveTimer;

	private const int reviveDuration = 100;

	private Light light;

	[Preserve]
	public RibChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.25f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "ribb_n_", "1122233444");
		animation.Add("s", "ribb_s_", "1122233444");
		animation.Add("w", "ribb_w_", "1122233444");
		animation.Add("e", "ribb_e_", "1122233444");
		animation.Add("spin", new List<SpriteName>
		{
			SpriteName.ribb_n_1,
			SpriteName.ribb_e_1,
			SpriteName.ribb_s_1,
			SpriteName.ribb_w_1
		});
		PosShift = new Vector2(-3f, -7f);
		ShadowShift = new Vector2(0f, 4f);
	}

	public override void InitStepSounds()
	{
		StepSounds.Put(SoundName.ribb_step_1);
		StepSounds.Put(SoundName.ribb_step_2);
	}

	public override bool Paralized()
	{
		if (!reviving)
		{
			return base.Paralized();
		}
		return true;
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(9563694), 1.5f, 0f, this);
		light.FollowRate = 0.2f;
		light.ChangeRate = 0.05f;
		base.Load();
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		if (reviving)
		{
			return true;
		}
		base.playState.Camera.Shake("Ribb's resist");
		bool flag = false;
		if ((uint)injuryType > 1u && injuryType != InjuryType.Timeout && Abilities.SkillLevel[Skill.SpareSkull] > 0)
		{
			_inc(Stat.RibSkullsLost);
			flag = true;
			if (!StopFall())
			{
				return true;
			}
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rib_skull, 60, new Vector4(0f, 0f, 2f, 0.5f), "fg"), null));
			reviving = true;
			reviveTimer = 100;
			DeactivateSpellEffects();
		}
		if (!flag)
		{
			return base.TryResist(injuryType, offender);
		}
		return true;
	}

	public override bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		if (reviving)
		{
			return true;
		}
		return base.TryResistSpell(spellType, offender);
	}

	protected override bool TryResistFall()
	{
		if (reviving)
		{
			return true;
		}
		return base.TryResistFall();
	}

	private void StartReviveEffect()
	{
		Abilities.SkillLevel[Skill.SpareSkull]--;
		Vector2 vector = base.core.CurrentPlayState.LevelGenerator.NextSafePoint(base.WorldCoordinates);
		float dx = vector.X - base.WorldCoordinates.X;
		float dy = vector.Y - base.WorldCoordinates.Y;
		SuspendedStartFlying(dx, dy, 0.05f, ignoreObstacles: true, changeCourse: true);
		FacingDirection = new Vector2(0f, 1f);
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 4f).OnSpawn(delegate(Particle p)
		{
			p.Position += p.Offset * 5f;
		}).OnUpdate(delegate(Particle p)
		{
			p.Dead = p.Age > 30;
			p.Position -= p.Offset * 0.2f;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer["fg", -1, false].DrawSpriteW(_(SpriteName.glow_big), p.Position, default(Color).FromRgb(8439569) * 0.7f, new Vector2(0.2f * (float)p.Age / 30f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.AttachTo(this)
			.Emit(30, 1, once: true, 2);
	}

	public override void Update()
	{
		if (reviveTimer > 0)
		{
			if (reviveTimer == 99)
			{
				SendMessage(new PlaySoundMessage(SoundName.ribb_break));
			}
			reviveTimer--;
			if (reviveTimer > 85)
			{
				base.playState.Camera.ZoomBox.Set("ribb revive", 1.5f, inWorld: true, 0.05f, 0.02f);
			}
			if (reviveTimer == 70)
			{
				StartReviveEffect();
				SendMessage(new PlaySoundMessage(SoundName.ribb_fix), 35);
			}
			if (reviveTimer == 0)
			{
				reviving = false;
				AnnounceAbilityStatus(Abilities.SkillDesc[Skill.SpareSkull], Abilities.SkillLevel[Skill.SpareSkull]);
			}
		}
		if (reviving)
		{
			light.TargetRadius = 3.5f;
			light.TargetIntencity = 0.7f;
		}
		else
		{
			light.TargetRadius = 1.5f;
			light.TargetIntencity = 0f;
		}
		base.Update();
	}

	public override void Draw()
	{
		if (reviving)
		{
			float num = Component._m(Component._sin((float)reviveTimer * (float)Math.PI / 100f) * 2f, 1f);
			base.core.Renderer["fg", -2, false].FillScreen(Color.Black * num * 0.6f);
			int num2 = 7;
			for (int i = 0; i < num2; i++)
			{
				float num3 = (float)i * (float)Math.PI * 2f / (float)num2 + (float)base.worldTicks * 0.08f;
				Vector2 position = base.WorldCenter.Clone();
				float num4 = 20f * num;
				position.X += Component._cos(num3) * num4;
				position.Y += Component._sin(num3) * num4;
				Sprite sprite = ((i == 4) ? _(SpriteName.rib_bone_1) : ((i > 2) ? _(SpriteName.rib_bone_2) : _(SpriteName.rib_bone_1)));
				base.core.Renderer["fg", -1, false].DrawSpriteW(sprite, position, null, null, (float)(-(base.worldTicks + i * 20)) * 0.2f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		else
		{
			base.Draw();
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.ribb_death));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rib_skull), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rib_bone_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rib_bone_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rib_bone_2), null));
		for (int i = 0; i < 4; i++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.rib_bone_3), null));
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.ribb_death));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rib_bone_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rib_bone_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rib_bone_2), null));
		for (int i = 0; i < 4; i++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.rib_bone_3), null));
		}
		return true;
	}
}
