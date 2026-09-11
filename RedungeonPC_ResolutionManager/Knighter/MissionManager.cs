using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Knighter.Helpers;

namespace Knighter;

public class MissionManager : Component
{
	public static Dictionary<MissionAction, MissionMeta> Metas = new Dictionary<MissionAction, MissionMeta>
	{
		{
			MissionAction.RunMeters,
			new MissionMeta
			{
				Round = 1,
				N = 100,
				Step = 50,
				Description = "Run [n] meters"
			}
		},
		{
			MissionAction.BreakWebs,
			new MissionMeta
			{
				N = 3,
				Step = 2,
				Description = "Break [n] webs"
			}
		},
		{
			MissionAction.BreakPots,
			new MissionMeta
			{
				N = 5,
				Step = 3,
				Description = "Break [n] pots"
			}
		},
		{
			MissionAction.DieOfInjury,
			new MissionMeta
			{
				N = 1,
				Step = 1,
				Description = "Die of [injury] [n] times"
			}
		},
		{
			MissionAction.LootChests,
			new MissionMeta
			{
				N = 3,
				Step = 2,
				Description = "Loot [n] chests"
			}
		},
		{
			MissionAction.DieBetween,
			new MissionMeta
			{
				N = 50,
				M = 75,
				Step = 25,
				Description = "Die between [n] and [m] meters"
			}
		},
		{
			MissionAction.CollectCoins,
			new MissionMeta
			{
				N = 25,
				Step = 25,
				Description = "Collect [n] coins"
			}
		},
		{
			MissionAction.KillCreatures,
			new MissionMeta
			{
				N = 1,
				Step = 1,
				Description = "Kill [n] [creature]"
			}
		},
		{
			MissionAction.UseJumpers,
			new MissionMeta
			{
				N = 3,
				Step = 2,
				Description = "Use [n] jumpers"
			}
		},
		{
			MissionAction.SlideMeters,
			new MissionMeta
			{
				N = 10,
				Step = 5,
				Description = "Slide [n] meters"
			}
		}
	};

	private const int MaxMissions = 3;

	private readonly List<Mission> currentMissions;

	private readonly Dictionary<MissionAction, int> statistics;

	private int completed;

	private int rounds;

	private Shuffler shuffler;

	public MissionManager()
	{
		currentMissions = new List<Mission>();
		statistics = new Dictionary<MissionAction, int>();
		foreach (MissionAction value in Enum.GetValues(typeof(MissionAction)))
		{
			statistics[value] = 0;
		}
	}

	public override void Load()
	{
		ResetShuffler();
		base.Load();
	}

	public override void Update()
	{
		base.Update();
	}

	public void OnRunBegin()
	{
		foreach (Mission currentMission in currentMissions)
		{
			if (currentMission.SingleRun)
			{
				currentMission.Reset();
			}
		}
	}

	public void OnRunEnd()
	{
		foreach (Mission item in currentMissions.FindAll((Mission m) => m.Completed()))
		{
			OnMissionCompleted(item);
		}
		currentMissions.RemoveAll((Mission m) => m.Completed());
	}

	public void BuildMissions()
	{
		while (currentMissions.Count < 3)
		{
			currentMissions.Add(BuildRandomMission());
		}
	}

	private void ResetShuffler()
	{
		int n = Metas.Values.Count((MissionMeta m) => m.Round <= rounds);
		shuffler = new Shuffler(n);
	}

	private MissionAction SelectNextMission()
	{
		if (shuffler.Empty())
		{
			rounds++;
			ResetShuffler();
		}
		return (from p in Metas
			where p.Value.Round <= rounds
			select p.Key).ToList()[shuffler.Next()];
	}

	public Mission BuildRandomMission()
	{
		MissionAction missionAction = SelectNextMission();
		int n = Metas[missionAction].N + statistics[missionAction] * Metas[missionAction].Step;
		int m = Metas[missionAction].M + statistics[missionAction] * Metas[missionAction].Step;
		Mission mission = null;
		switch (missionAction)
		{
		case MissionAction.RunMeters:
			mission = new StatMission(missionAction, Stat.CoveredDistance, n);
			break;
		case MissionAction.BreakWebs:
			mission = new StatMission(missionAction, Stat.WebsBroken, n);
			break;
		case MissionAction.LootChests:
			mission = new StatMission(missionAction, Stat.ChestsLooted, n);
			break;
		case MissionAction.DieBetween:
			mission = new BetweenStatMission(missionAction, Stat.CoveredDistance, n, m)
			{
				SingleRun = true
			};
			break;
		case MissionAction.CollectCoins:
			mission = new StatMission(missionAction, Stat.CoinsCollected, n);
			break;
		case MissionAction.BreakPots:
			mission = new StatMission(missionAction, Stat.PotsBroken, n);
			break;
		case MissionAction.DieOfInjury:
		{
			List<Stat> list3 = new List<Stat>
			{
				Stat.KilledByGravity,
				Stat.KilledBySpikes
			};
			List<string> list4 = new List<string> { "gravity", "spikes" };
			int random = SciHelper.GetRandom(0, list3.Count - 1);
			mission = new StatMission(missionAction, list3[random], n).SetString("injury", list4[random]);
			break;
		}
		case MissionAction.KillCreatures:
		{
			List<Stat> list = new List<Stat>
			{
				Stat.BatsKilled,
				Stat.SlimesKilled,
				Stat.WispsKilled
			};
			List<string> list2 = new List<string> { "bats", "slimes", "wisps" };
			int random = SciHelper.GetRandom(0, list.Count - 1);
			mission = new StatMission(missionAction, list[random], n).SetString("creature", list2[random]);
			break;
		}
		case MissionAction.UseJumpers:
			mission = new StatMission(missionAction, Stat.JumpersUsed, n);
			break;
		case MissionAction.SlideMeters:
			mission = new StatMission(missionAction, Stat.MetersSlided, n);
			break;
		}
		mission.Reset();
		statistics[missionAction]++;
		return mission;
	}

	private void OnMissionCompleted(Mission mission)
	{
		completed++;
	}

	public static string GetMissionDescription(Mission mission)
	{
		Regex regex = new Regex("\\[[a-zA-z]+\\]");
		string text = Metas[mission.Action].Description;
		Match match = regex.Match(text);
		while (match.Success)
		{
			string name = match.Value.Substring(1, match.Value.Length - 2);
			text = text.Replace(match.Value, mission.GetString(name));
			match = match.NextMatch();
		}
		return text;
	}

	private static string GetRandomValueFromEnum(Type enumType)
	{
		Array values = Enum.GetValues(enumType);
		int random = SciHelper.GetRandom(0, values.Length - 1);
		return values.GetValue(random).ToString();
	}

	private void DebugPrintMissions()
	{
		for (int i = 0; i < currentMissions.Count; i++)
		{
			Mission mission = currentMissions[i];
			base.core.DebugWatch($"{i + 1}. {GetMissionDescription(mission)}", mission.Completed() ? "completed" : (mission as StatMission).Progress.ToString(), 100);
		}
	}
}
