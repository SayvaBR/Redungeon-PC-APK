using System;

namespace Knighter;

public sealed class Timer
{
	public int StartTick { get; private set; }

	public int StartDelay { get; private set; }

	public int Interval { get; private set; }

	public int Loops { get; private set; }

	public Action<Timer> OnTimer { get; private set; }

	public bool IsActive { get; private set; }

	public bool IsFinished { get; private set; }

	public int ElapsedTicks { get; private set; }

	public int ElapsedLoops { get; private set; }

	public Timer(int startTick, int startDelay, int interval, int loops, Action<Timer> onTimer)
	{
		StartTick = startTick;
		StartDelay = startDelay;
		Interval = interval;
		Loops = loops;
		OnTimer = onTimer;
		IsActive = true;
	}

	public void Stop()
	{
		IsActive = false;
	}

	public void Update(int ticks)
	{
		if (IsActive && !IsFinished)
		{
			int num = ticks - StartTick - StartDelay;
			int elapsedLoops;
			if (num >= 0 && num % Interval == 0)
			{
				elapsedLoops = ElapsedLoops + 1;
				ElapsedLoops = elapsedLoops;
				OnTimer(this);
			}
			elapsedLoops = ElapsedTicks + 1;
			ElapsedTicks = elapsedLoops;
			IsFinished |= Loops > 0 && ElapsedLoops == Loops;
		}
	}
}
