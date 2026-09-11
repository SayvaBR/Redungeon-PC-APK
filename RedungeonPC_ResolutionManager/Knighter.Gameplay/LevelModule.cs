using System.Collections.Generic;
using Knighter.Helpers;

namespace Knighter.Gameplay;

public class LevelModule : Component
{
	public string Name;

	public string Id;

	public int Width;

	public int Height;

	public int EnterX;

	public int ExitX;

	public int SpawnX;

	public int SpawnY;

	public int Group;

	public readonly List<TileDesc> Tiles;

	public bool Debug;

	private List<ElementType> elements;

	public TileDesc this[int x, int y] => Tiles[y * Width + x];

	public bool HasElement(ElementType elementType)
	{
		return elements.Contains(elementType);
	}

	public LevelModule(JsonObject json)
	{
		Tiles = new List<TileDesc>();
		elements = new List<ElementType>();
		if (json.Contains("name"))
		{
			Name = json["name"].ToString();
		}
		if (json.Contains("id"))
		{
			Id = json["id"].ToString();
		}
		if (json.Contains("old-id"))
		{
			Id = json["old-id"].ToString();
		}
		Width = json["width"].ToInt();
		Height = json["height"].ToInt();
		if (json.Contains("enter-x"))
		{
			EnterX = json["enter-x"].ToInt();
		}
		if (json.Contains("exit-x"))
		{
			ExitX = json["exit-x"].ToInt();
		}
		if (json.Contains("spawn-x"))
		{
			SpawnX = json["spawn-x"].ToInt();
		}
		if (json.Contains("spawn-y"))
		{
			SpawnY = json["spawn-y"].ToInt();
		}
		if (json.Contains("group"))
		{
			Group = json["group"].ToInt();
		}
		if (json.Contains("debug"))
		{
			Debug = json["debug"].ToInt() == 1;
		}
		foreach (JsonObject item in json["tiles"].ToListOfObjects())
		{
			ElementType elementType = ElementType.None;
			if (item.Contains("element"))
			{
				elementType = TileDesc.ElementTypeFromName((item["element"] as JsonObject)["type"].ToString());
			}
			TileDesc tileDesc = new TileDesc
			{
				ParentModule = this,
				TileType = LevelModules.TileTypeFromName(item["type"].ToString()),
				ElementType = elementType
			};
			if (!elements.Contains(elementType))
			{
				elements.Add(elementType);
			}
			if (elementType != ElementType.None)
			{
				JsonObject jsonObject = (tileDesc.ElementJson = item["element"] as JsonObject);
				if (elementType == ElementType.Platform && jsonObject.Contains("module"))
				{
					JsonObject jsonObject2 = jsonObject["module"] as JsonObject;
					foreach (JsonObject item2 in jsonObject2["tiles"].ToListOfObjects())
					{
						if (jsonObject2.Contains("element"))
						{
							elementType = TileDesc.ElementTypeFromName((item2["element"] as JsonObject)["type"].ToString());
							if (!elements.Contains(elementType))
							{
								elements.Add(elementType);
							}
						}
					}
				}
			}
			Tiles.Add(tileDesc);
		}
	}
}
