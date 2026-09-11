using System;
using System.Collections.Generic;
using System.Diagnostics;
using Knighter.Entities;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter;

public class Achievements : Component
{
	public static readonly Dictionary<Achievement, AchievementMeta> Metas = new Dictionary<Achievement, AchievementMeta>
	{
		{ Achievement.LykosEatFiftyBones, new AchievementMeta(SId.ACHIEVEMENT_RAVENOUS_name,
			SId.ACHIEVEMENT_RAVENOUS_brief, SId.ACHIEVEMENT_RAVENOUS_debrief,
			SpriteName.lykos_bite, new Color(115, 51, 19), new Color(255, 181, 68), new Color(76, 34, 19)) },
		{
			Achievement.FirstUnlock,
			new AchievementMeta(SId.ACHIEVEMENT_NEW_FACES_name, SId.ACHIEVEMENT_NEW_FACES_brief, SId.ACHIEVEMENT_NEW_FACES_debrief, SpriteName.icon_lock_open, default(Color).FromRgb(598071), default(Color).FromRgb(970646), default(Color).FromRgb(598071), hidden: false, 0f, -3f)
		},
		{
			Achievement.FirstUpgrade,
			new AchievementMeta(SId.ACHIEVEMENT_NEW_POWERS_name, SId.ACHIEVEMENT_NEW_POWERS_brief, SId.ACHIEVEMENT_NEW_POWERS_debrief, SpriteName.achievement_star, default(Color).FromRgb(10237702), default(Color).FromRgb(16758083), default(Color).FromRgb(3808523), hidden: false, 0f, -10f)
		},
		{
			Achievement.UnlockAllOfThem,
			new AchievementMeta(SId.ACHIEVEMENT_PANTHEON_name, SId.ACHIEVEMENT_PANTHEON_brief, SId.ACHIEVEMENT_PANTHEON_debrief, SpriteName.achievement_pantheon, default(Color).FromRgb(1057873), default(Color).FromRgb(6139391), default(Color).FromRgb(399164), hidden: false, 0f, -7f)
		},
		{
			Achievement.PlayForOneHour,
			new AchievementMeta(SId.ACHIEVEMENT_VETERAN_name, SId.ACHIEVEMENT_VETERAN_brief, SId.ACHIEVEMENT_VETERAN_debrief, SpriteName.achievement_veteran, default(Color).FromRgb(2623248), default(Color).FromRgb(11026850), default(Color).FromRgb(2623248))
		},
		{
			Achievement.LootHundredChests,
			new AchievementMeta(SId.ACHIEVEMENT_TREASUREHUNTER_name, SId.ACHIEVEMENT_TREASUREHUNTER_brief, SId.ACHIEVEMENT_TREASUREHUNTER_debrief, SpriteName.achievement_chest, default(Color).FromRgb(2165519), default(Color).FromRgb(14903568), default(Color).FromRgb(2165519))
		},
		{
			Achievement.CollectThousandCoins,
			new AchievementMeta(SId.ACHIEVEMENT_PICKUPMASTER_name, SId.ACHIEVEMENT_PICKUPMASTER_brief, SId.ACHIEVEMENT_PICKUPMASTER_debrief, SpriteName.achievement_coins, default(Color).FromRgb(10900243), default(Color).FromRgb(16303176), default(Color).FromRgb(5445136))
		},
		{
			Achievement.KillHundredBats,
			new AchievementMeta(SId.ACHIEVEMENT_JOKER_name, SId.ACHIEVEMENT_JOKER_brief, SId.ACHIEVEMENT_JOKER_debrief, SpriteName.bat_3, default(Color).FromRgb(1774122), default(Color).FromRgb(4569718), default(Color).FromRgb(1774122))
		},
		{
			Achievement.KillHundredSlimes,
			new AchievementMeta(SId.ACHIEVEMENT_PESTCONTROL_name, SId.ACHIEVEMENT_PESTCONTROL_brief, SId.ACHIEVEMENT_PESTCONTROL_debrief, SpriteName.slime_2, default(Color).FromRgb(336662), default(Color).FromRgb(2802993), default(Color).FromRgb(336662), hidden: false, 0f, -7f)
		},
		{
			Achievement.Webmaster,
			new AchievementMeta(SId.ACHIEVEMENT_WEBMASTER_name, SId.ACHIEVEMENT_WEBMASTER_brief, SId.ACHIEVEMENT_WEBMASTER_debrief, SpriteName.spider_web_2, default(Color).FromRgb(1184275), default(Color).FromRgb(5070470), default(Color).FromRgb(1184275))
		},
		{
			Achievement.KnightDeflectFiftyThings,
			new AchievementMeta(SId.ACHIEVEMENT_UNDERCOVER_name, SId.ACHIEVEMENT_UNDERCOVER_brief, SId.ACHIEVEMENT_UNDERCOVER_debrief, SpriteName.skill_token_shield, default(Color).FromRgb(1190468), default(Color).FromRgb(3258329), default(Color).FromRgb(1190468))
		},
		{
			Achievement.CreepScareHundredCreatures,
			new AchievementMeta(SId.ACHIEVEMENT_HORRORSHOW_name, SId.ACHIEVEMENT_HORRORSHOW_brief, SId.ACHIEVEMENT_HORRORSHOW_debrief, SpriteName.skill_hud_melon, default(Color).FromRgb(2575901), default(Color).FromRgb(3458142), default(Color).FromRgb(1194512))
		},
		{
			Achievement.NathanBreakFiveHundredObstacles,
			new AchievementMeta(SId.ACHIEVEMENT_SAFERPLACE_name, SId.ACHIEVEMENT_SAFERPLACE_brief, SId.ACHIEVEMENT_SAFERPLACE_debrief, SpriteName.skill_hud_wrench, default(Color).FromRgb(2032130), default(Color).FromRgb(15808311), default(Color).FromRgb(2032130))
		},
		{
			Achievement.IchitakaCollectTwoThousandCoinsWithMagnet,
			new AchievementMeta(SId.ACHIEVEMENT_MONEYLOVESME_name, SId.ACHIEVEMENT_MONEYLOVESME_brief, SId.ACHIEVEMENT_MONEYLOVESME_debrief, SpriteName.achievement_magnet, default(Color).FromRgb(4523268), default(Color).FromRgb(16756023), default(Color).FromRgb(4523268))
		},
		{
			Achievement.VampireFlyFiftyMetersAsBat,
			new AchievementMeta(SId.ACHIEVEMENT_BATAMBITION_name, SId.ACHIEVEMENT_BATAMBITION_brief, SId.ACHIEVEMENT_BATAMBITION_debrief, SpriteName.kazhan_bat_1, default(Color).FromRgb(1250608), default(Color).FromRgb(14168481), default(Color).FromRgb(1250608))
		},
		{
			Achievement.VesnaDieWhileCastingSunrise,
			new AchievementMeta(SId.ACHIEVEMENT_LIGHTRITU_name, SId.ACHIEVEMENT_LIGHTRITU_brief, SId.ACHIEVEMENT_LIGHTRITU_debrief, SpriteName.vesna_cast_3, default(Color).FromRgb(15497728), default(Color).FromRgb(16771719), default(Color).FromRgb(10243328), hidden: true, 0f, -10f)
		},
		{
			Achievement.MageSpendThreeMinutesInSloMo,
			new AchievementMeta(SId.ACHIEVEMENT_ISLOU_name, SId.ACHIEVEMENT_ISLOU_brief, SId.ACHIEVEMENT_ISLOU_debrief, SpriteName.skill_hud_sandclock, default(Color).FromRgb(409896), default(Color).FromRgb(8570154), default(Color).FromRgb(409896))
		},
		{
			Achievement.RibLoseFiftySkulls,
			new AchievementMeta(SId.ACHIEVEMENT_FIFTYHEADS_name, SId.ACHIEVEMENT_FIFTYHEADS_brief, SId.ACHIEVEMENT_FIFTYHEADS_debrief, SpriteName.rib_skull_glow, default(Color).FromRgb(1250333), default(Color).FromRgb(4758285), default(Color).FromRgb(1250333))
		},
		{
			Achievement.MedusaDrawFiftySigns,
			new AchievementMeta(SId.ACHIEVEMENT_SSSIGNS_name, SId.ACHIEVEMENT_SSSIGNS_brief, SId.ACHIEVEMENT_SSSIGNS_debrief, SpriteName.skill_hud_snake, default(Color).FromRgb(201509), default(Color).FromRgb(443218), default(Color).FromRgb(201509))
		},
		{
			Achievement.RikCollectHundredFireballs,
			new AchievementMeta(SId.ACHIEVEMENT_FIREEATER_name, SId.ACHIEVEMENT_FIREEATER_brief, SId.ACHIEVEMENT_FIREEATER_debrief, SpriteName.achievement_fire, default(Color).FromRgb(5903616), default(Color).FromRgb(16757276), default(Color).FromRgb(5903616), hidden: false, 0f, -8f)
		},
		{
			Achievement.PanicBotDepleteHundredZappers,
			new AchievementMeta(SId.ACHIEVEMENT__name, SId.ACHIEVEMENT__brief, SId.ACHIEVEMENT__debrief, SpriteName.achievement_blackout, default(Color).FromRgb(2237993), default(Color).FromRgb(15406357), default(Color).FromRgb(2237993), hidden: false, 0f, -10f)
		},
		{
			Achievement.BragFireHundredTimes,
			new AchievementMeta(SId.ACHIEVEMENT_ARRGUMENT_name, SId.ACHIEVEMENT_ARRGUMENT_brief, SId.ACHIEVEMENT_ARRGUMENT_debrief, SpriteName.skill_hud_gun, default(Color).FromRgb(1643563), default(Color).FromRgb(1876735), default(Color).FromRgb(1643563))
		},
		{
			Achievement.SmashDozenPanicBots,
			new AchievementMeta(SId.ACHIEVEMENT_PANIC_name, SId.ACHIEVEMENT_PANIC_brief, SId.ACHIEVEMENT_PANIC_debrief, SpriteName.ppanic_bot_2, default(Color).FromRgb(4718706), default(Color).FromRgb(16732240), default(Color).FromRgb(4718706), hidden: true, 0f, -8f)
		}
	};

