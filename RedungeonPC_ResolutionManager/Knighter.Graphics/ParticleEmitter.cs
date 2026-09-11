using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public class ParticleEmitter : Component
{
	public bool InWorld;

	public Vector2 Position;

	public float Radius;

	public Vector2 Size;

	private int interval;

	private int elapsed;

	private int toEmit;

	private int burstCount;

	private bool dieWhenEmpty;

	private int delay;

	private int max = -1;

	public bool LocalAttachment;

	private readonly LinkedList<Particle> particles;

	public int ActiveCount => particles.Count;

	private Action<Particle> DoSpawn;

	private Action<Particle> DoUpdate;

	private Action<Particle> DoDraw;

	public int Age { get; private set; }

	public bool Emitting { get; private set; }

	public int DeadCount { get; private set; }

	public int BornCount { get; private set; }

	public Entity HostEntity { get; private set; }

	public bool Dead { get; private set; }

	public int Count => particles.Count;

	public ParticleEmitter()
	{
		particles = new LinkedList<Particle>();
		DeadCount = 0;
		BornCount = 0;
	}

	public ParticleEmitter OnSpawn(Action<Particle> spawner)
	{
		DoSpawn = spawner;
		return this;
	}

	public ParticleEmitter OnUpdate(Action<Particle> updater)
	{
		DoUpdate = updater;
		return this;
	}

	public ParticleEmitter OnDraw(Action<Particle> painter)
	{
		DoDraw = painter;
		return this;
	}

	public ParticleEmitter AttachTo(Entity target, bool local = false)
	{
		HostEntity = target;
		LocalAttachment = local;
		return this;
	}

	public ParticleEmitter Max(int value)
	{
		max = value;
		return this;
	}

	public ParticleEmitter Delay(int value)
	{
		delay = value;
		return this;
	}

	public ParticleEmitter Burst(int count, bool once = true)
	{
		toEmit = count;
		interval = 0;
		dieWhenEmpty = once;
		Emitting = true;
		elapsed = 0;
		return this;
	}

	public ParticleEmitter Emit(int count, int interval = 5, bool once = true, int burstCount = 1)
	{
		toEmit = count;
		this.burstCount = burstCount;
		this.interval = interval;
		dieWhenEmpty = once;
		Emitting = true;
		elapsed = interval;
		return this;
	}

	public ParticleEmitter Start(int interval = 5)
	{
		toEmit = -1;
		burstCount = 1;
		this.interval = interval;
		dieWhenEmpty = false;
		Emitting = true;
		elapsed = interval;
		return this;
	}

	public ParticleEmitter RandomDelay()
	{
		elapsed = Component._rnd(1, interval);
		return this;
	}

	public ParticleEmitter Start(int count = 1, int interval = 5)
	{
		toEmit = -1;
		burstCount = count;
		this.interval = interval;
		dieWhenEmpty = false;
		Emitting = true;
		elapsed = interval;
		return this;
	}

	public ParticleEmitter Stop()
	{
		toEmit = 0;
		dieWhenEmpty = true;
		Emitting = false;
		return this;
	}

	public ParticleEmitter DieWhenEmpty()
	{
		dieWhenEmpty = true;
		return this;
	}

	public ParticleEmitter Pause()
	{
		toEmit = 0;
		dieWhenEmpty = false;
		Emitting = false;
		return this;
	}

	public ParticleEmitter Kill()
	{
		Stop();
		particles.Clear();
		Dead = true;
		return this;
	}

	public void SpawnParticle(Vector2? position = null)
	{
		if (max <= 0 || particles.Count < max)
		{
			Vector2 vector = position ?? ((Radius >= 0f) ? SciHelper.GetRandomVectorInCircle(Radius) : SciHelper.GetRandomVectorInRect(Size));
			Vector2 offset = ((Radius >= 0f) ? vector : (vector - Size * 0.5f));
			Particle particle = new Particle
			{
				InWorld = InWorld,
				Position = Position + vector,
				Offset = offset,
				Age = 0,
				Parent = this
			};
			particles.AddLast(particle);
			if (DoSpawn != null)
			{
				DoSpawn(particle);
			}
			elapsed = 0;
			BornCount++;
		}
	}

	public override void Update()
	{
		Age++;
		if (InWorld && base.core.CurrentPlayState == null)
		{
			Stop();
			particles.Clear();
			Dead = true;
		}
		if (HostEntity != null)
		{
			Position = (LocalAttachment ? HostEntity.Center : HostEntity.WorldCenter);
			if (HostEntity.Unloaded)
			{
				Stop();
				HostEntity = null;
			}
		}
		if (Dead)
		{
			return;
		}
		if (delay > 0)
		{
			delay--;
			return;
		}
		if (Emitting)
		{
			if (elapsed == interval)
			{
				if (interval == 0)
				{
					for (int i = 1; i <= toEmit; i++)
					{
						SpawnParticle();
						if (toEmit > 0)
						{
							toEmit--;
						}
					}
				}
				else
				{
					for (int j = 1; j <= burstCount; j++)
					{
						SpawnParticle();
					}
					if (toEmit > 0)
					{
						toEmit--;
					}
				}
				elapsed = 0;
				if (toEmit == 0)
				{
					Emitting = false;
				}
			}
			else
			{
				elapsed++;
			}
		}
		foreach (Particle particle in particles)
		{
			if (DoUpdate != null)
			{
				DoUpdate(particle);
			}
			particle.Age++;
			if (particle.Dead)
			{
				DeadCount++;
			}
		}
		particles.RemoveAll((Particle p) => p.Dead);
		if (particles.Count == 0)
		{
			Dead = !Emitting && dieWhenEmpty;
		}
		base.Update();
	}

	public override void Draw()
	{
		foreach (Particle particle in particles)
		{
			if (DoDraw != null)
			{
				DoDraw(particle);
			}
		}
		base.Draw();
	}
}
