using System;

namespace Knighter;

public class StatMission : Mission
{
	public int N
	{
		get
		{
			return GetInt("n");
		}
		set
		{
			SetInt("n", value);
		}
	}

	public Stat Stat
	{
		get
		{
			return (Stat)Enum.Parse(typeof(Stat), GetString("stat"));
		}
		set
		{
			SetString("stat", value.ToString());
		}
	}

	public int Start
	{
		get
		{
			return GetInt("start");
		}
		set
		{
			SetInt("start", value);
		}
	}

	public int Progress => _stat(Stat) - Start;

	public StatMission(MissionAction action, Stat stat, int n)
		: base(action)
	{
		N = n;
		Stat = stat;
	}

	public override void Reset()
	{
		Start = _stat(Stat);
	}

	public override bool Completed()
	{
		return Progress >= N;
	}
}