	public static Dictionary<InjuryType, Stat> CauseOfDeathStat = new Dictionary<InjuryType, Stat>
	{
		{
			InjuryType.Bat,
			Stat.KilledByBat
		},
		{
			InjuryType.Bolt,
			Stat.KilledByCrossbow
		},
		{
			InjuryType.Crushed,
			Stat.KilledByPiston
		},
		{
			InjuryType.Fall,
			Stat.KilledByGravity
		},
		{
			InjuryType.Saw,
			Stat.KilledBySaw
		},
		{
			InjuryType.Slime,
			Stat.KilledBySlime
		},
		{
			InjuryType.Sword,
			Stat.KilledByRotoblade
		},
		{
			InjuryType.Spikes,
			Stat.KilledBySpikes
		},
		{
			InjuryType.Timeout,
			Stat.KilledByDarkness
		},
		{
			InjuryType.Zap,
			Stat.KilledByZapper
		},
		{
			InjuryType.Axe,
			Stat.KilledByAxe
		},
		{
			InjuryType.Flame,
			Stat.KilledByFlame
		},
		{
			InjuryType.Follower,
			Stat.KilledByFollower
		},
		{
			InjuryType.DeadBattery,
			Stat.KilledByDeadBattery
		},
		{
			InjuryType.Serpent,
			Stat.KilledBySerpent
		}
	};

