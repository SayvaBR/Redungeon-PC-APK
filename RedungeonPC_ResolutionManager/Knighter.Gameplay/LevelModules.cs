using System;
using System.Collections.Generic;
using System.Net;
using Knighter.Helpers;

namespace Knighter.Gameplay;

public class LevelModules : Component
{
	private static List<string> levelModuleTypeNames = new List<string> { "starting-modules", "ending-modules", "corridor-modules" };

	private static List<string> tileTypeNames = new List<string> { "floor", "wall", "pit", "fragile", "ice" };

	private readonly Dictionary<LevelModuleType, List<LevelModule>> levelModules;

	public List<LevelModule> this[LevelModuleType lmt] => levelModules[lmt];

	private static string NameFromLevelModuleType(LevelModuleType lmt)
	{
		return levelModuleTypeNames[(int)lmt];
	}

	public static TileType TileTypeFromName(string name)
	{
		return (TileType)tileTypeNames.IndexOf(name);
	}

	public LevelModules()
	{
		levelModules = new Dictionary<LevelModuleType, List<LevelModule>>();
	}

	public override void Load()
	{
		LoadFromFile("modules.json");
		base.Load();
	}

	public void FetchFromInternet()
	{
		WebClient webClient = new WebClient();
		string text = "";
		try
		{
			text = webClient.DownloadString("https://www.dropbox.com/s/qh60nu45nimslny/modules.json?dl=0&raw=1");
		}
		catch (Exception)
		{
		}
		if (text != string.Empty)
		{
			Build(JsonReader.FromText(text));
		}
	}

	private void LoadFromFile(string fileName)
	{
		Build(JsonReader.FromFile($"Content/Modules/{fileName}"));
	}

	private void Build(JsonReader jr)
	{
		foreach (LevelModuleType value in Enum.GetValues(typeof(LevelModuleType)))
		{
			levelModules[value] = new List<LevelModule>();
			foreach (JsonObject item2 in jr[NameFromLevelModuleType(value)].ToListOfObjects())
			{
				LevelModule item = new LevelModule(item2);
				levelModules[value].Add(item);
			}
		}
	}
}
