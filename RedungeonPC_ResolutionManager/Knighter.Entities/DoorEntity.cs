using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class DoorEntity : Entity
{
	private Animation fire1;

	private Animation fire2;

	private Sprite fireGlow;

	private Sprite shadow;

	public DoorEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		fire1 = new Animation(0.25f);
		fire1.AddAndPlay("burn", new List<SpriteName>
		{
			SpriteName.torch_fire_blue_1,
			SpriteName.torch_fire_blue_2,
			SpriteName.torch_fire_blue_3,
			SpriteName.torch_fire_blue_4,
			SpriteName.torch_fire_blue_5,
			SpriteName.torch_fire_blue_6
		});
		fire1.SkipToRandomFrame();
		fire2 = new Animation(0.25f);
		fire2.AddAndPlay("burn", new List<SpriteName>
		{
			SpriteName.torch_fire_blue_1,
			SpriteName.torch_fire_blue_2,
			SpriteName.torch_fire_blue_3,
			SpriteName.torch_fire_blue_4,
			SpriteName.torch_fire_blue_5,
			SpriteName.torch_fire_blue_6
		});
		fire2.SkipToRandomFrame();
		fireGlow = _(SpriteName.glow);
		shadow = _(SpriteName.dungeon_wall_shadow);
	}

	public override void Load()
	{
		SendMessage(new SpawnEntityMessage(new ObstacleEntity((int)x - 1, (int)y, invisible: true), CurrentPlatform));
		SendMessage(new SpawnEntityMessage(new ObstacleEntity((int)x + 1, (int)y, invisible: true), CurrentPlatform));
		base.Load();
	}

	public override void Update()
	{
		fire1.Update();
		fire2.Update();
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer["bg", base.Z, false].DrawSpriteW(_(SpriteName.dungeon_floor_door_bg), base.WorldPosition.Shift(-21f, -16f));
		base.core.Renderer[base.Z + 5].DrawSpriteW(_(SpriteName.dungeon_floor_door), base.WorldPosition.Shift(-21f, -16f));
		base.core.Renderer[base.Z + 5].DrawSpriteW(fireGlow, base.WorldCenter.Shift(-18f, -10f) + new Vector2((float)Math.Sin((float)base.worldTicks / 4f) * 0.5f, 3f + (float)Math.Sin((float)base.worldTicks / 8f) * 0.3f), Color.SkyBlue * (0.75f + 0.25f * (float)Math.Sin((float)base.worldTicks / 5f)), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z + 5].DrawSpriteW(fire1.GetCurrentFrame(), base.WorldCenter.Shift(-18f, -10f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z + 5].DrawSpriteW(fireGlow, base.WorldCenter.Shift(16f, -10f) + new Vector2((float)Math.Sin((float)base.worldTicks / 4f) * 0.5f, 3f + (float)Math.Sin((float)base.worldTicks / 8f) * 0.3f), Color.SkyBlue * (0.75f + 0.25f * (float)Math.Sin((float)base.worldTicks / 5f)), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z + 5].DrawSpriteW(fire2.GetCurrentFrame(), base.WorldCenter.Shift(16f, -10f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg", (base.Tile.Y + 1 + (int)base.CurrentMap.Y) * 16 + 1, false].DrawSpriteW(shadow, base.WorldPosition.Shift(-17f, 15f));
		base.core.Renderer["bg", (base.Tile.Y + 1 + (int)base.CurrentMap.Y) * 16 + 1, false].DrawSpriteW(shadow, base.WorldPosition.Shift(15f, 15f));
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}

	public override void CollideWith(Entity other)
	{
		var _ = other is PlayerEntity;
		base.CollideWith(other);
	}
}
