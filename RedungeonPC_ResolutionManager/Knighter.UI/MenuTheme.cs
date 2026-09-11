using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

public static class MenuTheme
{
	public static readonly Color Surface = new(37, 30, 35);
	public static readonly Color Raised = new(53, 42, 45);
	public static readonly Color Border = new(101, 75, 59);
	public static readonly Color Accent = new(245, 179, 71);

	public static float TextScale => MathHelper.Clamp(Core.Instance.OptionsData.UiScale, 0.85f, 1.15f);
	public static void Shine(Renderer r, RectangleF b, int depth, float seconds, float opacity = 0.3f)
	{
		var sprite = Core.Instance.SpriteManager.GetSprite(SpriteName.button_gloss);
		float width = b.Width - 6f;
		float x = seconds * 110f % (width + sprite.Width + 100f) - sprite.Width;
		int left = (int)System.MathF.Max(0, -x), right = (int)System.MathF.Max(0, x + sprite.Width - width);
		if (left + right >= sprite.Width) return;
		var slice = sprite.Reduce(left, 0, right, 0);
		r["fg", depth, false].DrawSpriteS(slice, new Vector2(b.X + 3 + System.MathF.Max(0, x), b.Y + 2), Color.White * opacity,
			new Vector2(1, System.MathF.Max(1, b.Height - 6) / sprite.Height));
	}

	public static void Button(Renderer r, RectangleF b, int depth, bool pressed = false, float opacity = 1)
	{
		var sprite = Core.Instance.SpriteManager.GetSprite(pressed ? SpriteName.button_pressed : SpriteName.button);
		var cap = sprite.Reduce(0, 0, 1, 0);
		var middle = sprite.Reduce(sprite.Width - 1, 0, 0, 0);
		float scale = b.Height / sprite.Height, cw = cap.Width * scale;
		r["fg", depth, false].DrawSpriteS(cap, b.TopLeft, Color.White * opacity, new Vector2(scale));
		r["fg", depth + 1, false].DrawSpriteS(middle, b.TopLeft + new Vector2(cw, 0), Color.White * opacity,
			new Vector2(System.MathF.Max(1, b.Width - cw * 2), scale));
		r["fg", depth + 2, false].DrawSpriteS(cap, b.TopRight - new Vector2(cw, 0), Color.White * opacity, new Vector2(scale), 0, SpriteFlip.Horizontal);
	}

	public static void Panel(Renderer r, RectangleF b, int depth, float focus = 0f)
	{
		focus = MathHelper.Clamp(focus, 0f, 1f);
		r["fg", depth, false].DrawRectangleS(b, Color.Lerp(Border, Accent, focus));
		r["fg", depth + 1, false].DrawRectangleS(new RectangleF(b.X + 1, b.Y + 1, b.Width - 2, b.Height - 2), Color.Lerp(Surface, Raised, focus));
		if (focus > 0.01f)
			r["fg", depth + 2, false].DrawRectangleS(new RectangleF(b.X + 1, b.Y + 3, 2, b.Height - 6), Accent * focus);
	}
}
