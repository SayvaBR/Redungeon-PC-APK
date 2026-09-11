using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PotEntity : Entity
{
	private enum PotType
	{
		Normal,
		Snowman
	}

	private static BagOf<SoundName> crackSoundBag;

	private static BagOf<SoundName> smashSoundBag;

	private static BagOf<SoundName> snowmanSoundBag;

	private static BagOf<int> lootBag;

	private bool cracked;

	private Sprite spriteNormal;

	private Sprite spriteCracked;

	private Sprite spriteN;

	private Sprite spriteE;

	private Sprite spriteW;

	private Sprite spriteS;

	private Sprite sprite;

	private int hitTimer;

	private PotType type;

	private Vector2 offset;

	private bool early;

	public bool Important { get; private set; }

	static PotEntity()
	{
		lootBag = new BagOf<int>().Put(0, 60).Put(1, 40).Put(2, 20)
			.Put(3, 10)
			.Put(5, 5)
			.Put(10, 5);
		crackSoundBag = new BagOf<SoundName>().Put(SoundName.pot_crack_1);
		smashSoundBag = new BagOf<SoundName>().Put(SoundName.pot_smash_1).Put(SoundName.pot_smash_2).Put(SoundName.pot_smash_3)
			.Put(SoundName.pot_smash_4)
			.Put(SoundName.pot_smash_5);
		snowmanSoundBag = new BagOf<SoundName>().Put(SoundName.snowman_crack_1).Put(SoundName.snowman_crack_2).Put(SoundName.snowman_crack_4);
	}

	public PotEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		early = y > -10;
		Important = desc["important"] == 1;
		type = PotType.Normal;
		if (base.core.Holiday == Holiday.Xmas && (early || SciHelper.ChanceRoll(0.7f) || desc.ParentModule.Name.Contains("xmas")))
		{
			type = PotType.Snowman;
		}
		switch (type)
		{
		case PotType.Normal:
			spriteNormal = _(SpriteName.pot);
			spriteCracked = _(SpriteName.pot_cracked);
			spriteN = _(SpriteName.pot_hit_n);
			spriteE = _(SpriteName.pot_hit_e);
			spriteW = _(SpriteName.pot_hit_w);
			spriteS = _(SpriteName.pot_hit_s);
			offset = new Vector2(-1f, -3f);
			break;
		case PotType.Snowman:
			spriteNormal = _(SpriteName.snowman);
			spriteCracked = _(SpriteName.snowman_cracked);
			spriteN = _(SpriteName.snowman_hit_n);
			spriteE = _(SpriteName.snowman_hit_e);
			spriteW = _(SpriteName.snowman_hit_w);
			spriteS = _(SpriteName.snowman_hit_s);
			offset = new Vector2(0f, -3f);
			break;
		}
		cracked = false;
		sprite = spriteNormal;
		hitTimer = 0;
		if (desc["cracked"] == 1)
		{
			cracked = true;
			sprite = spriteCracked;
		}
	}

	public override void Load()
	{
		base.Load();
	}

	public override void Update()
	{
		if (hitTimer > 0)
		{
			hitTimer--;
			if (hitTimer == 0)
			{
				sprite = spriteCracked;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldPosition + offset);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldPosition + offset.Shift(0f, 13f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	private void SnowBurst(Vector2 dir)
	{
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 2f).OnSpawn(delegate(Particle p)
		{
			p.Velocity = SciHelper.GetRandomVectorInCircle(0.3f).Shift(0f, 0.2f);
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += dir * 0.9f + p.Velocity;
			p.Dead = p.Age > 30;
		})
			.OnDraw(delegate(Particle p)
			{
				float num = (float)p.Age / 30f;
				base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.pixel), p.Position, null, Vector2.One * 5f * (1f - num), 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Burst(5);
	}

	public override void InteractWith(Entity other)
	{
		bool flag = false;
		Vector2 vector = Vector2.Zero;
		if (cracked)
		{
			return;
		}
		if (other is PlayerEntity playerEntity)
		{
			flag = true;
			vector = playerEntity.FacingDirection;
		}
		if (other is BoxEntity boxEntity)
		{
			flag = true;
			vector = boxEntity.LastMovementDir;
		}
		if (flag)
		{
			cracked = true;
			if (vector.X > 0f)
			{
				sprite = spriteE;
			}
			else if (vector.X < 0f)
			{
				sprite = spriteW;
			}
			else if (vector.Y > 0f)
			{
				sprite = spriteS;
			}
			else if (vector.Y < 0f)
			{
				sprite = spriteN;
			}
			hitTimer = 5;
			switch (type)
			{
			case PotType.Normal:
				SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.pot_fragment_1, 40), null));
				SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.pot_fragment_1, 40), null));
				SendMessage(new PlayWorldSoundMessage(crackSoundBag.DrawDifferent(), base.WorldCenter));
				break;
			case PotType.Snowman:
			{
				Vector2 dir = vector;
				SendMessage(new SpawnEntityMessage(new FragmentEntity(direction: new Vector4(Component._rnd(-0.05f, 0.05f) + dir.X * 0.15f, Component._rnd(-0.05f, 0.05f) + dir.Y * 0.15f, Component._rnd(2.5f, 3.5f), Component._rnd(-1f, 1f)), coordinates: base.WorldCenterCoordinates, spriteName: SpriteName.snowman_head, ttl: 40), null));
				SendMessage(new PlayWorldSoundMessage(SoundName.snowman_crack_3, base.WorldCenter));
				SnowBurst(dir);
				break;
			}
			}
		}
		base.InteractWith(other);
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity { Flying: false } playerEntity)
		{
			Break(playerEntity);
			int num = 0;
			if (!early)
			{
				num = lootBag.Draw();
			}
			if (num > 0)
			{
				playerEntity.CollectCoins(num, this, default(Color).FromRgb(14853902));
				base.core.ParticleManager.MakeItemToHudEmitter(base.WorldCenter, ItemType.GoldCoin, many: true).Emit(num);
			}
		}
		if (other is BoxEntity offender)
		{
			Break(offender);
		}
		base.CollideWith(other);
	}

	public override void Break(Entity offender)
	{
		if (Important && !(offender is PlayerEntity))
		{
			return;
		}
		SendMessage(new RemoveEntityMessage(this));
		SendMessage(new PlayWorldSoundMessage((type == PotType.Snowman) ? snowmanSoundBag.DrawDifferent() : smashSoundBag.DrawDifferent(), base.WorldCenter));
		_inc(Stat.PotsBroken);
		Vector2 dir = ((offender is PlayerEntity) ? (offender as PlayerEntity).FacingDirection : Vector2.Zero);
		for (int i = 0; i < 3; i++)
		{
			Vector4 direction = new Vector4(Component._rnd(-0.05f, 0.05f) + dir.X * 0.15f, Component._rnd(-0.05f, 0.05f) + dir.Y * 0.15f, Component._rnd(2.5f, 3.5f), Component._rnd(-1f, 1f));
			SpriteName spriteName = SpriteName.pixel;
			switch (type)
			{
			case PotType.Normal:
				spriteName = i switch
				{
					1 => SpriteName.pot_fragment_3, 
					0 => SpriteName.pot_fragment_2, 
					_ => SpriteName.pot_fragment_4, 
				};
				break;
			case PotType.Snowman:
				spriteName = i switch
				{
					1 => SpriteName.snowman_fragment_3, 
					0 => SpriteName.snowman_fragment_2, 
					_ => SpriteName.snowman_fragment_4, 
				};
				break;
			}
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, spriteName, 50, direction), null));
		}
		if (type == PotType.Snowman)
		{
			SnowBurst(dir);
		}
		base.Break(offender);
	}

	public override bool IsPassableFor(Entity other)
	{
		if ((!(other is FragmentEntity) && !(other is PlayerEntity) && !(other is BoxEntity)) || !cracked)
		{
			if (other is PlayerEntity)
			{
				return (other as PlayerEntity).Flying;
			}
			return false;
		}
		return true;
	}
}
