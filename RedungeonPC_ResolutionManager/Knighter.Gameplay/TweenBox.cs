using Microsoft.Xna.Framework;

namespace Knighter.Gameplay;

public class TweenBox : Component
{
	private readonly FloatBox value;

	public float From { get; private set; }

	public float To { get; private set; }

	public int T { get; private set; }

	public int Duration { get; private set; }

	public bool Running { get; private set; }

	public TweenBox(FloatBox value)
	{
		this.value = value;
		Running = false;
	}

	public void Start(float from, float to, int duration)
	{
		From = from;
		To = to;
		Duration = duration;
		T = 0;
		Running = true;
		Update();
	}

	public override void Update()
	{
		if (Running)
		{
			int t = T + 1;
			T = t;
			value.F = MathHelper.Lerp(From, To, (float)T / (float)Duration);
			if (T == Duration)
			{
				Running = false;
			}
		}
	}
}
