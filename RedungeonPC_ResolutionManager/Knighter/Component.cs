using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;

namespace Knighter;

public abstract class Component
{
	protected Renderer R => core.Renderer;

	protected Core core => Core.Instance;

	protected int ticks => core.Ticks;

	protected int topSafeArea => core.Renderer.TopSafeArea;

	protected string __(SId id)
	{
		return core.LocaleManager.GetForCurrentLocale(id.ToString());
	}

	protected Sprite _(SpriteName name)
	{
		return core.SpriteManager.GetSprite(name);
	}

	protected Sprite _(string name, string backupName = "pixel")
	{
		return core.SpriteManager.GetSprite(name, backupName);
	}

	protected static float _rnd(float f1, float f2)
	{
		return SciHelper.GetRandom(f1, f2);
	}

	protected static int _rnd(int i1, int i2)
	{
		return SciHelper.GetRandom(i1, i2);
	}

	protected static float _m(float f1, float f2)
	{
		return Math.Min(f1, f2);
	}

	protected static float _M(float f1, float f2)
	{
		return Math.Max(f1, f2);
	}

	protected static float _sin(float x)
	{
		return (float)Math.Sin(x);
	}

	protected static float _cos(float x)
	{
		return (float)Math.Cos(x);
	}

	protected void _inc(Stat stat, int number = 1)
	{
		core.ProfileData.IncStat(stat, number);
	}

	protected int _stat(Stat stat)
	{
		return core.ProfileData.GetStat(stat);
	}

	public virtual void Load()
	{
	}

	public virtual void Unload()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Draw()
	{
	}

	public virtual void OnMessage(Message message, object sender)
	{
	}

	protected void Subscribe(MessageType type)
	{
		core.MessageManager.Subscribe(type, this);
	}

	protected void Unsubscribe(MessageType type)
	{
		core.MessageManager.Unsubscribe(type, this);
	}

	protected void SendMessage(Message message, int delay = 0)
	{
		core.MessageManager.Send(message, this, delay);
	}

	protected void Screen(string name)
	{
		core.Analytics.TrackScreen(name);
	}

	protected void Event(AnalyticsCategory category, string action)
	{
		core.Analytics.TrackEvent(category, action, "");
	}

	protected void Event(AnalyticsCategory category, string action, string label)
	{
		core.Analytics.TrackEvent(category, action, label);
	}

	protected void Event(AnalyticsCategory category, string action, int value)
	{
		core.Analytics.TrackEvent(category, action, value.ToString(), value);
	}

	protected void Event(AnalyticsCategory category, string action, string label, int value)
	{
		core.Analytics.TrackEvent(category, action, label, value);
	}

	protected void Exception(string message, bool isFatal)
	{
		core.Analytics.TrackException(message, isFatal);
	}
}
