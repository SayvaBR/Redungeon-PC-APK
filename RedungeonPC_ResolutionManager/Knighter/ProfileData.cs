using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Helpers;
using Knighter.Localization;

namespace Knighter;

public class ProfileData : Component
{
	public int Coins;

	public int LastDistance;

	public int BestDistance;

	public bool AdsRemoved;

	public bool CoinDoublerEnabled;

	public bool AppRated;

	public bool FacebookLiked;

	public bool FeedbackSent;

	public bool TwitterFollowed;

	public bool TwitterEnemindsFollowed;

	public bool FacebookEnemindsLiked;

	public bool LearnedSwipes;

	public bool ControlsSelectorPending;

	public bool DiscoveredFactsScreen;

	public Character Character;

	public readonly Dictionary<Character, CharacterData> Characters;

	public int CurrentSlotIndex;

	public string PreviousSessionTime;

	public string FreeCoinsLastTime;

	public string FirstLaunchTime;

	public bool AutoSignIn;

	public bool UseCloud;

	public bool InitiallyMerged;

	public string LastSyncTime;

	public Language Locale;

	public bool LanguageSelectorPending;

	private readonly Dictionary<Achievement, bool> achievements;

	private readonly Dictionary<Stat, int> stats;

	public readonly DeltaSyncData DeltaSyncData;

	public int CurrentCharLevel => Characters[Character].Level;

