using Knighter.Entities;
using Knighter.States;

namespace Knighter;

public abstract class SpellEffect : Component
{
	protected int transT;

	protected int transDir;

	protected int transD = 30;

	public int Strength;

	protected PlayState playState;

	protected float transA
	{
		get
		{
			if (!Transitioning)
			{
				if (!Active)
				{
					return 0f;
				}
				return 1f;
			}
			return (float)transT / (float)transD;
		}
	}

	protected bool Transitioning
	{
		get
		{
			if (transT >= 0)
			{
				return transT <= transD;
			}
			return false;
		}
	}

	public bool Active => Strength > 0;

	protected PlayerEntity player => playState?.Player;

	protected SpellEffect(PlayState playState)
	{
		this.playState = playState;
		Strength = 0;
		transT = -1;
	}

	public virtual void Activate()
	{
		if (!Transitioning && transT < 0)
		{
			transT = 0;
		}
		transDir = 1;
		UpdatePosteffects();
	}

	public virtual void Deactivate()
	{
		if (!Transitioning && transT > transD)
		{
			transT = transD;
		}
		transDir = -1;
		UpdatePosteffects();
	}

	public abstract void DeactivatePosteffects();

	public abstract void UpdatePosteffects();

	public override void Update()
	{
		if (Transitioning)
		{
			transT += transDir;
			if (transT < 0)
			{
				DeactivatePosteffects();
			}
		}
		if (Active || Transitioning)
		{
			UpdatePosteffects();
		}
		base.Update();
	}
}