	public static readonly Dictionary<Achievement, int> Targets = new Dictionary<Achievement, int>
	{
		{ Achievement.LykosEatFiftyBones, 50 },
		{
			Achievement.FirstUnlock,
			1
		},
		{
			Achievement.FirstUpgrade,
			1
		},
		{
			Achievement.UnlockAllOfThem,
			Enum.GetNames(typeof(Character)).Length - 1
		},
		{
			Achievement.PlayForOneHour,
			216000
		},
		{
			Achievement.LootHundredChests,
			100
		},
		{
			Achievement.CollectThousandCoins,
			1000
		},
		{
			Achievement.KillHundredBats,
			100
		},
		{
			Achievement.KillHundredSlimes,
			100
		},
		{
			Achievement.Webmaster,
			50
		},
		{
			Achievement.KnightDeflectFiftyThings,
			50
		},
		{
			Achievement.CreepScareHundredCreatures,
			100
		},
		{
			Achievement.NathanBreakFiveHundredObstacles,
			500
		},
		{
			Achievement.IchitakaCollectTwoThousandCoinsWithMagnet,
			2000
		},
		{
			Achievement.VampireFlyFiftyMetersAsBat,
			50
		},
		{
			Achievement.MageSpendThreeMinutesInSloMo,
			10800
		},
		{
			Achievement.RibLoseFiftySkulls,
			50
		},
		{
			Achievement.MedusaDrawFiftySigns,
			50
		},
		{
			Achievement.RikCollectHundredFireballs,
			100
		},
		{
			Achievement.PanicBotDepleteHundredZappers,
			100
		},
		{
			Achievement.BragFireHundredTimes,
			100
		},
		{
			Achievement.SmashDozenPanicBots,
			12
		}
	};

