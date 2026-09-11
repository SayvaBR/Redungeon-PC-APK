using System;
using System.Collections.Generic;

namespace Knighter;

public sealed class TimerManager : Component
{
	private readonly List<Timer> timers;

	private readonly List<Timer> timersToAdd;

	public TimerManager()
	{
		timers = new List<Timer>();
		timersToAdd = new List<Timer>();
	}

	public override void Update()
	{
		foreach (Timer item in timersToAdd)
		{
			timers.Add(item);
		}
		timersToAdd.Clear();
		List<Timer> list = new List<Timer>();
		foreach (Timer timer in timers)
		{
			timer.Update(base.ticks);
			if (timer.IsFinished)
			{
				list.Add(timer);
			}
		}
		foreach (Timer item2 in list)
		{
			timers.Remove(item2);
		}
	}

	public Timer CreateTimer(int startDelay, int interval, int loops, Action<Timer> onTimer)
	{
		Timer timer = new Timer(base.ticks, startDelay, interval, loops, onTimer);
		timersToAdd.Add(timer);
		return timer;
	}

	public Timer RunOnce(int delay, Action<Timer> onTimer)
	{
		return CreateTimer(delay, 1, 1, onTimer);
	}
}
