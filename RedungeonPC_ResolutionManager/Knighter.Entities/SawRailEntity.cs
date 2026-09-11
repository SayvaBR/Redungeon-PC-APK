using Knighter.Graphics;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SawRailEntity : Entity
{
	private readonly Sprite sprite;

	public SawRailEntity(int x, int y)
		: base(x, y, 0.1f, 0.1f)
	{
		sprite = base.core.SpriteManager.GetSprite(SpriteName.saw_rail);
	}

	public override void Draw()
	{
		base.core.Renderer["bg", 1000, false].DrawSpriteW(sprite, 16f * new Vector2(base.Tile.WorldCoordinates.X, base.Tile.WorldCoordinates.Y) + new Vector2(0f, 1f));
		base.Draw();
	}
}
