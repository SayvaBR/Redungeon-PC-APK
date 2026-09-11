using System;
using System.Collections.Generic;
using System.IO;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

public sealed class SpriteManager : Component
{
	public Dictionary<Language, Sprite> ExternalFonts;

	private readonly Dictionary<string, Sprite> sprites;

	private readonly Dictionary<string, Texture2D> textures;

	private readonly Dictionary<Texture2D, string> textureNamesByTexture = new Dictionary<Texture2D, string>();

	public Texture2D Font { get; private set; }

	public Texture2D Clouds { get; private set; }

	public Sprite Pixel { get; private set; }

	public string DefaultTexture { get; private set; }

	public SpriteManager()
	{
		sprites = new Dictionary<string, Sprite>();
		textures = new Dictionary<string, Texture2D>();
		ExternalFonts = new Dictionary<Language, Sprite>();
	}

	public override void Load()
	{
		LoadFromFile("atlas.json");
		Font = LoadRawTexture("Fonts/font.png");
		AddTexture("font", Font);
		Clouds = LoadRawTexture("Images/clouds.png");
		AddTexture("splash-knight", LoadRawTexture("Images/splash-knight.png"));
		LoadCharacterAtlas("lykos");
		// Folhas de prompts são PNGs crus (não fazem parte do atlas). Cada célula
		// usada por InputPromptIcons tem 64x64 pixels.
		AddTexture("input-prompts-xbox", LoadRawTexture("Images/xbox.png"));
		AddTexture("input-prompts-playstation", LoadRawTexture("Images/playstation.png"));
		AddTexture("input-prompts-clean", CleanPromptTexture.Create(core.GraphicsDevice));
		foreach (string key in new[] { "enter", "escape", "q", "p", "shift", "tab", "c" })
			AddTexture("key-" + key, LoadRawTexture("Images/Inputs/keyboard_" + key + ".png"));
		AddTexture("pause-ps", LoadRawTexture("Images/Inputs/pause_ps.png"));
		Pixel = _(SpriteName.pixel);
	}

	private void LoadCharacterAtlas(string name)
	{
		string textureName = name + "-atlas";
		AddTexture(textureName, LoadRawTexture("Images/" + name + ".png"));
		var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int[]>>(
			File.ReadAllText(Path.Combine(core.Content.RootDirectory, "Images", name + ".json")));
		foreach (var entry in data)
		{
			int[] r = entry.Value;
			sprites.Add(entry.Key, new Sprite
			{
				TextureName = textureName, X = r[0], Y = r[1],
				SrcWidth = r[2], SrcHeight = r[3], Width = r[2], Height = r[3],
				LinkX = r.Length > 4 ? r[4] : 0, LinkY = r.Length > 5 ? r[5] : 0
			});
		}
	}

	private void LoadExternalFonts()
	{
		for (int i = 0; i < Enum.GetNames(typeof(Language)).Length; i++)
		{
			Language language = (Language)i;
			if (Locale.UsesExternalFont(language))
			{
				string text = $"font_{language.ToString()}";
				Texture2D texture2D = LoadRawTexture($"Fonts/{text}.png");
				AddTexture(text, texture2D);
				Sprite value = new Sprite
				{
					X = 0,
					Y = 0,
					Width = texture2D.Width,
					Height = texture2D.Height,
					SrcWidth = texture2D.Width,
					SrcHeight = texture2D.Height,
					TextureName = text
				};
				ExternalFonts.Add(language, value);
			}
		}
	}

	private Texture2D LoadRawTexture(string relativePath)
	{
		string path = Path.Combine(base.core.Content.RootDirectory, relativePath);
		using FileStream stream = File.OpenRead(path);
		return Texture2D.FromStream(base.core.GraphicsDevice, stream);
	}

	public override void Unload()
	{
		if (textures.TryGetValue("lykos-atlas", out var characterAtlas)) characterAtlas.Dispose();
		if (textures.TryGetValue("input-prompts-clean", out var cleanPrompts)) cleanPrompts.Dispose();
		sprites.Clear();
		textures.Clear();
	}

	public void AddTexture(string textureName, Texture2D texture)
	{
		textures.Add(textureName, texture);
		textureNamesByTexture[texture] = textureName;
	}

	public void AddOrReplaceTexture(string textureName, Texture2D texture)
	{
		textures[textureName] = texture;
		textureNamesByTexture[texture] = textureName;
	}

