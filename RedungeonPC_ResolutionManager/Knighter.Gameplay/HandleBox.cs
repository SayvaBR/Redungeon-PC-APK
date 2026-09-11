using System;
using System.Collections.Generic;

namespace Knighter.Gameplay;

public class HandleBox : Component
{
	private class FloatHandle
	{
		public float Value;

		public float Target;

		public int TTL;

		public bool InWorld;

		public float IntroRate;

		public float OutroRate;
	}

	private Dictionary<string, FloatHandle> handles;

	public float GlobalFactor;

	private float lastValue;

	public float Value { get; private set; }

	public bool Changed { get; private set; }

	public HandleBox(float globalFactor = 1f)
	{
		handles = new Dictionary<string, FloatHandle>();
		GlobalFactor = globalFactor;
		lastValue = globalFactor;
		Changed = true;
		Value = GlobalFactor;
	}

	public void Set(string id, float value, bool inWorld, float introRate = 0.2f, float outroRate = 0.1f, int ttl = 2)
	{
		if (!handles.ContainsKey(id))
		{
			handles[id] = new FloatHandle
			{
				Value = 1f,
				Target = value,
				TTL = ttl,
				InWorld = inWorld,
				IntroRate = introRate,
				OutroRate = outroRate
			};
		}
		else
		{
			FloatHandle floatHandle = handles[id];
			floatHandle.Target = value;
			floatHandle.TTL = ttl;
			floatHandle.IntroRate = introRate;
			floatHandle.OutroRate = outroRate;
		}
	}

	public void SetFixed(string id, float value, bool inWorld, float introRate = 0.2f, float outroRate = 0.1f)
	{
		Set(id, value, inWorld, introRate, outroRate, -1);
	}

	public void Remove(string id)
	{
		if (handles.ContainsKey(id))
		{
			handles[id].TTL = 0;
		}
	}

	public override void Update()
	{
		string text = "";
		foreach (KeyValuePair<string, FloatHandle> handle in handles)
		{
			FloatHandle value = handle.Value;
			if (value.TTL != 0)
			{
				value.Value += (value.Target - value.Value) * value.IntroRate;
			}
			if (value.TTL > 0 && (!value.InWorld || (!base.core.CurrentPlayState.Paused && base.core.CurrentPlayState.UnpauseTimer <= 0)))
			{
				value.TTL--;
			}
			if (value.TTL == 0)
			{
				value.Value += (1f - value.Value) * value.OutroRate;
				if (Math.Abs(value.Value - 1f) < 0.01f)
				{
					text = handle.Key;
				}
			}
		}
		if (text != "")
		{
			handles.Remove(text);
		}
		Value = GlobalFactor;
		foreach (KeyValuePair<string, FloatHandle> handle2 in handles)
		{
			Value *= handle2.Value.Value;
		}
		Changed = !lastValue.Equals(Value);
		lastValue = Value;
		base.Update();
	}
}
