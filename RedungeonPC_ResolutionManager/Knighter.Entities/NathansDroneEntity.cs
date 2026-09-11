using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class NathansDroneEntity : Entity
{
	private Entity host;

	private PlayerEntity player;

	private Animation anim;

	private const int laserInterval = 300;

	private int untilLaserReady = 300;

	private Entity target;

	private Entity fastTarget;

	private int fastTargetDelay;

	private const int targetBreakDelay = 50;

	private int untilTargetBroken = 50;

	private Vector2 crosshairPos;

	private Vector2 dPos;

	private int side;

	private int breakTimer;

	private Light light;

	public static BagOf<SoundName> Beeps;

	private int tillNextBeep;

	static NathansDroneEntity()
	{
		Beeps = new BagOf<SoundName>().Put(SoundName.drone_beep_1).Put(SoundName.drone_beep_2).Put(SoundName.drone_beep_3)
			.Put(SoundName.drone_beep_4);
	}

	public NathansDroneEntity(int x, int y, PlayerEntity player, int side)
		: base(x, y, 0f, 0f)
	{
		SetFlying(value: true);
		this.player = player;
		host = player;
		this.side = side;
		if (side == -1)
		{
			untilLaserReady -= 150;
		}
		anim = new Animation();
		anim.Add("fly", "nathan_drone_", "123");
		anim.Play("fly");
		Beep();
	}

	private void Beep()
	{
		SendMessage(new PlayWorldSoundMessage(Beeps.DrawDifferent(), base.WorldCenter));
		tillNextBeep = Component._rnd(100, 200);
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(Color.Red, 2f, 0f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 0.2f;
		base.Load();
	}

	public override void Update()
	{
		anim.Update();
		if (fastTargetDelay > 0)
		{
			fastTargetDelay--;
			if (fastTargetDelay == 5 && fastTarget != null && !fastTarget.IsBroken)
			{
				fastTarget.Break(null);
			}
		}
		if (player.Dead)
		{
			float num = 0.2f * (1f - (float)breakTimer / 30f) * (float)((side == 0) ? 1 : side);
			x += Component._cos((float)base.worldTicks * 0.5f) * num;
			y += Component._sin((float)base.worldTicks * 1f) * num;
			breakTimer++;
			if (breakTimer == 10)
			{
				Break(null);
			}
			return;
		}
		float num2 = ((host == player) ? 0.04f : 0.02f);
		x += (host.WorldCenterCoordinates.X - x + ((host == player) ? ((float)(side * 2)) : 0f)) * num2 * ((host == player) ? 0.4f : 1f);
		y += (host.WorldCenterCoordinates.Y - y + ((host == player) ? (-1.5f) : 1f)) * num2;
		if (!base.core.CurrentPlayState.Started)
		{
			return;
		}
		tillNextBeep--;
		if (tillNextBeep == 0)
		{
			Beep();
		}
		if (untilLaserReady > 0)
		{
			untilLaserReady--;
		}
		if (untilLaserReady == 0 && base.worldTicks.Mod(10) == 0 && target == null)
		{
			FindTarget();
		}
		if (target != null)
		{
			crosshairPos += (target.WorldCenter - crosshairPos) * 0.1f;
			if (untilTargetBroken > 0)
			{
				untilTargetBroken--;
			}
			else if (!target.IsBroken)
			{
				target.Break(null);
				untilLaserReady = 300 + Component._rnd(-70, 40);
				host = player;
				if (SciHelper.ChanceRoll(0.2f))
				{
					ItemEntity itemEntity = new ItemEntity(target.WorldCenterCoordinates.X - 0.5f, target.WorldCenterCoordinates.Y - 0.5f, ItemEntity.ValueToType(base.core.CurrentPlayState.LevelGenerator.AvgCoinValue()));
					itemEntity.SetTarget(player, 40);
					SendMessage(new SpawnEntityMessage(itemEntity, null));
				}
			}
		}
		else
		{
			host = player;
		}
		if (target != null && target.IsBroken)
		{
			target = null;
		}
		if (target != null && !target.IsBroken && (untilTargetBroken == 49 || untilTargetBroken == 39))
		{
			SendMessage(new PlayWorldSoundMessage(Beeps.DrawDifferent(), base.WorldCenter));
		}
		if (target != null && !target.IsBroken && untilTargetBroken == 29)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.drone_laser, base.WorldCenter));
		}
		light.Offset = dPos;
		if (untilTargetBroken < 30 && untilTargetBroken % 3 != 0 && !player.Dead)
		{
			light.Intencity = 1f + 0.2f * Component._sin((float)base.worldTicks * 0.5f);
		}
		else
		{
			light.Intencity = 0f;
		}
		base.Update();
	}

	public override void Break(Entity offender)
	{
		SendMessage(new RemoveEntityMessage(this));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates + dPos / 16f, SpriteName.nathan_drone_off, 120, new Vector4(0f, 0f, 1f, -0.3f * (float)((side == 0) ? 1 : side)), "", 20f), null));
		base.core.ParticleManager.AddSmoke(base.WorldCenter + dPos, base.Z);
		SendMessage(new PlayWorldSoundMessage(SoundName.drone_break, base.WorldCenter));
		base.Break(offender);
	}

	private void FindTarget()
	{
		if (!IsBroken)
		{
			List<Entity> list = base.core.CurrentPlayState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 5f).FindAll((Entity e) => !e.IsBroken && (e is RotobladeEntity || e is SpikesEntity || e is CrossbowEntity || (e is PistonEntity && !(e as PistonEntity).Unbreakable) || e is SawEntity || e is ZapperEntity || (e is FireballEntity && !(e is CannonballEntity)) || e is GrillEntity || e is CannonEntity) && base.WorldCoordinates.Y - e.WorldCoordinates.Y >= 0f);
			if (list.Count > 0)
			{
				target = list[SciHelper.GetRandom(0, list.Count - 1)];
				untilTargetBroken = 50;
				crosshairPos = base.WorldCenter.Shift(0f, -20f);
				host = target;
				_inc(Stat.NathanObstaclesBroken, list.Count);
			}
		}
	}

	public void ShootAt(Entity fastTarget)
	{
		this.fastTarget = fastTarget;
		fastTargetDelay = 10;
		SendMessage(new PlaySoundMessage(SoundName.drone_laser_single, 1f, Component._rnd(-0.2f, 0.2f)));
	}

	public override void Draw()
	{
		Sprite currentFrame = anim.GetCurrentFrame();
		int num = base.worldTicks;
		if (side == -1)
		{
			num -= 500;
		}
		dPos = new Vector2(Component._cos((float)num * 0.06f) * 10f, Component._sin((float)num * 0.04f) * 5f);
		float num2 = Component._cos((float)(num - 700) * 0.06f) * (float)Math.PI * (player.Dead ? 0.4f : 0.1f) + (player.Dead ? ((float)base.ticks * 0.4f) : 0f);
		bool targetPractice = ((NathanChar)player).TargetPractice;
		string layer = (targetPractice ? "fg" : "default");
		base.core.Renderer[layer, targetPractice ? 2 : (base.Z + 10), false].DrawSpriteW(currentFrame, base.WorldPosition.Shift(0f, -20f) + dPos, null, null, num2, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(currentFrame, base.WorldPosition.Shift(0f, 10f) + dPos.Shift(0f, (0f - dPos.Y) * 2f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f - num2, SpriteFlip.Vertical, SpriteOrigin.Center);
		if (target != null && untilTargetBroken < 30 && untilTargetBroken % 3 != 0 && !player.Dead)
		{
			Vector2 randomVectorInCircle = SciHelper.GetRandomVectorInCircle(5f);
			base.core.Renderer[layer, targetPractice ? 1 : (base.Z + 9), false].DrawLineW(base.WorldPosition.Shift(0f, -20f) + dPos, target.WorldCenter + randomVectorInCircle, Color.Red);
			base.core.Renderer[layer, targetPractice ? 1 : (base.Z + 9), false].DrawSpriteW(_(SpriteName.glow_big), target.WorldCenter + randomVectorInCircle, Color.Red, new Vector2(0.3f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		if (fastTargetDelay > 0 && fastTarget != null)
		{
			Vector2 randomVectorInCircle2 = SciHelper.GetRandomVectorInCircle(5f);
			base.core.Renderer[layer, targetPractice ? 1 : (base.Z + 9), false].DrawLineW(base.WorldPosition.Shift(0f, -20f) + dPos, fastTarget.WorldCenter + randomVectorInCircle2, Color.Red);
			base.core.Renderer[layer, targetPractice ? 1 : (base.Z + 9), false].DrawSpriteW(_(SpriteName.glow_big), fastTarget.WorldCenter + randomVectorInCircle2, Color.Red, new Vector2(0.3f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		base.Draw();
	}
}
