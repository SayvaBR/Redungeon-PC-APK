using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class WallEntity : Entity
{
	private static readonly Color CONTOUR_COLOR = default(Color).FromRgb(0);

	private Sprite wallSprite;

	private Sprite shadow;

	public WallEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
		wallSprite = _(SpriteName.dungeon_wall_1);
		shadow = _(SpriteName.dungeon_wall_shadow);
	}

	public override void Draw()
	{
		Vector2 position = base.WorldPosition - new Vector2(0f, 10f);
		Sprite sprite = wallSprite.Clone();
		RectangleF rectangleF = new RectangleF(position.X, position.Y, 16f, 26f);
		bool flag = false;
		if (base.Tile == null || base.CurrentMap[base.Tile.X - 1, base.Tile.Y] == null || base.CurrentMap[base.Tile.X - 1, base.Tile.Y].Type != TileType.Wall)
		{
			rectangleF.X--;
			rectangleF.Width += 1f;
			flag = true;
		}
		if (base.Tile == null || base.CurrentMap[base.Tile.X + 1, base.Tile.Y] == null || base.CurrentMap[base.Tile.X + 1, base.Tile.Y].Type != TileType.Wall)
		{
			sprite = sprite.Reduce(0, 0, 1, 0);
			flag = true;
		}
		if (base.Tile == null || base.CurrentMap[base.Tile.X, base.Tile.Y - 1] == null || base.CurrentMap[base.Tile.X, base.Tile.Y - 1].Type != TileType.Wall)
		{
			rectangleF.Y--;
			rectangleF.Height += 1f;
			flag = true;
		}
		if (flag)
		{
			base.core.Renderer[base.Z + 2].DrawRectangleW(rectangleF, CONTOUR_COLOR);
		}
		if (base.CurrentMap[base.Tile.X, base.Tile.Y + 1] != null && base.CurrentMap[base.Tile.X, base.Tile.Y + 1].Type != TileType.Wall)
		{
			base.core.Renderer["bg", (base.Tile.Y + 1 + (int)base.CurrentMap.Y) * 16 + 1, false].DrawSpriteW(shadow, base.WorldPosition.Shift(-1f, 15f));
		}
		base.core.Renderer["default", base.Z + 2, true].DrawSpriteW(sprite, position);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return false;
	}
}