	/// <summary>Nome registrado de uma textura (ex.: "atlas.png"), usado pelo Sprite Inspector. "?" se desconhecida.</summary>
	public string GetTextureName(Texture2D texture)
	{
		return textureNamesByTexture.TryGetValue(texture, out string name) ? name : "?";
	}

	/// <summary>
	/// Acha o nome do sprite no atlas (ex.: "player_walk_3") a partir da textura e do
	/// recorte (SourceRect) usados num desenho — é o caminho inverso do atlas.
	/// Usado só pelo Sprite Inspector; faz uma busca linear (não é hot path).
	/// </summary>
	public string FindSpriteName(Texture2D texture, Rectangle sourceRect)
	{
		string textureName = GetTextureName(texture);
		foreach (KeyValuePair<string, Sprite> entry in sprites)
		{
			Sprite s = entry.Value;
			if (s.TextureName == textureName && s.X == sourceRect.X && s.Y == sourceRect.Y
				&& s.SrcWidth == sourceRect.Width && s.SrcHeight == sourceRect.Height)
			{
				return entry.Key;
			}
		}
		return textureName;
	}

	private void LoadFromFile(string fileName)
	{
		JsonReader jsonReader = JsonReader.FromFile($"Content/Images/{fileName}");
		string text = jsonReader["image-path"].ToString().Replace("\"", "");
		AddTexture(text, base.core.Content.Load<Texture2D>("Images/atlas"));
		DefaultTexture = text;
		foreach (JsonObject item in jsonReader["sprites"].ToListOfObjects())
		{
			string key = item["name"].ToString();
			int x = item["x"].ToInt();
			int y = item["y"].ToInt();
			int num = item["width"].ToInt();
			int num2 = item["height"].ToInt();
			int width = num;
			int height = num2;
			if (item.Contains("full-width"))
			{
				width = item["full-width"].ToInt();
				height = item["full-height"].ToInt();
			}
			int offXL = 0;
			int offXR = 0;
			int offYT = 0;
			int offYB = 0;
			if (item.Contains("off-x-l"))
			{
				offXL = item["off-x-l"].ToInt();
				offXR = item["off-x-r"].ToInt();
				offYT = item["off-y-t"].ToInt();
				offYB = item["off-y-b"].ToInt();
			}
			int linkX = 0;
			int linkY = 0;
			if (item["link"].ToString() == "true")
			{
				linkX = item["link-x"].ToInt();
				linkY = item["link-y"].ToInt();
			}
			sprites.Add(key, new Sprite
			{
				TextureName = text,
				X = x,
				Y = y,
				SrcWidth = num,
				SrcHeight = num2,
				Width = width,
				Height = height,
				OffXL = offXL,
				OffXR = offXR,
				OffYT = offYT,
				OffYB = offYB,
				LinkX = linkX,
				LinkY = linkY
			});
		}
	}

	public Sprite GetSprite(string name, string backupName = "pixel")
	{
		if (!sprites.ContainsKey(name))
		{
			return sprites[backupName].Clone();
		}
		return sprites[name].Clone();
	}

	public Sprite TryGetSprite(string name)
	{
		if (!sprites.ContainsKey(name))
		{
			return null;
		}
		return sprites[name].Clone();
	}

	public Sprite GetSprite(SpriteName name)
	{
		return GetSprite(name.ToString());
	}

	public bool HasSprite(string name)
	{
		return sprites.ContainsKey(name);
	}

	public Texture2D GetTexture(string name)
	{
		if (!textures.ContainsKey(name))
		{
			return null;
		}
		return textures[name];
	}

	public Sprite MakeCharSprite(char ch)
	{
		int num = "ABCDEFGHIJKLMNOPQRSTUVWXYZ.,!?\"'/\\<>()[]{}abcdefghijklmnopqrstuvwxyz_               0123456789+-=*:;                          ".IndexOf(ch);
		return new Sprite
		{
			X = num % 42 * 6,
			Y = num / 42 * 8,
			SrcWidth = 6,
			SrcHeight = 8,
			TextureName = "font"
		};
	}

	public Sprite MakeFullSpriteFromScreenshot(Screenshot screenshot)
	{
		Texture2D texture = screenshot.Texture;
		return new Sprite
		{
			X = 0,
			Y = 0,
			Width = texture.Width,
			Height = texture.Height,
			SrcWidth = texture.Width,
			SrcHeight = texture.Height,
			TextureName = "screenshot"
		};
	}
}
