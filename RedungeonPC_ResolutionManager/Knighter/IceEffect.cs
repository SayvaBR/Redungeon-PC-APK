using Knighter.Entities;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.States;
using Microsoft.Xna.Framework;

namespace Knighter;

public class IceEffect : SpellEffect
{
	private int animT;

	private int animD = 30;

	private BagOf<SoundName> breakSounds;

	public IceEffect(PlayState playState)
		: base(playState)
	{
		breakSounds = new BagOf<SoundName>().Put(SoundName.breaking_ice_1).Put(SoundName.breaking_ice_2).Put(SoundName.breaking_ice_3)
			.Put(SoundName.breaking_ice_4);
	}

	public override void Activate()
	{
		if (!base.player.Flying && !base.player.Falling)
		{
			base.Activate();
			Strength = 5;
			animT = animD;
			base.player.HoldingWeb?.ReleasePlayer();
			SendMessage(new PlaySoundMessage(SoundName.ice_growing));
			base.core.Renderer.PostEffectManager.EnableEffect(PostEffectType.Frost);
		}
	}

	public override void Deactivate()
	{
		if (base.Active)
		{
			SendMessage(new PlaySoundMessage(breakSounds.DrawDifferent()));
		}
		base.Deactivate();
		SpawnShards(Vector2.Zero);
		animT = 0;
		Strength = 0;
		base.core.Renderer.PostEffectManager.DisableEffect(PostEffectType.Frost);
	}

	public override void DeactivatePosteffects()
	{
	}

	public override void UpdatePosteffects()
	{
	}

	public override void Update()
	{
		base.Update();
		if (animT > 0)
		{
			animT--;
		}
	}

	private void SpawnShards(Vector2 direction)
	{
		if (Strength != 0)
		{
			SendMessage(new PlaySoundMessage(breakSounds.DrawDifferent()));
			for (int i = 0; i < 3; i++)
			{
				SendMessage(new SpawnEntityMessage(new FragmentEntity(direction: new Vector4(Component._rnd(-0.2f, 0.2f), Component._rnd(-0.2f, 0.2f), Component._rnd(2.5f, 3.5f), Component._rnd(-0.2f, 0.2f)), coordinates: base.player.WorldCenterCoordinates, spriteName: i switch
				{
					1 => SpriteName.ice_shard_2, 
					0 => SpriteName.ice_shard_1, 
					_ => SpriteName.ice_shard_3, 
				}, ttl: 50), null));
			}
			base.core.ParticleManager.AddEmitter(inWorld: true, base.player.WorldCenter, 10f).OnSpawn(delegate(Particle p)
			{
				p.Velocity = SciHelper.GetRandomVectorInCircle(0.3f).Shift(0f, 0.2f);
			}).OnUpdate(delegate(Particle p)
			{
				p.Position += direction * 0.3f + p.Velocity;
				p.Dead = p.Age > 30;
			})
				.OnDraw(delegate(Particle p)
				{
					float num = (float)p.Age / 30f;
					base.core.Renderer[base.player.Z + 10].DrawSpriteW(_(SpriteName.pixel), p.Position, null, Vector2.One * 5f * (1f - num), 0f, SpriteFlip.None, SpriteOrigin.Center);
				})
				.Burst(5);
		}
	}

	public void Push(Vector2 direction)
	{
		if (animT <= 0)
		{
			Strength--;
			if (Strength == 0)
			{
				Deactivate();
			}
			else
			{
				SpawnShards(direction);
			}
		}
	}

	public override void Draw()
	{
		if (base.Active)
		{
			int num = (int)((float)Strength * (float)(animD - animT) / (float)animD);
			if (num < 1)
			{
				num = 1;
			}
			base.core.Renderer[base.player.LastLayer, base.player.LastZ - 1, false].DrawSpriteW(_("ice_cage_back_" + num), base.player.WorldCenter + base.player.LastSpriteShift.Shift(0f, 7f), Color.White * base.player.LastSpriteAlpha, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			base.core.Renderer[base.player.LastLayer, base.player.LastZ + 1, false].DrawSpriteW(_("ice_cage_" + num), base.player.WorldCenter + base.player.LastSpriteShift.Shift(0f, 7f), Color.White * base.player.LastSpriteAlpha, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			if (base.player.LastSpriteShift.Y <= 0f)
			{
				base.core.Renderer["bg", base.player.LastZ + 32, false].DrawSpriteW(_("ice_cage_back_" + num), base.player.WorldCenter + base.player.LastSpriteShift.Shift(0f, -3f), Color.Black * 0.2f * base.player.LastSpriteAlpha, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical, SpriteOrigin.TopCenter);
			}
		}
	}
}
