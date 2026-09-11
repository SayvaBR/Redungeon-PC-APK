using System.Collections.Generic;
using Knighter.Helpers;

namespace Knighter.Graphics;

public sealed class Animation : Component
{
	private readonly Dictionary<string, List<Sprite>> animations;

	private float timeLine;

	public string CurrentSequence { get; private set; }

	public float Speed { get; set; }

	public bool Loop { get; set; }

	public bool Paused { get; private set; }

	public bool Reverse { get; set; }

	public bool Pong { get; set; }

	public bool JustStopped { get; private set; }

	public Animation(float speed = 0.2f, bool loop = true)
	{
		Speed = speed;
		Loop = loop;
		Paused = false;
		Reverse = false;
		Pong = false;
		JustStopped = false;
		CurrentSequence = "";
		animations = new Dictionary<string, List<Sprite>>();
		Reset();
	}

	public int DurationOf(string sequence)
	{
		return (int)((float)animations[sequence].Count / Speed);
	}

	public void Reset()
	{
		timeLine = (Reverse ? ((float)(animations[CurrentSequence].Count - 1)) : 0f);
	}

	public void Add(string name, IEnumerable<Sprite> frames)
	{
		animations[name] = new List<Sprite>();
		animations[name].AddRange(frames);
	}

	public Animation AddAndPlay(string name, IEnumerable<Sprite> frames)
	{
		Add(name, frames);
		Play(name);
		return this;
	}

	public Animation Add(string name, IEnumerable<SpriteName> frames)
	{
		animations[name] = new List<Sprite>();
		foreach (SpriteName frame in frames)
		{
			animations[name].Add(base.core.SpriteManager.GetSprite(frame.ToString()));
		}
		return this;
	}

	public Animation Add(string name, string baseSpriteName, string framesChain)
	{
		animations[name] = new List<Sprite>();
		if (framesChain.Length == 0)
		{
			animations[name].Add(base.core.SpriteManager.GetSprite(baseSpriteName));
		}
		else
		{
			for (int i = 0; i < framesChain.Length; i++)
			{
				animations[name].Add(base.core.SpriteManager.GetSprite(baseSpriteName + framesChain[i]));
			}
		}
		return this;
	}

	public Animation AddAndPlay(string name, IEnumerable<SpriteName> frames)
	{
		Add(name, frames);
		Play(name);
		return this;
	}

	public bool ContainsSequence(string name)
	{
		return animations.ContainsKey(name);
	}

	public List<Sprite> GetSequence(string name)
	{
		if (!ContainsSequence(name))
		{
			return null;
		}
		return animations[name];
	}

	public Animation Play(string name = "")
	{
		JustStopped = false;
		if (name == "")
		{
			name = CurrentSequence;
		}
		Paused = false;
		if (CurrentSequence == name)
		{
			return this;
		}
		CurrentSequence = name;
		Reset();
		return this;
	}

	public void Pause()
	{
		Paused = true;
	}

	public void Stop()
	{
		CurrentSequence = "";
		Paused = true;
	}

	private void InternalUpdate(bool forward = true, bool frameByFrame = false)
	{
		JustStopped = false;
		if (Paused || CurrentSequence == "")
		{
			return;
		}
		if (forward ^ Reverse)
		{
			timeLine += (frameByFrame ? 1f : Speed);
			if (timeLine >= (float)animations[CurrentSequence].Count)
			{
				if (Pong)
				{
					Reverse = !Reverse;
					timeLine -= Speed * 2f;
				}
				else if (!Loop)
				{
					timeLine -= Speed;
					Paused = true;
					JustStopped = true;
				}
				else
				{
					Reset();
				}
			}
			return;
		}
		timeLine -= (frameByFrame ? 1f : Speed);
		if (timeLine < 0f)
		{
			if (Pong)
			{
				Reverse = !Reverse;
				timeLine += 2f * Speed;
			}
			else if (!Loop)
			{
				timeLine += Speed;
				Paused = true;
				JustStopped = true;
			}
			else
			{
				Reset();
			}
		}
	}

	public override void Update()
	{
		InternalUpdate();
	}

	public void StepBackward(int steps = 1)
	{
		for (int i = 0; i < steps; i++)
		{
			InternalUpdate(forward: false);
		}
	}

	public void StepForward(int steps = 1)
	{
		for (int i = 0; i < steps; i++)
		{
			InternalUpdate();
		}
	}

	public void FrameBack(int steps = 1)
	{
		for (int i = 0; i < steps; i++)
		{
			InternalUpdate(forward: false, frameByFrame: true);
		}
	}

	public void FrameForward(int steps = 1)
	{
		for (int i = 0; i < steps; i++)
		{
			InternalUpdate(forward: true, frameByFrame: true);
		}
	}

	public Sprite GetCurrentFrame()
	{
		return animations[CurrentSequence][(int)timeLine];
	}

	public Sprite GetFrame(int frameNumber)
	{
		return animations[CurrentSequence][frameNumber];
	}

	public int GetCurrentFrameNumber()
	{
		return (int)timeLine;
	}

	public Animation SkipToRandomFrame()
	{
		timeLine = SciHelper.GetRandom(0f, animations[CurrentSequence].Count - 1);
		return this;
	}
}
