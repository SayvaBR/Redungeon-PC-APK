using System;

namespace Knighter.Graphics;

[Flags]
public enum SpriteOrigin
{
	TopLeft = 1,
	TopCenter = 2,
	TopRight = 4,
	CenterLeft = 8,
	Center = 0x10,
	CenterRight = 0x20,
	BottomLeft = 0x40,
	BottomCenter = 0x80,
	BottomRight = 0x100
}