	private readonly Dictionary<Achievement, int> previousPercentComplete;

	public Achievements()
	{
		previousPercentComplete = new Dictionary<Achievement, int>();
	}

	public override void Load()
	{
		foreach (Achievement value in Enum.GetValues(typeof(Achievement)))
		{
			if (IsIncremental(value))
			{
				previousPercentComplete[value] = GetProgress(value) * 100 / Targets[value];
			}
		}
		base.Load();
	}

	public static bool IsIncremental(Achievement achievement)
	{
		return Targets.ContainsKey(achievement);
	}

	public int GetProgress(Achievement achievement)
	{
		if (!IsIncremental(achievement))
		{
			return -1;
		}
		if (base.core.ProfileData.IsAchievementUnlocked(achievement))
		{
			return Targets[achievement];
		}
		return achievement switch
		{
			Achievement.FirstUnlock => base.core.ProfileData.GetNumberOfUnlocks(), 
			Achievement.FirstUpgrade => base.core.ProfileData.GetNumberOfUpgrades(), 
			Achievement.UnlockAllOfThem => base.core.ProfileData.GetNumberOfUnlocks(), 
			Achievement.PlayForOneHour => _stat(Stat.TicksInGame), 
			Achievement.LootHundredChests => _stat(Stat.ChestsLooted), 
			Achievement.KnightDeflectFiftyThings => _stat(Stat.KnightDeflectedWithShield), 
			Achievement.CreepScareHundredCreatures => _stat(Stat.CreepCreaturesScared), 
			Achievement.NathanBreakFiveHundredObstacles => _stat(Stat.NathanObstaclesBroken), 
			Achievement.IchitakaCollectTwoThousandCoinsWithMagnet => _stat(Stat.IchitakaCoinsCollectedWithMagnet), 
			Achievement.VampireFlyFiftyMetersAsBat => _stat(Stat.VampireMetersFlownAsBat), 
			Achievement.MageSpendThreeMinutesInSloMo => _stat(Stat.MageTicksInSloMo), 
			Achievement.RibLoseFiftySkulls => _stat(Stat.RibSkullsLost), 
			Achievement.MedusaDrawFiftySigns => _stat(Stat.MedusaSignsDrawn), 
			Achievement.RikCollectHundredFireballs => _stat(Stat.RikFireballsCollected), 
			Achievement.PanicBotDepleteHundredZappers => _stat(Stat.PanicBotZappersDepleted), 
			Achievement.BragFireHundredTimes => _stat(Stat.BraggTimesFired), 
			Achievement.KillHundredBats => _stat(Stat.BatsKilled), 
			Achievement.KillHundredSlimes => _stat(Stat.SlimesKilled), 
			Achievement.CollectThousandCoins => _stat(Stat.CoinsCollected), 
			Achievement.Webmaster => _stat(Stat.FlawlessWebs), 
			Achievement.SmashDozenPanicBots => _stat(Stat.SmashedPanicBots),
			Achievement.LykosEatFiftyBones => _stat(Stat.LykosBonesEaten),
			_ => 0, 
		};
	}

