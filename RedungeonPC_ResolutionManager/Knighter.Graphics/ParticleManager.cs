using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public sealed class ParticleManager : Component
{
	private readonly LinkedList<ParticleEmitter> emitters;

	public ParticleManager()
	{
		emitters = new LinkedList<ParticleEmitter>();
	}

	public int ActiveParticleCount
	{
		get
		{
			int count = 0;
			foreach (ParticleEmitter emitter in emitters)
			{
				count += emitter.ActiveCount;
			}
			return count;
		}
	}

	public ParticleEmitter AddEmitter(bool inWorld, Vector2 position, float radius = 0f)
	{
		ParticleEmitter particleEmitter = new ParticleEmitter
		{
			InWorld = inWorld,
			Position = position,
			Radius = radius
		};
		emitters.AddLast(particleEmitter);
		return particleEmitter;
	}

	public ParticleEmitter AddEmitter(bool inWorld, Vector2 position, float width, float height)
	{
		ParticleEmitter particleEmitter = new ParticleEmitter
		{
			InWorld = inWorld,
			Position = position,
			Radius = -1f,
			Size = new Vector2(width, height)
		};
		emitters.AddLast(particleEmitter);
		return particleEmitter;
	}

	public void KillEmittersInWorld()
	{
		emitters.RemoveAll((ParticleEmitter e) => e.InWorld);
	}

	public override void Update()
	{
		emitters.RemoveAll((ParticleEmitter e) => e.Dead);
		bool flag = base.core.CurrentPlayState != null && (base.core.CurrentPlayState.Paused || base.core.CurrentPlayState.UnpauseTimer > 0);
		foreach (ParticleEmitter emitter in emitters)
		{
			if (emitter.InWorld && base.core.CurrentPlayState == null)
			{
				emitter.Kill();
			}
			if (!(emitter.InWorld && flag))
			{
				emitter.Update();
			}
		}
		base.Update();
	}

	public override void Draw()
	{
#if DEBUG
		if (DevTools.SuppressParticles)
		{
			return;
		}
#endif
		foreach (ParticleEmitter emitter in emitters)
		{
			emitter.Draw();
		}
		base.Draw();
	}

	public void AddSmoke(Vector2 position, int Z, int length = 2, Entity host = null)
	{
		base.core.ParticleManager.AddEmitter(inWorld: true, position, 2f).OnSpawn(delegate
		{
		}).OnUpdate(delegate(Particle p)
		{
			p.Dead = p.Age > 60;
			p.Position += new Vector2(p.Offset.X * 0.05f, p.Offset.Y * 0.1f - 0.5f);
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[Z].DrawSpriteW(_(SpriteName.smoke), p.Position, scale: new Vector2(2f * (float)p.Age / 60f), tint: Color.White * ((float)(60 - p.Age) / 60f), rotation: p.Offset.X * (float)p.Age * 0.02f, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			})
			.Emit(length, 3, once: true, 2)
			.AttachTo(host);
	}

	public ParticleEmitter MakeItemToHudEmitter(Vector2 position, ItemType type, bool many = false, bool isMoney = true)
	{
		Vector2 target = (isMoney ? new Vector2(base.core.Renderer.ScreenWidth - 30, 5f) : new Vector2((float)base.core.Renderer.ScreenWidth / 2f, 20f));
		string sequence = ItemEntity.itemAnimations[(int)type];
		return AddEmitter(inWorld: false, position, 4f).OnSpawn(delegate(Particle p)
		{
			p.Position = base.core.Renderer.ToScreen(p.Position);
			switch (type)
			{
			case ItemType.CandyCane:
			case ItemType.Ginger:
				SendMessage(new PlaySoundMessage(SoundName.crunch));
				break;
			case ItemType.Tangerine:
				SendMessage(new PlaySoundMessage(SoundName.gulp));
				break;
			case ItemType.SkullKey:
				SendMessage(new PlaySoundMessage(SoundName.coin, 1f, -0.5f));
				break;
			default:
				base.core.PlayCoinSound();
				break;
			}
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += (target - p.Position) * 0.04f;
			p.Dead = p.Age > 50;
		}).OnDraw(delegate(Particle p)
		{
			if (!base.core.TakingScreenshot)
			{
				Sprite sprite = base.core.SpriteManager.GetSprite(ItemEntity.itemNames[(int)type] + "_" + sequence[(int)((float)p.Age * 0.25f) % sequence.Length]);
				base.core.Renderer["fg", 1000, false].DrawSpriteS(sprite, p.Position, Color.White * ((float)Math.Min(50 - p.Age, 10) / 10f), new Vector2(many ? Component._M(Component._m(p.Age, 5f) / 5f, 0.3f) : 1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		});
	}
}
