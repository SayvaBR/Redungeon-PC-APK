using System.Collections.Generic;
using Knighter.Helpers;

namespace Knighter.Graphics;

public class SpriteFont : Component
{
	public readonly Sprite Sprite;

	public readonly Sprite ShadowSprite;

	public readonly int LineHeight;

	public readonly int SpaceWidth;

	private readonly Dictionary<char, SpriteGlyph> spriteGlyphs;

	public SpriteGlyph this[char ch]
	{
		get
		{
			spriteGlyphs.TryGetValue(ch, out var value);
			return value ?? spriteGlyphs['\ufffd'];
		}
	}

	public SpriteFont(JsonObject json)
	{
		Sprite = base.core.SpriteManager.GetSprite(json["sprite-name"].ToString());
		LineHeight = json["line-height"].ToInt();
		SpaceWidth = json["space-width"].ToInt();
		spriteGlyphs = new Dictionary<char, SpriteGlyph>();
		foreach (JsonObject item in json["glyphs"].ToListOfObjects())
		{
			string text = item["glyph"].ToString();
			if (text.Length != 0)
			{
				spriteGlyphs.Add(text[0], new SpriteGlyph(this, item));
			}
		}
	}

	public bool HasSpriteGlyphFor(char ch)
	{
		spriteGlyphs.TryGetValue(ch, out var value);
		return value != null;
	}
}
