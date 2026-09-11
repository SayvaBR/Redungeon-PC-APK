using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Tiles;

public class DungeonTile : Tile
{
	private static BagOf<SpriteName> bagOfTiles;

	private static BagOf<SoundName> vineSounds;

	private static BagOf<SoundName> crumbleSounds;

	private Sprite sprite;

	private readonly Animation anim;

	private int crumbleTimer;

	private const int crumbleDelay = 70;

	private int grassAnim = -1;

	private const int grassAnimMax = 20;

	private int grassDelay;

	private int[] grassBlades = new int[3] { 1, 1, 1 };

	private bool[] grassFlip = new bool[3];

	private int lavaAnim = -1;

	private int lavaAnimMax = 60;

	private int lavaDelay;

	public bool IceTrailN;

	public bool IceTrailE;

	public bool IceTrailS;

	public bool IceTrailW;

	static DungeonTile()
	{
		bagOfTiles = new BagOf<SpriteName>().Put(SpriteName.dungeon_floor_1, 80).Put(SpriteName.dungeon_floor_2, 2).Put(SpriteName.dungeon_floor_3, 2)
			.Put(SpriteName.dungeon_floor_4, 5)
			.Put(SpriteName.dungeon_floor_5, 5)
			.Put(SpriteName.dungeon_floor_6, 3)
			.Put(SpriteName.dungeon_floor_7, 3);
		vineSounds = new BagOf<SoundName>().Put(SoundName.vine_1).Put(SoundName.vine_2).Put(SoundName.vine_3);
		crumbleSounds = new BagOf<SoundName>().Put(SoundName.crumble_1).Put(SoundName.crumble_2).Put(SoundName.crumble_3);
	}

	public DungeonTile(int x, int y, TileType type)
		: base(x, y, type)
	{
		switch (Type)
		{
		case TileType.Floor:
			if (base.core.ProfileData.Character == Character.Vesna)
			{
				grassBlades[0] = Component._rnd(1, 4);
				grassBlades[1] = Component._rnd(1, 4);
				grassBlades[2] = Component._rnd(1, 4);
				grassFlip[0] = SciHelper.ChanceRoll();
				grassFlip[1] = SciHelper.ChanceRoll();
				grassFlip[2] = SciHelper.ChanceRoll();
			}
			break;
		case TileType.Fragile:
			anim = new Animation(0.2f, loop: false);
			if (base.core.ProfileData.Character != Character.Vesna)
			{
				anim.Add("crumble", "fragile_tile_", "0123456");
			}
			else
			{
				anim.Add("crumble", "fragile_tile_vines_", "012345");
			}
			anim.Play("crumble");
			anim.Pause();
			break;
		case TileType.Ice:
			anim = new Animation(0.3f, loop: false);
			anim.Add("shine", "ice_tile_", "0123450");
			anim.Play("shine");
			anim.Pause();
			break;
		case TileType.Wall:
		case TileType.Pit:
			break;
		}
	}

	public override void Load()
	{
		if (Map.Platform == null)
		{
			sprite = _(bagOfTiles.Draw());
		}
		else
		{
			sprite = base.core.SpriteManager.GetSprite(((X + Y) % 2 == 0) ? SpriteName.platform_1 : SpriteName.platform_2);
		}
		base.Load();
	}

	public override void AddEntity(Entity entity)
	{
		if (Type == TileType.Fragile && entity is PlayerEntity && !entity.Flying && crumbleTimer == 0)
		{
			crumbleTimer = 70;
			SendMessage(new PlayWorldSoundMessage(vineSounds.DrawDifferent(), base.WorldCoordinates * 16f));
		}
		if (Type == TileType.Floor && entity is PlayerEntity && base.core.ProfileData.Character == Character.Vesna && !entity.Flying && grassAnim == -1)
		{
			GrowGrass(10);
		}
		if (Type == TileType.Floor && entity is PlayerEntity && base.core.ProfileData.Character == Character.Golem && !entity.Flying && lavaAnim == -1)
		{
			MakeLava(10);
		}
		base.AddEntity(entity);
	}

	public void GrowGrass(int delay)
	{
		if (grassAnim < 0)
		{
			grassAnim = 0;
			grassDelay = delay;
		}
	}

	public void MakeLava(int delay)
	{
		if (lavaAnim < 0)
		{
			lavaAnim = 0;
			lavaDelay = delay;
		}
	}

