using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

// One cursor per menu. Animation advances in Update, never in Draw.
public sealed class MenuCursor : Component
{
	private Vector2 position;
	private bool visible;
	private float scale;
	public void Step(RectangleF bounds)
	{
		if (bounds == null) { visible = false; return; }
		var b = bounds;
		var sprite = _(SpriteName.swipe_hand_1);
		scale = Math.Min(0.75f, Math.Min((b.Width - 8f) / sprite.Width, (b.Height - 6f) / sprite.Height));
		if (scale <= 0) { visible = false; return; }
		// The fingertip points up-left: keep the entire hand inside the selected
		// control, aimed inward. Never interpolate across neighbouring controls.
		position = new Vector2(b.Right - sprite.Width * scale - 4f,
			b.Bottom - sprite.Height * scale - 3f);
		visible = true;
	}
	public void Render(string layer, int depth)
	{
		if (Settings.IsTouchDevice && InputDeviceTracker.CurrentDevice != InputDevice.Gamepad)
			return;
		if (visible && !InputDeviceTracker.UsingMouse)
			R[layer, depth, false].DrawSpriteS(_(SpriteName.swipe_hand_1),
				new Vector2(MathF.Round(position.X), MathF.Round(position.Y)), Color.White,
				Vector2.One * scale, 0, SpriteFlip.None, SpriteOrigin.TopLeft);
	}
}
