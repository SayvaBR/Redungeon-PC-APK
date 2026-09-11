using System;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public sealed class Sprite
{
	public int X;

	public int Y;

	public int SrcWidth;

	public int SrcHeight;

	public int Width;

	public int Height;

	public int OffXL;

	public int OffXR;

	public int OffYT;

	public int OffYB;

	public int LinkX;

	public int LinkY;

	public string TextureName;

	public Vector2 Link => new Vector2(LinkX, LinkY);

	public Vector2 Size => new Vector2(Width, Height);

	public Vector2 SrcSize => new Vector2(SrcWidth, SrcHeight);

	public Sprite ClampWidth(int maxWidth)
	{
		Sprite sprite = Clone();
		sprite.SrcWidth = Math.Min(SrcWidth, maxWidth);
		return sprite;
	}

	public Sprite Reduce(int left, int top, int right, int bottom)
	{
		Sprite sprite = Clone();
		sprite.X += Math.Min(left, SrcWidth);
		sprite.Y += Math.Min(top, SrcHeight);
		sprite.SrcWidth = Math.Max(SrcWidth - left - right, 0);
		sprite.SrcHeight = Math.Max(SrcHeight - top - bottom, 0);
		sprite.Width = Math.Max(Width - left - right, 0);
		sprite.Height = Math.Max(Height - top - bottom, 0);
		return sprite;
	}

	public Sprite Clone()
	{
		return MemberwiseClone() as Sprite;
	}

	public Vector2 GetOffset(SpriteFlip flip)
	{
		return flip switch
		{
			SpriteFlip.None => new Vector2(OffXL, OffYT), 
			SpriteFlip.Horizontal => new Vector2(OffXR, OffYT), 
			SpriteFlip.Vertical => new Vector2(OffXL, OffYB), 
			SpriteFlip.Horizontal | SpriteFlip.Vertical => new Vector2(OffXR, OffYB), 
			_ => new Vector2(OffXL, OffYT), 
		};
	}
}
