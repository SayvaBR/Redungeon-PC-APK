using System.Collections.Generic;
using Knighter.Helpers;

namespace Knighter.Graphics;

public class SpriteGlyph : Component
{
	public readonly Sprite Sprite;

	public readonly Sprite ShadowSprite;

	public readonly int Base;

	public readonly int Offset;

	private readonly Dictionary<char, int> Kerning;

	public SpriteGlyph(SpriteFont spriteFont, JsonObject json)
	{
		Sprite = new Sprite
		{
			X = spriteFont.Sprite.X + json["x"].ToInt(),
			Y = spriteFont.Sprite.Y + json["y"].ToInt(),
			Width = json["w"].ToInt(),
			Height = json["h"].ToInt(),
			SrcWidth = json["w"].ToInt(),
			SrcHeight = json["h"].ToInt(),
			TextureName = spriteFont.Sprite.TextureName
		};
		Base = json["base"].ToInt();
		if (json.Contains("offset"))
		{
			Offset = json["offset"].ToInt();
		}
		if (!json.Contains("kerning"))
		{
			return;
		}
		Kerning = new Dictionary<char, int>();
		foreach (JsonObject item in json["kerning"].ToListOfObjects())
		{
			Kerning[item["after"].ToString()[0]] = item["offset"].ToInt();
		}
	}

	public int GetKerningFor(char ch)
	{
		if (Kerning == null || !Kerning.ContainsKey(ch))
		{
			return 0;
		}
		return Kerning[ch];
	}
}
