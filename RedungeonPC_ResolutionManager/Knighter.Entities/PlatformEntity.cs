using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Helpers;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PlatformEntity : Entity
{
	private Vector2 spawn;

	private List<PathNode> path;

	private int delay;

	private bool departed;

	private bool arrived;

	private int startingWorldTick;

	public override bool IsPlatform => true;

	public TileMap Map { get; private set; }

	public bool OneWay { get; private set; }

	public Direction FirstDirection { get; private set; }

	public PlatformEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		width = desc["width"];
		height = desc["height"];
		delay = desc["delay"];
		if (desc.Flipped)
		{
			base.x -= width - 1f;
		}
		spawn = new Vector2(base.x, base.y);
		padding = 0.4f;
		int num = 0;
		int num2 = 0;
		path = new List<PathNode>();
		foreach (JsonObject item in desc.ElementJson["path"].ToListOfObjects())
		{
			PathNode pathNode = new PathNode
			{
				Dx = ((!desc.Flipped) ? 1 : (-1)) * item["dx"].ToInt(),
				Dy = item["dy"].ToInt(),
				TicksPerTile = item["ticks-per-tile"].ToInt(),
				StopTime = item["stop-time"].ToInt()
			};
			num += pathNode.Dx;
			num2 += pathNode.Dy;
			path.Add(pathNode);
		}
		OneWay = num != 0 || num2 != 0;
		if (OneWay)
		{
			FirstDirection = Direction.North;
			if (path.Count > 0)
			{
				PathNode pathNode2 = path[0];
				if (pathNode2.Dx < 0)
				{
					FirstDirection = Direction.West;
				}
				if (pathNode2.Dx > 0)
				{
					FirstDirection = Direction.East;
				}
				if (pathNode2.Dy < 0)
				{
					FirstDirection = Direction.North;
				}
				if (pathNode2.Dy > 0)
				{
					FirstDirection = Direction.South;
				}
			}
		}
		LevelModule levelModule = null;
		if (desc.ElementJson.Contains("module"))
		{
			levelModule = new LevelModule(desc.ElementJson["module"] as JsonObject)
			{
				Name = desc.ParentModule.Name
			};
		}
		Map = new TileMap(this);
		for (int i = 0; (float)i < height; i++)
		{
			for (int j = 0; (float)j < width; j++)
			{
				int num3 = ((!desc.Flipped) ? j : ((int)width - j - 1));
				int num4 = i;
				TileType type = ((levelModule != null) ? ((levelModule[j, i].ElementType == ElementType.Fragile) ? TileType.Fragile : levelModule[j, i].TileType) : TileType.Floor);
				DungeonTile value = new DungeonTile(num3, num4, type);
				Map[num3, num4] = value;
				if (levelModule != null)
				{
					base.core.CurrentPlayState.LevelGenerator.PopulateTile(levelModule[j, i].Flip(desc.Flipped), num3, num4, this);
				}
			}
		}
	}

	public override void Update()
	{
		int num = 0;
		foreach (PathNode item in path)
		{
			num += item.TicksPerTile * item.Distance + item.StopTime;
		}
		bool flag = arrived;
		arrived = OneWay && departed && base.worldTicks >= startingWorldTick + num - 1;
		bool flag2 = arrived != flag;
		bool num2 = num > 0 && (!OneWay || (departed && (!arrived || flag2)));
		int num3 = 0;
		int num4 = 0;
		if (num2)
		{
			int num5 = (base.worldTicks - startingWorldTick + delay).Mod(num);
			float num6 = 0f;
			float num7 = 0f;
			int num8 = -1;
			if (num5 > 0)
			{
				foreach (PathNode item2 in path)
				{
					num8++;
					num5 -= item2.StopTime;
					if (num5 <= 0)
					{
						break;
					}
					int num9 = item2.Distance * item2.TicksPerTile;
					if (num9 > 0 && num5 <= num9)
					{
						num6 = MathHelper.Lerp(num6, num6 + (float)item2.Dx, flag2 ? 1f : ((float)num5 / (float)num9));
						num7 = MathHelper.Lerp(num7, num7 + (float)item2.Dy, flag2 ? 1f : ((float)num5 / (float)num9));
						num3 = Math.Sign(item2.Dx);
						num4 = Math.Sign(item2.Dy);
						break;
					}
					num5 -= num9;
					num6 += (float)item2.Dx;
					num7 += (float)item2.Dy;
				}
			}
			x = spawn.X + num6;
			y = spawn.Y + num7;
		}
		UpdateTiles();
		UpdateWorldTilesForPassangers(num3, num4);
		Map.Update();
		Map.X = x;
		Map.Y = y;
		base.Update();
	}

	private void UpdateWorldTilesForPassangers(float dx, float dy)
	{
		List<Entity> list = new List<Entity>();
		foreach (Tile item in Map)
		{
			foreach (Entity entity in item.Entities)
			{
				if (!entity.UpdateWorldTilesFromPlatform())
				{
					list.Add(entity);
				}
			}
		}
		foreach (Entity item2 in list)
		{
			item2.TryMoveToCoordinates(Map, item2.Coordinates + new Vector2(-Math.Sign(dx), -Math.Sign(dy)));
		}
	}

	public override void CollideWith(Entity other)
	{
		if (OneWay && other is PlayerEntity && !(other as PlayerEntity).Flying && !departed)
		{
			startingWorldTick = base.worldTicks;
			departed = true;
		}
		base.CollideWith(other);
	}

	public override void Draw()
	{
		Map.Draw();
		base.Draw();
	}
}
