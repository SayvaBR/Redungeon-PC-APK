using System;
using System.Collections.Generic;
using Knighter.Entities;

namespace Knighter;

public class DeltaSyncData : Component
{
	public int Coins;

	public Dictionary<Stat, int> Stats;

	public Dictionary<Character, bool> Unlocks;

	public Dictionary<Character, int> Levels;

	public DeltaSyncData()
	{
		Stats = new Dictionary<Stat, int>();
		foreach (Stat value in Enum.GetValues(typeof(Stat)))
		{
			Stats[value] = 0;
		}
		Unlocks = new Dictionary<Character, bool>();
		Levels = new Dictionary<Character, int>();
		foreach (Character value2 in Enum.GetValues(typeof(Character)))
		{
			Unlocks[value2] = value2 == Character.Knight;
			Levels[value2] = 1;
		}
	}

	public override void Update()
	{
		Coins = base.core.ProfileData.Coins;
		foreach (Stat value in Enum.GetValues(typeof(Stat)))
		{
			Stats[value] = _stat(value);
		}
		foreach (Character value2 in Enum.GetValues(typeof(Character)))
		{
			Unlocks[value2] = base.core.ProfileData.Characters[value2].Unlocked;
			Levels[value2] = base.core.ProfileData.Characters[value2].Level;
		}
	}

	public void LoadFromStorage()
	{
		base.core.Storage.TryGetInt("delta-coins", ref Coins);
		foreach (Stat value in Enum.GetValues(typeof(Stat)))
		{
			int result = 0;
			base.core.Storage.TryGetInt($"delta-stat-{value}", ref result);
			Stats[value] = result;
		}
		foreach (Character value2 in Enum.GetValues(typeof(Character)))
		{
			bool result2 = false;
			base.core.Storage.TryGetBool($"delta-character-{value2}-unlocked", ref result2);
			Unlocks[value2] = result2;
			int result3 = 1;
			base.core.Storage.TryGetInt($"delta-character-{value2}-level", ref result3);
			Levels[value2] = result3;
		}
	}

	public void SaveIntoStorage()
	{
		base.core.Storage.SetInt("delta-coins", Coins);
		foreach (Stat value in Enum.GetValues(typeof(Stat)))
		{
			base.core.Storage.SetInt($"delta-stat-{value}", Stats[value]);
		}
		foreach (Character value2 in Enum.GetValues(typeof(Character)))
		{
			base.core.Storage.SetBool($"delta-character-{value2}-unlocked", Unlocks[value2]);
			base.core.Storage.SetInt($"delta-character-{value2}-level", Levels[value2]);
		}
	}
}
