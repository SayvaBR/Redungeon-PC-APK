using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class MageChar : PlayerEntity
{
	private int sloMoDuration;

	private int maxSloMoDuration;

	private ParticleEmitter sloMoEmitter;

	private bool teleporting;

	private int teleportingTime;

	private int teleportCooldown = -1;

	private float teleportDistance;

	private const int maxTeleportCooldown = 60;

	private const int portalInterval = 600;

	private int lastPortalAge;

	private int portalsCreated;

	private ParticleEmitter teleportEmitter;

	private Light staffLight;

	[Preserve]
	public MageChar(int x, int y)
		: base(x, y)
	{
		lastPortalAge = ((base.playState.Session.Distance == 0) ? 550 : 0);
		normalAnimSpeed = 0.1f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "mage_n_", "1234");
		animation.Add("e", "mage_e_", "1234");
		animation.Add("w", "mage_w_", "1234");
		animation.Add("s", "mage_s_", "1234");
		animation.Add("spin", "mage_fall_", "11112222");
		maxSloMoDuration = 60 * ((base.core.ProfileData.CurrentCharLevel < 3) ? 2 : 5);
		PosShift = new Vector2(-4f, -6f);
		teleportEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			Vector2 vector = new Vector2(Component._sin((float)base.worldTicks * 0.4f) + Component._cos((float)base.worldTicks * 0.2f), Component._cos((float)base.worldTicks * 0.5f) + Component._sin((float)base.worldTicks * 0.3f)) * 3f;
			p.Position += vector;
			p.Velocity = SciHelper.GetRandomVectorInCircle(0.5f);
			p.Offset.X = ((teleportCooldown == -1) ? 1f : (2f - (float)teleportCooldown / 60f));
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position += p.Velocity;
				p.Dead = p.Age > 50;
			})
			.OnDraw(delegate(Particle p)
			{
				float num = (float)p.Age / 50f;
				base.core.Renderer["fg", -10, false].DrawSpriteW(_(SpriteName.portal_glow), p.Position.Shift(-9f, -20f), Color.White * (1f - num), new Vector2(0.5f) * p.Offset.X / Component._M(0.1f, base.playState.Camera.Zoom));
			});
	}

	public override void Load()
	{
		staffLight = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(8439569), 2f, 0.3f, this);
		staffLight.FollowRate = 0.3f;
		staffLight.ChangeRate = 0.05f;
		base.Load();
	}

	public override bool InteractsWithWorld()
	{
		return !teleporting;
	}

	public override void TryTriggerAbility()
	{
		if (!Dead && !teleporting)
		{
			if (Abilities.SkillCharge[Skill.SloMo] < 1f)
			{
				base.playState.Hud.AbilitiesHud.skillPanels[Skill.SloMo].Shake();
				SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_still_charging)), CurrentPlatform));
				return;
			}
			Abilities.SkillCharge[Skill.SloMo] = 0f;
			sloMoDuration = maxSloMoDuration;
			base.playState.SloMoFactor = 0.3f;
			base.playState.SloMoAffectsPlayer = false;
			base.playState.SloMo = true;
			StartEmitter();
			SendMessage(new PlaySoundMessage(SoundName.aether_time_in));
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(Abilities.SkillDesc[Skill.SloMo].Name), default(Color).FromRgb(2656789), 1f, 30), CurrentPlatform));
			base.TryTriggerAbility();
		}
	}

	public override void Update()
	{
		if (Dead)
		{
			base.playState.SloMo = false;
			StopEmitter();
			base.Update();
			return;
		}
		if (!teleporting)
		{
			staffLight.TargetRadius = 1f;
			staffLight.TargetIntencity = 1f;
		}
		if (sloMoDuration > 0)
		{
			_inc(Stat.MageTicksInSloMo);
			int num = 30;
			float num2 = 1f;
			if (sloMoDuration < num)
			{
				num2 = (float)sloMoDuration / (float)num;
			}
			if (sloMoDuration > maxSloMoDuration - num)
			{
				num2 = (float)(maxSloMoDuration - sloMoDuration) / (float)num;
			}
			base.core.Renderer.PostEffectManager.DrunkA = 0.01f * num2;
			staffLight.TargetRadius += num2 * 10f;
			staffLight.TargetIntencity += num2 * 0.1f;
			base.core.AudioManager.MusicVolumeBox.Set("slo-mo", 0.2f, inWorld: true);
			sloMoDuration--;
			if (sloMoDuration == 40)
			{
				SendMessage(new PlaySoundMessage(SoundName.aether_time_out));
			}
			if (sloMoDuration == 0)
			{
				base.playState.SloMo = false;
				StopEmitter();
			}
		}
		if (teleporting)
		{
			base.core.AudioManager.MusicVolumeBox.Set("teleport", 0.2f, inWorld: true);
			teleportingTime++;
			if (teleportingTime < 20)
			{
				base.playState.Camera.ZoomBox.Set("teleport", 0.7f, inWorld: true, 0.2f, 0.01f);
			}
			if (teleportCooldown > 0)
			{
				teleportCooldown--;
				if (teleportCooldown == 0)
				{
					FinishTeleporting();
				}
			}
		}
		if (staffLight != null)
		{
			if (FacingDirection.X < 0f)
			{
				staffLight.Offset = new Vector2(-6f, -5f);
			}
			else if (FacingDirection.X > 0f)
			{
				staffLight.Offset = new Vector2(-3f, 5f);
			}
			else if (FacingDirection.Y < 0f)
			{
				staffLight.Offset = new Vector2(7f, 0f);
			}
			else if (FacingDirection.Y > 0f)
			{
				staffLight.Offset = new Vector2(-7f, 0f);
			}
		}
		if (base.playState.Started)
		{
			lastPortalAge++;
			if (lastPortalAge > 600)
			{
				TrySpawnPortal();
			}
		}
		base.Update();
	}

	private void TrySpawnPortal()
	{
		int num = (int)base.WorldCoordinates.X + Component._rnd(-5, 5);
		int num2 = (int)base.WorldCoordinates.Y - 5 + Component._rnd(-2, 0);
		Tile tile = base.levelMap[num, num2];
		Tile tile2 = base.levelMap[num, num2 + 1];
		Tile tile3 = base.levelMap[num, num2 - 1];
		Tile tile4 = base.levelMap[num + 1, num2];
		Tile tile5 = base.levelMap[num - 1, num2];
		if (tile != null && tile.IsPassableFor(this) && ((tile3 != null && tile3.IsPassableFloorFor(this)) || (tile4 != null && tile4.IsPassableFloorFor(this)) || (tile5 != null && tile5.IsPassableFloorFor(this)) || (tile2 != null && tile2.IsPassableFloorFor(this))))
		{
			SendMessage(new SpawnEntityMessage(new PortalEntity(num, num2, (portalsCreated == 0) ? 250 : 150), null));
			lastPortalAge = 0;
			portalsCreated++;
		}
	}

	private void StartEmitter()
	{
		StopEmitter();
		sloMoEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, Vector2.Zero, base.core.Renderer.ScreenWidth, base.core.Renderer.ScreenHeight).OnSpawn(delegate(Particle p)
		{
			p.Velocity = new Vector2(Component._rnd(-0.2f, 0.2f), Component._rnd(2f, 10f));
			p.Offset = p.Position.Clone();
		}).OnUpdate(delegate(Particle p)
		{
			p.Position = p.Offset + new Vector2(Component._cos((float)base.ticks * p.Velocity.X), Component._sin((float)base.ticks * p.Velocity.X)) * p.Velocity.Y;
			p.Dead = p.Age > 40;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer["fg", -99, false].DrawDotS(p.Position.X, p.Position.Y, default(Color).FromRgb(15660712) * ((float)(40 - p.Age) / 40f), Component._M(1f, p.Velocity.Y / 4f) * (float)p.Age / 40f);
			});
		sloMoEmitter.Start(10, 20);
	}

	private void StopEmitter()
	{
		if (sloMoEmitter != null)
		{
			sloMoEmitter.Stop();
		}
	}

	public override void Draw()
	{
		if (sloMoDuration > 0)
		{
			if (sloMoDuration > maxSloMoDuration - 30)
			{
				float num = (float)(maxSloMoDuration - sloMoDuration) / 30f;
				Vector2 vector = new Vector2(0f, -8f);
				if (FacingDirection.Y > 0f || FacingDirection.X < 0f)
				{
					vector.X = -6f;
				}
				if (FacingDirection.Y < 0f)
				{
					vector.X = 7f;
				}
				Renderer renderer = base.core.Renderer[base.Z + 5];
				Sprite sprite = _(SpriteName.camera_flash);
				Vector2 position = base.WorldCenter + vector;
				Color? tint = default(Color).FromRgb(16777215);
				float rotation = num * 2f;
				renderer.DrawSpriteW(sprite, position, tint, new Vector2(Component._sin((float)Math.PI * num) * 1.5f), rotation, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer[base.Z - 2].DrawSpriteW(_(SpriteName.glow_big), base.WorldCenter, default(Color).FromRgb(8439569) * Component._sin((float)Math.PI * num), new Vector2(1f + 0.5f * num), 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			float num2 = Component._m(Component._sin((float)sloMoDuration * (float)Math.PI / (float)maxSloMoDuration) * 5f, 1f);
			base.core.Renderer["bg", base.Z + 300, false].DrawSpriteW(_(SpriteName.mage_hourglass), base.WorldCenter + base.dAnim, Color.White * 0.1f, rotation: (float)Math.PI * (float)sloMoDuration / 600f, scale: new Vector2(num2), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			if (!base.core.CurrentPlayState.Paused)
			{
				float num3 = 28f * num2;
				float num4 = num3 * (float)sloMoDuration / (float)maxSloMoDuration;
				RectangleF rectangleF = new RectangleF(base.WorldCenter.X + base.dAnim.X - num3 / 2f, base.WorldCenter.Y + base.dAnim.Y + 8f + 20f * (1f - num2), num3, 2.5f);
				rectangleF = rectangleF.Grow(-1f, -1f, 1f, 1f);
				base.core.Renderer["fg", -10, false].DrawRectangleW(rectangleF, Color.Black * num2);
				rectangleF = rectangleF.Grow(1f, 1f, -1f, -1f);
				base.core.Renderer["fg", -10, false].DrawRectangleW(rectangleF, default(Color).FromRgb(1583617) * num2);
				rectangleF.Width = num4;
				rectangleF.X += (num3 - num4) / 2f;
				base.core.Renderer["fg", -10, false].DrawRectangleW(rectangleF, default(Color).FromRgb(3379989) * num2);
			}
		}
		if (teleporting)
		{
			Vector2 vector2 = new Vector2(Component._sin((float)base.worldTicks * 0.4f) + Component._cos((float)base.worldTicks * 0.2f), Component._cos((float)base.worldTicks * 0.5f) + Component._sin((float)base.worldTicks * 0.3f)) * 3f;
			base.core.Renderer["fg", -10, false].DrawSpriteW(_(SpriteName.portal_glow), base.WorldCenter.Shift(-15f, -29f) + vector2, null, Vector2.One / Component._M(0.1f, base.playState.Camera.Zoom));
			if (teleportingTime < 2 || (teleportCooldown > 0 && teleportCooldown < 3))
			{
				base.core.Renderer["fg", -20, false].FillScreen(default(Color).FromRgb(2774129));
			}
		}
		else
		{
			base.Draw();
		}
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.aether_death));
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 4f).OnSpawn(delegate
		{
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += new Vector2((0f - p.Offset.X) / 70f, -3f + p.Offset.Y / 5f);
			p.Dead = p.Age > 70;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z + 5].DrawSpriteW(_(SpriteName.glow_big), p.Position, ((p.Offset.Y > 2f) ? default(Color).FromRgb(12070200) : ((p.Offset.Y > -1.8f) ? default(Color).FromRgb(9511981) : default(Color).FromRgb(16755868))) * ((float)(70 - p.Age) / 70f), new Vector2(0.08f, (float)(p.Age + 10) / 70f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Emit(1, 1, once: true, 30);
		if (!bolt)
		{
			FragmentEntity fragmentEntity = new FragmentEntity(base.WorldCenterCoordinates, SpriteName.mage_staff);
			SendMessage(new SpawnEntityMessage(fragmentEntity, null));
			staffLight.Follow(fragmentEntity);
			staffLight.FollowRate = 1f;
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(8439569), 1f, 0.6f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 0.05f;
		FragmentEntity fragmentEntity = new FragmentEntity(pos, SpriteName.mage_staff);
		SendMessage(new SpawnEntityMessage(fragmentEntity, null));
		light.Follow(fragmentEntity);
		return true;
	}

	protected override void StopFlying()
	{
		if (teleporting)
		{
			teleportCooldown = 60;
			staffLight.TargetIntencity = 1f;
		}
		base.StopFlying();
	}

	private void FinishTeleporting()
	{
		teleporting = false;
		teleportingTime = 0;
		teleportCooldown = -1;
		teleportEmitter.Pause();
		SendMessage(new PlaySoundMessage(SoundName.aether_portal_exit));
	}

	public void Teleport()
	{
		Vector2 coordinates = base.core.CurrentPlayState.LevelGenerator.NextSafePoint(base.WorldCoordinates);
		bool flag = false;
		while (!flag)
		{
			Vector2 vector = base.core.CurrentPlayState.LevelGenerator.NextSafePoint(coordinates);
			flag = base.WorldCoordinates.Y - vector.Y >= (float)Component._rnd(10, 20) || vector.Equals(Vector2.Zero);
			if (coordinates.Equals(vector))
			{
				break;
			}
			if (!vector.Equals(Vector2.Zero))
			{
				coordinates = vector.Clone();
			}
		}
		int num = (int)Math.Round(coordinates.X - base.WorldCoordinates.X);
		int num2 = (int)Math.Round(coordinates.Y - base.WorldCoordinates.Y);
		teleporting = true;
		SuspendedStartFlying(num, num2, 0.15f, ignoreObstacles: true, changeCourse: true);
		FacingDirection = new Vector2(0f, -1f);
		staffLight.TargetIntencity = 0f;
		teleportEmitter.Start(1);
		if (sloMoDuration > 0)
		{
			sloMoDuration = 1;
		}
		teleportDistance = (FlightTarget - base.WorldCoordinates).Value.LengthSquared();
		SendMessage(new PlaySoundMessage(SoundName.aether_portal_enter));
	}

	public override bool Paralized()
	{
		if (!teleporting)
		{
			return base.Paralized();
		}
		return true;
	}

	protected override bool TryResistFall()
	{
		if (!teleporting)
		{
			return base.TryResistFall();
		}
		return true;
	}

	public override bool SpawnFallFragments()
	{
		staffLight.Active = false;
		Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(8439569), 1f, 0.6f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 0.05f;
		FragmentEntity fragmentEntity = new FragmentEntity(base.WorldCenterCoordinates.Shift(-0.15f, -0.15f), SpriteName.mage_staff, -1, new Vector4(-0.11f, -0.03f, 1.8f, 0.4f));
		SendMessage(new SpawnEntityMessage(fragmentEntity, null));
		light.Follow(fragmentEntity);
		return true;
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		if (!teleporting || teleportingTime <= 0)
		{
			return base.TryResist(injuryType, offender);
		}
		return true;
	}

	public override bool TryResistSpell(SpellType spellType, Entity offender = null)
	{
		if (teleporting && teleportingTime > 0)
		{
			return true;
		}
		return base.TryResistSpell(spellType, offender);
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.mage_shot;
	}
}