	public override void Update()
	{
		if (base.ticks % 100 != 0)
		{
			return;
		}
		List<Achievement> list = new List<Achievement>();
		foreach (Achievement value in Enum.GetValues(typeof(Achievement)))
		{
			if (IsIncremental(value) && !base.core.ProfileData.IsAchievementUnlocked(value))
			{
				int progress = GetProgress(value);
				int num = Targets[value];
				int num2 = 100 * progress / num;
				if (num2 > previousPercentComplete[value])
				{
					list.Add(value);
					previousPercentComplete[value] = num2;
				}
				if (progress >= num)
				{
					base.core.ProfileData.UnlockAchievement(value);
					NotifyUnlock(value);
				}
			}
		}
		if (list.Count > 0)
		{
			base.core.Scores.ReportAchievmentsProgress(list);
		}
	}

	public void Unlock(Achievement achievement)
	{
		if (!base.core.ProfileData.IsAchievementUnlocked(achievement))
		{
			base.core.ProfileData.UnlockAchievement(achievement);
			base.core.Scores.ReportAchievment(achievement);
			NotifyUnlock(achievement);
		}
	}

	/// <summary>
	/// Dispara o popup estilo Steam (ver GameHud.ShowAchievementUnlocked)
	/// pra essa conquista, se der: precisa de uma PlayState ativa e no
	/// topo da pilha de estados (nada de popup tocando por baixo de um
	/// menu, pausa, ou da tela de Continue).
	/// </summary>
	private void NotifyUnlock(Achievement achievement)
	{
		var currentPlayState = base.core.CurrentPlayState;
		if (currentPlayState != null && currentPlayState.IsTopState && currentPlayState.Hud != null && Metas.TryGetValue(achievement, out AchievementMeta meta))
		{
			currentPlayState.Hud.ShowAchievementUnlocked(meta);
		}
	}

	private int debugToastIndex;

	/// <summary>
	/// Testa o popup de conquista (U no DevTools) sem marcar nada como
	/// desbloqueado de verdade — não mexe em ProfileData nem em Scores,
	/// só dispara o toast visual/som/vibração. Cicla pela lista de
	/// conquistas a cada chamada, pra testar ícones/cores diferentes.
	/// </summary>
	[Conditional("DEBUG")]
	public void DebugTestToast()
	{
		var currentPlayState = base.core.CurrentPlayState;
		if (currentPlayState == null || currentPlayState.Hud == null || Metas.Count == 0)
		{
			return;
		}
		List<AchievementMeta> allMetas = new List<AchievementMeta>(Metas.Values);
		AchievementMeta meta = allMetas[debugToastIndex % allMetas.Count];
		debugToastIndex++;
		currentPlayState.Hud.ShowAchievementUnlocked(meta);
	}

	[Conditional("DEBUG")]
	public static void PrintAchievementIdsForGameCenter()
	{
		for (int i = 0; i < Enum.GetNames(typeof(Achievement)).Length; i++)
		{
		}
	}
}
