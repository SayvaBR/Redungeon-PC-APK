namespace Knighter;

public class BetweenStatMission : StatMission
{
	public int M
	{
		get
		{
			return GetInt("m");
		}
		set
		{
			SetInt("m", value);
		}
	}

	public BetweenStatMission(MissionAction action, Stat stat, int n, int m)
		: base(action, stat, n)
	{
		M = m;
	}

	public override bool Completed()
	{
		if (base.Progress >= base.N)
		{
			return base.Progress <= M;
		}
		return false;
	}
}
