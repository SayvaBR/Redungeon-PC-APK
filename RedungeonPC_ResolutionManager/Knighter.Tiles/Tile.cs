using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Tiles;

public abstract class Tile : Component
{
	public int X;

	public int Y;

	public TileMap Map;

	public TileType Type;

	public List<Entity> Entities;

	public Vector2 Coordinates => new Vector2(X, Y);

	public Vector2 WorldCoordinates => new Vector2(X, Y) + new Vector2(Map.X, Map.Y);

	protected Tile(int x, int y, TileType type)
	{
		X = x;
		Y = y;
		Type = type;
		Entities = new List<Entity>();
	}

	public virtual void AddEntity(Entity entity)
	{
		Entities.Add(entity);
	}

	public void RemoveEntity(Entity entity)
	{
		Entities.Remove(entity);
	}

	public void RemoveAllEntities()
	{
		foreach (Entity entity in Entities)
		{
			SendMessage(new RemoveEntityMessage(entity));
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public static bool IsPitOrNull(Tile tile)
	{
		if (tile != null && tile.Type != TileType.Pit)
		{
			return tile.Type == TileType.Fragile;
		}
		return true;
	}

	public bool IsPassableFor(Entity entity)
	{
		bool flag = true;
		if (Entities == null)
		{
			return flag;
		}
		foreach (Entity entity2 in Entities)
		{
			if (entity2 != entity)
			{
				flag = flag && entity2.IsPassableFor(entity);
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public bool IsPassableFloorFor(Entity entity)
	{
		if (Type == TileType.Floor || Type == TileType.Fragile || Type == TileType.Blocker)
		{
			return IsPassableFor(entity);
		}
		return false;
	}

	public bool ContainsPlatform()
	{
		bool result = false;
		if (Entities == null)
		{
			return result;
		}
		foreach (Entity entity in Entities)
		{
			if (entity is PlatformEntity)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool ContainsHangingObstaclesFor(Entity entity)
	{
		bool flag = false;
		if (Entities == null)
		{
			return flag;
		}
		foreach (Entity entity2 in Entities)
		{
			if (entity2 != entity)
			{
				flag = !entity2.IsPassableFor(entity) && entity2.CurrentPlatform != entity.CurrentPlatform;
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public bool MostlyContains(Entity entity)
	{
		return new RectangleF(WorldCoordinates.X, WorldCoordinates.Y, 1f, 1f).Contains(entity.WorldCenterCoordinates);
	}

	public override void Draw()
	{
		if (Settings.HighlightOccupiedTiles && Entities.Count > 0)
		{
			base.core.Renderer.DrawRectangleW(((float)X + Map.X) * 16f, ((float)Y + Map.Y) * 16f, 16f, 16f, Color.Red * 0.3f);
		}
		base.Draw();
	}
}
