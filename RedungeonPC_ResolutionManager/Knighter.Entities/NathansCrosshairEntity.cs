using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class NathansCrosshairEntity : Entity
{
	private Entity target;

	private PlayerEntity player;

	private NathansDroneEntity drone;

	private float rotation;

	private float progress;

	private float scale;

	private bool orderGiven;

	public NathansCrosshairEntity(int x, int y, Entity target, PlayerEntity player, NathansDroneEntity drone)
		: base(x, y, 1f, 1f)
	{
		this.target = target;
		rotation = Component._rnd(0f, (float)Math.PI * 2f);
		this.player = player;
		this.drone = drone;
	}

	public override void Update()
	{
		rotation += 0.05f;
		if (progress >= 0f)
		{
			scale += 0.2f;
			if (scale > 1f)
			{
				scale = 1f;
			}
			progress += 0.05f;
			if (progress > 1f)
			{
				progress = -1f;
				SendMessage(new PlayWorldSoundMessage(NathansDroneEntity.Beeps.DrawDifferent(), target.WorldCenter, 1f, Component._rnd(-0.2f, 0.2f)));
			}
		}
		else
		{
			progress -= 0.025f;
			if (progress < -1.4f)
			{
				scale -= 0.2f;
				if (scale < 0f)
				{
					scale = 0f;
				}
			}
			if ((double)progress <= -1.4 && !orderGiven)
			{
				if (SciHelper.ChanceRoll(0.2f))
				{
					ItemEntity itemEntity = new ItemEntity(target.WorldCenterCoordinates.X - 0.5f, target.WorldCenterCoordinates.Y - 0.5f, ItemEntity.ValueToType((int)Component._M(base.core.CurrentPlayState.LevelGenerator.AvgCoinValue() - 1, 1f)));
					itemEntity.SetTarget(player, 40);
					SendMessage(new SpawnEntityMessage(itemEntity, null));
				}
				base.core.CurrentPlayState.Camera.Shake("crosshair", 3f);
				drone.ShootAt(target);
				orderGiven = true;
			}
			if (progress <= -2f)
			{
				SendMessage(new RemoveEntityMessage(this));
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = ((progress >= 0f) ? progress : 1f);
		base.core.Renderer["fg"].DrawSpriteW(_(SpriteName.nate_crosshair), player.WorldCenter + (target.WorldCenter - player.WorldCenter) * num, Color.White * scale, rotation: rotation, scale: Vector2.One * scale, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		base.Draw();
	}
}