	public override void Update()
	{
		if (anim != null)
		{
			anim.Update();
		}
		if (Type == TileType.Ice && (base.core.CurrentPlayState.WorldTicks - (X + Y) * 6) % 100 == 0)
		{
			anim.Reset();
			anim.Play();
		}
		if (Type == TileType.Fragile && crumbleTimer > 0)
		{
			if (base.core.ProfileData.Character != Character.Vesna)
			{
				crumbleTimer--;
				if (crumbleTimer == 15)
				{
					anim.Play();
					SendMessage(new PlayWorldSoundMessage(crumbleSounds.DrawDifferent(), base.WorldCoordinates * 16f, 0.6f));
				}
				if (crumbleTimer == 0)
				{
					Type = TileType.Pit;
					List<Entity> list = new List<Entity>();
					list.AddRange(Entities);
					foreach (Entity item in list)
					{
						if (!item.Flying)
						{
							item.LeaveTile(this);
							item.EnterTile(this);
							item.TryMoveToCoordinates(Map, base.Coordinates);
						}
					}
				}
			}
			else
			{
				if (crumbleTimer == 30 && anim.Paused)
				{
					anim.Play();
				}
				crumbleTimer = (int)Component._m(crumbleTimer, 40f);
				crumbleTimer--;
			}
		}
		if (grassAnim >= 0 && grassAnim < 20)
		{
			if (grassDelay > 0)
			{
				grassDelay--;
			}
			else
			{
				grassAnim++;
			}
		}
		if (lavaAnim >= 0 && lavaAnim < lavaAnimMax)
		{
			if (lavaDelay > 0)
			{
				lavaDelay--;
			}
			else
			{
				lavaAnim++;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		DrawTile();
	}

	public void DrawWispLight(Vector2 source, Color color)
	{
		if (Type != TileType.Floor && Type != TileType.Ice && Type != TileType.Fragile) return;
		Sprite surface = Type == TileType.Floor ? sprite : anim.GetCurrentFrame();
		Vector2 origin = WorldCoordinates * 16f - Vector2.One;
		// Light only the top surface, reusing its texels so joints remain visible.
		for (int sy = 0; sy < 16; sy += 8)
		for (int sx = 0; sx < 16; sx += 8)
		{
			Vector2 position = origin + new Vector2(sx, sy);
			float falloff = Math.Max(0f, 1f - Vector2.Distance(position + new Vector2(4), source) / 44f);
			if (falloff <= 0f) continue;
			Sprite patch = surface.Reduce(sx, sy, Math.Max(0, surface.Width - sx - 8), Math.Max(0, surface.Height - sy - 8));
			for (int pass = 0; pass < 3; pass++)
				core.Renderer["floor-light", 0, false].DrawSpriteW(patch, position, color * (falloff * falloff * 0.95f));
		}
	}

	private void DrawTile()
	{
		switch (Type)
		{
		case TileType.Floor:
			if (grassAnim >= 0)
			{
				Vector2 v = new Vector2((float)X + Map.X, (float)Y + Map.Y);
				bool flag = (X + Y) % 2 != 0;
				int num = 0;
				if (Map.Platform != null)
				{
					num = 0;
				}
				num = (int)(5f * (float)grassAnim / 20f);
				int num2 = (int)Math.Round((float)Y + Map.Y) * 16;
				if (num > 0)
				{
					Sprite sprite2 = base.core.SpriteManager.GetSprite("grass_" + grassBlades[0] + "_" + num);
					base.core.Renderer[num2 - 1, false].DrawSpriteW(sprite2, 16f * v.Shift(flag ? 0.33f : 0.66f, 0.33f) - new Vector2(1f, 2f) - sprite2.Link, Color.White * 0.8f, null, 0f, grassFlip[0] ? SpriteFlip.Horizontal : SpriteFlip.None);
					sprite2 = base.core.SpriteManager.GetSprite("grass_" + grassBlades[1] + "_" + num);
					base.core.Renderer[num2 - 1, false].DrawSpriteW(sprite2, 16f * v.Shift(flag ? 0.66f : 0.33f, 0.66f) - new Vector2(1f, 2f) - sprite2.Link, Color.White * 0.8f, null, 0f, grassFlip[1] ? SpriteFlip.Horizontal : SpriteFlip.None);
					sprite2 = base.core.SpriteManager.GetSprite("grass_" + grassBlades[2] + "_" + num);
					base.core.Renderer[num2 + 16, false].DrawSpriteW(sprite2, 16f * v.Shift(flag ? 0.33f : 0.66f, 1f) - new Vector2(1f, 2f) - sprite2.Link, Color.White * 0.8f, null, 0f, grassFlip[2] ? SpriteFlip.Horizontal : SpriteFlip.None);
				}
			}
			if (lavaAnim >= 0)
			{
				Vector2 vector = new Vector2((float)X + Map.X, (float)Y + Map.Y);
				bool flag2 = (X + Y) % 2 != 0;
				int num3 = 0;
				float num4 = 0.8f;
				num3 = (int)(7f * (float)lavaAnim / (float)lavaAnimMax);
				if (num3 > 5)
				{
					num3 = 5;
					num4 = ((num3 == 6) ? 0.7f : 0.6f);
				}
				if (num3 > 0)
				{
					Sprite sprite3 = _("rik_lava_" + num3);
					base.core.Renderer["bg", (Y + (int)Map.Y) * 16 + 1, false].DrawSpriteW(sprite3, 16f * vector - new Vector2(0f, 1f), Color.White * num4, null, 0f, flag2 ? SpriteFlip.Horizontal : SpriteFlip.None);
				}
			}
			goto case TileType.Wall;
		case TileType.Wall:
			base.core.Renderer["bg", (Y + (int)Map.Y) * 16, true].DrawSpriteW(this.sprite, 16f * new Vector2((float)X + Map.X, (float)Y + Map.Y) - new Vector2(1f));
			if (Map.Platform == null)
			{
				if (Tile.IsPitOrNull(Map[X, Y + 1]))
				{
					Sprite sprite = _(SpriteName.dungeon_pit_wall);
					Tile tile = Map[X + 1, Y + 1];
					Tile tile2 = Map[X + 1, Y];
					if (Type == TileType.Wall && Tile.IsPitOrNull(tile2) && Tile.IsPitOrNull(tile))
					{
						sprite = sprite.Reduce(0, 0, 1, 0);
					}
					base.core.Renderer["bg", (Y + (int)Map.Y) * 16, true].DrawSpriteW(sprite, 16f * new Vector2((float)X + Map.X, (float)Y + Map.Y) + new Vector2(-1f, 16f));
				}
			}
			else if (Map.Platform.OneWay)
			{
				float rotation = 0f;
				switch (Map.Platform.FirstDirection)
				{
				case Direction.East:
					rotation = (float)Math.PI / 2f;
					break;
				case Direction.West:
					rotation = -(float)Math.PI / 2f;
					break;
				case Direction.South:
					rotation = (float)Math.PI;
					break;
				}
				base.core.Renderer["bg", (Y + (int)Map.Y) * 16, true].DrawSpriteW(_(SpriteName.platform_arrow), 16f * new Vector2((float)X + Map.X + 0.5f, (float)Y + Map.Y + 0.5f) - new Vector2(0.5f), default(Color).FromRgb(1575186), null, rotation, SpriteFlip.None, SpriteOrigin.Center);
			}
			break;
		case TileType.Fragile:
			base.core.Renderer["bg", (Y + (int)Map.Y) * 16, true].DrawSpriteW(anim.GetCurrentFrame(), 16f * new Vector2((float)X + Map.X, (float)Y + Map.Y) - new Vector2(1f) + ((crumbleTimer > 15 && !base.core.CurrentPlayState.Paused) ? SciHelper.GetRandomVectorInCircle(0.7f * (float)(70 - crumbleTimer) / 55f) : Vector2.Zero));
			break;
		case TileType.Ice:
		{
			Vector2 position = 16f * new Vector2((float)X + Map.X, (float)Y + Map.Y) - new Vector2(1f);
			int depth = (Y + (int)Map.Y) * 16;
			base.core.Renderer["bg", depth, true].DrawSpriteW(anim.GetCurrentFrame(), position);
			if (IceTrailN)
			{
				base.core.Renderer["bg", depth, true].DrawSpriteW(_(SpriteName.ice_trail_n), position);
			}
			if (IceTrailE)
			{
				base.core.Renderer["bg", depth, true].DrawSpriteW(_(SpriteName.ice_trail_e), position);
			}
			if (IceTrailW)
			{
				base.core.Renderer["bg", depth, true].DrawSpriteW(_(SpriteName.ice_trail_w), position);
			}
			if (IceTrailS)
			{
				base.core.Renderer["bg", depth, true].DrawSpriteW(_(SpriteName.ice_trail_s), position);
			}
			break;
		}
		case TileType.Pit:
			if (anim != null)
			{
				base.core.Renderer["bg", (Y + (int)Map.Y) * 16, true].DrawSpriteW(anim.GetCurrentFrame(), 16f * new Vector2((float)X + Map.X, (float)Y + Map.Y) - new Vector2(1f));
			}
			break;
		}
		base.Draw();
	}
}
