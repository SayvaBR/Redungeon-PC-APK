using System.Collections;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Tiles;

public class TileMap : Component, IEnumerable<Tile>, IEnumerable
{
	private readonly Dictionary<int, Tile> map;

	public readonly PlatformEntity Platform;

	public float X;

	public float Y;

	public Tile this[int x, int y]
	{
		get
		{
			map.TryGetValue(Hash(x, y), out var value);
			return value;
		}
		set
		{
			AddTile(value);
		}
	}

	public Tile this[float x, float y]
	{
		get
		{
			return this[(int)x, (int)y];
		}
		set
		{
			this[(int)x, (int)y] = value;
		}
	}

	public Tile this[Vector2 pos]
	{
		get
		{
			return this[pos.X, pos.Y];
		}
		set
		{
			this[pos.X, pos.Y] = value;
		}
	}

	public TileMap(PlatformEntity platform)
	{
		Platform = platform;
		map = new Dictionary<int, Tile>();
	}

	public TileMap()
	{
		Platform = null;
		map = new Dictionary<int, Tile>();
	}

	public override void Update()
	{
		using (IEnumerator<Tile> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Update();
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		using (IEnumerator<Tile> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Draw();
			}
		}
		base.Draw();
	}

	public void AddTile(Tile tile)
	{
		tile.Map = this;
		tile.Load();
		map[Hash(tile.X, tile.Y)] = tile;
	}

	public void RemoveTile(Tile tile)
	{
		foreach (Entity entity in tile.Entities)
		{
			if (!(entity is PlayerEntity))
			{
				SendMessage(new RemoveEntityMessage(entity));
			}
		}
		int key = Hash(tile.X, tile.Y);
		map[key] = null;
		map.Remove(key);
	}

	public void Clear()
	{
		using (IEnumerator<Tile> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Tile current = enumerator.Current;
				RemoveTile(current);
			}
		}
		map.Clear();
	}

	public int GetTilesCount()
	{
		return map.Keys.Count;
	}

	private static int Hash(int x, int y)
	{
		return x * 1000000000 + y;
	}

	public IEnumerator<Tile> GetEnumerator()
	{
		return ((IEnumerable<Tile>)map.Values).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