	public ProfileData()
	{
		Coins = 0;
		Character = Character.Knight;
		Characters = new Dictionary<Character, CharacterData>();
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			Characters.Add(value, new CharacterData
			{
				Unlocked = false,
				Level = 1
			});
		}
		Characters[Character.Knight].Unlocked = true;
		achievements = new Dictionary<Achievement, bool>();
		ResetAchievements();
		stats = new Dictionary<Stat, int>();
		ResetStats();
		LearnedSwipes = false;
		ControlsSelectorPending = true;
		DiscoveredFactsScreen = false;
		CurrentSlotIndex = 0;
		PreviousSessionTime = string.Empty;
		FreeCoinsLastTime = string.Empty;
		FirstLaunchTime = string.Empty;
		AutoSignIn = true;
		DeltaSyncData = new DeltaSyncData();
		UseCloud = true;
		LastSyncTime = string.Empty;
		Locale = Language.en_US;
		LanguageSelectorPending = true;
	}

	public int GetNumberOfUnlocks()
	{
		int num = -1;
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			if (Characters[value].Unlocked)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfUpgrades()
	{
		int num = 0;
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			num += Characters[value].Level - 1;
		}
		return num;
	}

	public void AddCoins(int amount)
	{
		Coins += amount;
		SaveIntoStorage();
	}

	public void RemoveAds()
	{
		AdsRemoved = true;
		SaveIntoStorage();
	}

	public void EnableCoinDoubler()
	{
		CoinDoublerEnabled = true;
		SaveIntoStorage();
	}

	public bool IsAchievementUnlocked(Achievement achievement)
	{
		return achievements[achievement];
	}

	public void UnlockAchievement(Achievement achievement, bool saveImmediately = true)
	{
		achievements[achievement] = true;
		if (saveImmediately)
		{
			SaveIntoStorage();
		}
		Event(AnalyticsCategory.Overall, "achievement", achievement.ToString());
	}

	public void ResetAchievements()
	{
		achievements.Clear();
		for (int i = 0; i < Enum.GetNames(typeof(Achievement)).Length; i++)
		{
			achievements.Add((Achievement)i, value: false);
		}
	}

	public void ResetStats()
	{
		stats.Clear();
		for (int i = 0; i < Enum.GetNames(typeof(Stat)).Length; i++)
		{
			stats.Add((Stat)i, 0);
		}
	}

	public void IncStat(Stat stat, int number = 1)
	{
		stats[stat] += number;
	}

	public void SetStat(Stat stat, int number)
	{
		stats[stat] = number;
	}

	public int GetStat(Stat stat)
	{
		return stats[stat];
	}

	public void LoadFromStorage()
	{
		base.core.Storage.TryGetInt("coins", ref Coins);
		base.core.Storage.TryGetInt("last-distance", ref LastDistance);
		base.core.Storage.TryGetInt("best-distance", ref BestDistance);
		bool result = false;
		base.core.Storage.TryGetBool("remove-ads", ref result);
		AdsRemoved = result;
		bool result2 = false;
		base.core.Storage.TryGetBool("coin-doubler-enabled", ref result2);
		CoinDoublerEnabled = result2;
		base.core.Storage.TryGetBool("app-rated", ref AppRated);
		base.core.Storage.TryGetBool("facebook-liked", ref FacebookLiked);
		base.core.Storage.TryGetBool("feedback-sent", ref FeedbackSent);
		base.core.Storage.TryGetBool("twitter-followed", ref TwitterFollowed);
		base.core.Storage.TryGetBool("twitter-eneminds-followed", ref TwitterEnemindsFollowed);
		base.core.Storage.TryGetBool("facebook-eneminds-liked", ref FacebookEnemindsLiked);
		base.core.Storage.TryGetBool("learned-swipes", ref LearnedSwipes);
		if (!base.core.Storage.TryGetBool("controls-selector-pending-v2", ref ControlsSelectorPending))
		{
			base.core.ProfileData.ControlsSelectorPending = true;
		}
		base.core.Storage.TryGetBool("discovered-facts-screen", ref DiscoveredFactsScreen);
		int result3 = 0;
		if (base.core.Storage.TryGetInt("character", ref result3) && Enum.IsDefined(typeof(Character), result3))
		{
			Character = (Character)result3;
		}
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			base.core.Storage.TryGetBool($"character-{value}-unlocked", ref Characters[value].Unlocked);
			base.core.Storage.TryGetInt($"character-{value}-level", ref Characters[value].Level);
			// Migrate the experimental Wolf slot, which used to have five empty levels.
			Characters[value].Level = Math.Clamp(Characters[value].Level, 1, CharDescription.Get[value].Levels.Count);
		}
		// Esta edição é uma versão completa: todo o elenco fica disponível no
		// nível máximo sem alterar moedas, estatísticas ou personagem selecionado.
		// =========================================
		// UPD. Now it'll works only on debug build
		#if DEBUG
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			Characters[value].Unlocked = true;
			Characters[value].Level = CharDescription.Get[value].Levels.Count;
		}
		#endif

		foreach (Achievement value2 in Enum.GetValues(typeof(Achievement)))
		{
			bool result4 = false;
			base.core.Storage.TryGetBool($"achievment-{value2}", ref result4);
			achievements[value2] = result4;
		}
		foreach (Stat value3 in Enum.GetValues(typeof(Stat)))
		{
			int result5 = 0;
			base.core.Storage.TryGetInt($"stat-{value3}", ref result5);
			stats[value3] = result5;
		}
		base.core.Storage.TryGetInt("current-slot-index", ref CurrentSlotIndex);
		base.core.Storage.TryGetString("last-save-time", ref PreviousSessionTime);
		base.core.Storage.TryGetString("free-coins-last-time", ref FreeCoinsLastTime);
		if (!base.core.Storage.TryGetString("first-launch-time", ref FirstLaunchTime))
		{
			FirstLaunchTime = DateTimeHelper.SafeNow();
		}
		base.core.Storage.TryGetBool("auto-sign-in", ref AutoSignIn);
		base.core.Storage.TryGetBool("use-cloud", ref UseCloud);
		base.core.Storage.TryGetBool("initially-merged", ref InitiallyMerged);
		base.core.Storage.TryGetString("last-sync-time", ref LastSyncTime);
		string result6 = string.Empty;
		base.core.Storage.TryGetString("locale", ref result6);
		if (result6.Equals(string.Empty))
		{
			result6 = Language.en_US.ToString();
		}
		Locale = (Language)Enum.Parse(typeof(Language), result6);
		if (!base.core.Storage.TryGetBool("language-selector-pending", ref LanguageSelectorPending))
		{
			base.core.ProfileData.LanguageSelectorPending = true;
		}
	}

	public void SaveIntoStorage()
	{
		base.core.Storage.SetInt("coins", Coins);
		base.core.Storage.SetInt("last-distance", LastDistance);
		base.core.Storage.SetInt("best-distance", BestDistance);
		base.core.Storage.SetBool("remove-ads", AdsRemoved);
		base.core.Storage.SetBool("coin-doubler-enabled", CoinDoublerEnabled);
		base.core.Storage.SetBool("app-rated", AppRated);
		base.core.Storage.SetBool("facebook-liked", FacebookLiked);
		base.core.Storage.SetBool("feedback-sent", FeedbackSent);
		base.core.Storage.SetBool("twitter-followed", TwitterFollowed);
		base.core.Storage.SetBool("twitter-eneminds-followed", TwitterEnemindsFollowed);
		base.core.Storage.SetBool("facebook-eneminds-liked", FacebookEnemindsLiked);
		base.core.Storage.SetBool("learned-swipes", LearnedSwipes);
		base.core.Storage.SetBool("controls-selector-pending-v2", ControlsSelectorPending);
		base.core.Storage.SetBool("discovered-facts-screen", DiscoveredFactsScreen);
		base.core.Storage.SetInt("character", (int)Character);
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			base.core.Storage.SetBool($"character-{value}-unlocked", Characters[value].Unlocked);
			base.core.Storage.SetInt($"character-{value}-level", Characters[value].Level);
		}
		foreach (Achievement value2 in Enum.GetValues(typeof(Achievement)))
		{
			base.core.Storage.SetBool($"achievment-{value2}", achievements[value2]);
		}
		foreach (Stat value3 in Enum.GetValues(typeof(Stat)))
		{
			base.core.Storage.SetInt($"stat-{value3}", stats[value3]);
		}
		base.core.Storage.SetInt("current-slot-index", CurrentSlotIndex);
		base.core.Storage.SetString("last-save-time", DateTimeHelper.SafeNow());
		base.core.Storage.SetString("free-coins-last-time", FreeCoinsLastTime);
		base.core.Storage.SetString("first-launch-time", FirstLaunchTime);
		base.core.Storage.SetBool("auto-sign-in", AutoSignIn);
		base.core.Storage.SetBool("use-cloud", UseCloud);
		base.core.Storage.SetBool("initially-merged", InitiallyMerged);
		base.core.Storage.SetString("last-sync-time", LastSyncTime);
		base.core.Storage.SetString("locale", Locale.ToString());
		base.core.Storage.SetBool("language-selector-pending", LanguageSelectorPending);
		base.core.Storage.Save();
	}
}
