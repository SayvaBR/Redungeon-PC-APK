using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PetrifiedEntity : Entity
{
	private enum PetrifiedCreature
	{
		Slime,
		Bat,
		Follower,
		Wisp,
		Serpent
	}

	private PetrifiedCreature creature;

	private Vector2 prevPos;

	private string spriteStr;

	private SpriteName sprite;

	private bool fast;

	public static BagOf<SoundName> PetrificationSounds;

	public static BagOf<SoundName> StoneBreakingSounds;

	static PetrifiedEntity()
	{
		PetrificationSounds = new BagOf<SoundName>().Put(SoundName.medusa_petrif_1).Put(SoundName.medusa_petrif_2).Put(SoundName.medusa_petrif_3);
		StoneBreakingSounds = new BagOf<SoundName>().Put(SoundName.medusa_petrif_break_1).Put(SoundName.medusa_petrif_break_2).Put(SoundName.medusa_petrif_break_3);
	}

	public PetrifiedEntity(Entity victim, Vector2 prevPos)
		: base(0f, 0f, 0.3f, 0.3f)
	{
		this.prevPos = prevPos;
		if (victim == null)
		{
			creature = PetrifiedCreature.Serpent;
			x = prevPos.X;
			y = prevPos.Y;
			UpdateTiles();
			return;
		}
		x = victim.CenterCoordinates.X;
		y = victim.CenterCoordinates.Y;
		spriteStr = SpriteName.rock_slime.ToString();
		if (victim is SlimeEntity)
		{
			creature = PetrifiedCreature.Slime;
			sprite = SpriteName.rock_slime;
		}
		if (victim is BatEntity)
		{
			creature = PetrifiedCreature.Bat;
			sprite = SpriteName.rock_bat;
			fast = true;
		}
		if (victim is FollowerEntity)
		{
			creature = PetrifiedCreature.Follower;
			sprite = SpriteName.rock_follower;
			if ((victim as FollowerEntity).Kind == FollowerKind.Red)
			{
				(victim as FollowerEntity).SpawnCoins(null);
			}
			fast = true;
		}
		if (victim is WispEntity)
		{
			creature = PetrifiedCreature.Follower;
			sprite = SpriteName.rock_wisp;
			fast = true;
			x -= 0.2f;
			y -= 0.1f;
			(victim as WispEntity).ParticleBurst();
		}
		spriteStr = sprite.ToString();
		x -= width * 0.5f;
		y -= height * 0.5f;
		SendMessage(new RemoveEntityMessage(victim));
		if (!fast)
		{
			return;
		}
		SendMessage(new RemoveEntityMessage(this), 1);
		Vector2 vector = victim.WorldCenter - prevPos;
		SendMessage(new SpawnEntityMessage(new FragmentEntity(victim.WorldCenterCoordinates, sprite, 600, new Vector4(vector.X / 16f, vector.Y / 16f, 2f, Component._rnd(-0.1f, 0.1f)), "", 0.6f).SetTintFlash(default(Color).FromRgb(9895680), Color.White, 20).OnFall(delegate(FragmentEntity f)
		{
			SendMessage(new RemoveEntityMessage(f));
			for (int i = 0; i < 5; i++)
			{
				Vector4 direction = new Vector4(Component._rnd(-0.05f, 0.05f), Component._rnd(-0.05f, 0.05f), Component._rnd(2.5f, 3.5f), Component._rnd(-1f, 1f));
				SpriteName spriteName = SpriteName.pixel;
				spriteName = i switch
				{
					3 => SpriteName.rock_shard_4, 
					2 => SpriteName.rock_shard_3, 
					1 => SpriteName.rock_shard_2, 
					0 => SpriteName.rock_shard_1, 
					_ => SpriteName.rock_shard, 
				};
				SendMessage(new PlayWorldSoundMessage(StoneBreakingSounds.DrawDifferent(), base.WorldCenter));
				SendMessage(new SpawnEntityMessage(new FragmentEntity(f.WorldCenterCoordinates, spriteName, 50, direction, "", 0.05f), null));
				base.core.CurrentPlayState.Camera.Shake("petri-fall");
			}
		}), null));
	}

	public override void Load()
	{
		Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(9563694), 0.8f, 0.4f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 0.03f;
		light.Radius = 4f;
		light.Intencity = 0.9f;
		light.TargetIntencity = 0f;
		base.Load();
	}

	public void Setup(string sprite)
	{
		spriteStr = sprite;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z].DrawSpriteW(_(spriteStr), base.WorldCenter.Shift(-7f, -9f));
		base.core.Renderer["bg", base.Z + 80, false].DrawSpriteW(_(spriteStr), base.WorldCenter.Shift(-7f, -6f), Color.Black * 0.2f, null, 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}

	public override void CollideWith(Entity other)
	{
		if (!fast && !(other is PlatformEntity) && !(other is PetrifiedEntity) && !(other is FollowerPadEntity) && !(other is WebEntity) && (!(other is PlayerEntity) || !((PlayerEntity)other).Flying) && !(other is FragmentEntity) && !(other is ItemEntity) && (!(other is GrillEntity) || (other as GrillEntity).Active) && (!(other is SpikesEntity) || (other as SpikesEntity).Active) && (creature != PetrifiedCreature.Serpent || (!(other is PetrifiedEntity { creature: PetrifiedCreature.Serpent }) && !(other is SerpentEntity { BreakT: >=0 }))))
		{
			Break(other);
			base.CollideWith(other);
		}
	}

	public override void Break(Entity offender)
	{
		SendMessage(new RemoveEntityMessage(this));
		SendMessage(new PlayWorldSoundMessage(StoneBreakingSounds.DrawDifferent(), base.WorldCenter));
		Vector2 vector = ((offender is PlayerEntity) ? (offender as PlayerEntity).FacingDirection : Vector2.Zero);
		for (int i = 0; i < 5; i++)
		{
			Vector4 direction = new Vector4(Component._rnd(-0.05f, 0.05f) + vector.X * 0.15f, Component._rnd(-0.05f, 0.05f) + vector.Y * 0.15f, Component._rnd(2.5f, 3.5f), Component._rnd(-1f, 1f));
			SpriteName spriteName = SpriteName.pixel;
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, i switch
			{
				3 => SpriteName.rock_shard_4, 
				2 => SpriteName.rock_shard_3, 
				1 => SpriteName.rock_shard_2, 
				0 => SpriteName.rock_shard_1, 
				_ => SpriteName.rock_shard, 
			}, 50, direction), null));
		}
		base.Break(offender);
	}
}
