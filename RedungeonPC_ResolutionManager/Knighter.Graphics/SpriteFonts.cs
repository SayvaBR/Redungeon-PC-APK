using System.Collections.Generic;
using Knighter.Helpers;

namespace Knighter.Graphics;

public class SpriteFonts : Component
{
	private readonly Dictionary<string, SpriteFont> spriteFonts;

	public SpriteFont this[string name] => spriteFonts[name];

	public SpriteFonts()
	{
		spriteFonts = new Dictionary<string, SpriteFont>();
	}

	public override void Load()
	{
		LoadFromFile("Content/Fonts/fonts.json");
		base.Load();
	}

	private void LoadFromFile(string filePath)
	{
		foreach (JsonObject item in JsonReader.FromFile(filePath)["fonts"].ToListOfObjects())
		{
			spriteFonts.Add(item["sprite-name"].ToString(), new SpriteFont(item));
		}
	}
}
