using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class CreepChar : PlayerEntity
{
	private int lastFrame;

	private const int r = 8;

	private int scareDuration;

	private int maxScareDuration = 50;

	private bool slideFrameReached;

	private bool frozen;

	[Preserve]
	public CreepChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.2f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("jump", "creep_", "123345");
		animation.Play("jump");
		customAnimation = true;
		PosShift = new Vector2(-3f, -17f);
		base.ZappedSprite = SpriteName.zapped_melon;
	}

	public override void InitStepSounds()
	{
		StepSounds.Put(SoundName.creep_step_1);
		StepSounds.Put(SoundName.creep_step_2);
		StepSounds.Put(SoundName.creep_step_3);
	}

	public override void PlayStepSound()
	{
	}

	public override void Jump(Vector2 direction)
	{
		base.Jump(direction);
	}

	public override void Update()
	{
		frozen = base.SpellEffects[SpellType.Ice].Active;
		if (frozen && (animation.GetCurrentFrameNumber() == 2 || animation.GetCurrentFrameNumber() == 3))
		{
			animation.FrameBack();
			animation.FrameBack();
		}
		if (scareDuration > 0)
		{
			scareDuration--;
			if (scareDuration > maxScareDuration - 25)
			{
				base.playState.Camera.ZoomBox.Set("boo!", 1.9f + 0.2f * Component._sin((float)base.worldTicks * 0.05f), inWorld: true, 0.07f, 0.05f);
			}
			if (scareDuration == maxScareDuration - 1)
			{
				base.core.CurrentPlayState.Hud.ShowAlert("scare-creatures", __(Abilities.SkillDesc[Skill.ScareCreatures].Name), CharDescription.Get[Character.Creep].Color1, 90, Abilities.SkillDesc[Skill.ScareCreatures].HudMainIcon);
			}
		}
		if (BSlide.Sliding)
		{
			animation.Update();
			animation.Update();
			animation.Update();
			animation.Update();
			if (!slideFrameReached && animation.GetCurrentFrameNumber() == 5)
			{
				slideFrameReached = true;
				animation.Pause();
			}
		}
		else
		{
			slideFrameReached = false;
			if (animation.Paused)
			{
				animation.Play();
			}
		}
		int currentFrameNumber = animation.GetCurrentFrameNumber();
		if (base.Age > 120 && currentFrameNumber == 1 && currentFrameNumber != lastFrame && !Dead && !base.Falling && !base.Flying && scareDuration == 0)
		{
			SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates.Shift(0f, 0.2f), "dust_", "1234"), CurrentPlatform));
			SendMessage(new PlayWorldSoundMessage(StepSounds.DrawDifferent(), base.WorldCenter, 0.2f));
		}
		lastFrame = currentFrameNumber;
		base.Update();
	}

	public override void TryTriggerAbility()
	{
		if (Dead)
		{
			return;
		}
		if (Abilities.SkillCharge[Skill.ScareCreatures] < 1f)
		{
			base.playState.Hud.AbilitiesHud.skillPanels[Skill.ScareCreatures].Shake();
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_still_charging)), CurrentPlatform));
			return;
		}
		List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 8f).FindAll((Entity c) => !c.IsBroken && (c is BatEntity || c is SlimeEntity || (c is FollowerEntity && !(c as FollowerEntity).Important) || c is WispEntity || (c is SerpentEntity && !(c as SerpentEntity).IsChineseDragon && (c as SerpentEntity).Part == SerpentEntity.SerpentPart.Head)));
		if (list.Count > 0)
		{
			_inc(Stat.CreepCreaturesScared, list.Count);
			Abilities.SkillCharge[Skill.ScareCreatures] = 0f;
			scareDuration = maxScareDuration;
			int num = 0;
			foreach (Entity item in list)
			{
				item.Break(this);
				ItemEntity itemEntity = new ItemEntity(item.WorldCenterCoordinates.X - 0.5f, item.WorldCenterCoordinates.Y - 0.5f, ItemEntity.ValueToType(base.playState.LevelGenerator.AvgCoinValue()));
				itemEntity.SetTarget(this, 70 + num * 15);
				SendMessage(new SpawnEntityMessage(itemEntity, null));
				num++;
			}
			base.playState.Camera.Shake("scaring", 1f, maxScareDuration);
			SendMessage(new PlaySoundMessage(SoundName.creep_scare));
			base.SpellEffects[SpellType.Ice].Deactivate();
		}
		else
		{
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_BOO_alert_no_one_to_scare)), CurrentPlatform));
		}
		base.TryTriggerAbility();
	}

	public override bool WebCapture(WebEntity web)
	{
		return false;
	}

	protected override bool TryResistFall()
	{
		bool flag = false;
		if (Abilities.SkillLevel[Skill.Bridger] > 0 && (base.playState.TileMap[base.WorldCoordinates - FacingDirection].WorldCoordinates - tileBeforeJump.WorldCoordinates).IsEqualTo(Vector2.Zero) && base.worldTicks - lastJumpTick < 2)
		{
			Tile tile = base.playState.TileMap[base.WorldCoordinates + FacingDirection];
			if (tile != null && tile.IsPassableFor(this) && (tile.Type != TileType.Pit || tile.ContainsPlatform()))
			{
				flag = true;
				SuspendedStartFlying((int)FacingDirection.X, (int)FacingDirection.Y, 0.04f, ignoreObstacles: false, changeCourse: true);
				SendMessage(new PlaySoundMessage(SoundName.swoosh_1));
				Vector2 trailDir = FacingDirection.Clone();
				int trailZ = base.Z;
				ParticleEmitter trailEmitter = null;
				trailEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldPosition - FacingDirection * 16f).OnSpawn(delegate(Particle p)
				{
					p.Aux.X = trailEmitter.BornCount;
				}).OnUpdate(delegate(Particle p)
				{
					p.Dead = p.Age > 15;
				})
					.OnDraw(delegate(Particle p)
					{
						float num = 1f - (float)p.Age / 15f;
						base.core.Renderer[trailZ].DrawSpriteW(_("creep_" + (int)(p.Aux.X + 1f)), p.Position + PosShift + trailDir * p.Aux.X * 7.5f, Color.White * 0.5f * num);
					})
					.Emit(4, 2);
			}
		}
		if (!flag)
		{
			return base.TryResistFall();
		}
		return true;
	}

	public override bool TryResist(InjuryType injuryType, Entity offender = null)
	{
		if (scareDuration > 0)
		{
			return true;
		}
		return base.TryResist(injuryType, offender);
	}

	public override void Draw()
	{
		if (scareDuration > 0)
		{
			float num = Component._m(Component._sin((float)scareDuration * (float)Math.PI / (float)maxScareDuration) * 2f, 1f);
			base.core.Renderer["fg", -2, false].FillScreen(Color.Black * num * 0.6f);
			Vector2 vector = base.WorldCenter.Shift(0f, -10f - 20f * num);
			float num2 = 1f;
			base.core.Renderer["fg", 3, false].DrawSpriteW(_(SpriteName.creep_body), vector.Shift(0f, 8f), null, new Vector2(num * (num2 + Component._sin(base.worldTicks) * 0.1f)), 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
			base.core.Renderer["fg", 4, false].DrawSpriteW(_(SpriteName.creep_head), vector, null, new Vector2(num * (num2 + Component._sin(base.worldTicks) * 0.1f)), Component._sin((float)base.worldTicks * 0.3f) * 0.2f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg", 3, false].DrawSpriteW(_(SpriteName.creep_arm_right), vector.Shift(10f * num, 17f), null, new Vector2(num * num2), Component._sin((float)base.worldTicks * 0.5f) * 0.4f + 0.2f, SpriteFlip.None, SpriteOrigin.BottomLeft);
			base.core.Renderer["fg", 3, false].DrawSpriteW(_(SpriteName.creep_arm_left), vector.Shift(-10f * num, 17f), null, new Vector2(num * num2), (0f - Component._sin((float)base.worldTicks * 0.4f)) * 0.4f - 0.2f, SpriteFlip.None, SpriteOrigin.BottomRight);
			base.core.Renderer["fg", 2, false].DrawSpriteW(_(SpriteName.glow_huge), vector, default(Color).FromRgb(12379508) * (num * 0.6f), new Vector2(num * 1.5f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		int num3 = ((fallAnim != 0) ? (((fallAnim - 10) * (fallAnim - 10) - 100) / 8) : 0);
		Sprite sprite = ((fallAnim == 0) ? animation.GetCurrentFrame() : animation.GetFrame(4));
		float num4 = (frozen ? 0f : ((float)Math.Sin((float)(base.worldTicks + 15) * 0.1f) * 1.5f));
		float rotation = (frozen ? 0f : ((float)Math.Sin((float)(base.worldTicks + 15) * 0.1f) * 0.05f));
		Renderer renderer = ((base.Falling && num3 > 0) ? base.core.Renderer["bg", base.Z + 1, false] : base.core.Renderer[base.Z + 1]);
		float num5 = 1f - ((float)fallAnim - 20f) / 20f;
		LastSpriteShift = base.dAnim.Shift(0f, (float)num3 - dropAnim);
		LastSpritePos = base.WorldPosition + LastSpriteShift + PosShift;
		LastSpritePos = base.WorldPosition + PosShift.Shift(num4, 0f) + base.dAnim + new Vector2(0f, (float)num3 - dropAnim);
		LastLayer = ((base.Falling && num3 > 0) ? "bg" : "default");
		LastSpriteAlpha = burnOpacity * ((fallAnim == 0) ? 1f : (num5 * num5 * num5));
		LastZ = base.Z + 1;
		if (scareDuration < 10 || scareDuration > maxScareDuration - 10)
		{
			renderer.DrawSpriteW(sprite, LastSpritePos, ((fallAnim == 0) ? Color.White : Color.Lerp(Color.Black, Color.White, num5 * num5 * num5)) * burnOpacity, null, rotation);
		}
		if (num3 <= 0)
		{
			renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldPosition.Shift(num4, 25f) + PosShift + base.dAnim - new Vector2(0f, (float)num3 - dropAnim * 0.5f), Color.Black * 0.2f * burnOpacity, new Vector2(1f, 0.8f), rotation, SpriteFlip.Vertical);
		}
		DrawBurningFlame();
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.creep_death));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.creep_slice_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.creep_slice_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.creep_slice_2), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.creep_slice_3), null));
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.creep_stick), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.creep_arms), null));
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
				base.core.Renderer.DrawDotW(p.Position.X, p.Position.Y - 8f, default(Color).FromRgb(13971991) * ((float)(25 - p.Age) / 25f), 1f);
			})
			.Burst(20);
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		if (bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.creep_stick), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.creep_arms), null));
		}
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.creep_shot;
	}
}
