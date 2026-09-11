using System.Collections.Generic;
using Knighter.Helpers;

namespace Knighter.Gameplay;

public class TileDesc
{
	private static List<string> elementTypeNames = new List<string>
	{
		"", "spikes", "saw", "saw-rail", "chest", "torch", "piston", "platform", "bat", "loot",
		"crossbow", "obstacle", "ghost", "rotoblade", "empty", "pusher", "slime", "fragile", "spider", "web",
		"pot", "fountain", "door", "object", "zapper", "text", "statue", "firewall", "follower", "wisp",
		"cannon", "teleport", "box", "item-rail", "blockers", "button"
	};

	public LevelModule ParentModule;

	public TileType TileType;

	public ElementType ElementType;

	public JsonObject ElementJson;

	public bool Flipped;

	public int this[string name] => ElementJson[name].ToInt();

	public static ElementType ElementTypeFromName(string name)
	{
		return (ElementType)elementTypeNames.IndexOf(name);
	}

	public string Str(string name)
	{
		return ElementJson[name].ToString();
	}

	public TileDesc Flip(bool value)
	{
		if (value)
		{
			return new TileDesc
			{
				TileType = TileType,
				ElementType = ElementType,
				ElementJson = ElementJson,
				Flipped = true,
				ParentModule = ParentModule
			};
		}
		return this;
	}
}
